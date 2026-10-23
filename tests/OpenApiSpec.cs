using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Fastpix.Tests;

internal sealed class EndpointInfo
{
    public required string Path { get; init; }
    public required string OperationId { get; init; }
    public required JObject Responses { get; init; }
    public List<JObject> Parameters { get; init; } = new();
    public string Method { get; init; } = "GET";
}

/// <summary>
/// Loads the OpenAPI spec, extracts GET endpoints, and builds per-status JSON
/// Schema validators using NJsonSchema. Mirrors the spec handling in
/// fastpix-php/Tests/validate-get-endpoints.ts (refs -> #/definitions, plus an
/// OpenAPI `nullable` -> JSON-Schema type-union conversion).
/// </summary>
internal static class OpenApiSpec
{
    public static JObject Load(string specPath)
    {
        var yaml = new YamlStream();
        using (var reader = File.OpenText(specPath)) yaml.Load(reader);
        return (JObject)ToJToken(yaml.Documents[0].RootNode);
    }

    /// <summary>
    /// YAML node -> JToken. Plain scalars resolve by the YAML 1.2 core schema (null, bool, int, float,
    /// else string); quoted and block scalars always stay strings. Duplicate keys throw.
    /// </summary>
    private static JToken ToJToken(YamlNode node) => node switch
    {
        YamlMappingNode map => new JObject(map.Children.Select(kv => new JProperty(((YamlScalarNode)kv.Key).Value!, ToJToken(kv.Value)))),
        YamlSequenceNode seq => new JArray(seq.Children.Select(ToJToken)),
        YamlScalarNode { Style: ScalarStyle.Plain } s => PlainScalar(s.Value ?? ""),
        YamlScalarNode s => new JValue(s.Value),
        _ => throw new NotSupportedException($"YAML node {node.NodeType} at {node.Start}"),
    };

    // ponytail: .inf/.nan stay strings (JSON cannot hold them); the spec has none.
    private static JValue PlainScalar(string v) => v switch
    {
        "" or "~" or "null" or "Null" or "NULL" => JValue.CreateNull(),
        "true" or "True" or "TRUE" => new JValue(true),
        "false" or "False" or "FALSE" => new JValue(false),
        _ when Regex.IsMatch(v, "^0x[0-9a-fA-F]+$") => new JValue(Convert.ToInt64(v[2..], 16)),
        _ when Regex.IsMatch(v, "^0o[0-7]+$") => new JValue(Convert.ToInt64(v[2..], 8)),
        _ when Regex.IsMatch(v, "^[-+]?[0-9]+$") && long.TryParse(v, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var l) => new JValue(l),
        _ when Regex.IsMatch(v, @"^[-+]?(\.[0-9]+|[0-9]+(\.[0-9]*)?)([eE][-+]?[0-9]+)?$") => new JValue(double.Parse(v, NumberStyles.Float, CultureInfo.InvariantCulture)),
        _ => new JValue(v),
    };

    public static List<EndpointInfo> ExtractGetEndpoints(JObject spec)
    {
        var result = new List<EndpointInfo>();
        if (spec["paths"] is not JObject paths) return result;

        foreach (var pathProp in paths.Properties())
        {
            if (pathProp.Value is not JObject methods) continue;
            if (methods["get"] is not JObject get) continue;

            var parameters = new List<JObject>();
            if (get["parameters"] is JArray opParams)
                parameters.AddRange(opParams.OfType<JObject>());
            if (methods["parameters"] is JArray pathParams)
                parameters.AddRange(pathParams.OfType<JObject>());

            result.Add(new EndpointInfo
            {
                Path = pathProp.Name,
                OperationId = get["operationId"]?.ToString() ?? pathProp.Name,
                Responses = get["responses"] as JObject ?? new JObject(),
                Parameters = parameters,
                Method = "GET",
            });
        }

        return result;
    }

    /// <summary>operationId -> endpoint for POST/PUT/PATCH/DELETE operations.</summary>
    public static Dictionary<string, EndpointInfo> ExtractNonGetEndpoints(JObject spec)
    {
        var result = new Dictionary<string, EndpointInfo>();
        if (spec["paths"] is not JObject paths) return result;

        foreach (var pathProp in paths.Properties())
        {
            if (pathProp.Value is not JObject methods) continue;
            foreach (var method in new[] { "post", "put", "patch", "delete" })
            {
                if (methods[method] is not JObject op) continue;
                var opId = op["operationId"]?.ToString();
                if (opId == null) continue;
                result[opId] = new EndpointInfo
                {
                    Path = pathProp.Name,
                    OperationId = opId,
                    Responses = op["responses"] as JObject ?? new JObject(),
                    Method = method.ToUpperInvariant(),
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Build a status -> compiled JSON Schema map for one endpoint's responses.
    /// Returns null when no response defines an application/json schema.
    /// </summary>
    public static async Task<Dictionary<string, JsonSchema>?> BuildValidatorsAsync(JObject spec, EndpointInfo endpoint)
    {
        var validators = new Dictionary<string, JsonSchema>();

        foreach (var statusProp in endpoint.Responses.Properties())
        {
            var schema = statusProp.Value?["content"]?["application/json"]?["schema"];
            if (schema == null) continue;

            try
            {
                validators[statusProp.Name] = await CompileSchemaAsync(spec, schema);
            }
            catch
            {
                // A schema NJsonSchema can't compile is treated as "no validator"
                // for that status rather than crashing the whole run; SelfCheckAsync
                // fails the run at startup if that ever happens.
            }
        }

        return validators.Count > 0 ? validators : null;
    }

    /// <summary>
    /// Offline startup check: every application/json response schema of <paramref name="endpoints"/>
    /// must yield a validator, and the probe schema must reject wrong-typed `success` (boolean) and
    /// `data` (object) fields, and the spec-wide <see cref="SpecProbesAsync"/> must pass. Returns the
    /// validator count and one line per problem.
    /// </summary>
    public static async Task<(int Validators, List<string> Problems)> SelfCheckAsync(
        JObject spec, IReadOnlyCollection<EndpointInfo> endpoints, string probeOpId, string probeStatus)
    {
        var problems = new List<string>();
        var count = 0;
        JsonSchema? probe = null;

        foreach (var ep in endpoints)
        {
            var validators = await BuildValidatorsAsync(spec, ep);
            foreach (var status in ep.Responses.Properties().Where(p => p.Value?["content"]?["application/json"]?["schema"] != null).Select(p => p.Name))
            {
                if (validators != null && validators.TryGetValue(status, out var v))
                {
                    count++;
                    if (ep.OperationId == probeOpId && status == probeStatus) probe = v;
                }
                else problems.Add($"{ep.Method} {ep.Path} ({ep.OperationId}) response {status}: no validator (schema did not compile)");
            }
        }

        if (probe == null)
        {
            problems.Add($"probe {probeOpId} response {probeStatus}: no validator");
            return (count, problems);
        }

        foreach (var (field, body) in new[] { ("success", @"{""success"":""yes""}"), ("data", @"{""success"":true,""data"":""x""}") })
            if (!Validate(probe, JObject.Parse(body)).Any(f => f.Path?.Contains(field) == true))
                problems.Add($"probe {probeOpId} response {probeStatus}: wrong-typed '{field}' in {body} passed validation");

        problems.AddRange(await SpecProbesAsync(spec));
        return (count, problems);
    }

    /// <summary>
    /// Spec-wide probes: Load must keep YAML scalar types (a string `nullable: "true"` is silently ignored),
    /// quoted scalars must stay strings, and compiled schemas must still enforce minLength and
    /// additionalProperties. One line per failed probe.
    /// </summary>
    private static async Task<List<string>> SpecProbesAsync(JObject spec)
    {
        var problems = new List<string>();

        var overall = ExtractGetEndpoints(spec).FirstOrDefault(e => e.OperationId == "list_overall_values");
        var overallSchema = overall == null ? null : (await BuildValidatorsAsync(spec, overall))?.GetValueOrDefault("200");
        const string nullBody = @"{""success"":true,""data"":{""value"":null,""totalWatchTime"":null,""totalPlayTime"":null,""globalValue"":null}}";
        if (overallSchema == null) problems.Add("nullable probe: list_overall_values response 200 has no validator");
        else if (Validate(overallSchema, JObject.Parse(nullBody)) is { Count: > 0 } f)
            problems.Add($"nullable probe: list_overall_values 200 rejected nulls in nullable fields: {string.Join("; ", f.Select(x => $"{x.Path} {x.Message}"))}");

        foreach (var (component, rule, bad, good) in new[]
        {
            ("CloudPlayoutProgramRequest", "minLength 3", @"{""name"":""ab""}", @"{""name"":""abc""}"),
            ("CreatePlaylistRequestManual", "additionalProperties: false",
                @"{""name"":""p"",""referenceId"":""r"",""type"":""manual"",""bogus"":1}", @"{""name"":""p"",""referenceId"":""r"",""type"":""manual""}"),
        })
        {
            try
            {
                var schema = await CompileSchemaAsync(spec, new JObject { ["allOf"] = new JArray { new JObject { ["$ref"] = $"#/components/schemas/{component}" } } });
                if (Validate(schema, JObject.Parse(bad)).Count == 0) problems.Add($"constraint probe: {component} accepted {bad} ({rule})");
                if (Validate(schema, JObject.Parse(good)) is { Count: > 0 } g) problems.Add($"constraint probe: {component} rejected valid {good}: {g[0].Path} {g[0].Message}");
            }
            catch (Exception e)
            {
                problems.Add($"constraint probe: {component} did not compile: {e.Message}");
            }
        }

        var media = spec["components"]?["schemas"]?["GetAllMediaResponse"]?["properties"];
        var timespan = spec.SelectToken("$.paths['/data/viewlist'].get.parameters[?(@.name == 'timespan[]')].schema.enum[0]");
        foreach (var (what, token, type) in new[]
        {
            ("GetAllMediaResponse.frameRate example \"30.000\" (quoted)", media?["frameRate"]?["example"], JTokenType.String),
            ("/data/viewlist timespan[] enum 60:minutes (plain, not a number)", timespan, JTokenType.String),
            ("GetAllMediaResponse.duration example 10.5 (plain)", media?["duration"]?["example"], JTokenType.Float),
            ("GetAllMediaResponse.isAudioOnly nullable (plain)", media?["isAudioOnly"]?["nullable"], JTokenType.Boolean),
        })
            if (token?.Type != type)
                problems.Add($"string-typing probe: {what} loaded as {token?.Type.ToString() ?? "missing"}, expected {type}");

        return problems;
    }

    /// <summary>Compile one spec schema node (e.g. a requestBody schema) with the spec's components as definitions.</summary>
    public static async Task<JsonSchema> CompileSchemaAsync(JObject spec, JToken schema)
    {
        var definitions = spec["components"]?["schemas"] as JObject ?? new JObject();
        var transformed = TransformSchemaNode(schema.DeepClone());
        var composed = transformed is JObject schemaObj ? (JObject)schemaObj.DeepClone() : new JObject { ["allOf"] = new JArray { transformed } };
        composed["definitions"] = TransformSchemaNode(definitions.DeepClone());
        // Discriminator mappings also hold component refs (as plain strings); point them at definitions too.
        return await JsonSchema.FromJsonAsync(composed.ToString().Replace("\"#/components/schemas/", "\"#/definitions/"));
    }

    public static List<ValidationFinding> Validate(JsonSchema schema, JToken? body)
    {
        var token = body ?? JValue.CreateNull();
        var errors = schema.Validate(token);
        return errors.Select(e => new ValidationFinding
        {
            Path = string.IsNullOrEmpty(e.Path) ? null : e.Path,
            Message = $"{e.Kind}{(string.IsNullOrEmpty(e.Property) ? "" : $" ({e.Property})")}",
        }).ToList();
    }

    /// <summary>
    /// Recursively: rewrite OpenAPI component refs to JSON-Schema definition refs,
    /// and convert `nullable: true` into a JSON-Schema-compatible nullable type.
    /// </summary>
    private static JToken TransformSchemaNode(JToken node)
    {
        switch (node)
        {
            case JArray arr:
            {
                var outArr = new JArray();
                foreach (var item in arr) outArr.Add(TransformSchemaNode(item));
                return outArr;
            }
            case JObject obj:
            {
                var outObj = new JObject();
                bool nullable = obj["nullable"]?.Type == JTokenType.Boolean && obj["nullable"]!.Value<bool>();

                foreach (var prop in obj.Properties())
                {
                    if (prop.Name == "nullable") continue; // not a JSON Schema keyword

                    if (prop.Name == "$ref" && prop.Value.Type == JTokenType.String)
                    {
                        outObj["$ref"] = prop.Value.ToString().Replace("#/components/schemas/", "#/definitions/");
                        continue;
                    }

                    outObj[prop.Name] = TransformSchemaNode(prop.Value);
                }

                if (nullable)
                {
                    ApplyNullable(outObj);
                }

                return outObj;
            }
            default:
                return node;
        }
    }

    private static void ApplyNullable(JObject schema)
    {
        // type: "string" -> type: ["string","null"]
        if (schema["type"]?.Type == JTokenType.String)
        {
            var t = schema["type"]!.ToString();
            schema["type"] = new JArray { t, "null" };
            return;
        }

        // oneOf/anyOf -> add a null branch
        foreach (var key in new[] { "oneOf", "anyOf" })
        {
            if (schema[key] is JArray branches)
            {
                branches.Add(new JObject { ["type"] = "null" });
                return;
            }
        }

        // $ref-only nullable -> wrap so null is also accepted
        if (schema["$ref"] != null)
        {
            var refToken = schema["$ref"]!;
            schema.Remove("$ref");
            schema["oneOf"] = new JArray
            {
                new JObject { ["$ref"] = refToken },
                new JObject { ["type"] = "null" },
            };
        }
    }
}

internal sealed class ValidationFinding
{
    public string? Path { get; init; }
    public string? Message { get; init; }
}

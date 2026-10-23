using System.Net.Http.Headers;
using Fastpix.Models.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fastpix.Tests;

using Ctx = Dictionary<string, string?>;

internal sealed class LifecycleStep
{
    public required string OperationId { get; init; }
    public required string Phase { get; init; } // CREATE | UPDATE | DELETE
    public string[] Needs { get; init; } = Array.Empty<string>();
    public Func<Ctx, Ctx> Request { get; init; } = _ => new Ctx();
    public Action<JToken?, Ctx>? Capture { get; init; }
    public string? RetryOn { get; init; }
}

internal sealed class StepResult
{
    public required string OperationId { get; set; }
    public required string Method { get; set; }
    public required string Path { get; set; }
    public required string Phase { get; set; }
    public string Status { get; set; } = "FAIL"; // PASS | FAIL | SKIP
    public int? HttpStatus { get; set; }
    public bool? OpenapiValid { get; set; }
    public List<ValidationFinding> OpenapiErrors { get; set; } = new();
    public bool SdkOk { get; set; }
    public string? SdkError { get; set; }
    public List<string> MissingInSdk { get; set; } = new();
    public List<string> MissingInApi { get; set; } = new();
    public string? Note { get; set; }
    public string? CapturedId { get; set; }
}

/// <summary>
/// Non-GET (POST/PUT/PATCH/DELETE) lifecycle validator. Runs CREATE -> UPDATE ->
/// DELETE so every mutation happens before teardown. Because these calls mutate
/// live data, each is invoked once and BOTH the SDK value and the raw wire body
/// are captured from the single response. Port of validate-non-get-endpoints.ts.
/// </summary>
internal static class NonGetValidator
{
    private const string ArtifactsDirName = "artifacts-non-get";
    private const string ReportFile = "NON_GET_ENDPOINTS_VALIDATION_REPORT.md";
    private const int MaxPreviewChars = 4000;

    private static string? Str(JToken? t) => t == null || t.Type == JTokenType.Null ? null : t.ToString();

    internal static readonly List<LifecycleStep> Steps = new()
    {
        // ---- CREATE ----
        new() { OperationId = "create_signing_key", Phase = "CREATE", Capture = (v, c) => c["signingKeyId"] = Str(v?["data"]?["id"]) },
        new() { OperationId = "create-a-playlist", Phase = "CREATE", Capture = (v, c) => c["playlistId"] = Str(v?["data"]?["id"]) },
        new() { OperationId = "create-new-stream", Phase = "CREATE", Capture = (v, c) => c["streamId"] = Str(v?["data"]?["streamId"]) ?? Str(v?["data"]?["id"]) },
        new() { OperationId = "create-media", Phase = "CREATE", Capture = (v, c) => { c["mediaId"] = Str(v?["data"]?["id"]); c["mediaPlaybackId"] = Str(v?["data"]?["playbackIds"]?[0]?["id"]); } },
        new() { OperationId = "create-media-playback-id", Phase = "CREATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] }, Capture = (v, c) => c["createdPlaybackId"] = Str(v?["data"]?["playbackIds"]?[0]?["id"]) ?? Str(v?["data"]?["id"]) },
        new() { OperationId = "Add-media-track", Phase = "CREATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] }, Capture = (v, c) => c["trackId"] = Str(v?["data"]?["id"]) },
        new() { OperationId = "create-playbackId-of-stream", Phase = "CREATE", Needs = new[] { "streamId" }, Request = c => new Ctx { ["streamId"] = c["streamId"] }, Capture = (v, c) => c["streamPlaybackId"] = Str(v?["data"]?["playbackIds"]?[0]?["id"]) ?? Str(v?["data"]?["id"]) },
        new() { OperationId = "create-simulcast-of-stream", Phase = "CREATE", Needs = new[] { "streamId" }, Request = c => new Ctx { ["streamId"] = c["streamId"] }, Capture = (v, c) => c["simulcastId"] = Str(v?["data"]?["simulcastId"]) ?? Str(v?["data"]?["id"]) },
        new() { OperationId = "direct-upload-video-media", Phase = "CREATE", Capture = (v, c) => c["uploadId"] = Str(v?["data"]?["uploadId"]) ?? Str(v?["data"]?["id"]) },
        new() { OperationId = "Generate-subtitle-track", Phase = "CREATE", Needs = new[] { "mediaId", "trackId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"], ["trackId"] = c["trackId"] } },

        // ---- UPDATE ----
        new() { OperationId = "updated-media", Phase = "UPDATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "updated-source-access", Phase = "UPDATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "updated-mp4Support", Phase = "UPDATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "update-media-summary", Phase = "UPDATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "update-media-chapters", Phase = "UPDATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "update-media-named-entities", Phase = "UPDATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "update-media-moderation", Phase = "UPDATE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "update-media-track", Phase = "UPDATE", Needs = new[] { "mediaId", "trackId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"], ["trackId"] = c["trackId"] } },
        new() { OperationId = "update-domain-restrictions", Phase = "UPDATE", Needs = new[] { "mediaId", "mediaPlaybackId" }, RetryOn = "not ready for updates", Request = c => new Ctx { ["mediaId"] = c["mediaId"], ["playbackId"] = c["mediaPlaybackId"] } },
        new() { OperationId = "update-user-agent-restrictions", Phase = "UPDATE", Needs = new[] { "mediaId", "mediaPlaybackId" }, RetryOn = "not ready for updates", Request = c => new Ctx { ["mediaId"] = c["mediaId"], ["playbackId"] = c["mediaPlaybackId"] } },
        new() { OperationId = "update-live-stream-domain-restrictions", Phase = "UPDATE", Needs = new[] { "streamId", "streamPlaybackId" }, Request = c => new Ctx { ["streamId"] = c["streamId"], ["playbackId"] = c["streamPlaybackId"] } },
        new() { OperationId = "update-live-stream-user-agent-restrictions", Phase = "UPDATE", Needs = new[] { "streamId", "streamPlaybackId" }, Request = c => new Ctx { ["streamId"] = c["streamId"], ["playbackId"] = c["streamPlaybackId"] } },
        new() { OperationId = "update-a-playlist", Phase = "UPDATE", Needs = new[] { "playlistId" }, Request = c => new Ctx { ["playlistId"] = c["playlistId"] } },
        new() { OperationId = "add-media-to-playlist", Phase = "UPDATE", Needs = new[] { "playlistId", "mediaId" }, Request = c => new Ctx { ["playlistId"] = c["playlistId"], ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "change-media-order-in-playlist", Phase = "UPDATE", Needs = new[] { "playlistId", "mediaId" }, Request = c => new Ctx { ["playlistId"] = c["playlistId"], ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "update-live-stream", Phase = "UPDATE", Needs = new[] { "streamId" }, Request = c => new Ctx { ["streamId"] = c["streamId"] } },
        new() { OperationId = "update-specific-simulcast-of-stream", Phase = "UPDATE", Needs = new[] { "streamId", "simulcastId" }, Request = c => new Ctx { ["streamId"] = c["streamId"], ["simulcastId"] = c["simulcastId"] } },
        new() { OperationId = "disable-live-stream", Phase = "UPDATE", Needs = new[] { "streamId" }, Request = c => new Ctx { ["streamId"] = c["streamId"] } },
        new() { OperationId = "enable-live-stream", Phase = "UPDATE", Needs = new[] { "streamId" }, Request = c => new Ctx { ["streamId"] = c["streamId"] } },
        new() { OperationId = "complete-live-stream", Phase = "UPDATE", Needs = new[] { "streamId" }, Request = c => new Ctx { ["streamId"] = c["streamId"] } },
        new() { OperationId = "cancel-upload", Phase = "UPDATE", Needs = new[] { "uploadId" }, Request = c => new Ctx { ["uploadId"] = c["uploadId"] } },

        // ---- DELETE (last) ----
        new() { OperationId = "delete-media-from-playlist", Phase = "DELETE", Needs = new[] { "playlistId", "mediaId" }, Request = c => new Ctx { ["playlistId"] = c["playlistId"], ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "delete-a-playlist", Phase = "DELETE", Needs = new[] { "playlistId" }, Request = c => new Ctx { ["playlistId"] = c["playlistId"] } },
        new() { OperationId = "delete-media-track", Phase = "DELETE", Needs = new[] { "mediaId", "trackId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"], ["trackId"] = c["trackId"] } },
        new() { OperationId = "delete-media-playback-id", Phase = "DELETE", Needs = new[] { "mediaId", "createdPlaybackId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"], ["playbackId"] = c["createdPlaybackId"] } },
        new() { OperationId = "delete-simulcast-of-stream", Phase = "DELETE", Needs = new[] { "streamId", "simulcastId" }, Request = c => new Ctx { ["streamId"] = c["streamId"], ["simulcastId"] = c["simulcastId"] } },
        new() { OperationId = "delete-playbackId-of-stream", Phase = "DELETE", Needs = new[] { "streamId", "streamPlaybackId" }, Request = c => new Ctx { ["streamId"] = c["streamId"], ["playbackId"] = c["streamPlaybackId"] } },
        new() { OperationId = "delete-live-stream", Phase = "DELETE", Needs = new[] { "streamId" }, Request = c => new Ctx { ["streamId"] = c["streamId"] } },
        new() { OperationId = "delete-media", Phase = "DELETE", Needs = new[] { "mediaId" }, Request = c => new Ctx { ["mediaId"] = c["mediaId"] } },
        new() { OperationId = "delete_signing_key", Phase = "DELETE", Needs = new[] { "signingKeyId" }, Request = c => new Ctx { ["signingKeyId"] = c["signingKeyId"] } },
    };

    public static async Task<int> RunAsync()
    {
        var testsDir = EndpointValidator.ResolveProjectDir();
        var specPath = EndpointValidator.ResolveSpecPath(testsDir);

        Console.WriteLine($"Spec: {specPath}");

        var spec = OpenApiSpec.Load(specPath);
        var endpoints = OpenApiSpec.ExtractNonGetEndpoints(spec);

        var baseUrl = Environment.GetEnvironmentVariable("FASTPIX_BASE_URL")
            ?? Environment.GetEnvironmentVariable("FASTPIX_SERVER_URL")
            ?? spec["servers"]?[0]?["url"]?.ToString()
            ?? "https://api.fastpix.com/v1/";

        var username = Environment.GetEnvironmentVariable("FASTPIX_USERNAME") ?? "";
        var password = Environment.GetEnvironmentVariable("FASTPIX_PASSWORD") ?? "";
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)
            || username == "your-access-token" || password == "your-secret-key")
        {
            Console.Error.WriteLine("Set FASTPIX_USERNAME and FASTPIX_PASSWORD env vars for BasicAuth (use real credentials).");
            return 2;
        }

        Console.Error.WriteLine("⚠️  This run CREATES, UPDATES and DELETES real resources in the workspace.");

        var sdk = new FastpixSDK(security: new Security { Username = username, Password = password }, serverUrl: baseUrl);
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(Fixtures.BasicAuthHeader(username, password));
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var ctx = new Ctx();
        var results = new List<StepResult>();

        for (int i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            endpoints.TryGetValue(step.OperationId, out var ep);
            var r = new StepResult
            {
                OperationId = step.OperationId,
                Method = ep?.Method ?? "?",
                Path = ep?.Path ?? "?",
                Phase = step.Phase,
            };
            Console.WriteLine($"[{i + 1}/{Steps.Count}] ({step.Phase}) {step.OperationId}");

            if (ep == null)
            {
                r.Status = "SKIP";
                r.Note = "operationId not found in spec";
                results.Add(r);
                continue;
            }

            var missingDeps = step.Needs.Where(k => string.IsNullOrEmpty(ctx.GetValueOrDefault(k))).ToList();
            if (missingDeps.Count > 0)
            {
                r.Status = "SKIP";
                r.Note = $"missing dependency: {string.Join(", ", missingDeps)}";
                Console.WriteLine($"  ⤳ SKIP ({r.Note})");
                results.Add(r);
                continue;
            }

            if (step.OperationId == "Generate-subtitle-track")
            {
                Console.Write($"  ⏳ waiting for track {ctx["trackId"]} to be ready... ");
                Console.WriteLine(await WaitForTrackReadyAsync(http, baseUrl, ctx["mediaId"]!, ctx["trackId"]!));
            }

            var request = step.Request(ctx);
            var res = await NonGetSdkInvoker.InvokeAsync(sdk, step.OperationId, request);

            if (step.RetryOn != null)
            {
                int attempt = 0;
                while (!res.Ok && attempt < 24 && ($"{res.ErrorMessage} {res.ErrorBodyJson}").Contains(step.RetryOn))
                {
                    attempt++;
                    if (attempt == 1) Console.Write("  ⏳ resource not ready, retrying");
                    else Console.Write(".");
                    await Task.Delay(5000);
                    res = await NonGetSdkInvoker.InvokeAsync(sdk, step.OperationId, request);
                }
                if (attempt > 0) Console.WriteLine();
            }

            if (!res.Ok)
            {
                r.Status = "FAIL";
                r.SdkOk = false;
                r.HttpStatus = res.StatusCode;
                r.SdkError = $"{res.ErrorName}: {res.ErrorMessage}";
                WriteArtifacts(testsDir, step.OperationId, res.ErrorBodyJson, JToken.FromObject(new { name = res.ErrorName, message = res.ErrorMessage }));
                Console.WriteLine($"  ❌ FAIL — {Truncate(r.SdkError, 120)}");
                results.Add(r);
                continue;
            }

            step.Capture?.Invoke(res.Value, ctx);

            if (step.OperationId == "create-media" && !string.IsNullOrEmpty(ctx.GetValueOrDefault("mediaId")))
            {
                Console.Write($"  ⏳ waiting for media {ctx["mediaId"]} to be Ready... ");
                Console.WriteLine(await WaitForMediaReadyAsync(http, baseUrl, ctx["mediaId"]!));
            }

            r.CapturedId = CapturedIdFor(step.OperationId, ctx);
            r.SdkOk = true;
            r.HttpStatus = res.StatusCode;

            // OpenAPI response-schema validation against the raw wire body.
            var validators = await OpenApiSpec.BuildValidatorsAsync(spec, ep);
            if (validators != null && res.StatusCode != null)
            {
                var schema = validators.TryGetValue(res.StatusCode.Value.ToString(), out var s) ? s
                    : validators.TryGetValue("default", out var d) ? d : null;
                if (schema != null)
                {
                    var findings = OpenApiSpec.Validate(schema, res.RawBody);
                    r.OpenapiValid = findings.Count == 0;
                    r.OpenapiErrors = findings;
                }
            }

            // Path diff: raw API body vs SDK value.
            var apiNorm = JsonDiff.NormalizeJsonForComparison(res.RawBody);
            var sdkNorm = res.Value is JObject or JArray ? JsonDiff.NormalizeJsonForComparison(res.Value) : null;
            var apiPaths = JsonDiff.CollectJsonPaths(apiNorm, includeEmptyArrays: false);
            var sdkPaths = sdkNorm != null ? JsonDiff.CollectJsonPaths(sdkNorm, includeEmptyArrays: false) : new HashSet<string>();
            r.MissingInSdk = sdkPaths.Count > 0 ? JsonDiff.SortUnique(apiPaths.Where(p => !sdkPaths.Contains(p))) : new();
            r.MissingInApi = sdkPaths.Count > 0 ? JsonDiff.SortUnique(sdkPaths.Where(p => !apiPaths.Contains(p))) : new();

            WriteArtifacts(testsDir, step.OperationId, res.RawBody, res.Value);

            r.Status = (r.OpenapiValid is null or true) && r.MissingInSdk.Count == 0 && r.MissingInApi.Count == 0 ? "PASS" : "FAIL";
            Console.WriteLine($"  {(r.Status == "PASS" ? "✅ PASS" : "❌ FAIL")} (HTTP {res.StatusCode}){(r.CapturedId != null ? $" id={r.CapturedId}" : "")}");
            results.Add(r);
        }

        WriteReport(testsDir, results, ctx);

        var pass = results.Count(r => r.Status == "PASS");
        var fail = results.Count(r => r.Status == "FAIL");
        var skip = results.Count(r => r.Status == "SKIP");
        Console.WriteLine($"Summary: total={results.Count} pass={pass} fail={fail} skip={skip}");
        return 0;
    }

    private static string? CapturedIdFor(string op, Ctx c) => op switch
    {
        "create_signing_key" => c.GetValueOrDefault("signingKeyId"),
        "create-a-playlist" => c.GetValueOrDefault("playlistId"),
        "create-new-stream" => c.GetValueOrDefault("streamId"),
        "create-media" => c.GetValueOrDefault("mediaId"),
        "create-media-playback-id" => c.GetValueOrDefault("createdPlaybackId"),
        "Add-media-track" => c.GetValueOrDefault("trackId"),
        "create-playbackId-of-stream" => c.GetValueOrDefault("streamPlaybackId"),
        "create-simulcast-of-stream" => c.GetValueOrDefault("simulcastId"),
        "direct-upload-video-media" => c.GetValueOrDefault("uploadId"),
        _ => null,
    };

    // ---- async resource polling ----

    private static async Task<string> WaitForMediaReadyAsync(HttpClient http, string baseUrl, string mediaId, int timeoutMs = 180000, int intervalMs = 5000)
    {
        var url = $"{baseUrl.TrimEnd('/')}/on-demand/{mediaId}";
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        var last = "unknown";
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var body = JToken.Parse(await http.GetStringAsync(url));
                last = Str(body["data"]?["status"]) ?? last;
                if (last == "Ready") return "Ready";
                if (last is "Errored" or "Failed") return last;
            }
            catch { /* transient */ }
            await Task.Delay(intervalMs);
        }
        return last;
    }

    private static async Task<string> WaitForTrackReadyAsync(HttpClient http, string baseUrl, string mediaId, string trackId, int timeoutMs = 180000, int intervalMs = 5000)
    {
        var url = $"{baseUrl.TrimEnd('/')}/on-demand/{mediaId}";
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        var last = "absent";
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var body = JToken.Parse(await http.GetStringAsync(url));
                var track = (body["data"]?["tracks"] as JArray)?.FirstOrDefault(t => Str(t["id"]) == trackId);
                if (track != null)
                {
                    last = Str(track["status"]) ?? "present";
                    if (last is "Ready" or "present") return last;
                }
            }
            catch { /* transient */ }
            await Task.Delay(intervalMs);
        }
        return last;
    }

    // ---- artifacts + report ----

    private static void WriteArtifacts(string testsDir, string operationId, JToken? rawBody, JToken? sdkValue)
    {
        var dir = Path.Combine(testsDir, ArtifactsDirName);
        Directory.CreateDirectory(dir);
        var slug = System.Text.RegularExpressions.Regex.Replace(operationId, "[^a-zA-Z0-9_.-]+", "_");
        File.WriteAllText(Path.Combine(dir, $"{slug}.raw.json"), (rawBody ?? JValue.CreateNull()).ToString(Formatting.Indented));
        File.WriteAllText(Path.Combine(dir, $"{slug}.sdk.json"), (sdkValue ?? JValue.CreateNull()).ToString(Formatting.Indented));
    }

    private static void WriteReport(string testsDir, List<StepResult> results, Ctx ctx)
    {
        var total = results.Count;
        var pass = results.Count(r => r.Status == "PASS");
        var fail = results.Count(r => r.Status == "FAIL");
        var skip = results.Count(r => r.Status == "SKIP");

        var lines = new List<string>
        {
            "# Non-GET endpoints validation report", "",
            $"Generated: {DateTime.UtcNow:o}", "",
            "## Summary", "",
            $"- **Total**: {total}",
            $"- **PASS**: {pass}",
            $"- **FAIL**: {fail}",
            $"- **SKIP**: {skip}", "",
            "## Captured resources", "",
        };
        foreach (var (k, v) in ctx) lines.Add($"- `{k}`: {v ?? "(not created)"}");
        lines.Add("");

        lines.Add("## Consolidated report");
        lines.Add("");
        lines.Add("| Phase | Method | OperationId | HTTP | OpenAPI valid | SDK | Missing in SDK | Missing in API | Status |");
        lines.Add("|---|---|---|---:|:--:|:--:|---|---|:--:|");
        foreach (var phase in new[] { "CREATE", "UPDATE", "DELETE" })
        {
            foreach (var r in results.Where(x => x.Phase == phase))
            {
                var ov = r.OpenapiValid == null ? "—" : r.OpenapiValid.Value ? "✅" : "❌";
                var sdk = r.Status == "SKIP" ? "—" : r.SdkOk ? "✅" : "❌";
                string Mis(List<string> a) => a.Count > 0 ? string.Join(", ", a.Select(p => $"`{p}`")) : "None";
                var st = r.Status == "PASS" ? "✅ PASS" : r.Status == "SKIP" ? "⤳ SKIP" : "❌ FAIL";
                lines.Add($"| {r.Phase} | {r.Method} | `{r.OperationId}` | {(r.HttpStatus?.ToString() ?? "—")} | {ov} | {sdk} | {Mis(r.MissingInSdk)} | {Mis(r.MissingInApi)} | {st} |");
            }
        }
        lines.Add("");

        lines.Add("## Per-operation details");
        lines.Add("");
        foreach (var r in results)
        {
            lines.Add($"### {r.OperationId} (`{r.Method} {r.Path}`)");
            lines.Add($"- **Phase**: {r.Phase}");
            lines.Add($"- **Status**: {r.Status}");
            if (r.HttpStatus != null) lines.Add($"- **HTTP status**: {r.HttpStatus}");
            if (r.CapturedId != null) lines.Add($"- **Captured id**: `{r.CapturedId}`");
            if (r.Note != null) lines.Add($"- **Note**: {r.Note}");
            if (r.SdkError != null) lines.Add($"- **SDK error**: {Truncate(r.SdkError, MaxPreviewChars)}");
            if (r.OpenapiErrors.Count > 0)
            {
                lines.Add("- **OpenAPI errors**:");
                foreach (var e in r.OpenapiErrors.Take(20)) lines.Add($"  - {(e.Path != null ? $"`{e.Path}` " : "")}{e.Message}".TrimEnd());
            }
            if (r.MissingInSdk.Count > 0) { lines.Add("- **Missing in SDK (present in API)**:"); foreach (var p in r.MissingInSdk) lines.Add($"  - `{p}`"); }
            if (r.MissingInApi.Count > 0) { lines.Add("- **Missing in API (present in SDK)**:"); foreach (var p in r.MissingInApi) lines.Add($"  - `{p}`"); }
            lines.Add("");
        }

        File.WriteAllText(Path.Combine(testsDir, ReportFile), string.Join("\n", lines));
        Console.WriteLine($"Report generated: {Path.Combine(testsDir, ReportFile)}");
    }

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}

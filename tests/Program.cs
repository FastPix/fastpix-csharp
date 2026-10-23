using Fastpix.Tests;

// Endpoint validation harness for the Fastpix C# SDK.
//
//   dotnet run --project tests              # GET endpoints (default, read-only)
//   dotnet run --project tests -- get       # GET endpoints
//   dotnet run --project tests -- non-get   # POST/PUT/PATCH/DELETE lifecycle (MUTATES data)
//   dotnet run --project tests -- all       # both
//   dotnet run --project tests -- cloud-playout  # Cloud Playout live contract run (MUTATES data; see README)
//   dotnet run --project tests -- self-check     # offline schema self-check for every mode, no credentials
//
// Every mode first runs the offline schema self-check for the schemas it uses and exits 3 if a
// response schema yields no validator or a wrong-typed body passes validation.
//
// Each mode:
//   1. calls the live API + validates the raw response against the OpenAPI schema,
//   2. calls the matching C# SDK method in-process,
//   3. diffs JSON paths between the raw API body and the SDK-parsed body,
//   4. writes artifacts + a markdown report under tests/.
//
// Requires real BasicAuth credentials:
//   export FASTPIX_USERNAME=... FASTPIX_PASSWORD=...
// Optional: FASTPIX_BASE_URL, FASTPIX_SPEC.

var mode = (args.FirstOrDefault() ?? "get").ToLowerInvariant();

string[] checks = mode switch
{
    "get" => ["get"],
    "non-get" or "nonget" or "mutate" => ["non-get"],
    "all" => ["get", "non-get"],
    "cloud-playout" => ["cloud-playout"],
    "self-check" => ["get", "non-get", "cloud-playout"],
    _ => [],
};
if (checks.Length > 0 && !await SchemaSelfCheckAsync(checks)) return 3;
if (mode == "self-check") return 0;

return mode switch
{
    "get" => await EndpointValidator.RunAsync(),
    "non-get" or "nonget" or "mutate" => await NonGetValidator.RunAsync(),
    "all" => Math.Max(await EndpointValidator.RunAsync(), await NonGetValidator.RunAsync()),
    "cloud-playout" => await CloudPlayoutLiveRun.RunAsync(),
    _ => Fail($"Unknown mode '{mode}'. Use: get | non-get | all | cloud-playout | self-check"),
};

static int Fail(string msg)
{
    Console.Error.WriteLine(msg);
    return 2;
}

static async Task<bool> SchemaSelfCheckAsync(string[] checks)
{
    var spec = OpenApiSpec.Load(EndpointValidator.ResolveSpecPath(EndpointValidator.ResolveProjectDir()));
    var gets = OpenApiSpec.ExtractGetEndpoints(spec);
    var nonGets = OpenApiSpec.ExtractNonGetEndpoints(spec);
    var ok = true;
    foreach (var check in checks)
    {
        var (endpoints, probeOp, probeStatus) = check switch
        {
            "get" => (gets, "get-playlist-by-id", "200"),
            "non-get" => (NonGetValidator.Steps.Select(s => s.OperationId).Distinct().Where(nonGets.ContainsKey).Select(id => nonGets[id]).ToList(), "create-a-playlist", "201"),
            _ => (gets.Concat(nonGets.Values).Where(e => e.Path.StartsWith("/cloud-playout/channels", StringComparison.Ordinal)).ToList(), "create-cloud-playout-channel", "201"),
        };
        var (validators, problems) = await OpenApiSpec.SelfCheckAsync(spec, endpoints, probeOp, probeStatus);
        Console.WriteLine($"Schema self-check ({check}): {validators} response validators over {endpoints.Count} operations, {problems.Count} problem(s)");
        foreach (var p in problems) Console.Error.WriteLine($"  self-check failed: {p}");
        ok &= problems.Count == 0;
    }
    return ok;
}

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Fastpix.Models.Components;
using Fastpix.Models.Requests;
using Fastpix.Utils;
using Fastpix.Utils.Retries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using File = System.IO.File;
using Input = Fastpix.Models.Components.Input;

namespace Fastpix.Tests;

/// <summary>One HTTP exchange as it went over the wire.</summary>
internal sealed class CallRecord
{
    public int Seq { get; init; }
    public required string Method { get; init; }
    public required string Path { get; init; } // spec-relative, no host
    public string Query { get; init; } = "";
    public string? RequestBody { get; init; }
    public int Status { get; set; }
    public List<string> ContentType { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public byte[] Body { get; set; } = Array.Empty<byte>();
    public bool Bom => Body.Length >= 3 && Body[0] == 0xEF && Body[1] == 0xBB && Body[2] == 0xBF;
    public string Text => Encoding.UTF8.GetString(Bom ? Body[3..] : Body);
    public string? Error { get; set; }
}

/// <summary>
/// Records every request/response, paces writes (bucket of 20, refill 1/s) and refuses any write
/// that names a channel the run did not create.
/// </summary>
internal sealed class RecordingHandler : DelegatingHandler
{
    public List<CallRecord> Calls { get; } = new();
    public HashSet<string> Protected { get; } = new();
    private readonly string _basePath;
    private double _tokens = 20;
    private DateTime _last = DateTime.UtcNow;

    public RecordingHandler(string basePath) : base(new HttpClientHandler())
    {
        _basePath = basePath;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var abs = request.RequestUri!.AbsolutePath;
        var path = abs.StartsWith(_basePath, StringComparison.Ordinal) ? abs[_basePath.Length..] : abs;
        var isWrite = request.Method != HttpMethod.Get;
        if (isWrite && Protected.Any(id => path.Contains(id, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Refused: write {request.Method} {path} touches a pre-existing channel");
        if (isWrite) await TakeTokenAsync(ct);

        var rec = new CallRecord
        {
            Seq = Calls.Count + 1,
            Method = request.Method.Method,
            Path = path,
            Query = request.RequestUri.Query.TrimStart('?'),
            RequestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(ct),
        };
        Calls.Add(rec);

        HttpResponseMessage resp;
        try
        {
            resp = await base.SendAsync(request, ct);
        }
        catch (Exception e)
        {
            rec.Error = e.GetType().Name + ": " + e.Message;
            throw;
        }

        var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
        var copy = new ByteArrayContent(bytes);
        foreach (var h in resp.Content.Headers.NonValidated)
        {
            copy.Headers.TryAddWithoutValidation(h.Key, h.Value);
            rec.Headers[h.Key] = string.Join(" | ", h.Value);
        }
        foreach (var h in resp.Headers.NonValidated) rec.Headers[h.Key] = string.Join(" | ", h.Value);
        rec.ContentType = resp.Content.Headers.NonValidated.TryGetValues("Content-Type", out var ctv) ? ctv.ToList() : new();
        rec.Status = (int)resp.StatusCode;
        rec.Body = bytes;
        resp.Content = copy;
        return resp;
    }

    private async Task TakeTokenAsync(CancellationToken ct)
    {
        while (true)
        {
            var now = DateTime.UtcNow;
            _tokens = Math.Min(20, _tokens + (now - _last).TotalSeconds);
            _last = now;
            if (_tokens >= 1) { _tokens -= 1; return; }
            await Task.Delay(TimeSpan.FromSeconds(1 - _tokens), ct);
        }
    }
}

/// <summary>The SDK's transport, routed through the recording handler.</summary>
internal sealed class RecordingClient : IFastpixHttpClient
{
    private static readonly FastpixHttpClient Cloner = new();
    private readonly HttpClient _http;

    public RecordingClient(HttpClient http) => _http = http;

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken? cancellationToken = null)
        => _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken ?? CancellationToken.None);

    public Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request) => Cloner.CloneAsync(request);
}

internal sealed class OpOutcome<T>
{
    public T? Res { get; init; }
    public CallRecord? Call { get; init; }
    public bool Ok { get; init; }
    public JToken? Json => CloudPlayoutLiveRun.ParseRaw(Call);
}

internal sealed class OpRow
{
    public required string OperationId { get; init; }
    public int Calls { get; set; }
    public List<string> Reasons { get; } = new();
    public bool Pass => Calls > 0 && Reasons.Count == 0;
}

/// <summary>
/// Live contract run of the 33 Cloud Playout operations (OpenSpec add-cloud-playout, tasks 9.1/9.3).
/// Every call goes through the SDK; the recording handler captures the wire exchange so each
/// operation is checked against the spec (request and response) and the SDK's parsed model is
/// compared with the raw body. Everything the run creates is deleted in finally blocks.
/// </summary>
internal static class CloudPlayoutLiveRun
{
    private const string SintelUrl = "https://media.w3.org/2010/05/sintel/trailer.mp4";
    private const string OverlayPngUrl = "https://upload.wikimedia.org/wikipedia/commons/4/47/PNG_transparency_demonstration_1.png";
    private static readonly Regex NaiveTime = new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$");
    private static readonly Regex TimestampLike = new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}");
    private static readonly Regex UrlRx = new(@"(rtmps?|https?|srt)://[^\s""'<>|]+");
    private static readonly Regex WsRx = new(@"""workspaceId""\s*:\s*""([^""]+)""");

    // Filled from the analysis of the runs: class for each operation that does not pass.
    private static readonly Dictionary<string, string> Classes = new();

    private static JObject _spec = null!;
    private static readonly Dictionary<string, (string Method, string Template, JObject Op)> Ops = new();
    private static readonly Dictionary<string, OpRow> Rows = new();
    private static readonly List<string> Notes = new();
    private static readonly HashSet<string> WorkspaceIds = new();
    private static RecordingHandler _rec = null!;
    private static HttpClient _http = null!;
    private static FastpixSDK _sdk = null!;
    private static string _baseUrl = "";
    private static string _host = "";
    private static string _auth = "";
    private static string _artifacts = "";
    private static readonly Dictionary<string, JsonSchema?> SchemaCache = new();

    public static async Task<int> RunAsync()
    {
        SelfCheck();
        var testsDir = EndpointValidator.ResolveProjectDir();
        var specPath = EndpointValidator.ResolveSpecPath(testsDir);
        _spec = OpenApiSpec.Load(specPath);
        LoadOps();
        await SchemaSelfCheckAsync();

        _baseUrl = Environment.GetEnvironmentVariable("FASTPIX_BASE_URL")
            ?? Environment.GetEnvironmentVariable("FASTPIX_SERVER_URL")
            ?? _spec["servers"]?[0]?["url"]?.ToString()
            ?? "https://api.fastpix.com/v1/";
        var username = Environment.GetEnvironmentVariable("FASTPIX_USERNAME") ?? "";
        var password = Environment.GetEnvironmentVariable("FASTPIX_PASSWORD") ?? "";
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)
            || username == "your-access-token" || password == "your-secret-key")
        {
            Console.Error.WriteLine("Set FASTPIX_USERNAME and FASTPIX_PASSWORD env vars for BasicAuth (use real credentials).");
            return 2;
        }

        var baseUri = new Uri(_baseUrl);
        _host = baseUri.Host;
        _auth = Fixtures.BasicAuthHeader(username, password);
        _artifacts = Path.Combine(testsDir, "artifacts", "cloud-playout");
        if (Directory.Exists(_artifacts)) Directory.Delete(_artifacts, true);
        Directory.CreateDirectory(_artifacts);

        _rec = new RecordingHandler(baseUri.AbsolutePath.TrimEnd('/'));
        _http = new HttpClient(_rec) { Timeout = TimeSpan.FromSeconds(60) };
        _sdk = new FastpixSDK(
            security: new Security { Username = username, Password = password },
            serverUrl: _baseUrl,
            client: new RecordingClient(_http),
            retryConfig: new RetryConfig(RetryConfig.RetryStrategy.NONE));

        Console.WriteLine($"Cloud Playout live contract run, environment: {EnvLabel}, spec: {Path.GetFileName(specPath)}");
        Console.Error.WriteLine("This run CREATES and DELETES real resources (media, playlist, live stream, filler, overlay, channels).");

        // Step 0: access check and the channels that exist before the run.
        var pre = await RawAsync("GET", "/cloud-playout/channels?type=all&limit=50");
        if (pre.Status != 200)
        {
            Console.Error.WriteLine($"Access check failed: GET /cloud-playout/channels?type=all -> {pre.Status}");
            return 2;
        }
        var preIds = (ParseRaw(pre)?["data"] as JArray ?? new JArray()).Select(c => c["id"]!.ToString()).ToList();
        var preCount = ParseRaw(pre)?["pagination"]?["totalRecords"]?.Value<int>() ?? preIds.Count;
        foreach (var id in preIds) _rec.Protected.Add(id);
        Console.WriteLine($"Step 0: 200, {preCount} pre-existing channel(s); writes on them are refused.");

        var fx = new Fixtures0();
        var created = new List<string>();
        try
        {
            await SetupFixturesAsync(fx);
            var loop = await SequenceAAsync(fx, created);
            // One channel at a time: B only starts once A's channel is gone (404).
            var aGone = created.Count == 0 && (loop == null || (await RawAsync("GET", $"/cloud-playout/channels/{loop}")).Status == 404);
            if (aGone) await SequenceBAsync(fx, created);
            else Notes.Add("B skipped: the loop channel from A still exists");
        }
        catch (Exception e)
        {
            Notes.Add($"Run aborted: {e.GetType().Name}: {Redact(e.Message)}");
            Console.Error.WriteLine($"Run aborted: {e.GetType().Name}: {Redact(e.Message)}");
        }
        finally
        {
            await SweepChannelsAsync(created, preIds);
            await TeardownFixturesAsync(fx);
        }

        var post = await RawAsync("GET", "/cloud-playout/channels?type=all&limit=50");
        var postCount = ParseRaw(post)?["pagination"]?["totalRecords"]?.Value<int>() ?? -1;
        var cleanup = new List<string> { $"Channels: pre-run {preCount}, post-run {postCount} ({(postCount == preCount ? "back to pre-run count" : "MISMATCH")})" };
        cleanup.AddRange(await VerifyGoneAsync(fx));

        return WriteReport(cleanup, preCount, postCount);
    }

    // ---------------------------------------------------------------- fixtures

    private sealed class Fixtures0
    {
        public string? MediaId, PlaylistId, StreamId, StreamKey, FillerId, OverlayId;
        public int MediaSeconds;
    }

    private static async Task SetupFixturesAsync(Fixtures0 fx)
    {
        Console.WriteLine("Phase 0: fixtures");
        await Fixture("create-media", async () =>
        {
            await _sdk.InputVideo.CreateMediaAsync(new CreateMediaRequest
            {
                Inputs = new() { Input.CreatePullVideoInput(new PullVideoInput { Url = SintelUrl }) },
                AccessPolicy = CreateMediaRequestAccessPolicy.Public,
                Metadata = new() { ["name"] = "sdk-live-media" },
            });
        }, j => fx.MediaId = j?["data"]?["id"]?.ToString());
        if (fx.MediaId == null) throw new InvalidOperationException("media create failed");

        var deadline = DateTime.UtcNow.AddMinutes(10);
        string? status = null;
        while (DateTime.UtcNow < deadline)
        {
            JToken? j = null;
            await Fixture("get-media", () => _sdk.ManageVideos.GetByIdAsync(fx.MediaId), x => j = x, quiet: true);
            status = j?["data"]?["status"]?.ToString();
            if (string.Equals(status, "Ready", StringComparison.OrdinalIgnoreCase))
            {
                fx.MediaSeconds = (int)Math.Floor(j?["data"]?["duration"]?.Value<double>() ?? 0);
                break;
            }
            if (string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase)) break;
            await Task.Delay(5000);
        }
        Console.WriteLine($"  media status {status}, {fx.MediaSeconds} s");
        if (!string.Equals(status, "Ready", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("media never became Ready");

        await Fixture("create-a-playlist", () => _sdk.Playlist.CreateAsync(CreatePlaylistRequest.CreateManual(new CreatePlaylistRequestManual
        {
            Name = "sdk-live-playlist",
            ReferenceId = "sdklive" + Guid.NewGuid().ToString("N")[..12],
            Type = CreatePlaylistRequestManualType.Manual,
        })), j => fx.PlaylistId = j?["data"]?["id"]?.ToString());
        if (fx.PlaylistId != null)
            await Fixture("add-media-to-playlist", () => _sdk.Playlists.AddMediaAsync(fx.PlaylistId, new MediaIdsRequest { MediaIds = new() { fx.MediaId } }), _ => { });

        await Fixture("create-new-stream", () => _sdk.LiveStreams.CreateAsync(new CreateLiveStreamRequest
        {
            PlaybackSettings = new PlaybackSettings(),
            InputMediaSettings = new InputMediaSettings { EnableRecording = false, Metadata = new() { ["livestream_name"] = "sdk-live-target" } },
        }), j => { fx.StreamId = j?["data"]?["streamId"]?.ToString(); fx.StreamKey = j?["data"]?["streamKey"]?.ToString(); });

        var filler = await RawAsync("POST", "/cloud-playout/fillers", new JObject { ["name"] = "sdk-live-bumper", ["type"] = "bumper", ["mediaId"] = fx.MediaId });
        fx.FillerId = filler.Status == 201 ? ParseRaw(filler)?["data"]?["id"]?.ToString() : null;
        Console.WriteLine($"  POST /cloud-playout/fillers -> {filler.Status}");

        var overlay = await RawAsync("POST", "/cloud-playout/overlays", new JObject
        {
            ["name"] = "sdk-live-logo", ["type"] = "static", ["url"] = OverlayPngUrl, ["channelType"] = "loop", ["overlayLayoutType"] = "logo",
        });
        fx.OverlayId = overlay.Status == 201 ? ParseRaw(overlay)?["data"]?["id"]?.ToString() : null;
        Console.WriteLine($"  POST /cloud-playout/overlays -> {overlay.Status}");
    }

    private static async Task TeardownFixturesAsync(Fixtures0 fx)
    {
        Console.WriteLine("Teardown: fixtures (reverse order)");
        if (fx.OverlayId != null) await RawDeleteAsync($"/cloud-playout/overlays/{fx.OverlayId}");
        if (fx.FillerId != null) await RawDeleteAsync($"/cloud-playout/fillers/{fx.FillerId}");
        if (fx.StreamId != null) await Fixture("delete-live-stream", () => _sdk.LiveStreams.DeleteAsync(fx.StreamId), _ => { }, tolerate404: true);
        if (fx.PlaylistId != null) await Fixture("delete-a-playlist", () => _sdk.Playlists.DeleteAsync(fx.PlaylistId), _ => { }, tolerate404: true);
        if (fx.MediaId != null) await Fixture("delete-media", () => _sdk.ManageVideos.DeleteMediaAsync(fx.MediaId), _ => { }, tolerate404: true);
    }

    private static async Task<List<string>> VerifyGoneAsync(Fixtures0 fx)
    {
        var lines = new List<string>();
        async Task Check(string what, string? id, string path)
        {
            if (id == null) { lines.Add($"{what}: not created"); return; }
            var r = await RawAsync("GET", path);
            lines.Add($"{what}: GET -> {r.Status} ({(r.Status == 404 ? "deleted" : "STILL PRESENT?")})");
        }
        await Check("media", fx.MediaId, $"/on-demand/{fx.MediaId}");
        await Check("playlist", fx.PlaylistId, $"/on-demand/playlists/{fx.PlaylistId}");
        await Check("live stream", fx.StreamId, $"/live/streams/{fx.StreamId}");
        await Check("filler", fx.FillerId, $"/cloud-playout/fillers/{fx.FillerId}");
        await Check("overlay", fx.OverlayId, $"/cloud-playout/overlays/{fx.OverlayId}");
        return lines;
    }

    /// <summary>A fixture call through the SDK (not part of the 33); 404 tolerated on deletes.</summary>
    private static async Task Fixture(string label, Func<Task> call, Action<JToken?> capture, bool quiet = false, bool tolerate404 = false)
    {
        var mark = _rec.Calls.Count;
        try { await call(); }
        catch (Exception e) when (tolerate404 || e is Fastpix.Models.Errors.FastpixException)
        {
            // status is read from the recording below
        }
        var c = _rec.Calls.Skip(mark).LastOrDefault();
        if (!quiet || c?.Status >= 400) Console.WriteLine($"  {label} {c?.Method} {c?.Path} -> {c?.Status}");
        capture(ParseRaw(c));
        if (c != null) WriteArtifact(c, label, null, new List<string>());
    }

    // ---------------------------------------------------------------- sequence A (loop)

    private static async Task<string?> SequenceAAsync(Fixtures0 fx, List<string> created)
    {
        Console.WriteLine("Sequence A: loop channel");
        var start = NextMinute(DateTime.UtcNow.AddMinutes(5));
        var end = start.AddMinutes(60); // keep production windows short
        var create = await Op("create-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.CreateAsync(new CloudPlayoutChannelRequest
        {
            Name = "sdk-live-loop", Type = CloudPlayoutChannelRequestType.Loop, StartTime = F(start), EndTime = F(end), LiveSupport = false,
        }));
        var ch = create.Json?["data"]?["id"]?.ToString();
        if (ch == null) { Notes.Add("A: channel create failed, sequence A skipped"); return null; }
        created.Add(ch);
        // RTMPS ingest host: the channel's ingestionPoint, or (loop without live support) its destinationUrl.
        var ingest = Str(create.Json?["data"]?["ingestionPoint"]) ?? Str(create.Json?["data"]?["destinationUrl"]);
        string? programId = null;
        var activated = false;
        try
        {
            await Op("get-cloud-playout-channel-by-id", () => _sdk.CloudPlayoutChannels.GetByIdAsync(ch));
            await Op("list-cloud-playout-channels", () => _sdk.CloudPlayoutChannels.ListAsync(ListCloudPlayoutChannelsType.Loop));
            await Op("list-cloud-playout-channels", () => _sdk.CloudPlayoutChannels.ListAsync(ListCloudPlayoutChannelsType.All));
            await Op("update-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.UpdateAsync(ch, new CloudPlayoutChannelUpdateRequest { Name = "sdk-live-loop-renamed" }));

            var prog = await Op("create-cloud-playout-program", () => _sdk.CloudPlayoutLoopPrograms.CreateAsync(ch, new CloudPlayoutProgramRequest { Name = "sdk-live-prog" }));
            programId = prog.Json?["data"]?["id"]?.ToString();
            if (programId != null)
            {
                var pid = programId;
                await Op("add-media-to-cloud-playout-program", () => _sdk.CloudPlayoutLoopPrograms.AddMediaAsync(ch, pid, new CloudPlayoutProgramMediaRequest { MediaIds = new() { fx.MediaId! } }));
                if (fx.PlaylistId != null)
                    await Op("add-playlist-to-cloud-playout-program", () => _sdk.CloudPlayoutLoopPrograms.AddPlaylistAsync(ch, pid, new CloudPlayoutProgramPlaylistRequest { PlaylistId = fx.PlaylistId }));
                else Missing("add-playlist-to-cloud-playout-program", "playlist fixture not created");

                var details = await Op("get-cloud-playout-program-media-details", () => _sdk.CloudPlayoutLoopPrograms.GetMediaDetailsAsync(ch, pid));
                var items = (details.Json?["data"] as JArray ?? new JArray()).OfType<JObject>().ToList();
                var vodIds = items.Where(i => i["type"]?.ToString() != "bumper").Select(i => i["id"]!.ToString()).ToList();

                string? bumperItem = null;
                if (fx.FillerId != null && vodIds.Count > 0)
                {
                    await Op("add-bumper-to-cloud-playout-program-item", () => _sdk.CloudPlayoutLoopPrograms.AddBumperAsync(ch, pid, new CloudPlayoutProgramBumperRequest
                    {
                        BumperId = fx.FillerId, EventId = vodIds[0], BumperType = CloudPlayoutProgramBumperRequestBumperType.Preroll,
                    }));
                    var after = await Op("get-cloud-playout-program-media-details", () => _sdk.CloudPlayoutLoopPrograms.GetMediaDetailsAsync(ch, pid));
                    bumperItem = (after.Json?["data"] as JArray ?? new JArray()).OfType<JObject>().FirstOrDefault(i => i["type"]?.ToString() == "bumper")?["id"]?.ToString();
                }
                else Missing("add-bumper-to-cloud-playout-program-item", fx.FillerId == null ? "bumper filler could not be created" : "no program item");

                // Delete one item (the bumper, else the playlist item), then the playlist item in a batch; the first vod item stays so the channel has content.
                var single = bumperItem ?? (vodIds.Count > 2 ? vodIds[2] : null);
                if (single != null) await Op("delete-cloud-playout-program-item", () => _sdk.CloudPlayoutLoopPrograms.DeleteItemAsync(ch, pid, single));
                else Missing("delete-cloud-playout-program-item", "no spare program item");
                if (vodIds.Count > 1)
                    await Op("delete-cloud-playout-program-items", () => _sdk.CloudPlayoutLoopPrograms.DeleteItemsAsync(ch, pid, new CloudPlayoutProgramItemDeleteRequest { EventIds = new() { vodIds[1] } }));
                else Missing("delete-cloud-playout-program-items", "fewer than two program items");

                await Op("get-cloud-playout-program-by-id", () => _sdk.CloudPlayoutLoopPrograms.GetByIdAsync(ch, pid));
                await Op("update-cloud-playout-program", () => _sdk.CloudPlayoutLoopPrograms.UpdateAsync(ch, pid, new CloudPlayoutProgramRequest { Name = "sdk-live-prog2" }));
                await Op("list-cloud-playout-programs", () => _sdk.CloudPlayoutLoopPrograms.ListAsync(ch));
            }

            if (fx.OverlayId != null)
            {
                var add = await Op("add-overlay-to-cloud-playout-channel", () => _sdk.CloudPlayoutChannelOverlays.AddAsync(ch, new CloudPlayoutChannelOverlayRequest
                {
                    OverlayId = fx.OverlayId, StartTime = F(start.AddMinutes(1)), Duration = 600,
                }));
                var placement = add.Json?["data"]?["id"]?.ToString();
                await Op("get-all-cloud-playout-channel-overlays", () => _sdk.CloudPlayoutChannelOverlays.ListAsync(ch));
                if (placement != null)
                {
                    await Op("update-cloud-playout-channel-overlay", () => _sdk.CloudPlayoutChannelOverlays.UpdateAsync(ch, placement, new CloudPlayoutChannelOverlayUpdateRequest
                    {
                        Opacity = 80, Repeat = new CloudPlayoutOverlayRepeat { EverySeconds = 120, ShowForSeconds = 30 },
                    }));
                    await Op("update-cloud-playout-channel-overlay", () => _sdk.CloudPlayoutChannelOverlays.UpdateAsync(ch, placement, new CloudPlayoutChannelOverlayUpdateRequest { RemoveRepeat = true }));
                    await Op("delete-cloud-playout-channel-overlay", () => _sdk.CloudPlayoutChannelOverlays.DeleteAsync(ch, placement));
                }
            }
            else
            {
                foreach (var o in new[] { "add-overlay-to-cloud-playout-channel", "update-cloud-playout-channel-overlay", "delete-cloud-playout-channel-overlay" })
                    Missing(o, "overlay could not be created");
                await Op("get-all-cloud-playout-channel-overlays", () => _sdk.CloudPlayoutChannelOverlays.ListAsync(ch));
            }

            await Op("get-cloud-playout-channel-schedule", () => _sdk.CloudPlayoutScheduleAndEpg.GetScheduleAsync(ch, GetCloudPlayoutChannelScheduleType.Loop, F(start), F(start.AddHours(1))));
            await Op("export-cloud-playout-channel-schedule", () => _sdk.CloudPlayoutScheduleAndEpg.ExportScheduleAsync(ch, ExportCloudPlayoutChannelScheduleType.Loop, F(start), F(start.AddHours(1))));
            await Op("get-cloud-playout-channel-epg", () => _sdk.CloudPlayoutScheduleAndEpg.GetEpgAsync(ch, GetCloudPlayoutChannelEpgType.Loop, F(start), F(start.AddHours(1))));

            var target = SimulcastTarget(ingest, fx.StreamKey);
            if (target != null)
            {
                var sc = await Op("add-cloud-playout-channel-simulcast", () => _sdk.CloudPlayoutSimulcast.CreateAsync(ch, new CloudPlayoutSimulcastRequest { Url = target }));
                var simulcastId = sc.Json?["data"]?["simulcastId"]?.ToString();
                var list = await Op("get-cloud-playout-channel-simulcasts", () => _sdk.CloudPlayoutSimulcast.ListAsync(ch));
                var dest = list.Json?["data"]?["destinationLiveId"]?.ToString();
                if (simulcastId != null && dest != null)
                    await Op("delete-cloud-playout-channel-simulcast", () => _sdk.CloudPlayoutSimulcast.DeleteAsync(dest, simulcastId));
                else Missing("delete-cloud-playout-channel-simulcast", "no simulcastId/destinationLiveId");
            }
            else
            {
                Missing("add-cloud-playout-channel-simulcast", "no RTMP target (no ingestionPoint/destinationUrl, or no streamKey)");
                await Op("get-cloud-playout-channel-simulcasts", () => _sdk.CloudPlayoutSimulcast.ListAsync(ch));
                Missing("delete-cloud-playout-channel-simulcast", "no simulcast added");
            }

            start = await EnsureLeadAsync(ch, start, end - start);
            var act = await Op("activate-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.ActivateAsync(ch));
            activated = act.Call?.Status == 200;
            if (activated)
            {
                var status = await PollStatusAsync(ch, new[] { "running" }, TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(15));
                Notes.Add($"A: after activate, status reached '{status}' (waited for running, cap 10 min)");
                var de = await Op("deactivate-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.DeactivateAsync(ch));
                if (de.Call?.Status == 200) activated = false;
                var idle = await PollStatusAsync(ch, new[] { "idle" }, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5));
                Notes.Add($"A: after deactivate, status '{idle}'");
            }
            if (programId != null)
            {
                var pid = programId;
                var del = await Op("delete-cloud-playout-program", () => _sdk.CloudPlayoutLoopPrograms.DeleteAsync(ch, pid));
                if (del.Call?.Status == 200) programId = null;
            }
            var dc = await Op("delete-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.DeleteAsync(ch));
            if (dc.Call?.Status == 200) created.Remove(ch);
        }
        finally
        {
            if (created.Contains(ch)) await CleanupChannelAsync(ch, activated, programId);
            if (!created.Contains(ch)) Console.WriteLine("  A: channel deleted");
        }
        return ch;
    }

    // ---------------------------------------------------------------- sequence B (schedule)

    private static async Task SequenceBAsync(Fixtures0 fx, List<string> created)
    {
        Console.WriteLine("Sequence B: schedule channel");
        var start = NextMinute(DateTime.UtcNow.AddMinutes(5));
        var end = start.AddMinutes(60); // keep production windows short
        var create = await Op("create-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.CreateAsync(new CloudPlayoutChannelRequest
        {
            Name = "sdk-live-sched", Type = CloudPlayoutChannelRequestType.Schedule, StartTime = F(start), EndTime = F(end),
        }));
        var ch = create.Json?["data"]?["id"]?.ToString();
        if (ch == null) { Notes.Add("B: channel create failed, sequence B skipped"); return; }
        created.Add(ch);
        var active = false;
        try
        {
            var media = fx.MediaId!;
            var one = await Op("add-event-to-cloud-playout-schedule-channel", () => _sdk.CloudPlayoutScheduleChannel.AddEventAsync(ch, new CloudPlayoutScheduleEventRequest
            {
                Event = new CloudPlayoutScheduleEventCreate { Type = CloudPlayoutScheduleEventCreateType.Vod, ReferenceId = media, StartTime = F(start.AddMinutes(1)) },
            }));
            var eventId = one.Json?["data"]?[0]?["id"]?.ToString();

            await Op("add-events-to-cloud-playout-schedule-channel", () => _sdk.CloudPlayoutScheduleChannel.AddEventsAsync(ch, new CloudPlayoutScheduleEventBatchRequest
            {
                Events = new()
                {
                    new CloudPlayoutScheduleEventBatchItem { Type = CloudPlayoutScheduleEventBatchItemType.Vod, ReferenceId = media, StartTime = F(start.AddMinutes(5)) },
                    new CloudPlayoutScheduleEventBatchItem { Type = CloudPlayoutScheduleEventBatchItemType.Live, Name = "sdk-live-slot", StartTime = F(start.AddMinutes(10)), EndTime = F(start.AddMinutes(16)) },
                },
            }));
            await Op("get-cloud-playout-schedule-channel-events", () => _sdk.CloudPlayoutScheduleChannel.ListEventsAsync(ch, startTime: F(start)));

            if (eventId != null)
            {
                var s2 = start.AddMinutes(2);
                await Op("update-cloud-playout-schedule-channel-event", () => _sdk.CloudPlayoutScheduleChannel.UpdateEventAsync(eventId, new CloudPlayoutScheduleEventUpdateRequest
                {
                    Event = new CloudPlayoutScheduleEventUpdate { StartTime = F(s2), EndTime = F(s2.AddSeconds(fx.MediaSeconds)), ReferenceId = media },
                }));
                await Op("delete-cloud-playout-schedule-channel-event", () => _sdk.CloudPlayoutScheduleChannel.DeleteEventAsync(eventId));
            }
            else
            {
                Missing("update-cloud-playout-schedule-channel-event", "single event add returned no id");
                Missing("delete-cloud-playout-schedule-channel-event", "single event add returned no id");
            }

            await Op("get-cloud-playout-channel-schedule", () => _sdk.CloudPlayoutScheduleAndEpg.GetScheduleAsync(ch, GetCloudPlayoutChannelScheduleType.Schedule, F(start), F(start.AddHours(1))));
            await Op("export-cloud-playout-channel-schedule", () => _sdk.CloudPlayoutScheduleAndEpg.ExportScheduleAsync(ch, ExportCloudPlayoutChannelScheduleType.Schedule, F(start), F(start.AddHours(1))));
            await Op("get-cloud-playout-channel-epg", () => _sdk.CloudPlayoutScheduleAndEpg.GetEpgAsync(ch, GetCloudPlayoutChannelEpgType.Schedule, F(start), F(start.AddHours(1))));

            start = await EnsureLeadAsync(ch, start, end - start);
            var a1 = await Op("activate-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.ActivateAsync(ch));
            active = a1.Call?.Status == 200;
            Notes.Add($"B: first activate -> {a1.Call?.Status}, status now '{await StatusAsync(ch)}'");
            if (active)
            {
                var d1 = await Op("deactivate-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.DeactivateAsync(ch));
                if (d1.Call?.Status == 200) active = false;
                Notes.Add($"B: first deactivate -> {d1.Call?.Status}, status now '{await PollStatusAsync(ch, new[] { "idle" }, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5))}'");

                start = await EnsureLeadAsync(ch, start, end - start);
                var a2 = await Op("activate-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.ActivateAsync(ch), rowKey: "CP-01 re-activate (activate-cloud-playout-channel)");
                var lead = (start - DateTime.UtcNow).TotalSeconds;
                Notes.Add($"CP-01: re-activate after deactivate -> {a2.Call?.Status} {Redact(Trunc(a2.Call?.Text ?? "", 300))} (start was {lead:F0} s ahead)");
                if (a2.Call?.Status == 200)
                {
                    active = true;
                    var d2 = await Op("deactivate-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.DeactivateAsync(ch));
                    if (d2.Call?.Status == 200) active = false;
                }
            }
            var dc = await Op("delete-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.DeleteAsync(ch));
            if (dc.Call?.Status == 200) created.Remove(ch);
        }
        finally
        {
            if (created.Contains(ch)) await CleanupChannelAsync(ch, active, null);
            if (!created.Contains(ch)) Console.WriteLine("  B: channel deleted");
        }
    }

    // ---------------------------------------------------------------- channel helpers

    /// <summary>Activation needs the start at least 3 minutes ahead: move the window if it is closer than 3.5 minutes.</summary>
    private static async Task<DateTime> EnsureLeadAsync(string ch, DateTime start, TimeSpan length)
    {
        if (start - DateTime.UtcNow >= TimeSpan.FromSeconds(210)) return start;
        var ns = NextMinute(DateTime.UtcNow.AddMinutes(4));
        await Op("update-cloud-playout-channel", () => _sdk.CloudPlayoutChannels.UpdateAsync(ch, new CloudPlayoutChannelUpdateRequest { StartTime = F(ns), EndTime = F(ns + length) }));
        Notes.Add($"Channel window moved to start {F(ns)} so the start stays >= 3 min ahead at activation");
        return ns;
    }

    private static async Task<string?> StatusAsync(string ch)
    {
        var g = await Op("get-cloud-playout-channel-by-id", () => _sdk.CloudPlayoutChannels.GetByIdAsync(ch));
        return g.Json?["data"]?["status"]?.ToString();
    }

    private static async Task<string?> PollStatusAsync(string ch, string[] want, TimeSpan cap, TimeSpan every)
    {
        var deadline = DateTime.UtcNow + cap;
        string? s;
        while (true)
        {
            s = await StatusAsync(ch);
            Console.WriteLine($"    status {s}");
            if (s != null && want.Contains(s)) return s;
            if (DateTime.UtcNow + every > deadline) return s;
            await Task.Delay(every);
        }
    }

    /// <summary>Tolerant cleanup of a channel the run created (deactivate, program, channel; 404/409 fine).</summary>
    private static async Task CleanupChannelAsync(string ch, bool maybeActive, string? programId)
    {
        Console.WriteLine($"  cleanup channel {ch}");
        if (_rec.Protected.Contains(ch)) throw new InvalidOperationException("cleanup refused on a pre-existing channel");
        if (maybeActive) await Fixture("cleanup deactivate", () => _sdk.CloudPlayoutChannels.DeactivateAsync(ch), _ => { }, tolerate404: true);
        if (programId != null) await Fixture("cleanup delete program", () => _sdk.CloudPlayoutLoopPrograms.DeleteAsync(ch, programId), _ => { }, tolerate404: true);
        await Fixture("cleanup delete channel", () => _sdk.CloudPlayoutChannels.DeleteAsync(ch), _ => { }, tolerate404: true);
        var last = _rec.Calls.Last();
        if (last.Status == 409)
        {
            await Fixture("cleanup deactivate", () => _sdk.CloudPlayoutChannels.DeactivateAsync(ch), _ => { }, tolerate404: true);
            await Fixture("cleanup delete channel", () => _sdk.CloudPlayoutChannels.DeleteAsync(ch), _ => { }, tolerate404: true);
        }
    }

    /// <summary>Last safety net: delete any channel named sdk-live-* that did not exist before the run.</summary>
    private static async Task SweepChannelsAsync(List<string> created, List<string> preIds)
    {
        var r = await RawAsync("GET", "/cloud-playout/channels?type=all&limit=50");
        var stray = (ParseRaw(r)?["data"] as JArray ?? new JArray())
            .Where(c => !preIds.Contains(c["id"]!.ToString()) && (created.Contains(c["id"]!.ToString()) || (c["name"]?.ToString() ?? "").StartsWith("sdk-live-", StringComparison.Ordinal)))
            .Select(c => c["id"]!.ToString()).ToList();
        foreach (var id in stray) await CleanupChannelAsync(id, true, null);
    }

    // A label for logs and the report: FASTPIX_ENV when set, else "default".
    private static string EnvLabel => Environment.GetEnvironmentVariable("FASTPIX_ENV") is { Length: > 0 } e ? e : "default";

    private static string? SimulcastTarget(string? ingestionPoint, string? streamKey)
    {
        if (streamKey == null || ingestionPoint == null || !Uri.TryCreate(ingestionPoint, UriKind.Absolute, out var u)) return null;
        // The live stream model has no ingest URL (only streamKey); use the RTMPS ingest host the channel itself reports.
        return $"{u.Scheme}://{u.Authority}/live/{streamKey}";
    }

    // ---------------------------------------------------------------- per-operation checks

    private static async Task<OpOutcome<T>> Op<T>(string opId, Func<Task<T>> call, string? rowKey = null) where T : class
    {
        var key = rowKey ?? opId;
        if (!Rows.TryGetValue(key, out var row)) Rows[key] = row = new OpRow { OperationId = key };
        row.Calls++;

        var mark = _rec.Calls.Count;
        T? res = null;
        Exception? ex = null;
        try { res = await call(); }
        catch (Exception e) { ex = e; }
        var calls = _rec.Calls.Skip(mark).ToList();
        var c = calls.LastOrDefault();
        var reasons = new List<string>();
        if (calls.Count != 1) reasons.Add($"expected 1 HTTP call, saw {calls.Count}");

        JToken? parsed = null;
        if (c != null)
        {
            reasons.AddRange(await CheckRequestAsync(opId, c));
            reasons.AddRange(await CheckResponseAsync(opId, c));
        }
        if (ex != null) reasons.Add($"SDK threw {ex.GetType().Name}: {Trunc(ex.Message, 160)}");
        else if (c != null) reasons.AddRange(CheckParsed(opId, c, res!, out parsed));

        var red = reasons.Select(r => Redact(r)).ToList();
        foreach (var r in red) if (!row.Reasons.Contains(r)) row.Reasons.Add(r);
        Console.WriteLine($"  {(red.Count == 0 ? "PASS" : "FAIL")} {opId} {c?.Method} {c?.Path}{(string.IsNullOrEmpty(c?.Query) ? "" : "?" + c!.Query)} -> {c?.Status}");
        foreach (var r in red) Console.WriteLine($"       - {r}");
        if (c != null) WriteArtifact(c, opId, parsed, red);
        return new OpOutcome<T> { Res = res, Call = c, Ok = red.Count == 0 };
    }

    private static void Missing(string opId, string why)
    {
        if (!Rows.TryGetValue(opId, out var row)) Rows[opId] = row = new OpRow { OperationId = opId };
        row.Reasons.Add($"not run: {why} (c)");
        Console.WriteLine($"  SKIP {opId}: {why}");
    }

    private static async Task<List<string>> CheckRequestAsync(string opId, CallRecord c)
    {
        var r = new List<string>();
        var (method, template, op) = Ops[opId];
        if (!string.Equals(method, c.Method, StringComparison.OrdinalIgnoreCase)) r.Add($"verb {c.Method}, spec {method}");
        var rx = new Regex("^" + Regex.Replace(Regex.Escape(template), @"\\\{[^}]+}", "[^/]+") + "$");
        if (!rx.IsMatch(c.Path)) r.Add($"path {c.Path} does not match {template}");

        var q = System.Web.HttpUtility.ParseQueryString(c.Query);
        var qparams = Params(op, template).Where(p => p["in"]?.ToString() == "query").ToDictionary(p => p["name"]!.ToString());
        foreach (var k in q.AllKeys.Where(k => k != null))
        {
            if (!qparams.TryGetValue(k!, out var p)) { r.Add($"query '{k}' not in spec"); continue; }
            var enumVals = Resolve(p["schema"])?["enum"] as JArray;
            if (enumVals != null && !enumVals.Any(e => e.ToString() == q[k])) r.Add($"query {k}={q[k]} not in enum");
            if ((k == "startTime" || k == "endTime") && !NaiveTime.IsMatch(q[k] ?? "")) r.Add($"query {k}={q[k]} is not yyyy-MM-ddTHH:mm:ss (no Z/offset)");
        }
        foreach (var p in qparams.Values.Where(p => p["required"]?.Value<bool>() == true))
            if (q[p["name"]!.ToString()] == null) r.Add($"required query '{p["name"]}' missing");

        var bodySchema = op["requestBody"]?["content"]?["application/json"]?["schema"];
        if (bodySchema == null)
        {
            if (!string.IsNullOrEmpty(c.RequestBody)) r.Add("request has a body, spec has no requestBody");
        }
        else if (string.IsNullOrEmpty(c.RequestBody)) r.Add("request body missing");
        else
        {
            var body = ParseNoDates(c.RequestBody);
            var schema = await SchemaFor($"req:{opId}", bodySchema);
            if (schema != null)
                foreach (var f in OpenApiSpec.Validate(schema, body)) r.Add($"request body: {f.Path} {f.Message}");
            foreach (var t in body!.SelectTokens("$..*").OfType<JValue>())
            {
                var name = (t.Parent as JProperty)?.Name;
                if ((name == "startTime" || name == "endTime") && !NaiveTime.IsMatch(t.ToString()))
                    r.Add($"request {t.Path}={t} is not yyyy-MM-ddTHH:mm:ss (no Z/offset)");
            }
        }
        return r;
    }

    private static async Task<List<string>> CheckResponseAsync(string opId, CallRecord c)
    {
        var r = new List<string>();
        var responses = (JObject)Ops[opId].Op["responses"]!;
        var success = responses.Properties().First(p => p.Name.StartsWith('2'));
        var expected = int.Parse(success.Name);
        if (c.Status != expected)
        {
            r.Add($"status {c.Status}, spec {expected}: {Trunc(c.Text, 200)}");
            return r;
        }
        var specCt = ((JObject)success.Value["content"]!).Properties().First();
        var specMedia = MediaTypeHeaderValue.Parse(specCt.Name);
        if (c.ContentType.Count != 1 || !MediaTypeHeaderValue.TryParse(c.ContentType[0], out var got))
            r.Add($"Content-Type '{string.Join(" | ", c.ContentType)}' is not one media type (spec '{specCt.Name}')");
        else if (!string.Equals(got.MediaType, specMedia.MediaType, StringComparison.OrdinalIgnoreCase))
            r.Add($"Content-Type '{c.ContentType[0]}', spec '{specCt.Name}'");

        if (specMedia.MediaType == "application/json")
        {
            var schema = await SchemaFor($"res:{opId}", specCt.Value["schema"]);
            JToken? body = null;
            try { body = ParseNoDates(c.Text); } catch (Exception e) { r.Add($"response not JSON: {e.Message}"); }
            if (schema != null && body != null)
                foreach (var f in OpenApiSpec.Validate(schema, body)) r.Add($"response schema: {f.Path} {f.Message}");
        }
        return r;
    }

    private static List<string> CheckParsed(string opId, CallRecord c, object res, out JToken? parsed)
    {
        parsed = null;
        var r = new List<string>();
        var props = res.GetType().GetProperties();
        var meta = props.First(p => p.Name == "HttpMeta").GetValue(res) as HttpMetadata;
        if (meta?.Response == null) r.Add("HttpMeta.Response missing");

        var csvOrXml = props.FirstOrDefault(p => p.Name is "Csv" or "Xml");
        if (csvOrXml != null)
        {
            var text = csvOrXml.GetValue(res) as string;
            parsed = text;
            if (text == null) r.Add($"{csvOrXml.Name} is null");
            else if (text != c.Text) r.Add($"{csvOrXml.Name} differs from the raw body (length {text.Length} vs {c.Text.Length})");
            Notes.Add($"{opId}: raw body {(c.Bom ? "starts with a UTF-8 BOM" : "has no BOM")}; Content-Type '{string.Join(" | ", c.ContentType)}'");
            if (opId.StartsWith("export", StringComparison.Ordinal))
            {
                var cd = meta?.Response?.Content.Headers.NonValidated.TryGetValues("Content-Disposition", out var v) == true ? string.Join("", v) : null;
                if (string.IsNullOrEmpty(cd)) r.Add("Content-Disposition missing on HttpMeta.Response");
            }
            else
            {
                var tc = meta?.Response?.Headers.NonValidated.TryGetValues("X-Total-Count", out var v) == true ? string.Join("", v) : null;
                if (string.IsNullOrEmpty(tc)) r.Add("X-Total-Count missing on HttpMeta.Response");
            }
            return r;
        }

        var payloadProp = props.Where(p => p.Name is not ("HttpMeta" or "DefaultError")).FirstOrDefault(p => p.GetValue(res) != null);
        if (payloadProp == null) { r.Add("SDK returned no parsed payload"); return r; }
        var json = JsonConvert.SerializeObject(payloadProp.GetValue(res), Utilities.GetDefaultJsonSerializerSettings());
        parsed = ParseNoDates(json);
        var raw = JsonDiff.NormalizeJsonForComparison(ParseNoDates(c.Text));
        var model = JsonDiff.NormalizeJsonForComparison(parsed);

        var rawPaths = JsonDiff.CollectJsonPaths(raw, includeEmptyArrays: false);
        var modelPaths = JsonDiff.CollectJsonPaths(model, includeEmptyArrays: false);
        foreach (var p in JsonDiff.SortUnique(rawPaths.Except(modelPaths))) r.Add($"parsed model drops {p}");
        foreach (var p in JsonDiff.SortUnique(modelPaths.Except(rawPaths))) r.Add($"parsed model adds {p}");
        var diffs = new List<string>();
        CompareValues(raw!, model, "", diffs);
        r.AddRange(diffs.Distinct().Take(10));
        return r;
    }

    private static void CompareValues(JToken raw, JToken? model, string path, List<string> diffs)
    {
        if (raw.Type == JTokenType.Null || model == null || model.Type == JTokenType.Null) return;
        switch (raw)
        {
            case JObject ro when model is JObject mo:
                foreach (var p in ro.Properties()) CompareValues(p.Value, mo[p.Name], path.Length == 0 ? p.Name : $"{path}.{p.Name}", diffs);
                return;
            case JArray ra when model is JArray ma:
                if (ra.Count != ma.Count) diffs.Add($"{path}: {ra.Count} items raw, {ma.Count} parsed");
                for (var i = 0; i < Math.Min(ra.Count, ma.Count); i++) CompareValues(ra[i], ma[i], $"{path}[{i}]", diffs);
                return;
            case JObject or JArray:
                diffs.Add($"{path}: raw {raw.Type}, parsed {model.Type}");
                return;
        }
        if (!LeafEqual((JValue)raw, model)) diffs.Add($"{path}: raw {Show(raw)} vs parsed {Show(model)}");
    }

    /// <summary>Offline check of the raw-vs-parsed comparison, run before any live call.</summary>
    private static void SelfCheck()
    {
        List<string> Diff(string raw, string model)
        {
            var r = JsonDiff.NormalizeJsonForComparison(ParseNoDates(raw))!;
            var m = JsonDiff.NormalizeJsonForComparison(ParseNoDates(model));
            var d = JsonDiff.CollectJsonPaths(r, includeEmptyArrays: false).Except(JsonDiff.CollectJsonPaths(m, includeEmptyArrays: false)).ToList();
            CompareValues(r, m, "", d);
            return d;
        }
        void Assert(bool ok, string what) { if (!ok) throw new InvalidOperationException("self-check failed: " + what); }
        Assert(Diff(@"{""t"":""2026-10-22T06:24:00Z"",""n"":10,""x"":null}", @"{""t"":""2026-10-22T06:24:00.0000000Z"",""n"":10.0}").Count == 0, "instants/numbers/nulls equal");
        Assert(Diff(@"{""a"":1,""b"":""x""}", @"{""a"":1}").Count == 1, "dropped field detected");
        Assert(Diff(@"{""a"":""2026-10-22T06:24:00Z""}", @"{""a"":""2026-10-22T06:24:01Z""}").Count == 1, "different instant detected");
        Assert(Diff(@"{""a"":""10""}", @"{""a"":10}").Count == 1, "string vs number detected");
        Assert(Diff(@"{""a"":[1,2]}", @"{""a"":[1]}").Count == 1, "array length detected");
    }

    /// <summary>
    /// The request validator must compile and reject a wrong body, or every request check would pass
    /// silently. Response schemas are covered by the shared OpenApiSpec.SelfCheckAsync (Program.cs).
    /// </summary>
    private static async Task SchemaSelfCheckAsync()
    {
        const string op = "create-cloud-playout-channel";
        var req = await SchemaFor($"req:{op}", Ops[op].Op["requestBody"]!["content"]!["application/json"]!["schema"]);
        if (req == null) throw new InvalidOperationException("self-check failed: request schema did not compile");
        if (OpenApiSpec.Validate(req, JObject.Parse(@"{""name"":""ab"",""type"":""live""}")).Count == 0)
            throw new InvalidOperationException("self-check failed: request schema accepted an invalid body");
    }

    private static bool LeafEqual(JValue raw, JToken model)
    {
        if (model is not JValue m) return false;
        if (raw.Type is JTokenType.Integer or JTokenType.Float && m.Type is JTokenType.Integer or JTokenType.Float)
            return Convert.ToDecimal(raw.Value, CultureInfo.InvariantCulture) == Convert.ToDecimal(m.Value, CultureInfo.InvariantCulture);
        if (raw.Type == JTokenType.String && m.Type == JTokenType.String)
        {
            var a = raw.ToString();
            var b = m.ToString();
            if (a == b) return true;
            if (TimestampLike.IsMatch(a) && TimestampLike.IsMatch(b)
                && DateTimeOffset.TryParse(a, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var da)
                && DateTimeOffset.TryParse(b, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var db))
                return da.UtcTicks == db.UtcTicks;
            return false;
        }
        return raw.Type == m.Type && JToken.DeepEquals(raw, m);
    }

    // ---------------------------------------------------------------- spec + IO helpers

    private static void LoadOps()
    {
        foreach (var path in ((JObject)_spec["paths"]!).Properties().Where(p => p.Name.StartsWith("/cloud-playout/channels", StringComparison.Ordinal)))
            foreach (var m in ((JObject)path.Value).Properties().Where(m => m.Name is "get" or "post" or "put" or "patch" or "delete"))
                Ops[m.Value["operationId"]!.ToString()] = (m.Name.ToUpperInvariant(), path.Name, (JObject)m.Value);
        if (Ops.Count != 33) Console.Error.WriteLine($"warning: spec has {Ops.Count} cloud-playout channel operations, expected 33");
    }

    private static IEnumerable<JObject> Params(JObject op, string template)
    {
        var all = new List<JObject>();
        if (op["parameters"] is JArray a) all.AddRange(a.OfType<JObject>());
        if (_spec["paths"]![template]!["parameters"] is JArray b) all.AddRange(b.OfType<JObject>());
        return all.Select(p => (JObject)(Resolve(p) ?? p));
    }

    private static JToken? Resolve(JToken? node)
    {
        var r = node?["$ref"]?.ToString();
        if (r == null) return node;
        var target = _spec.SelectToken("$" + string.Join("", r.TrimStart('#', '/').Split('/').Select(s => $"['{s}']")));
        return Resolve(target);
    }

    private static async Task<JsonSchema?> SchemaFor(string key, JToken? schema)
    {
        if (schema == null) return null;
        if (!SchemaCache.TryGetValue(key, out var s))
        {
            try { s = await OpenApiSpec.CompileSchemaAsync(_spec, schema); }
            catch (Exception e) { Console.Error.WriteLine($"schema compile failed for {key}: {e.Message}"); s = null; }
            SchemaCache[key] = s;
        }
        return s;
    }

    internal static JToken? ParseRaw(CallRecord? c)
    {
        if (c == null || c.Body.Length == 0) return null;
        try { return ParseNoDates(c.Text); } catch { return null; }
    }

    private static JToken? ParseNoDates(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        using var reader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
        return JToken.ReadFrom(reader);
    }

    private static async Task<CallRecord> RawAsync(string method, string pathAndQuery, JObject? body = null)
    {
        var req = new HttpRequestMessage(new HttpMethod(method), _baseUrl.TrimEnd('/') + pathAndQuery);
        req.Headers.Authorization = AuthenticationHeaderValue.Parse(_auth);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (body != null) req.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");
        try { using var _ = await _http.SendAsync(req); }
        catch (Exception e) { Console.Error.WriteLine($"raw {method} {pathAndQuery.Split('?')[0]} failed: {e.GetType().Name}"); }
        var c = _rec.Calls.Last();
        CollectWorkspaceIds(c);
        WriteArtifact(c, "raw", null, new List<string>());
        return c;
    }

    private static async Task RawDeleteAsync(string path)
    {
        var c = await RawAsync("DELETE", path);
        Console.WriteLine($"  DELETE {path} -> {c.Status}{(c.Status == 404 ? " (already gone)" : "")}");
    }

    private static void WriteArtifact(CallRecord c, string label, JToken? parsed, List<string> reasons)
    {
        CollectWorkspaceIds(c);
        var safe = Regex.Replace(label, @"[^A-Za-z0-9_-]+", "_");
        var doc = new JObject
        {
            ["label"] = label,
            ["request"] = new JObject { ["method"] = c.Method, ["path"] = c.Path, ["query"] = c.Query, ["body"] = c.RequestBody == null ? null : (ParseNoDatesSafe(c.RequestBody) ?? c.RequestBody) },
            ["response"] = new JObject
            {
                ["status"] = c.Status,
                ["contentType"] = new JArray(c.ContentType),
                ["bom"] = c.Bom,
                ["headers"] = JObject.FromObject(c.Headers),
                ["body"] = ParseNoDatesSafe(c.Text) ?? c.Text,
            },
            ["parsed"] = parsed,
            ["reasons"] = new JArray(reasons),
            ["error"] = c.Error,
        };
        File.WriteAllText(Path.Combine(_artifacts, $"{c.Seq:D3}-{safe}.json"), doc.ToString(Formatting.Indented));
    }

    private static JToken? ParseNoDatesSafe(string text)
    {
        try { return ParseNoDates(text); } catch { return null; }
    }

    private static void CollectWorkspaceIds(CallRecord c)
    {
        foreach (Match m in WsRx.Matches(c.Text)) WorkspaceIds.Add(m.Groups[1].Value);
    }

    /// <summary>Console/report text never shows a URL, the host or a workspace id.</summary>
    private static string Redact(string s)
    {
        s = UrlRx.Replace(s, "<url>");
        if (_host.Length > 0) s = s.Replace(_host, "<host>", StringComparison.OrdinalIgnoreCase);
        foreach (var w in WorkspaceIds) s = s.Replace(w, "<workspaceId>");
        return s;
    }

    private static string? Str(JToken? t) => t == null || t.Type == JTokenType.Null ? null : t.ToString();
    private static string Show(JToken t) => Trunc(t.ToString(Formatting.None), 60);
    private static string Trunc(string s, int n) => s.Length <= n ? s : s[..n] + "...";
    private static string F(DateTime utc) => utc.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
    private static DateTime NextMinute(DateTime t) => new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0, DateTimeKind.Utc).AddMinutes(1);

    // ---------------------------------------------------------------- report

    private static int WriteReport(List<string> cleanup, int preCount, int postCount)
    {
        var ordered = Ops.Keys.Select(k => Rows.TryGetValue(k, out var row) ? row : new OpRow { OperationId = k }).ToList();
        var extra = Rows.Values.Where(r => !Ops.ContainsKey(r.OperationId)).ToList();
        var pass = ordered.Count(r => r.Pass);
        var fail = ordered.Count - pass;

        Console.WriteLine();
        Console.WriteLine($"{"operationId",-46} {"result",-6} calls  reason");
        foreach (var r in ordered.Concat(extra))
        {
            var reason = r.Calls == 0 && r.Reasons.Count == 0 ? "not run" : string.Join("; ", r.Reasons);
            Console.WriteLine($"{r.OperationId,-46} {(r.Pass ? "PASS" : "FAIL"),-6} {r.Calls,5}  {Trunc(reason, 160)}");
        }
        Console.WriteLine($"\n{pass} PASS, {fail} FAIL of {ordered.Count} operations");
        foreach (var n in Notes) Console.WriteLine($"note: {Redact(n)}");
        foreach (var l in cleanup) Console.WriteLine($"cleanup: {l}");

        var md = new StringBuilder();
        md.AppendLine($"# Cloud Playout live contract run ({EnvLabel})");
        md.AppendLine();
        md.AppendLine($"Run {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC. {pass} PASS, {fail} FAIL of {ordered.Count} operations. Per-call artifacts are next to this file.");
        md.AppendLine();
        md.AppendLine("| # | operationId | result | calls | class | reason |");
        md.AppendLine("|---|---|---|---|---|---|");
        var i = 0;
        foreach (var r in ordered.Concat(extra))
        {
            var reason = r.Calls == 0 && r.Reasons.Count == 0 ? "not run" : string.Join("; ", r.Reasons);
            var cls = r.Pass ? "" : Classes.GetValueOrDefault(r.OperationId, "unclassified");
            md.AppendLine($"| {(Ops.ContainsKey(r.OperationId) ? (++i).ToString() : "-")} | {r.OperationId} | {(r.Pass ? "PASS" : "FAIL")} | {r.Calls} | {cls} | {reason.Replace("|", "\\|")} |");
        }
        md.AppendLine();
        md.AppendLine("## Notes");
        foreach (var n in Notes) md.AppendLine($"- {Redact(n).Replace("|", "\\|")}");
        md.AppendLine();
        md.AppendLine("## Cleanup");
        foreach (var l in cleanup) md.AppendLine($"- {l}");
        File.WriteAllText(Path.Combine(_artifacts, "REPORT.md"), md.ToString());

        var allPass = fail == 0 && extra.All(r => r.Pass) && postCount == preCount;
        return allPass ? 0 : 1;
    }
}

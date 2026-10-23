using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Fastpix.Hooks;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Fastpix.Utils.Retries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Fastpix.UnitTests;

// One row per Cloud Playout operation, driven by reflection through a fake transport:
// verb, path, query, body, success code, payload and hook id. No network.
public class CloudPlayoutTests
{
    public sealed record Row(string Op, string Prop, string Method, string Args, string Verb, string Path, string Query, string? Body,
        int Code, string Payload, string Response, string? Pick = null, string? Expect = null, string ContentType = Json)
    {
        public int? Status { get; init; }
        public Dictionary<string, string>? Headers { get; init; }
    }

    private const string Json = "application/json";
    private const string Ok = """{"success":true}""";
    private const string Csv = "name,startTime\r\nCafé — Morning,2026-11-01T00:00:00\r\n";
    private const string Xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<tv><programme channel=\"c1\" start=\"20261101000000 +0000\"><title>Café — Frühstück</title></programme></tv>";
    private const string Ch = "/cloud-playout/channels";

    private static readonly Row[] All =
    {
        new("list-cloud-playout-channels", "CloudPlayoutChannels", "ListAsync", """{"type":"loop","limit":10,"offset":1,"orderBy":"desc"}""",
            "GET", Ch, "type=loop&limit=10&offset=1&orderBy=desc", null, 200, "CloudPlayoutChannelListResponse",
            """{"success":true,"data":[{"id":"c1","name":"Morning"},{"id":"c2","name":"Evening"}],"pagination":{"totalRecords":2,"currentOffset":1,"offsetCount":1}}""",
            "data[1].id", "c2"),
        new("create-cloud-playout-channel", "CloudPlayoutChannels", "CreateAsync",
            """{"body":{"name":"Morning","type":"schedule","startTime":"2026-11-01T00:00:00","endTime":"2026-11-02T00:00:00"}}""",
            "POST", Ch, "", """{"name":"Morning","type":"schedule","startTime":"2026-11-01T00:00:00","endTime":"2026-11-02T00:00:00"}""",
            201, "CloudPlayoutChannelResponse", """{"success":true,"data":{"id":"c1","name":"Morning","playbackUrl":"https://stream.fastpix.com/pb1.m3u8"}}""",
            "data.id", "c1"),
        new("get-cloud-playout-channel-by-id", "CloudPlayoutChannels", "GetByIdAsync", """{"channelId":"c1"}""",
            "GET", Ch + "/c1", "", null, 200, "CloudPlayoutChannelResponse", """{"success":true,"data":{"id":"c1","name":"Morning"}}""",
            "data.name", "Morning"),
        new("update-cloud-playout-channel", "CloudPlayoutChannels", "UpdateAsync", """{"channelId":"c1","body":{"name":"Evening"}}""",
            "PATCH", Ch + "/c1", "", """{"name":"Evening"}""", 200, "CloudPlayoutChannelResponse", """{"success":true,"data":{"id":"c1","name":"Evening"}}""",
            "data.name", "Evening"),
        new("delete-cloud-playout-channel", "CloudPlayoutChannels", "DeleteAsync", """{"channelId":"c1"}""",
            "DELETE", Ch + "/c1", "", null, 200, "CloudPlayoutChannelDeleteResponse", Ok, "success", "true"),
        new("activate-cloud-playout-channel", "CloudPlayoutChannels", "ActivateAsync", """{"channelId":"c1"}""",
            "POST", Ch + "/c1/activate", "", null, 200, "CloudPlayoutSuccessResponse", Ok, "success", "true"),
        new("deactivate-cloud-playout-channel", "CloudPlayoutChannels", "DeactivateAsync", """{"channelId":"c1"}""",
            "DELETE", Ch + "/c1/deactivate", "", null, 200, "CloudPlayoutSuccessResponse", Ok, "success", "true"),

        new("list-cloud-playout-programs", "CloudPlayoutLoopPrograms", "ListAsync", """{"channelId":"c1"}""",
            "GET", Ch + "/c1/programs", "", null, 200, "CloudPlayoutProgramListResponse",
            """{"success":true,"data":[{"id":"p1","name":"Block A","index":0,"duration":"00:10:00"},{"id":"p2","name":"Block B"}]}""",
            "data[1].id", "p2"),
        new("create-cloud-playout-program", "CloudPlayoutLoopPrograms", "CreateAsync", """{"channelId":"c1","body":{"name":"Block A"}}""",
            "POST", Ch + "/c1/programs", "", """{"name":"Block A"}""", 201, "CloudPlayoutProgramResponse",
            """{"success":true,"data":{"id":"p1","name":"Block A","index":1,"duration":"00:00:00"}}""", "data.duration", "00:00:00"),
        new("add-media-to-cloud-playout-program", "CloudPlayoutLoopPrograms", "AddMediaAsync",
            """{"channelId":"c1","programId":"p1","body":{"mediaIds":["m1","m2"]}}""",
            "POST", Ch + "/c1/programs/p1/media", "", """{"mediaIds":["m1","m2"]}""", 201, "CloudPlayoutProgramResponse",
            """{"success":true,"data":{"id":"p1","mediaCount":2}}""", "data.mediaCount", "2"),
        new("add-playlist-to-cloud-playout-program", "CloudPlayoutLoopPrograms", "AddPlaylistAsync",
            """{"channelId":"c1","programId":"p1","body":{"playlistId":"pl1"}}""",
            "POST", Ch + "/c1/programs/p1/playlist", "", """{"playlistId":"pl1"}""", 201, "CloudPlayoutProgramResponse",
            """{"success":true,"data":{"id":"p1","name":"Block A"}}""", "data.id", "p1"),
        new("get-cloud-playout-program-media-details", "CloudPlayoutLoopPrograms", "GetMediaDetailsAsync", """{"channelId":"c1","programId":"p1"}""",
            "GET", Ch + "/c1/programs/p1/media-details", "", null, 200, "CloudPlayoutProgramMediaListResponse",
            """{"success":true,"data":[{"id":"e1","programId":"p1","segments":[{"id":"sg1","bumper":{"preroll":{"id":"b1","name":"Intro","duration":5,"durationMillis":5000}}}]}]}""",
            "data[0].segments[0].bumper.preroll.name", "Intro"),
        new("delete-cloud-playout-program-item", "CloudPlayoutLoopPrograms", "DeleteItemAsync", """{"channelId":"c1","programId":"p1","eventId":"e1"}""",
            "DELETE", Ch + "/c1/programs/p1/event/e1", "", null, 200, "CloudPlayoutSuccessResponse", Ok, "success", "true"),
        new("delete-cloud-playout-program-items", "CloudPlayoutLoopPrograms", "DeleteItemsAsync",
            """{"channelId":"c1","programId":"p1","body":{"eventIds":["e1","e2"]}}""",
            "POST", Ch + "/c1/programs/p1/event/delete", "", """{"eventIds":["e1","e2"]}""", 200, "CloudPlayoutSuccessResponse", Ok, "success", "true"),
        new("add-bumper-to-cloud-playout-program-item", "CloudPlayoutLoopPrograms", "AddBumperAsync",
            """{"channelId":"c1","programId":"p1","body":{"bumperId":"b1","eventId":"e1","bumperType":"postroll"}}""",
            "POST", Ch + "/c1/programs/p1/bumper", "", """{"bumperId":"b1","eventId":"e1","bumperType":"postroll"}""", 201, "CloudPlayoutProgramResponse",
            """{"success":true,"data":{"id":"p1","name":"Block A"}}""", "data.id", "p1"),
        new("get-cloud-playout-program-by-id", "CloudPlayoutLoopPrograms", "GetByIdAsync", """{"channelId":"c1","programId":"p1"}""",
            "GET", Ch + "/c1/programs/p1", "", null, 200, "CloudPlayoutProgramResponse", """{"success":true,"data":{"id":"p1","name":"Block A"}}""",
            "data.name", "Block A"),
        new("update-cloud-playout-program", "CloudPlayoutLoopPrograms", "UpdateAsync", """{"channelId":"c1","programId":"p1","body":{"name":"Block B"}}""",
            "PATCH", Ch + "/c1/programs/p1", "", """{"name":"Block B"}""", 200, "CloudPlayoutProgramResponse",
            """{"success":true,"data":{"id":"p1","name":"Block B"}}""", "data.name", "Block B"),
        new("delete-cloud-playout-program", "CloudPlayoutLoopPrograms", "DeleteAsync", """{"channelId":"c1","programId":"p1"}""",
            "DELETE", Ch + "/c1/programs/p1", "", null, 200, "CloudPlayoutProgramDeleteResponse", Ok, "success", "true"),

        new("get-cloud-playout-schedule-channel-events", "CloudPlayoutScheduleChannel", "ListEventsAsync", """{"channelId":"c1","startTime":"2026-11-01T00:00:00"}""",
            "GET", Ch + "/c1/schedule-event", "startTime=2026-11-01T00%3A00%3A00", null, 200, "CloudPlayoutScheduleEventListResponse",
            """{"success":true,"data":[{"id":"ev1","channelId":"c1","referenceId":"m1","bumper":{"preroll":{"id":"m9","bumperId":"b1","duration":5}}}]}""",
            "data[0].bumper.preroll.bumperId", "b1"),
        new("add-event-to-cloud-playout-schedule-channel", "CloudPlayoutScheduleChannel", "AddEventAsync",
            """{"channelId":"c1","body":{"event":{"type":"vod","referenceId":"m1","startTime":"2026-11-01T10:00:00"}}}""",
            "POST", Ch + "/c1/schedule-event", "", """{"event":{"type":"vod","referenceId":"m1","startTime":"2026-11-01T10:00:00"}}""",
            201, "CloudPlayoutScheduleEventListResponse", """{"success":true,"data":[{"id":"ev1","referenceId":"m1"}]}""", "data[0].id", "ev1"),
        new("update-cloud-playout-schedule-channel-event", "CloudPlayoutScheduleChannel", "UpdateEventAsync",
            """{"eventId":"ev1","body":{"event":{"startTime":"2026-11-01T11:00:00","endTime":"2026-11-01T11:30:00"}}}""",
            "PATCH", Ch + "/ev1/schedule-event", "", """{"event":{"startTime":"2026-11-01T11:00:00","endTime":"2026-11-01T11:30:00"}}""",
            200, "CloudPlayoutScheduleEventResponse", """{"success":true,"data":{"id":"ev1","name":"Moved"}}""", "data.name", "Moved"),
        new("delete-cloud-playout-schedule-channel-event", "CloudPlayoutScheduleChannel", "DeleteEventAsync", """{"eventId":"ev1"}""",
            "DELETE", Ch + "/ev1/schedule-event", "", null, 200, "CloudPlayoutSuccessResponse", Ok, "success", "true"),
        new("add-events-to-cloud-playout-schedule-channel", "CloudPlayoutScheduleChannel", "AddEventsAsync",
            """{"channelId":"c1","body":{"events":[{"type":"vod","referenceId":"m1","startTime":"2026-11-01T10:00:00"},{"type":"live","referenceId":"l1","startTime":"2026-11-01T11:00:00","endTime":"2026-11-01T12:00:00"}]}}""",
            "POST", Ch + "/c1/schedule-events", "",
            """{"events":[{"type":"vod","referenceId":"m1","startTime":"2026-11-01T10:00:00"},{"type":"live","referenceId":"l1","startTime":"2026-11-01T11:00:00","endTime":"2026-11-01T12:00:00"}]}""",
            201, "CloudPlayoutScheduleEventListResponse", """{"success":true,"data":[{"id":"ev1"},{"id":"ev2","referenceId":"l1"}]}""", "data[1].id", "ev2"),

        new("get-all-cloud-playout-channel-overlays", "CloudPlayoutChannelOverlays", "ListAsync",
            """{"channelId":"c1","startTime":"2026-11-01T00:00:00","endTime":"2026-11-02T00:00:00"}""",
            "GET", Ch + "/c1/overlays", "startTime=2026-11-01T00%3A00%3A00&endTime=2026-11-02T00%3A00%3A00", null, 200, "CloudPlayoutChannelOverlayListResponse",
            """{"success":true,"data":[{"id":"oc1","overlayId":"o1","repeat":{"everySeconds":300,"showForSeconds":30}}]}""",
            "data[0].repeat.everySeconds", "300"),
        new("add-overlay-to-cloud-playout-channel", "CloudPlayoutChannelOverlays", "AddAsync",
            """{"channelId":"c1","body":{"overlayId":"o1","startTime":"2026-11-01T10:00:00","duration":60}}""",
            "POST", Ch + "/c1/overlays", "", """{"overlayId":"o1","startTime":"2026-11-01T10:00:00","duration":60}""",
            200, "CloudPlayoutChannelOverlaySingleResponse", """{"success":true,"data":{"id":"oc1","overlayId":"o1"}}""", "data.id", "oc1"),
        new("update-cloud-playout-channel-overlay", "CloudPlayoutChannelOverlays", "UpdateAsync",
            """{"channelId":"c1","overlayConfigId":"oc1","body":{"removeRepeat":true}}""",
            "PATCH", Ch + "/c1/overlays/oc1", "", """{"removeRepeat":true}""",
            200, "CloudPlayoutChannelOverlaySingleResponse", """{"success":true,"data":{"id":"oc1","overlayId":"o1"}}""", "data.id", "oc1"),
        new("delete-cloud-playout-channel-overlay", "CloudPlayoutChannelOverlays", "DeleteAsync", """{"channelId":"c1","overlayConfigId":"oc1"}""",
            "DELETE", Ch + "/c1/overlays/oc1", "", null, 200, "CloudPlayoutSuccessResponse", Ok, "success", "true"),

        new("get-cloud-playout-channel-schedule", "CloudPlayoutScheduleAndEpg", "GetScheduleAsync", """{"channelId":"c1","type":"schedule"}""",
            "GET", Ch + "/c1/schedule", "type=schedule", null, 200, "CloudPlayoutChannelScheduleResponse",
            """{"success":true,"data":{"schedule":[{"id":"m1","bumper":{"postroll":{"id":"m9","bumperId":"b1","name":"Outro","duration":5}}}],"videoCount":1}}""",
            "data.schedule[0].bumper.postroll.id", "m9"),
        new("export-cloud-playout-channel-schedule", "CloudPlayoutScheduleAndEpg", "ExportScheduleAsync",
            """{"channelId":"c1","type":"schedule","startTime":"2026-11-01T00:00:00","endTime":"2026-11-02T00:00:00"}""",
            "GET", Ch + "/c1/schedule/export", "type=schedule&startTime=2026-11-01T00%3A00%3A00&endTime=2026-11-02T00%3A00%3A00", null,
            200, "Csv", Csv, ContentType: "text/csv; charset=utf-8")
        {
            Headers = new() { ["Content-Disposition"] = "attachment; filename=\"schedule_c1_loop_from_a_to_b.csv\"" }
        },
        new("get-cloud-playout-channel-epg", "CloudPlayoutScheduleAndEpg", "GetEpgAsync",
            """{"channelId":"c1","type":"schedule","startTime":"2026-11-01T00:00:00","endTime":"2026-11-02T00:00:00"}""",
            "GET", Ch + "/c1/epg", "type=schedule&startTime=2026-11-01T00%3A00%3A00&endTime=2026-11-02T00%3A00%3A00", null,
            200, "Xml", Xml, ContentType: "application/xml")
        {
            Headers = new() { ["X-Total-Count"] = "3" }
        },

        new("get-cloud-playout-channel-simulcasts", "CloudPlayoutSimulcast", "ListAsync", """{"channelId":"c1"}""",
            "GET", Ch + "/simulcast/c1", "", null, 200, "CloudPlayoutSimulcastListResponse",
            """{"success":true,"data":{"destinationLiveId":"d1","playbackId":"pb1","simulcastResponses":[{"simulcastId":"s1","url":"rtmps://example.com/app"}]}}""",
            "data.destinationLiveId", "d1"),
        new("add-cloud-playout-channel-simulcast", "CloudPlayoutSimulcast", "CreateAsync", """{"channelId":"c1","body":{"url":"rtmps://example.com/app/key1"}}""",
            "POST", Ch + "/simulcast/c1", "", """{"url":"rtmps://example.com/app/key1"}""", 200, "CloudPlayoutSimulcastCreateResponse",
            """{"success":true,"data":{"simulcastId":"s1","url":"rtmps://example.com/app","streamKey":"key1","isEnabled":true}}""",
            "data.simulcastId", "s1"),
        new("delete-cloud-playout-channel-simulcast", "CloudPlayoutSimulcast", "DeleteAsync", """{"livestreamId":"d1","simulcastId":"s1"}""",
            "DELETE", Ch + "/d1/simulcast/s1", "", null, 200, "CloudPlayoutSuccessResponse", Ok, "success", "true"),
    };

    private static readonly Dictionary<string, Row> Rows = All.GroupBy(r => r.Op).ToDictionary(g => g.Key, g => g.First());

    private static readonly Dictionary<string, Row> Variants = new()
    {
        ["create-channel-spec-body"] = Rows["create-cloud-playout-channel"] with
        {
            Args = """{"body":{"name":"Morning","type":"loop","startTime":"2026-11-01T00:00:00","endTime":"2026-11-02T00:00:00","liveSupport":false,"genres":["news"]}}""",
            Body = """{"name":"Morning","type":"loop","startTime":"2026-11-01T00:00:00","endTime":"2026-11-02T00:00:00","liveSupport":false,"genres":["news"]}""",
        },
        ["add-event-playlist"] = Rows["add-event-to-cloud-playout-schedule-channel"] with
        {
            Args = """{"channelId":"c1","body":{"event":{"type":"playlist","referenceId":"pl1","startTime":"2026-11-01T12:00:00","endTime":"2026-11-01T13:00:00","name":"Noon"}}}""",
            Body = """{"event":{"type":"playlist","referenceId":"pl1","startTime":"2026-11-01T12:00:00","endTime":"2026-11-01T13:00:00","name":"Noon"}}""",
        },
        ["create-channel-200-not-success"] = Rows["create-cloud-playout-channel"] with { Status = 200 },
        ["add-overlay-201-not-success"] = Rows["add-overlay-to-cloud-playout-channel"] with { Status = 201 },
        ["add-simulcast-201-not-success"] = Rows["add-cloud-playout-channel-simulcast"] with { Status = 201 },
    };

    public static IEnumerable<object[]> Ops() => All.Select(r => new object[] { r.Op });
    public static IEnumerable<object[]> VariantNames() => Variants.Keys.Select(k => new object[] { k });

    [Theory]
    [MemberData(nameof(Ops))]
    public Task Operation_MatchesContract(string op) => Run(Rows[op]);

    [Theory]
    [MemberData(nameof(VariantNames))]
    public Task Variant_MatchesContract(string name) => Run(Variants[name]);

    [Fact]
    public void Table_CoversEverySpecOperation()
    {
        Assert.Equal(33, All.Length);
        Assert.Equal(All.Length, Rows.Count);
        Assert.Equal("startTime=2026-11-01T00%3A00%3A00",
            UrlBuilder.SerializeQueryParams(new() { ["startTime"] = new() { "2026-11-01T00:00:00" } }));
        Assert.False(JToken.DeepEquals(Parse("""{"startTime":"2026-11-01T00:00:00"}"""), Parse("""{"startTime":"2026-11-01T00:00:00Z"}""")));

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "openapi.yaml"))) dir = dir.Parent;
        if (dir == null) return;
        var specOps = Regex.Matches(File.ReadAllText(Path.Combine(dir.FullName, "openapi.yaml")), @"operationId:\s*(\S*cloud-playout\S*)")
            .Select(m => m.Groups[1].Value).ToHashSet();
        Assert.Equal(specOps.OrderBy(x => x), Rows.Keys.OrderBy(x => x));
    }

    [Fact]
    public void BothConstructors_ExposeEveryResource()
    {
        var viaArgs = new FastpixSDK(security: new Security { Username = "u", Password = "p" });
        var viaConfig = new FastpixSDK(viaArgs.SDKConfiguration);
        foreach (var sdk in new[] { viaArgs, viaConfig })
        {
            Assert.IsType<CloudPlayoutChannels>(sdk.CloudPlayoutChannels);
            Assert.IsType<CloudPlayoutLoopPrograms>(sdk.CloudPlayoutLoopPrograms);
            Assert.IsType<CloudPlayoutScheduleChannel>(sdk.CloudPlayoutScheduleChannel);
            Assert.IsType<CloudPlayoutChannelOverlays>(sdk.CloudPlayoutChannelOverlays);
            Assert.IsType<CloudPlayoutScheduleAndEpg>(sdk.CloudPlayoutScheduleAndEpg);
            Assert.IsType<CloudPlayoutSimulcast>(sdk.CloudPlayoutSimulcast);
        }
    }

    private sealed class FakeClient : FastpixHttpClient
    {
        private readonly Row _row;
        public HttpRequestMessage? Sent;
        public string? SentBody;
        public FakeClient(Row row) { _row = row; }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken? cancellationToken = null)
        {
            Sent = request;
            SentBody = request.Content == null ? null : await request.Content.ReadAsStringAsync();
            var content = new StringContent(_row.Response, System.Text.Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(_row.ContentType);
            var res = new HttpResponseMessage((HttpStatusCode)(_row.Status ?? _row.Code)) { Content = content, RequestMessage = request };
            foreach (var (k, v) in _row.Headers ?? new())
                if (!res.Headers.TryAddWithoutValidation(k, v)) content.Headers.TryAddWithoutValidation(k, v);
            return res;
        }
    }

    private sealed class OpRecorder : IBeforeRequestHook
    {
        public string? OperationId;
        public Task<HttpRequestMessage> BeforeRequestAsync(BeforeRequestContext hookCtx, HttpRequestMessage request)
        {
            OperationId = hookCtx.OperationID;
            return Task.FromResult(request);
        }
    }

    private static JToken Parse(string json) =>
        JsonConvert.DeserializeObject<JToken>(json, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None })!;

    private static string HexUpper(string s) => Regex.Replace(s, "%[0-9a-fA-F]{2}", m => m.Value.ToUpperInvariant());

    private static async Task Run(Row row)
    {
        var client = new FakeClient(row);
        var sdk = new FastpixSDK(security: new Security { Username = "u", Password = "p" }, client: client,
            retryConfig: new RetryConfig(RetryConfig.RetryStrategy.NONE, null, false));
        var hook = new OpRecorder();
        sdk.SDKConfiguration.Hooks.RegisterBeforeRequestHook(hook);
        var name = $"{row.Prop}.{row.Method}";

        var resource = typeof(FastpixSDK).GetProperty(row.Prop)?.GetValue(sdk);
        Assert.True(resource != null, $"FastpixSDK.{row.Prop} not found");
        var method = resource!.GetType().GetMethod(row.Method);
        Assert.True(method != null, $"{name} not found");

        var ps = method!.GetParameters();
        var args = (JObject)Parse(row.Args);
        foreach (var arg in args.Properties())
            Assert.True(ps.Any(p => p.Name == arg.Name), $"{name} parameter '{arg.Name}' not found");
        var order = ps.Select(p => p.Name!).Where(args.ContainsKey).ToList();
        Assert.True(order.SequenceEqual(args.Properties().Select(p => p.Name)), $"{name} parameter order is {string.Join(", ", order)}");
        var settings = Utilities.GetDefaultJsonDeserializerSettings();
        settings.DateParseHandling = DateParseHandling.None;
        var serializer = JsonSerializer.Create(settings);
        var values = ps.Select(p => args.TryGetValue(p.Name!, out var v) ? v.ToObject(p.ParameterType, serializer)
            : p.IsOptional ? Type.Missing
            : throw new Xunit.Sdk.XunitException($"{name} parameter '{p.Name}' has no value in row {row.Op}")).ToArray();

        var task = (Task)method.Invoke(resource, values)!;
        await task;
        var res = task.GetType().GetProperty("Result")!.GetValue(task)!;

        var sent = client.Sent!;
        Assert.Equal(new HttpMethod(row.Verb), sent.Method);
        var url = sent.RequestUri!.OriginalString;
        var q = url.IndexOf('?');
        Assert.Equal(new Uri(sdk.SDKConfiguration.GetTemplatedServerUrl()).AbsolutePath.TrimEnd('/') + row.Path, sent.RequestUri.AbsolutePath);
        Assert.Equal(HexUpper(row.Query), HexUpper(q < 0 ? "" : url[(q + 1)..]));
        if (row.Body == null)
        {
            Assert.Null(sent.Content);
        }
        else
        {
            Assert.Equal(Json, sent.Content!.Headers.ContentType!.MediaType);
            Assert.True(JToken.DeepEquals(Parse(row.Body), Parse(client.SentBody!)), $"{name} sent {client.SentBody}, expected {row.Body}");
        }
        Assert.Equal(row.Op, hook.OperationId);

        var payloadProp = res.GetType().GetProperty(row.Payload);
        Assert.True(payloadProp != null, $"{res.GetType().Name}.{row.Payload} not found");
        var payload = payloadProp!.GetValue(res);
        if (row.Status is int status && status != row.Code)
        {
            Assert.Null(payload);
            Assert.NotNull(res.GetType().GetProperty("DefaultError")?.GetValue(res));
            return;
        }
        Assert.NotNull(payload);
        if (row.Pick == null)
        {
            Assert.Equal(row.Response, (string)payload!);
        }
        else
        {
            var json = Parse(JsonConvert.SerializeObject(payload, Utilities.GetDefaultJsonSerializerSettings()));
            Assert.Equal(row.Expect, json.SelectToken(row.Pick)?.ToString(Formatting.None).Trim('"'));
        }

        var http = ((HttpMetadata)res.GetType().GetProperty("HttpMeta")!.GetValue(res)!).Response!;
        foreach (var (k, v) in row.Headers ?? new())
        {
            Assert.True(http.Headers.TryGetValues(k, out var got) || http.Content.Headers.TryGetValues(k, out got), $"header {k} not readable");
            Assert.Equal(v, string.Join(", ", got!));
        }
    }
}

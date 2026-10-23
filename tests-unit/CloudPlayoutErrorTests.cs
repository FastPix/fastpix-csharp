using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Fastpix.Models.Components;
using Fastpix.Models.Errors;
using Fastpix.Models.Requests;
using Fastpix.Utils;
using Fastpix.Utils.Retries;
using Xunit;

namespace Fastpix.UnitTests;

// Error and success-code paths for all 33 Cloud Playout operations through a fake
// transport with retries off: 4xx/5xx throw ApiException, wrong 2xx yields DefaultError. No network.
public class CloudPlayoutErrorTests
{
    private sealed class FakeClient : FastpixHttpClient
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;
        private readonly string _type;
        public FakeClient(HttpStatusCode status, string body, string type) { _status = status; _body = body; _type = type; }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken? cancellationToken = null)
        {
            var content = new StringContent(_body, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(_type);
            return Task.FromResult(new HttpResponseMessage(_status) { Content = content, RequestMessage = request });
        }
    }

    private static FastpixSDK Sdk(int status, string body, string type = "application/json") =>
        new(security: new Security { Username = "u", Password = "p" }, client: new FakeClient((HttpStatusCode)status, body, type),
            retryConfig: new RetryConfig(RetryConfig.RetryStrategy.NONE, null, false));

    private static string ErrorBody(int status) => $"{{\"success\":false,\"error\":{{\"code\":{status},\"message\":\"failed with {status}\"}}}}";

    private const string T0 = "2026-11-01T00:00:00";
    private const string T1 = "2026-11-02T00:00:00";

    private static readonly Dictionary<string, Func<FastpixSDK, Task>> Ops = new()
    {
        ["CloudPlayoutChannels.ListAsync"] = s => s.CloudPlayoutChannels.ListAsync(ListCloudPlayoutChannelsType.Loop),
        ["CloudPlayoutChannels.CreateAsync"] = s => s.CloudPlayoutChannels.CreateAsync(
            new CloudPlayoutChannelRequest { Name = "Morning", Type = CloudPlayoutChannelRequestType.Schedule, StartTime = T0, EndTime = T1 }),
        ["CloudPlayoutChannels.GetByIdAsync"] = s => s.CloudPlayoutChannels.GetByIdAsync("c1"),
        ["CloudPlayoutChannels.UpdateAsync"] = s => s.CloudPlayoutChannels.UpdateAsync("c1", new CloudPlayoutChannelUpdateRequest { Name = "Evening" }),
        ["CloudPlayoutChannels.DeleteAsync"] = s => s.CloudPlayoutChannels.DeleteAsync("c1"),
        ["CloudPlayoutChannels.ActivateAsync"] = s => s.CloudPlayoutChannels.ActivateAsync("c1"),
        ["CloudPlayoutChannels.DeactivateAsync"] = s => s.CloudPlayoutChannels.DeactivateAsync("c1"),

        ["CloudPlayoutLoopPrograms.ListAsync"] = s => s.CloudPlayoutLoopPrograms.ListAsync("c1"),
        ["CloudPlayoutLoopPrograms.CreateAsync"] = s => s.CloudPlayoutLoopPrograms.CreateAsync("c1", new CloudPlayoutProgramRequest { Name = "Block A" }),
        ["CloudPlayoutLoopPrograms.AddMediaAsync"] = s => s.CloudPlayoutLoopPrograms.AddMediaAsync("c1", "p1",
            new CloudPlayoutProgramMediaRequest { MediaIds = new() { "m1" } }),
        ["CloudPlayoutLoopPrograms.AddPlaylistAsync"] = s => s.CloudPlayoutLoopPrograms.AddPlaylistAsync("c1", "p1",
            new CloudPlayoutProgramPlaylistRequest { PlaylistId = "pl1" }),
        ["CloudPlayoutLoopPrograms.GetMediaDetailsAsync"] = s => s.CloudPlayoutLoopPrograms.GetMediaDetailsAsync("c1", "p1"),
        ["CloudPlayoutLoopPrograms.DeleteItemAsync"] = s => s.CloudPlayoutLoopPrograms.DeleteItemAsync("c1", "p1", "e1"),
        ["CloudPlayoutLoopPrograms.DeleteItemsAsync"] = s => s.CloudPlayoutLoopPrograms.DeleteItemsAsync("c1", "p1",
            new CloudPlayoutProgramItemDeleteRequest { EventIds = new() { "e1" } }),
        ["CloudPlayoutLoopPrograms.AddBumperAsync"] = s => s.CloudPlayoutLoopPrograms.AddBumperAsync("c1", "p1",
            new CloudPlayoutProgramBumperRequest { BumperId = "b1", EventId = "e1", BumperType = CloudPlayoutProgramBumperRequestBumperType.Postroll }),
        ["CloudPlayoutLoopPrograms.GetByIdAsync"] = s => s.CloudPlayoutLoopPrograms.GetByIdAsync("c1", "p1"),
        ["CloudPlayoutLoopPrograms.UpdateAsync"] = s => s.CloudPlayoutLoopPrograms.UpdateAsync("c1", "p1", new CloudPlayoutProgramRequest { Name = "Block B" }),
        ["CloudPlayoutLoopPrograms.DeleteAsync"] = s => s.CloudPlayoutLoopPrograms.DeleteAsync("c1", "p1"),

        ["CloudPlayoutScheduleChannel.ListEventsAsync"] = s => s.CloudPlayoutScheduleChannel.ListEventsAsync("c1", T0),
        ["CloudPlayoutScheduleChannel.AddEventAsync"] = s => s.CloudPlayoutScheduleChannel.AddEventAsync("c1",
            new CloudPlayoutScheduleEventRequest { Event = new CloudPlayoutScheduleEventCreate { Type = CloudPlayoutScheduleEventCreateType.Vod, ReferenceId = "m1", StartTime = T0 } }),
        ["CloudPlayoutScheduleChannel.UpdateEventAsync"] = s => s.CloudPlayoutScheduleChannel.UpdateEventAsync("ev1",
            new CloudPlayoutScheduleEventUpdateRequest { Event = new CloudPlayoutScheduleEventUpdate { StartTime = T0, EndTime = T1 } }),
        ["CloudPlayoutScheduleChannel.DeleteEventAsync"] = s => s.CloudPlayoutScheduleChannel.DeleteEventAsync("ev1"),
        ["CloudPlayoutScheduleChannel.AddEventsAsync"] = s => s.CloudPlayoutScheduleChannel.AddEventsAsync("c1", EventsBody),

        ["CloudPlayoutChannelOverlays.ListAsync"] = s => s.CloudPlayoutChannelOverlays.ListAsync("c1", T0, T1),
        ["CloudPlayoutChannelOverlays.AddAsync"] = s => s.CloudPlayoutChannelOverlays.AddAsync("c1", OverlayBody),
        ["CloudPlayoutChannelOverlays.UpdateAsync"] = s => s.CloudPlayoutChannelOverlays.UpdateAsync("c1", "oc1",
            new CloudPlayoutChannelOverlayUpdateRequest { RemoveRepeat = true }),
        ["CloudPlayoutChannelOverlays.DeleteAsync"] = s => s.CloudPlayoutChannelOverlays.DeleteAsync("c1", "oc1"),

        ["CloudPlayoutScheduleAndEpg.GetScheduleAsync"] = s => s.CloudPlayoutScheduleAndEpg.GetScheduleAsync("c1", GetCloudPlayoutChannelScheduleType.Schedule),
        ["CloudPlayoutScheduleAndEpg.ExportScheduleAsync"] = Export,
        ["CloudPlayoutScheduleAndEpg.GetEpgAsync"] = Epg,

        ["CloudPlayoutSimulcast.ListAsync"] = s => s.CloudPlayoutSimulcast.ListAsync("c1"),
        ["CloudPlayoutSimulcast.CreateAsync"] = s => s.CloudPlayoutSimulcast.CreateAsync("c1", SimulcastBody),
        ["CloudPlayoutSimulcast.DeleteAsync"] = s => s.CloudPlayoutSimulcast.DeleteAsync("d1", "s1"),
    };

    private static CloudPlayoutScheduleEventBatchRequest EventsBody => new()
    {
        Events = new() { new CloudPlayoutScheduleEventBatchItem { Type = CloudPlayoutScheduleEventBatchItemType.Vod, ReferenceId = "m1", StartTime = T0 } }
    };
    private static CloudPlayoutChannelOverlayRequest OverlayBody => new() { OverlayId = "o1", StartTime = T0, Duration = 60 };
    private static CloudPlayoutSimulcastRequest SimulcastBody => new() { Url = "rtmps://example.com/app/key1" };
    private static Task<ExportCloudPlayoutChannelScheduleResponse> Export(FastpixSDK s) =>
        s.CloudPlayoutScheduleAndEpg.ExportScheduleAsync("c1", ExportCloudPlayoutChannelScheduleType.Schedule, T0, T1);
    private static Task<GetCloudPlayoutChannelEpgResponse> Epg(FastpixSDK s) =>
        s.CloudPlayoutScheduleAndEpg.GetEpgAsync("c1", GetCloudPlayoutChannelEpgType.Schedule, T0, T1);

    private static readonly System.Type[] Resources =
    {
        typeof(CloudPlayoutChannels), typeof(CloudPlayoutLoopPrograms), typeof(CloudPlayoutScheduleChannel),
        typeof(CloudPlayoutChannelOverlays), typeof(CloudPlayoutScheduleAndEpg), typeof(CloudPlayoutSimulcast),
    };

    public static IEnumerable<object[]> ErrorCases() =>
        from op in Ops.Keys from status in new[] { 400, 422, 500 } select new object[] { op, status };

    [Fact]
    public void Ops_CoverEveryCloudPlayoutMethod()
    {
        var declared = Resources.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            .Select(m => $"{t.Name}.{m.Name}")).OrderBy(x => x);
        Assert.Equal(declared, Ops.Keys.OrderBy(x => x));
        Assert.Equal(33, Ops.Count);
    }

    [Theory]
    [MemberData(nameof(ErrorCases))]
    public async Task ErrorStatus_ThrowsApiExceptionFromAwaitedTask(string op, int status)
    {
        var body = ErrorBody(status);
        var task = Ops[op](Sdk(status, body));
        var ex = await Assert.ThrowsAsync<ApiException>(async () => await task);
        Assert.Equal((HttpStatusCode)status, ex.Response.StatusCode);
        Assert.Equal(body, ex.Body);
        var error = ResponseBodyDeserializer.DeserializeNotNull<DefaultError>(ex.Body);
        Assert.False(error.Success);
        Assert.Equal((long?)status, error.Error!.Code);
        Assert.Equal($"failed with {status}", error.Error.Message);
    }

    [Fact]
    public async Task DeleteChannel_404_ThrowsApiException()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => Sdk(404, ErrorBody(404)).CloudPlayoutChannels.DeleteAsync("c1"));
        Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
    }

    [Fact]
    public async Task Activate_409_ThrowsApiException()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => Sdk(409, ErrorBody(409)).CloudPlayoutChannels.ActivateAsync("c1"));
        Assert.Equal(HttpStatusCode.Conflict, ex.Response.StatusCode);
    }

    private const string Envelope = "{\"success\":true,\"data\":{\"id\":\"x1\"}}";

    // The spec's 201 operations; every other operation succeeds with 200.
    private static readonly HashSet<string> Created = new()
    {
        "CloudPlayoutChannels.CreateAsync", "CloudPlayoutLoopPrograms.CreateAsync", "CloudPlayoutLoopPrograms.AddMediaAsync",
        "CloudPlayoutLoopPrograms.AddPlaylistAsync", "CloudPlayoutLoopPrograms.AddBumperAsync",
        "CloudPlayoutScheduleChannel.AddEventAsync", "CloudPlayoutScheduleChannel.AddEventsAsync",
    };

    private static int SuccessCode(string op) => Created.Contains(op) ? 201 : 200;

    public static IEnumerable<object[]> CreatedOps() => Created.Select(op => new object[] { op });

    public static IEnumerable<object[]> AllOps() => Ops.Keys.Select(op => new object[] { op });

    [Theory]
    [MemberData(nameof(CreatedOps))]
    public async Task CreatedOp_200_DoesNotReturnEnvelope(string op)
    {
        var task = Ops[op](Sdk(200, Envelope));
        await task;
        var res = task.GetType().GetProperty("Result")!.GetValue(task)!;

        var payload = res.GetType().GetProperties().Where(p => p.Name is not "HttpMeta" and not "DefaultError").ToList();
        Assert.Single(payload);
        Assert.Null(payload[0].GetValue(res));
        var error = (DefaultError?)res.GetType().GetProperty("DefaultError")!.GetValue(res);
        Assert.True(error!.Success);
    }

    [Theory]
    [MemberData(nameof(AllOps))]
    public async Task SuccessCode_WithHtml_ThrowsUnknownContentType(string op)
    {
        var code = SuccessCode(op);
        var ex = await Assert.ThrowsAsync<ApiException>(async () => await Ops[op](Sdk(code, "<html></html>", "text/html")));
        Assert.Equal((HttpStatusCode)code, ex.Response.StatusCode);
        Assert.StartsWith("Unknown content type received", ex.Message);
    }

    [Fact]
    public async Task AddOverlay_201_DoesNotReturnEnvelope()
    {
        var res = await Sdk(201, Envelope).CloudPlayoutChannelOverlays.AddAsync("c1", OverlayBody);
        Assert.Null(res.CloudPlayoutChannelOverlaySingleResponse);
        Assert.True(res.DefaultError!.Success);
    }

    [Fact]
    public async Task AddSimulcast_201_DoesNotReturnEnvelope()
    {
        var res = await Sdk(201, Envelope).CloudPlayoutSimulcast.CreateAsync("c1", SimulcastBody);
        Assert.Null(res.CloudPlayoutSimulcastCreateResponse);
        Assert.True(res.DefaultError!.Success);
    }

    [Fact]
    public async Task UpdateChannel_201_DoesNotReturnEnvelope()
    {
        var res = await Sdk(201, Envelope).CloudPlayoutChannels.UpdateAsync("c1", new CloudPlayoutChannelUpdateRequest { Name = "Evening" });
        Assert.Null(res.CloudPlayoutChannelResponse);
        Assert.True(res.DefaultError!.Success);
    }

    [Fact]
    public async Task ExportAndEpg_Json400_ThrowApiException()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => Export(Sdk(400, ErrorBody(400))));
        Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
        Assert.Equal(ErrorBody(400), ex.Body);
        var ex2 = await Assert.ThrowsAsync<ApiException>(() => Epg(Sdk(400, ErrorBody(400))));
        Assert.Equal(HttpStatusCode.BadRequest, ex2.Response.StatusCode);
        Assert.Equal(ErrorBody(400), ex2.Body);
    }

    [Fact]
    public async Task ExportAndEpg_200WithJson_ThrowUnknownContentType()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => Export(Sdk(200, Envelope)));
        Assert.Equal(HttpStatusCode.OK, ex.Response.StatusCode);
        Assert.StartsWith("Unknown content type received", ex.Message);
        var ex2 = await Assert.ThrowsAsync<ApiException>(() => Epg(Sdk(200, Envelope)));
        Assert.Equal(HttpStatusCode.OK, ex2.Response.StatusCode);
        Assert.StartsWith("Unknown content type received", ex2.Message);
    }
}

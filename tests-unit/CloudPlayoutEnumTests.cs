using Fastpix.Models.Components;
using Fastpix.Models.Requests;
using Fastpix.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Fastpix.UnitTests;

// Cloud Playout time strings and enums. Request enums are strict and serialize to the spec's
// wire strings (spec values are written out below, not read from the code); response enums are open.
public class CloudPlayoutEnumTests
{
    private const string Start = "2026-11-01T00:00:00";
    private const string End = "2026-11-02T00:00:00";

    // ---- 7.3 time strings ----

    public static IEnumerable<object[]> TimeRequests() => new[]
    {
        new object[] { new CloudPlayoutChannelRequest { Name = "Morning", Type = CloudPlayoutChannelRequestType.Schedule, StartTime = Start, EndTime = End }, true },
        new object[] { new CloudPlayoutScheduleEventCreate { Type = CloudPlayoutScheduleEventCreateType.Live, StartTime = Start, EndTime = End, Name = "Live" }, true },
        new object[] { new CloudPlayoutScheduleEventBatchItem { Type = CloudPlayoutScheduleEventBatchItemType.Live, StartTime = Start, EndTime = End, Name = "Live" }, true },
        new object[] { new CloudPlayoutChannelOverlayRequest { OverlayId = "o1", StartTime = Start }, false },
        new object[] { new CloudPlayoutChannelUpdateRequest { StartTime = Start, EndTime = End }, true },
        new object[] { new CloudPlayoutScheduleEventUpdate { StartTime = Start, EndTime = End }, true },
        new object[] { new CloudPlayoutScheduleEventUpdateRequest { Event = new CloudPlayoutScheduleEventUpdate { StartTime = Start, EndTime = End } }, true },
    };

    // Raw text is checked: no Z, no offset, no milliseconds is added to a request time string.
    [Theory]
    [MemberData(nameof(TimeRequests))]
    public void RequestTimeStrings_SerializeUnchanged(object model, bool hasEnd)
    {
        var json = Utilities.SerializeJSON(model);

        Assert.Contains($"\"startTime\":\"{Start}\"", json);
        if (hasEnd)
        {
            Assert.Contains($"\"endTime\":\"{End}\"", json);
        }
    }

    [Fact]
    public void ResponseTime_WithZ_ParsesToTheUtcInstant()
    {
        var channel = ResponseBodyDeserializer.DeserializeNotNull<CloudPlayoutChannel>(
            "{\"startTime\":\"2026-11-01T00:00:00Z\",\"endTime\":\"2026-11-02T06:30:00Z\"}");

        Assert.Equal(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), channel.StartTime);
        Assert.Equal(DateTimeKind.Utc, channel.StartTime!.Value.Kind);
        Assert.Equal(new DateTime(2026, 11, 2, 6, 30, 0, DateTimeKind.Utc), channel.EndTime);
    }

    // ---- 7.4 request enums ----

    private static void Each<T>(string[] spec, Func<T, string> value, Func<string, T> toEnum, Func<T, string> serialized)
        where T : struct, Enum
    {
        Assert.Equal(spec.Length, Enum.GetValues<T>().Length);
        foreach (var s in spec)
        {
            var member = toEnum(s);
            Assert.Equal(s, value(member));
            Assert.Equal(s, serialized(member));
        }
        Assert.Throws<ArgumentException>(() => toEnum("bogus"));
    }

    private static string Field(object model, string key) =>
        (string)JObject.Parse(Utilities.SerializeJSON(model))[key]!;

    private static string Query(object request, string path = "/cloud-playout/channels/{channelId}/x") =>
        UrlBuilder.Build("https://h", path, request).Split('?')[1].Split('&').Single(p => p.StartsWith("type=")).Substring(5);

    private static CloudPlayoutChannelRequest Channel() =>
        new() { Name = "Morning", Type = CloudPlayoutChannelRequestType.Loop, StartTime = Start, EndTime = End };

    [Fact]
    public void ChannelRequestType_WritesWireValue() =>
        Each<CloudPlayoutChannelRequestType>(new[] { "loop", "schedule" },
            v => v.Value(), CloudPlayoutChannelRequestTypeExtension.ToEnum,
            v => { var c = Channel(); c.Type = v; return Field(c, "type"); });

    [Fact]
    public void ChannelRequestDestination_WritesWireValue() =>
        Each<CloudPlayoutChannelRequestDestination>(new[] { "fastpixlive" },
            v => v.Value(), CloudPlayoutChannelRequestDestinationExtension.ToEnum,
            v => { var c = Channel(); c.Destination = v; return Field(c, "destination"); });

    [Fact]
    public void ChannelRequestGenres_AllTwentyWriteWireValue() =>
        Each<CloudPlayoutChannelRequestGenresItem>(
            new[]
            {
                "news", "sports", "entertainment", "music", "movies", "documentary", "educational", "kids", "lifestyle", "religious",
                "business", "technology", "gaming", "fashion", "health", "food", "travel", "comedy", "drama", "reality",
            },
            v => v.Value(), CloudPlayoutChannelRequestGenresItemExtension.ToEnum,
            v =>
            {
                var c = Channel();
                c.Genres = new List<CloudPlayoutChannelRequestGenresItem> { v };
                return (string)JObject.Parse(Utilities.SerializeJSON(c))["genres"]![0]!;
            });

    [Fact]
    public void BumperRequestBumperType_WritesWireValue() =>
        Each<CloudPlayoutProgramBumperRequestBumperType>(new[] { "preroll", "postroll" },
            v => v.Value(), CloudPlayoutProgramBumperRequestBumperTypeExtension.ToEnum,
            v => Field(new CloudPlayoutProgramBumperRequest { BumperId = "b1", EventId = "e1", BumperType = v }, "bumperType"));

    [Fact]
    public void ScheduleEventCreateType_WritesWireValue() =>
        Each<CloudPlayoutScheduleEventCreateType>(new[] { "vod", "playlist", "live" },
            v => v.Value(), CloudPlayoutScheduleEventCreateTypeExtension.ToEnum,
            v => Field(new CloudPlayoutScheduleEventCreate { Type = v, StartTime = Start }, "type"));

    [Fact]
    public void ScheduleEventBatchItemType_WritesWireValue() =>
        Each<CloudPlayoutScheduleEventBatchItemType>(new[] { "vod", "live" },
            v => v.Value(), CloudPlayoutScheduleEventBatchItemTypeExtension.ToEnum,
            v => Field(new CloudPlayoutScheduleEventBatchItem { Type = v, StartTime = Start }, "type"));

    [Fact]
    public void BatchItemType_HasNoPlaylist_CreateTypeDoes()
    {
        Assert.DoesNotContain("Playlist", Enum.GetNames<CloudPlayoutScheduleEventBatchItemType>());
        Assert.Contains("Playlist", Enum.GetNames<CloudPlayoutScheduleEventCreateType>());
        Assert.Throws<ArgumentException>(() => CloudPlayoutScheduleEventBatchItemTypeExtension.ToEnum("playlist"));
        Assert.Equal(CloudPlayoutScheduleEventCreateType.Playlist, CloudPlayoutScheduleEventCreateTypeExtension.ToEnum("playlist"));
    }

    [Fact]
    public void ListChannelsType_WritesWireValue() =>
        Each<ListCloudPlayoutChannelsType>(new[] { "loop", "schedule", "all" },
            v => v.Value(), ListCloudPlayoutChannelsTypeExtension.ToEnum,
            v => Query(new ListCloudPlayoutChannelsRequest { Type = v }, "/cloud-playout/channels"));

    [Fact]
    public void GetScheduleType_WritesWireValue() =>
        Each<GetCloudPlayoutChannelScheduleType>(new[] { "loop", "schedule" },
            v => v.Value(), GetCloudPlayoutChannelScheduleTypeExtension.ToEnum,
            v => Query(new GetCloudPlayoutChannelScheduleRequest { ChannelId = "c1", Type = v }));

    [Fact]
    public void ExportScheduleType_WritesWireValue() =>
        Each<ExportCloudPlayoutChannelScheduleType>(new[] { "loop", "schedule" },
            v => v.Value(), ExportCloudPlayoutChannelScheduleTypeExtension.ToEnum,
            v => Query(new ExportCloudPlayoutChannelScheduleRequest { ChannelId = "c1", Type = v, StartTime = Start, EndTime = End }));

    [Fact]
    public void GetEpgType_WritesWireValue() =>
        Each<GetCloudPlayoutChannelEpgType>(new[] { "loop", "schedule" },
            v => v.Value(), GetCloudPlayoutChannelEpgTypeExtension.ToEnum,
            v => Query(new GetCloudPlayoutChannelEpgRequest { ChannelId = "c1", Type = v }));

    // ---- 7.4 response open enums ----

    [Fact]
    public void ChannelStatus_UnknownValue_StaysRaw_AndSiblingsParse()
    {
        var channel = ResponseBodyDeserializer.DeserializeNotNull<CloudPlayoutChannel>(
            "{\"id\":\"c1\",\"name\":\"Morning\",\"status\":\"paused\",\"type\":\"loop\",\"startTime\":\"2026-11-01T00:00:00Z\"}");

        Assert.Equal("paused", channel.Status!.Value);
        Assert.False(channel.Status.IsKnown);
        Assert.Equal("c1", channel.Id);
        Assert.Equal("Morning", channel.Name);
        Assert.Equal(CloudPlayoutChannelType.Loop, channel.Type);
        Assert.Equal(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), channel.StartTime);
    }

    [Fact]
    public void ChannelStatus_KnownValue_ResolvesToMember()
    {
        var channel = ResponseBodyDeserializer.DeserializeNotNull<CloudPlayoutChannel>("{\"status\":\"running\"}");

        Assert.Equal(CloudPlayoutChannelStatus.Running, channel.Status);
        Assert.True(channel.Status!.IsKnown);
    }

    // Each response open enum with its value list hard-coded from openapi.yaml.
    public static IEnumerable<object[]> OpenEnumSpecValues() => new[]
    {
        new object[] { typeof(CloudPlayoutChannelStatus), new[] { "draft", "scheduled", "starting", "running", "stopping", "idle", "completed", "deleting", "error" } },
        new object[] { typeof(CloudPlayoutChannelType), new[] { "loop", "schedule" } },
        new object[] { typeof(CloudPlayoutChannelOverlayType), new[] { "static", "dynamic" } },
        new object[] { typeof(CloudPlayoutProgramMediaType), new[] { "vod", "live", "bumper" } },
        new object[] { typeof(CloudPlayoutScheduleEventType), new[] { "vod", "live" } },
        new object[] { typeof(CloudPlayoutScheduleItemType), new[] { "vod", "live", "bumper" } },
    };

    [Theory]
    [MemberData(nameof(OpenEnumSpecValues))]
    public void ResponseOpenEnum_KnowsEverySpecValue(System.Type type, string[] spec)
    {
        var of = type.GetMethod("Of")!;
        foreach (var v in spec)
        {
            var member = of.Invoke(null, new object[] { v })!;
            Assert.True((bool)type.GetProperty("IsKnown")!.GetValue(member)!, $"{type.Name}: \"{v}\" is not a known member");
        }
        var known = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Count(f => f.FieldType == type);
        Assert.Equal(spec.Length, known);
    }

    public static IEnumerable<object[]> TypedModels() => new[]
    {
        typeof(CloudPlayoutChannel), typeof(CloudPlayoutChannelOverlay), typeof(CloudPlayoutProgramMedia),
        typeof(CloudPlayoutScheduleEvent), typeof(CloudPlayoutScheduleItem),
    }.Select(t => new object[] { t });

    // CloudPlayoutChannelType, ...OverlayType, ...ProgramMediaType, ...ScheduleEventType, ...ScheduleItemType
    [Theory]
    [MemberData(nameof(TypedModels))]
    public void ResponseTypeEnum_UnknownValue_StaysRaw_AndSiblingsParse(System.Type model)
    {
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject("{\"name\":\"n1\",\"type\":\"zzz-new\"}", model)!;
        var type = model.GetProperty("Type")!.GetValue(obj)!;

        Assert.Equal("zzz-new", (string)type.GetType().GetProperty("Value")!.GetValue(type)!);
        Assert.False((bool)type.GetType().GetProperty("IsKnown")!.GetValue(type)!);
        Assert.Equal("n1", (string)model.GetProperty("Name")!.GetValue(obj)!);
    }
}

using System.Reflection;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Fastpix.UnitTests;

// Cloud Playout has three different bumper shapes on the wire (segment, schedule item, schedule event);
// each must parse into its own type and none may be merged with another.
public class CloudPlayoutBumperTests
{
    private static JToken Parse(string json) =>
        JsonConvert.DeserializeObject<JToken>(json, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None })!;

    [Fact]
    public void SegmentBumper_PrerollOnly_ExposesPrerollAndNullPostroll()
    {
        var segment = ResponseBodyDeserializer.Deserialize<CloudPlayoutSegment>(
            """{"bumper":{"preroll":{"id":"b1","name":"Intro","duration":5,"durationMillis":5000}}}""")!;

        Assert.IsType<CloudPlayoutSegmentBumper>(segment.Bumper);
        var preroll = Assert.IsType<CloudPlayoutSegmentRoll>(segment.Bumper!.Preroll);
        Assert.Equal("b1", preroll.Id);
        Assert.Equal("Intro", preroll.Name);
        Assert.Equal(5L, preroll.Duration);
        Assert.Equal(5000L, preroll.DurationMillis);
        Assert.Null(preroll.Thumbnail);
        Assert.Null(segment.Bumper.Postroll);
    }

    [Fact]
    public void SegmentBumper_SpecExample_ParsesEveryRollField()
    {
        var segment = ResponseBodyDeserializer.Deserialize<CloudPlayoutSegment>(
            """
            {"bumper":{"postroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5,"durationMillis":5040,
            "thumbnail":"https://images.fastpix.com/9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88/thumbnail.png"}}}
            """)!;

        Assert.Null(segment.Bumper!.Preroll);
        var postroll = segment.Bumper.Postroll!;
        Assert.Equal("9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88", postroll.Id);
        Assert.Equal("Ad break intro", postroll.Name);
        Assert.Equal(5L, postroll.Duration);
        Assert.Equal(5040L, postroll.DurationMillis);
        Assert.Equal("https://images.fastpix.com/9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88/thumbnail.png", postroll.Thumbnail);
    }

    [Fact]
    public void ScheduleItemBumper_PostrollOnly_IdIsMediaAndBumperIdIsBumper()
    {
        var item = ResponseBodyDeserializer.Deserialize<CloudPlayoutScheduleItem>(
            """{"bumper":{"postroll":{"id":"m9","bumperId":"b1","name":"Outro","duration":5}}}""")!;

        Assert.IsType<CloudPlayoutScheduleItemBumper>(item.Bumper);
        Assert.Null(item.Bumper!.Preroll);
        var postroll = Assert.IsType<CloudPlayoutScheduleItemRoll>(item.Bumper.Postroll);
        Assert.Equal("m9", postroll.Id);
        Assert.Equal("b1", postroll.BumperId);
        Assert.Equal("Outro", postroll.Name);
        Assert.Equal(5L, postroll.Duration);
    }

    [Fact]
    public void ScheduleItemBumper_SpecExample_KeepsMediaAndBumperIdsApart()
    {
        var bumper = ResponseBodyDeserializer.Deserialize<CloudPlayoutScheduleItemBumper>(
            """
            {"preroll":{"id":"1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88",
            "name":"Ad break intro","duration":5}}
            """)!;

        Assert.Equal("1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310", bumper.Preroll!.Id);
        Assert.Equal("9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88", bumper.Preroll.BumperId);
        Assert.NotEqual(bumper.Preroll.Id, bumper.Preroll.BumperId);
    }

    [Fact]
    public void ScheduleEventBumper_SpecExample_ParsesIntoEventRollResponse()
    {
        var ev = ResponseBodyDeserializer.Deserialize<CloudPlayoutScheduleEvent>(
            """
            {"id":"b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64","bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88",
            "bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}}}
            """)!;

        Assert.IsType<CloudPlayoutScheduleEventBumper>(ev.Bumper);
        Assert.Null(ev.Bumper!.Postroll);
        var preroll = Assert.IsType<CloudPlayoutScheduleEventRollResponse>(ev.Bumper.Preroll);
        Assert.Equal("9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88", preroll.Id);
        Assert.Equal("9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88", preroll.BumperId);
        Assert.Equal(5L, preroll.Duration);
    }

    [Fact]
    public void EventUpdateBumperRequest_SerializesOnlyIdAndDuration_UnsetFieldsAbsent()
    {
        var body = new CloudPlayoutScheduleEventBumperRequest
        {
            Preroll = new CloudPlayoutScheduleEventRoll { Id = "b1", Duration = 5 },
        };

        Assert.True(JToken.DeepEquals(Parse("""{"preroll":{"id":"b1","duration":5}}"""), Parse(Utilities.SerializeJSON(body))));
        Assert.Equal("{}", Utilities.SerializeJSON(new CloudPlayoutScheduleEventBumperRequest()));
    }

    [Fact]
    public void EventUpdateBumperRequest_BothRolls_SerializeBothKeys()
    {
        var body = new CloudPlayoutScheduleEventBumperRequest
        {
            Preroll = new CloudPlayoutScheduleEventRoll { Id = "b1", Duration = 5 },
            Postroll = new CloudPlayoutScheduleEventRoll { Id = "b2", Duration = 7 },
        };

        Assert.True(JToken.DeepEquals(
            Parse("""{"preroll":{"id":"b1","duration":5},"postroll":{"id":"b2","duration":7}}"""),
            Parse(Utilities.SerializeJSON(body))));
    }

    [Fact]
    public void ThreeResponseRollTypes_AreDistinctClrTypes()
    {
        var types = new[] { typeof(CloudPlayoutSegmentRoll), typeof(CloudPlayoutScheduleItemRoll), typeof(CloudPlayoutScheduleEventRollResponse) };
        Assert.Equal(3, types.Distinct().Count());

        Assert.Equal(typeof(CloudPlayoutSegmentRoll), typeof(CloudPlayoutSegmentBumper).GetProperty("Preroll")!.PropertyType);
        Assert.Equal(typeof(CloudPlayoutScheduleItemRoll), typeof(CloudPlayoutScheduleItemBumper).GetProperty("Preroll")!.PropertyType);
        Assert.Equal(typeof(CloudPlayoutScheduleEventRollResponse), typeof(CloudPlayoutScheduleEventBumper).GetProperty("Preroll")!.PropertyType);
    }

    // Property sets from openapi.yaml components.schemas, hard-coded on purpose.
    public static IEnumerable<object[]> SchemaKeys => new[]
    {
        new object[] { typeof(CloudPlayoutSegmentRoll), new[] { "id", "name", "duration", "durationMillis", "thumbnail" } },
        new object[] { typeof(CloudPlayoutScheduleItemRoll), new[] { "id", "bumperId", "name", "duration" } },
        new object[] { typeof(CloudPlayoutScheduleEventRollResponse), new[] { "id", "bumperId", "duration" } },
        new object[] { typeof(CloudPlayoutScheduleEventRoll), new[] { "id", "duration" } },
        new object[] { typeof(CloudPlayoutSegmentBumper), new[] { "preroll", "postroll" } },
        new object[] { typeof(CloudPlayoutScheduleItemBumper), new[] { "preroll", "postroll" } },
        new object[] { typeof(CloudPlayoutScheduleEventBumper), new[] { "preroll", "postroll" } },
        new object[] { typeof(CloudPlayoutScheduleEventBumperRequest), new[] { "preroll", "postroll" } },
    };

    [Theory]
    [MemberData(nameof(SchemaKeys))]
    public void BumperType_JsonPropertyNames_MatchSpecSchemaKeys(Type type, string[] specKeys)
    {
        var actual = type.GetProperties()
            .Select(p => p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName)
            .ToList();

        Assert.All(actual, n => Assert.NotNull(n));
        Assert.Equal(specKeys.OrderBy(k => k), actual.OrderBy(k => k));
    }
}

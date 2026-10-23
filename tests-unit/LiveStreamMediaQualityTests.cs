using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Fastpix.UnitTests;

// Live stream responses carry an optional open-enum mediaQuality: unknown values are
// kept raw, an absent field stays null and is not written back.
public class LiveStreamMediaQualityTests
{
    public static IEnumerable<object[]> Dtos() => new[]
    {
        new object[] { typeof(CreateLiveStreamResponseDto) },
        new object[] { typeof(GetCreateLiveStreamResponseDto) },
        new object[] { typeof(PatchResponseData) },
    };

    private static object? Read(System.Type t, string json) => JsonConvert.DeserializeObject(json, t);

    private static object? Quality(System.Type t, object dto) => t.GetProperty("MediaQuality")!.GetValue(dto);

    private static object Prop(object o, string name) => o.GetType().GetProperty(name)!.GetValue(o)!;

    [Theory]
    [MemberData(nameof(Dtos))]
    public void KnownValue_ResolvesToMember(System.Type t)
    {
        var q = Quality(t, Read(t, "{\"mediaQuality\":\"standard\"}")!)!;
        Assert.True((bool)Prop(q, "IsKnown"));
        Assert.Equal("standard", (string)Prop(q, "Value"));
    }

    [Theory]
    [MemberData(nameof(Dtos))]
    public void UnknownValue_StaysRaw(System.Type t)
    {
        var q = Quality(t, Read(t, "{\"mediaQuality\":\"ultra\"}")!)!;
        Assert.False((bool)Prop(q, "IsKnown"));
        Assert.Equal("ultra", (string)Prop(q, "Value"));
    }

    [Theory]
    [MemberData(nameof(Dtos))]
    public void AbsentValue_IsNull(System.Type t)
    {
        Assert.Null(Quality(t, Read(t, "{}")!));
    }

    [Theory]
    [MemberData(nameof(Dtos))]
    public void Reserialize_WritesValue_AndOmitsKeyWhenNull(System.Type t)
    {
        var set = JObject.Parse(Utilities.SerializeJSON(Read(t, "{\"mediaQuality\":\"standard\"}")!));
        Assert.Equal("standard", (string)set["mediaQuality"]!);

        var unset = JObject.Parse(Utilities.SerializeJSON(Read(t, "{}")!));
        Assert.Null(unset["mediaQuality"]);
    }
}

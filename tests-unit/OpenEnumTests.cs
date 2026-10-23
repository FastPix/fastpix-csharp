using System.Reflection;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Fastpix.UnitTests;

// Response-side enums are open: an unknown server value is preserved as the raw
// string instead of failing the whole response. Request-side enums stay strict.
public class OpenEnumTests
{
    [Fact]
    public void UnknownResponseValue_IsPreserved_AndSiblingsSurvive()
    {
        var media = JsonConvert.DeserializeObject<Media>(
            "{\"sourceResolution\":\"1920\",\"status\":\"Ready\"}")!;

        Assert.Equal("1920", media.SourceResolution!.Value);
        Assert.False(media.SourceResolution.IsKnown);

        // the unknown value did not discard the rest of the payload
        Assert.Equal(MediaStatus.Ready, media.Status);
        Assert.True(media.Status!.IsKnown);
    }

    [Fact]
    public void KnownResponseValue_ResolvesToMember()
    {
        var media = JsonConvert.DeserializeObject<Media>("{\"sourceResolution\":\"1080p\"}")!;

        Assert.Equal(MediaSourceResolution.OneThousandAndEightyp, media.SourceResolution);
        Assert.True(media.SourceResolution!.IsKnown);
        Assert.Equal("1080p", media.SourceResolution.Value);
    }

    [Fact]
    public void UnknownValue_RoundTripsBackToSameString()
    {
        var media = JsonConvert.DeserializeObject<Media>("{\"sourceResolution\":\"1920\"}")!;
        var json = JObject.Parse(Utilities.SerializeJSON(media));
        Assert.Equal("1920", (string)json["sourceResolution"]!);
    }

    [Fact]
    public void AbsentOrNullEnum_IsNull_AndNotSerialized()
    {
        // status has no default, unlike the resolution fields
        Assert.Null(JsonConvert.DeserializeObject<Media>("{}")!.Status);
        Assert.Null(JsonConvert.DeserializeObject<Media>("{\"status\":null}")!.Status);

        var media = JsonConvert.DeserializeObject<Media>("{}")!;
        var json = JObject.Parse(Utilities.SerializeJSON(media));
        Assert.Null(json["status"]); // NullValueHandling.Ignore, not the string "null"
    }

    [Fact]
    public void RequestOnlyEnum_StaysStrict()
    {
        // SortOrder / MediaType are request-only and must still reject unknown values.
        Assert.True(typeof(SortOrder).IsEnum);
        Assert.Throws<System.ArgumentException>(() => SortOrderExtension.ToEnum("bogus"));
    }

    // Broad guard: every open enum in the assembly behaves per the spec. If a future
    // regeneration reverts one to a strict enum, OpenEnums() drops below the floor
    // and this fails.
    public static IEnumerable<object[]> OpenEnums() =>
        typeof(FastpixSDK).Assembly.GetTypes()
            .Where(t => t.IsClass
                        && t.GetMethod("Of", new[] { typeof(string) }) != null
                        && t.GetProperty("Value")?.PropertyType == typeof(string)
                        && t.GetProperty("IsKnown") != null)
            .Select(t => new object[] { t });

    [Fact]
    public void AllExpectedResponseEnums_AreOpen()
    {
        Assert.True(OpenEnums().Count() >= 57,
            $"expected at least 57 open enums, found {OpenEnums().Count()}");
    }

    [Theory]
    [MemberData(nameof(OpenEnums))]
    public void EveryOpenEnum_PreservesUnknown_AndRecognisesKnown(System.Type t)
    {
        var known = t.GetFields(BindingFlags.Public | BindingFlags.Static)
            .First(f => f.FieldType == t).GetValue(null)!;
        var of = t.GetMethod("Of", new[] { typeof(string) })!;
        string KnownVal() => (string)t.GetProperty("Value")!.GetValue(known)!;

        var recognised = of.Invoke(null, new object[] { KnownVal() })!;
        Assert.True((bool)t.GetProperty("IsKnown")!.GetValue(recognised)!);

        var unknown = of.Invoke(null, new object[] { "zzz-not-a-real-value" })!;
        Assert.False((bool)t.GetProperty("IsKnown")!.GetValue(unknown)!);
        Assert.Equal("zzz-not-a-real-value", (string)t.GetProperty("Value")!.GetValue(unknown)!);
    }
}

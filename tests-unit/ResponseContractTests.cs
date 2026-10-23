using System.Text.RegularExpressions;
using Xunit;

namespace Fastpix.UnitTests;

// Guards the public surface: every resource method's declared response wrapper
// must carry a property of the exact type the method deserializes on 2xx.
// Reads src/Fastpix/*.cs for the deserialized type and reflection for the wrapper.
public class ResponseContractTests
{
    private static readonly Regex MethodRx = new(
        @"public async Task<(?:Models\.Requests\.)?(\w+)> (\w+)\((.*?)\)\s*\{(.*?)\n        \}\n", RegexOptions.Singleline);
    private static readonly Regex DeserRx = new(@"(?:DeserializeOrThrow|DeserializeNotNull|DeserializeBody)<(\w+)>\(");

    private static string SrcDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "Fastpix"))) dir = dir.Parent;
        return Path.Combine(dir!.FullName, "src", "Fastpix");
    }

    public static IEnumerable<object[]> Methods()
    {
        foreach (var file in Directory.GetFiles(SrcDir(), "*.cs"))
        {
            var src = File.ReadAllText(file);
            foreach (Match m in MethodRx.Matches(src))
            {
                var deserialized = DeserRx.Matches(m.Groups[4].Value).Select(d => d.Groups[1].Value).Where(t => t != "DefaultError").Distinct().ToList();
                if (deserialized.Count == 0) continue; // no JSON payload (e.g. 204 delete)
                yield return new object[] { Path.GetFileNameWithoutExtension(file), m.Groups[2].Value, m.Groups[1].Value, deserialized.Single() };
            }
        }
    }

    // Text responses deserialize nothing but DefaultError, so the scan skips them; checked by name instead.
    public static IEnumerable<object[]> TextMethods() => new[]
    {
        new object[] { "CloudPlayoutScheduleAndEpg", "ExportScheduleAsync", "Csv" },
        new object[] { "CloudPlayoutScheduleAndEpg", "GetEpgAsync", "Xml" },
    };

    private static Type? DeclaredWrapper(string resource, string method) =>
        typeof(FastpixSDK).Assembly.GetType("Fastpix." + resource)?.GetMethod(method)?.ReturnType is { IsGenericType: true } t
            && t.GetGenericTypeDefinition() == typeof(Task<>) ? t.GetGenericArguments()[0] : null;

    [Fact]
    public void Scan_FindsEveryResourceMethod()
    {
        var text = TextMethods().Count(r => DeclaredWrapper((string)r[0], (string)r[1]) != null);
        Assert.Equal(2, text);
        var total = Methods().Count() + text;
        Assert.True(total >= 101, $"expected at least 101 methods, scan found {total}");
    }

    [Theory]
    [MemberData(nameof(TextMethods))]
    public void TextWrapper_CarriesStringPayload(string resource, string method, string property)
    {
        var wrapper = DeclaredWrapper(resource, method);
        Assert.True(wrapper != null, $"{resource}.{method}: method or Task<> wrapper not found");
        var prop = wrapper!.GetProperty(property);
        Assert.True(prop?.PropertyType == typeof(string), $"{resource}.{method}: Task<{wrapper.Name}> has no {property} property of type string");
    }

    [Theory]
    [MemberData(nameof(Methods))]
    public void DeclaredWrapper_CarriesDeserializedType(string resource, string method, string wrapper, string deserialized)
    {
        var asm = typeof(FastpixSDK).Assembly;
        var wrapperType = asm.GetType("Fastpix.Models.Requests." + wrapper);
        Assert.True(wrapperType != null, $"{resource}.{method}: wrapper {wrapper} not found");
        var carried = wrapperType!.GetProperties().Select(p => p.PropertyType.Name);
        Assert.True(carried.Contains(deserialized), $"{resource}.{method}: Task<{wrapper}> has no property of type {deserialized} (has {string.Join(", ", carried)})");
    }
}

#!/usr/bin/env dotnet
#:property NoWarn=$(NoWarn);CA1305
// Generates the SDK's model classes from the Management API OpenAPI snapshot.
//
//   dotnet run tools/GenerateModels.cs
//
// Reads spec/management-api.openapi.json and spec/enum-names.json and rewrites
// src/Agility.Management.Sdk/Models/Generated/. Hand-written additions go in partial classes
// under src/Agility.Management.Sdk/Models/, never in the generated files.
//
// Serialization rules that the SDK's contract depends on (see docs/concepts.md):
// - every reference-typed property is nullable, because the API may omit or null it;
// - collections and dictionaries are omitted from request bodies when null. The API reads a sent
//   list as a replacement (e.g. a zone's defaultModules, where [] clears), so "not set" must never
//   be written as [] or null;
// - explicit [JsonPropertyName] keeps the wire names exact (abortYN, relativeURL, eTag...).

using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

var root = FindRepoRoot();
var spec = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "spec", "management-api.openapi.json")))!;
var enumNames = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "spec", "enum-names.json")))!.AsObject();
var outDir = Path.Combine(root, "src", "Agility.Management.Sdk", "Models", "Generated");

var schemas = spec["components"]!["schemas"]!.AsObject();
var files = new SortedDictionary<string, string>(StringComparer.Ordinal);

foreach (var (name, node) in schemas)
{
    var schema = node!.AsObject();
    files[name] = schema["enum"] is JsonArray values
        ? GenerateEnum(name, schema, values)
        : GenerateClass(name, schema);
}

if (Directory.Exists(outDir))
    foreach (var f in Directory.GetFiles(outDir, "*.g.cs")) File.Delete(f);
Directory.CreateDirectory(outDir);
foreach (var (name, text) in files)
    File.WriteAllText(Path.Combine(outDir, name + ".g.cs"), text.ReplaceLineEndings("\n"));

var contextPath = Path.Combine(root, "src", "Agility.Management.Sdk", "Serialization", "ManagementJsonContext.g.cs");
File.WriteAllText(contextPath, GenerateContextRegistrations(files.Keys).ReplaceLineEndings("\n"));
Console.WriteLine($"Generated {files.Count} models into {Path.GetRelativePath(root, outDir)}");

string GenerateEnum(string name, JsonObject schema, JsonArray values)
{
    var sb = Header();
    sb.AppendLine(Doc(Describe(schema, $"The API's <c>{name}</c> values."), ""));
    if (schema["type"]?.GetValue<string>() == "string")
    {
        // String enums travel by name. The converter keeps the exact wire value.
        sb.AppendLine($"[JsonConverter(typeof(JsonStringEnumConverter<{name}>))]");
        sb.AppendLine($"public enum {name}\n{{");
        foreach (var v in values)
        {
            var wire = v!.GetValue<string>();
            sb.AppendLine($"    /// <summary><c>{wire}</c></summary>");
            sb.AppendLine($"    [JsonStringEnumMemberName(\"{wire}\")]");
            sb.AppendLine($"    {Pascal(wire)},");
        }
        sb.AppendLine("}\n");
        sb.AppendLine($"internal static class {name}Extensions\n{{");
        sb.AppendLine($"    /// <summary>The value the API expects in a query string.</summary>");
        sb.AppendLine($"    public static string ToApiValue(this {name} value) => value switch\n    {{");
        foreach (var v in values)
            sb.AppendLine($"        {name}.{Pascal(v!.GetValue<string>())} => \"{v.GetValue<string>()}\",");
        sb.AppendLine($"        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),\n    }};\n}}");
        return sb.ToString();
    }

    var names = enumNames[name]?.AsObject()
        ?? throw new InvalidOperationException($"spec/enum-names.json has no entry for integer enum {name}");
    var byValue = names.ToDictionary(kv => kv.Value!.GetValue<int>(), kv => kv.Key);
    sb.AppendLine($"public enum {name}\n{{");
    foreach (var v in values.Select(v => v!.GetValue<int>()).OrderBy(v => v))
    {
        if (!byValue.TryGetValue(v, out var member))
            throw new InvalidOperationException($"spec/enum-names.json has no name for {name} value {v}");
        sb.AppendLine($"    /// <summary><c>{v}</c></summary>");
        sb.AppendLine($"    {member} = {v},");
    }
    sb.AppendLine("}");
    return sb.ToString();
}

string GenerateClass(string name, JsonObject schema)
{
    var sb = Header();
    var required = (schema["required"] as JsonArray)?.Select(r => r!.GetValue<string>()).ToHashSet() ?? [];
    sb.AppendLine(Doc(Describe(schema, $"The API's <c>{name}</c> schema."), ""));
    sb.AppendLine($"public sealed partial class {name}\n{{");
    var props = schema["properties"]?.AsObject() ?? new JsonObject();
    var first = true;
    foreach (var (wire, pnode) in props)
    {
        var p = pnode!.AsObject();
        var (type, isValueType, isCollection) = MapType(p);
        var nullable = !isValueType || p["nullable"]?.GetValue<bool>() == true;
        var clr = Pascal(wire);
        if (clr == name) clr += "Value";

        if (!first) sb.AppendLine();
        first = false;
        var desc = p["description"]?.GetValue<string>();
        var notes = new List<string>();
        if (p["readOnly"]?.GetValue<bool>() == true) notes.Add("Read-only: returned by the API and ignored on save.");
        if (isCollection) notes.Add("Left out of the request when <see langword=\"null\"/>.");
        if (desc is not null) notes.Insert(0, Escape(desc));
        if (notes.Count > 0)
            sb.AppendLine(Doc(string.Join(" ", notes), "    "));
        sb.AppendLine($"    [JsonPropertyName(\"{wire}\")]");
        if (isCollection)
            sb.AppendLine("    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]");
        if (required.Contains(wire))
            sb.AppendLine("    [JsonRequired]");
        sb.AppendLine($"    public {type}{(nullable ? "?" : "")} {clr} {{ get; set; }}");
    }
    if (schema["additionalProperties"] is JsonObject)
    {
        if (!first) sb.AppendLine();
        sb.AppendLine("    /// <summary>Any other members the API returns.</summary>");
        sb.AppendLine("    [JsonExtensionData]");
        sb.AppendLine("    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }");
    }
    sb.AppendLine("}");
    return sb.ToString();
}

(string Type, bool IsValueType, bool IsCollection) MapType(JsonObject p)
{
    if (p["$ref"] is JsonNode r)
    {
        var target = r.GetValue<string>().Split('/')[^1];
        var isEnum = schemas[target]!["enum"] is not null;
        return (target, isEnum, false);
    }
    var type = p["type"]?.GetValue<string>();
    var format = p["format"]?.GetValue<string>();
    switch (type)
    {
        case "integer": return (format == "int64" ? "long" : "int", true, false);
        case "number": return (format == "float" ? "float" : "double", true, false);
        case "boolean": return ("bool", true, false);
        case "string":
            return format switch
            {
                "date-time" => ("DateTime", true, false),
                "uuid" => ("Guid", true, false),
                _ => ("string", false, false),
            };
        case "array":
            var item = p["items"] as JsonObject ?? new JsonObject();
            var (it, _, _) = MapType(item);
            return ($"List<{it}>", false, true);
        case "object":
            if (p["additionalProperties"] is JsonObject ap && ap.Count > 0)
            {
                var (vt, _, _) = MapType(ap);
                return ($"Dictionary<string, {vt}>", false, true);
            }
            // Free-form object, e.g. a content item's fields: keep it as JSON.
            return ("JsonObject", false, true);
        default:
            return ("JsonNode", false, false);
    }
}

string GenerateContextRegistrations(IEnumerable<string> names)
{
    var sb = Header("Agility.Management.Sdk.Serialization");
    sb.Insert(sb.ToString().IndexOf("namespace", StringComparison.Ordinal), "using Agility.Management.Sdk.Models;\n\n");
    sb.AppendLine("""
        /// <summary>
        /// Source-generated serialization for every type the SDK sends or receives.
        /// </summary>
        /// <remarks>
        /// Null properties are written, as the 1.x SDK did, except collections, which the generated models
        /// mark <c>WhenWritingNull</c>: the API treats a sent list as a replacement, so an unset list must be
        /// left out rather than sent as <c>null</c> or <c>[]</c>.
        /// </remarks>
        [JsonSourceGenerationOptions(
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
        """);
    // Types the clients use that aren't a schema of their own.
    string[] extras =
    [
        "int?", "string", "JsonNode",
        "List<AssetMedia>", "List<ContentContainer>", "List<ContentItem>", "List<ContentModel>",
        "List<ContentSectionDefinition>", "List<InstanceRole>", "List<Locale>", "List<Notification>",
        "List<PageModel>", "List<Sitemap>", "List<string>", "List<UrlRedirection>", "List<WebsiteUser>",
    ];
    foreach (var n in extras.Concat(names))
        sb.AppendLine($"[JsonSerializable(typeof({n}))]");
    sb.AppendLine("internal sealed partial class ManagementJsonContext : JsonSerializerContext;");
    return sb.ToString();
}

StringBuilder Header(string ns = "Agility.Management.Sdk.Models")
{
    var sb = new StringBuilder();
    sb.AppendLine("// <auto-generated>");
    sb.AppendLine("// Generated by tools/GenerateModels.cs from spec/management-api.openapi.json. Do not edit.");
    sb.AppendLine("// </auto-generated>");
    sb.AppendLine("#nullable enable");
    // The spec describes few properties; the ones it does describe are documented.
    sb.AppendLine("#pragma warning disable CS1591");
    sb.AppendLine("using System.Text.Json;");
    sb.AppendLine("using System.Text.Json.Nodes;");
    sb.AppendLine("using System.Text.Json.Serialization;");
    sb.AppendLine();
    sb.AppendLine("namespace " + ns + ";");
    sb.AppendLine();
    return sb;
}

static string Describe(JsonObject schema, string fallback) =>
    schema["description"]?.GetValue<string>() is string d ? Escape(d) : fallback;

static string Escape(string text) => System.Security.SecurityElement.Escape(text);

// text is XML doc content: callers escape any API-supplied text first.
static string Doc(string text, string indent)
{
    var lines = text.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);
    return string.Join("\n", lines.Select(l => indent + "/// " + l)
        .Prepend(indent + "/// <summary>")
        .Append(indent + "/// </summary>"));
}

static string Pascal(string s)
{
    var parts = Regex.Split(s, "[^A-Za-z0-9]+").Where(p => p.Length > 0);
    return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
}

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "spec"))) dir = dir.Parent;
    return dir?.FullName ?? throw new InvalidOperationException("Run from inside the repository.");
}

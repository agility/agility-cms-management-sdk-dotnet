using System.Globalization;
using System.Text;

namespace Agility.Management.Sdk.Http;

/// <summary>
/// Query-string parameters. A null value leaves the parameter out, so the API applies its own default.
/// Values are escaped and formatted invariantly.
/// </summary>
internal sealed class Query
{
    private readonly List<KeyValuePair<string, string>> _values = [];

    public Query Add(string name, string? value)
    {
        if (value is not null) _values.Add(new(name, value));
        return this;
    }

    public Query Add(string name, int? value) => Add(name, value?.ToString(CultureInfo.InvariantCulture));

    public Query Add(string name, bool? value) => Add(name, value switch { true => "true", false => "false", null => null });

    public Query Add(string name, DateTime? value) => Add(name, value?.ToString("o", CultureInfo.InvariantCulture));

    public Query Add(string name, IEnumerable<int>? values) =>
        Add(name, values is null ? null : string.Join(',', values.Select(v => v.ToString(CultureInfo.InvariantCulture))));

    public override string ToString()
    {
        if (_values.Count == 0) return "";
        var sb = new StringBuilder();
        foreach (var (name, value) in _values)
        {
            sb.Append(sb.Length == 0 ? '?' : '&');
            sb.Append(Uri.EscapeDataString(name)).Append('=').Append(Uri.EscapeDataString(value));
        }
        return sb.ToString();
    }
}

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Agility.Management.Sdk.Http;

/// <summary>
/// A relative API path built from an interpolated string. Literal text is kept as written and every
/// interpolated value is escaped as a single path segment, so a reference name or locale can't change
/// the route: <c>$"{locale}/item/{contentID}"</c>.
/// </summary>
[InterpolatedStringHandler]
internal ref struct ApiPath
{
    private DefaultInterpolatedStringHandler _builder;

    public ApiPath(int literalLength, int formattedCount)
    {
        _builder = new DefaultInterpolatedStringHandler(literalLength, formattedCount, CultureInfo.InvariantCulture);
    }

    public void AppendLiteral(string value) => _builder.AppendLiteral(value);

    public void AppendFormatted(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A path segment can't be null or empty.");
        _builder.AppendLiteral(Uri.EscapeDataString(value));
    }

    public void AppendFormatted(int value) => _builder.AppendFormatted(value);

    public void AppendFormatted(bool value) => _builder.AppendLiteral(value ? "true" : "false");

    public void AppendFormatted(Guid value) => _builder.AppendFormatted(value, "D");

    public string ToStringAndClear() => _builder.ToStringAndClear();
}

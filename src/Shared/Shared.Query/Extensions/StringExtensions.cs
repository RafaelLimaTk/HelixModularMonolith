namespace Shared.Query.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string s)
        => string.Concat(s.Select<char, string>((c, i) => i > 0 && char.IsUpper(c) ? "_" + char.ToLower(c) : char.ToLower(c).ToString()));
}

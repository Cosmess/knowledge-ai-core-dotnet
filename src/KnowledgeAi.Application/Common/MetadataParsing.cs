namespace KnowledgeAi.Application.Common;

public static class MetadataParsing
{
    public static TEnum ParseEnumOrDefault<TEnum>(string? value, TEnum defaultValue) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        var pascalCase = string.Concat(value.Split('_', '-').Select(CapitalizeFirstLetter));
        return Enum.TryParse(pascalCase, ignoreCase: true, out TEnum parsed) ? parsed : defaultValue;
    }

    private static string CapitalizeFirstLetter(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
}

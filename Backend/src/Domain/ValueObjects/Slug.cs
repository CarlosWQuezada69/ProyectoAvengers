using System.Text.RegularExpressions;

namespace ProyectoAvengers.Domain.ValueObjects;

public partial record Slug
{
    private static readonly Regex Pattern = SlugRegex();

    public string Value { get; }

    private Slug(string value) => Value = value;

    public static Slug Create(string value, int maxLength = 200)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > maxLength)
            throw new ArgumentException($"El slug no puede exceder {maxLength} caracteres.", nameof(value));

        if (!Pattern.IsMatch(normalized))
            throw new ArgumentException($"'{value}' no es un slug válido. Use solo letras minúsculas, números y guiones.", nameof(value));

        return new Slug(normalized);
    }

    public static implicit operator string(Slug slug) => slug.Value;

    public override string ToString() => Value;

    [GeneratedRegex(Constants.SlugPattern, RegexOptions.Compiled)]
    private static partial Regex SlugRegex();
}

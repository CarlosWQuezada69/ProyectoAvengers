using System.Text.RegularExpressions;

namespace ProyectoAvengers.Domain.ValueObjects;

public partial record Email
{
    private static readonly Regex Pattern = EmailRegex();

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant();

        if (!Pattern.IsMatch(normalized))
            throw new ArgumentException($"'{value}' no es un correo electrónico válido.", nameof(value));

        if (normalized.Length > 255)
            throw new ArgumentException("El correo electrónico no puede exceder 255 caracteres.", nameof(value));

        return new Email(normalized);
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}

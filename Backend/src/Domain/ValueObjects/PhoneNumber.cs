using System.Text.RegularExpressions;

namespace ProyectoAvengers.Domain.ValueObjects;

public partial record PhoneNumber
{
    private static readonly Regex Pattern = PhoneRegex();

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ').ToArray()).Trim();

        if (!Pattern.IsMatch(cleaned))
            throw new ArgumentException($"'{value}' no es un número telefónico válido.", nameof(value));

        return new PhoneNumber(cleaned);
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[\d\s-]{7,20}$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();
}

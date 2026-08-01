namespace ProyectoAvengers.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("El monto no puede ser negativo.", nameof(amount));

        Amount = amount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "MXN" : currency.ToUpperInvariant();
    }

    public static Money FromDecimal(decimal amount, string currency = "MXN") => new(amount, currency);

    public static Money Zero(string currency = "MXN") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"No se pueden sumar monedas diferentes: {Currency} y {other.Currency}.");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"No se pueden restar monedas diferentes: {Currency} y {other.Currency}.");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public override string ToString() => $"{Currency} {Amount:N2}";
}

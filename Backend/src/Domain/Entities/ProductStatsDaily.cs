namespace ProyectoAvengers.Domain.Entities;

public class ProductStatsDaily
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public DateOnly Date { get; private set; }
    public int Views { get; private set; }
    public int Purchases { get; private set; }

    public Product Product { get; private set; } = null!;

    private ProductStatsDaily() { }

    public ProductStatsDaily(Guid productId, DateOnly date)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Date = date;
    }

    public void IncrementViews() => Views++;
    public void IncrementPurchases() => Purchases++;
    public void AddViews(int count) => Views += count;
    public void AddPurchases(int count) => Purchases += count;
}

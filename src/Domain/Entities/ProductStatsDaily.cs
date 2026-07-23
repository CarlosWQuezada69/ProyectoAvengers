namespace ProyectoAvengers.Domain.Entities;

public class ProductStatsDaily
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateOnly Date { get; set; }
    public int Views { get; set; }
    public int Purchases { get; set; }

    public Product Product { get; set; } = null!;
}

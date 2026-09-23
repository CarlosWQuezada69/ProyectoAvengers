namespace ProyectoAvengers.Domain.Entities;

public class PageViewDaily
{
    public Guid Id { get; private set; }
    public string PageKey { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public int Views { get; private set; }

    private PageViewDaily() { }

    public PageViewDaily(string pageKey, DateOnly date)
    {
        Id = Guid.NewGuid();
        PageKey = pageKey;
        Date = date;
    }

    public void AddViews(int count) => Views += count;
}
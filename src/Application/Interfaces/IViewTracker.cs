namespace ProyectoAvengers.Application.Interfaces;

public interface IViewTracker
{
    void TrackView(Guid productId);
    Task FlushAsync(CancellationToken ct = default);
}

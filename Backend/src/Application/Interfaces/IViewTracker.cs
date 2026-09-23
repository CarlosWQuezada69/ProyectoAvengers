namespace ProyectoAvengers.Application.Interfaces;

public interface IViewTracker
{
    void TrackView(Guid productId);
    void TrackPageView(string pageKey);
    Task FlushAsync(CancellationToken ct = default);
}

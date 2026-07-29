using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Infrastructure.Services;

namespace ProyectoAvengers.Tests.Services;

public class InMemoryViewTrackerTests
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly InMemoryViewTracker _tracker;

    public InMemoryViewTrackerTests()
    {
        var services = new ServiceCollection();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.GetUserId()).Returns((Guid?)null);
        currentUserMock.Setup(x => x.GetIpAddress()).Returns((string?)null);

        services.AddScoped(_ => currentUserMock.Object);
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"ViewTrackerTest_{Guid.NewGuid()}"));

        var provider = services.BuildServiceProvider();
        _scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        _tracker = new InMemoryViewTracker(_scopeFactory);
    }

    [Fact]
    public void TrackView_DoesNotThrow_WhenTrackingMultipleTimes()
    {
        var productId = Guid.NewGuid();
        _tracker.TrackView(productId);
        _tracker.TrackView(productId);
        _tracker.TrackView(productId);

        Assert.True(true);
    }

    [Fact]
    public void TrackView_WithMultipleProducts_DoesNotThrow()
    {
        _tracker.TrackView(Guid.NewGuid());
        _tracker.TrackView(Guid.NewGuid());
        _tracker.TrackView(Guid.NewGuid());

        Assert.True(true);
    }
}

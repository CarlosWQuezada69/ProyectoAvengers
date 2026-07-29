namespace ProyectoAvengers.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetIpAddress();
}

namespace ProyectoAvengers.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { }

    public Role(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public bool HasUsersAssigned() => UserRoles.Count > 0;

    public void AssignPermissions(ICollection<Guid> permissionIds)
    {
        RolePermissions.Clear();
        foreach (var pid in permissionIds)
            RolePermissions.Add(new RolePermission { RoleId = Id, PermissionId = pid });
    }
}

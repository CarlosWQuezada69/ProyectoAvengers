using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProyectoAvengers.Api.Swagger;

public class RequirePermissionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var permissions = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<Authorization.RequirePermissionAttribute>()
            .ToList();

        if (permissions.Count == 0)
            return;

        var permissionStr = string.Join(", ", permissions.Select(p => $"`{p.Permission}`"));

        operation.Description = (operation.Description != null
            ? operation.Description + "\n\n"
            : "") + $"**Permisos requeridos:** {permissionStr}";
    }
}

using Microsoft.AspNetCore.Authorization;

namespace ProyectoAvengers.Api.Authorization;

public class AdminBaseRequirement : IAuthorizationRequirement
{
}

public class AdminBaseAuthorizationHandler : AuthorizationHandler<AdminBaseRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminBaseRequirement requirement)
    {
        if (context.User.Claims.Any(c => c.Type == "permission"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

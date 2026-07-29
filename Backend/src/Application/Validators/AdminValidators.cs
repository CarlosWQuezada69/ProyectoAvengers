using FluentValidation;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public class AssignRolesRequestValidator : AbstractValidator<AssignRolesRequest>
{
    public AssignRolesRequestValidator()
    {
        RuleFor(x => x.RoleIds).NotEmpty();
    }
}

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class AssignPermissionsRequestValidator : AbstractValidator<AssignPermissionsRequest>
{
    public AssignPermissionsRequestValidator()
    {
        RuleFor(x => x.PermissionIds).NotEmpty();
    }
}

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
}

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(500)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(500)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}

public class CreateRestrictionRequestValidator : AbstractValidator<CreateRestrictionRequest>
{
    public CreateRestrictionRequestValidator()
    {
        RuleFor(x => x.RestrictionType).NotEmpty()
            .Must(v => new[] { "AGE_MIN", "PURCHASE_LIMIT_USER", "PURCHASE_LIMIT_ORDER", "AVAILABILITY_WINDOW", "GEOGRAPHIC", "LIMITED_STOCK" }.Contains(v))
            .WithMessage("Tipo de restricción no válido");
    }
}

public class UpdateRestrictionRequestValidator : AbstractValidator<UpdateRestrictionRequest>
{
    public UpdateRestrictionRequestValidator()
    {
        RuleFor(x => x.RestrictionType).NotEmpty()
            .Must(v => new[] { "AGE_MIN", "PURCHASE_LIMIT_USER", "PURCHASE_LIMIT_ORDER", "AVAILABILITY_WINDOW", "GEOGRAPHIC", "LIMITED_STOCK" }.Contains(v))
            .WithMessage("Tipo de restricción no válido");
    }
}

public class CreateSliderItemRequestValidator : AbstractValidator<CreateSliderItemRequest>
{
    public CreateSliderItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSliderItemRequestValidator : AbstractValidator<UpdateSliderItemRequest>
{
    public UpdateSliderItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSliderOrderItemValidator : AbstractValidator<UpdateSliderOrderItem>
{
    public UpdateSliderOrderItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

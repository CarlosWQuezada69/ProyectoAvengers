using FluentValidation.TestHelper;
using ProyectoAvengers.Application.Validators;
using ProyectoAvengers.Shared.DTOs.Auth;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void LoginRequest_ValidEmailAndPassword_Passes()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "123456" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LoginRequest_EmptyEmail_Fails()
    {
        var request = new LoginRequest { Email = "", Password = "123456" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginRequest_InvalidEmail_Fails()
    {
        var request = new LoginRequest { Email = "notanemail", Password = "123456" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginRequest_EmptyPassword_Fails()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void CreateProduct_ValidRequest_Passes()
    {
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Producto de prueba",
            Slug = "producto-de-prueba",
            Price = 100,
            Stock = 10
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateProduct_EmptySku_Fails()
    {
        var request = new CreateProductRequest
        {
            Sku = "",
            Name = "Producto",
            Slug = "producto",
            Price = 100,
            Stock = 0
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Sku);
    }

    [Fact]
    public void CreateProduct_InvalidSlug_Fails()
    {
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Producto",
            Slug = "Invalid Slug With Spaces",
            Price = 100,
            Stock = 0
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void CreateProduct_PriceZero_Fails()
    {
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Producto",
            Slug = "producto",
            Price = 0,
            Stock = 0
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void CreateProduct_NegativeStock_Fails()
    {
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Producto",
            Slug = "producto",
            Price = 100,
            Stock = -1
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Stock);
    }
}

public class CreateCategoryRequestValidatorTests
{
    private readonly CreateCategoryRequestValidator _validator = new();

    [Fact]
    public void CreateCategory_ValidRequest_Passes()
    {
        var request = new CreateCategoryRequest
        {
            Name = "Categoría",
            Slug = "categoria",
            IsActive = true
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCategory_InvalidSlug_Fails()
    {
        var request = new CreateCategoryRequest
        {
            Name = "Categoría",
            Slug = "CATEGORIA_CON_MAYUSCULAS",
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }
}

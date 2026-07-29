using FluentValidation;
using ProyectoAvengers.Shared.DTOs.Account;
using ProyectoAvengers.Shared.DTOs.Auth;

namespace ProyectoAvengers.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}

public class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress();
    }
}

public class ChangeEmailConfirmRequestValidator : AbstractValidator<ChangeEmailConfirmRequest>
{
    public ChangeEmailConfirmRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

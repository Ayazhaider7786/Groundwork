using backend.Model.Requests.Auth;
using FluentValidation;

namespace backend.Validation.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .MaximumLength(128).WithMessage("Refresh token is not valid.");
    }
}

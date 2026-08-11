using backend.Model.Requests.Auth;
using FluentValidation;

namespace backend.Validation.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Email address is not valid.");

        // No complexity rules here: login must not reveal the password policy, and
        // an existing account may predate the current one.
        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

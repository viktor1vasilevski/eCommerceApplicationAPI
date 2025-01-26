using FluentValidation;
using Main.Requests.Auth;

namespace Main.Validations.Auth;

public class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required.")
            .MinimumLength(3).WithMessage("First Name must be at least 3 characters long.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required.")
            .MinimumLength(3).WithMessage("Last Name must be at least 3 characters long.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(5).WithMessage("Username must be at least 5 characters long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+$")
            .WithMessage("Please enter a valid email address in the format 'example@domain.com'.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{4,}$")
            .WithMessage("Password must be at least 4 characters long and include an uppercase letter, lowercase letter, number, and special character.");
    }
}

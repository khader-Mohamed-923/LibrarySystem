using FluentValidation;
using LibrarySystem.Contracts.Requests.Member;

namespace LibrarySystem.Services.Validators;

public class CreateMemberDtoValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("The FirstName field is required.")
            .MaximumLength(100).WithMessage("FirstName must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("The LastName field is required.")
            .MaximumLength(100).WithMessage("LastName must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("The Email field is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("The Phone field is required.")
            .Matches(@"^\+?[\d\s\-()]{7,20}$").WithMessage("Phone must be a valid phone number.");
    }
}

using FluentValidation;
using LibrarySystem.Contracts.Requests.Book;

namespace LibrarySystem.Services.Validators;

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("The Title field is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("The Author field is required.")
            .MaximumLength(200).WithMessage("Author must not exceed 200 characters.");

        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("The ISBN field is required.")
            .Matches(@"^(?:\d{9}[\dXx]|\d{13})$").WithMessage("ISBN must be a valid 10 or 13 digit format.");
    }
}

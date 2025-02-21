using FluentValidation;
using Natech.Caas.API.Request;

namespace Natech.Caas.API.Validators;

public class ListCatsRequestValidator : AbstractValidator<ListCatsRequest>
{
  public ListCatsRequestValidator()
  {
    RuleFor(x => x.Page)
      .GreaterThanOrEqualTo(1)
        .WithMessage("Page must be at least 1.");
    RuleFor(x => x.PageSize)
      .InclusiveBetween(1, 25)
        .WithMessage("PageSize must be between 1 and 25.");
    RuleFor(x => x.Tag)
      .MinimumLength(3)
        .When(x => !string.IsNullOrEmpty(x.Tag))
        .WithMessage("Tag must be at least 3 characters long.")
      .MaximumLength(50)
        .WithMessage("Tag must be at most 50 characters long.");
  }
}

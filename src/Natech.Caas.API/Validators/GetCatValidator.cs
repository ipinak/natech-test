using FluentValidation;
using Natech.Caas.API.Request;

namespace Natech.Caas.API.Validators;

public class GetCatValidator : AbstractValidator<GetCatRequest>
{
  public GetCatValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
        .WithMessage("You must provide an ID")
      .GreaterThan(0)
        .WithMessage("It should be a positive integer");
  }
}
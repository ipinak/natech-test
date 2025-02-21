using FluentValidation;
using Natech.Caas.API.Request;

namespace Natech.Caas.API.Validators;

public class GetCatRequestValidator : AbstractValidator<GetCatRequest>
{
  public GetCatRequestValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
        .WithMessage("You must provide an ID")
      .GreaterThan(0)
        .WithMessage("It should be a positive integer");
  }
}
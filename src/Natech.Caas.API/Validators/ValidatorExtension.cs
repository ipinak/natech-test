using FluentValidation;
using FluentValidation.AspNetCore;
using Natech.Caas.API.Request;

namespace Natech.Caas.API.Validators;

public static class ValidatorExtension
{
  public static IServiceCollection AddValidators(this IServiceCollection services)
  {
    services.AddFluentValidationAutoValidation();
    services.AddScoped<IValidator<ListCatsRequest>, ListCatsRequestValidator>();
    services.AddScoped<IValidator<GetCatRequest>, GetCatRequestValidator>();

    return services;
  }
}
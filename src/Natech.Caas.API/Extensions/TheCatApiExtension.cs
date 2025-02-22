using Natech.Caas.Core.Services;
using Natech.Caas.TheCatApi.Client;

namespace Natech.Caas.API.Extensions;

public static class TheCatApiExtension
{
  public static IServiceCollection AddCatApi(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<CatApiConfig>(configuration.GetSection("CatsApi"));
    services.AddHttpClient<ICatService, CatService>();
    services.AddSingleton<ITheCatApiClient, CatApiClient>();
    return services;
  }
}
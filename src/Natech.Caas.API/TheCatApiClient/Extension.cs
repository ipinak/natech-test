namespace Natech.Caas.API.TheCatApiClient;

public static class CatApiExtension
{
  public static IServiceCollection AddCatApi(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddTransient<ITheCatApiClient, CatApiClient>(sp => new CatApiClient(
        baseUrl: configuration.GetSection("CatsApi:Url").Value,
        apiKey: configuration.GetSection("CatsApi:ApiKey").Value,
        sp.GetRequiredService<ILogger<CatApiClient>>()
      )
    );
    return services;
  }
}
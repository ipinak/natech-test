using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Natech.Caas.API.Database;

namespace Natech.Caas.API.IntegrationTests;

public class BaseTest
{
  protected HttpClient _client;
  protected WebApplicationFactory<Program> _factory;
  protected const string BASE_URL = "/api";

  [SetUp]
  public void SetUp()
  {
    _factory = new WebApplicationFactory<Program>();
    _factory = _factory.WithWebHostBuilder(builder =>
        {
          builder.UseEnvironment(Consts.INTEGRATION_TESTING);
        });

    _client = _factory.CreateClient();

    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
  }

  [TearDown]
  public void TearDown()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureDeleted();
    _client.Dispose();
    _factory.Dispose();
  }
}
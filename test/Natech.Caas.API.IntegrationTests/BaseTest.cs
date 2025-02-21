using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Natech.Caas.Database;

namespace Natech.Caas.API.IntegrationTests;

public class BaseTest
{
  protected HttpClient _client;
  protected WebApplicationFactory<Program> _factory;
  protected const string BASE_URL = "/api";

  // private string _downloadPath;

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

    // // Define the download directory
    // _downloadPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "downloads");

    // // Ensure the directory exists
    // if (!Directory.Exists(_downloadPath))
    // {
    //   Directory.CreateDirectory(_downloadPath);
    // }
  }

  [TearDown]
  public void TearDown()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureDeleted();

    // if (Directory.Exists(_downloadPath))
    // {
    //   Directory.Delete(_downloadPath, recursive: true);
    // }

    _client.Dispose();
    _factory.Dispose();
  }
}
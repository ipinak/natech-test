using System.Text.Json;
using Natech.Caas.TheCatApi.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Natech.Caas.TheCatApi.Client;

public class CatApiClient : ITheCatApiClient
{
  private readonly ILogger<CatApiClient> _logger;
  private readonly HttpClient _httpClient;
  private readonly CatApiConfig _config;

  public CatApiClient(
    IHttpClientFactory httpClientFactory,
    IOptions<CatApiConfig> config,
    ILogger<CatApiClient> logger)
  {
    _logger = logger;
    _config = config.Value ?? throw new ArgumentException(nameof(config));
    _httpClient = httpClientFactory.CreateClient("TheCatApiClient");

    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
  }

  public async Task<IEnumerable<CatImage>> GetRandomCatImagesAsync(int limit = 25)
  {
    using (var response = await _httpClient.GetAsync($"{_config.BaseUrl}/images/search?size=med&mime_types=jpg&format=json&has_breeds=true&order=RANDOM&page=0&limit={limit}"))
    {
      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to fetch with code {0}", response.StatusCode);
        return Array.Empty<CatImage>();
      }

      string json = await response.Content.ReadAsStringAsync();
      var cats = JsonSerializer.Deserialize<CatImage[]>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      return cats?.Length > 0 ? cats : Array.Empty<CatImage>();
    }
  }
}

using System.Text.Json;
using Natech.Caas.TheCatApi.Dtos;
using Microsoft.Extensions.Logging;

namespace Natech.Caas.TheCatApi.Client;

public class CatApiClient : ITheCatApiClient
{
  private readonly ILogger<CatApiClient> _logger;
  private readonly HttpClient _httpClient;
  private readonly string _baseUrl;

  public CatApiClient(string baseUrl, string apiKey, ILogger<CatApiClient> logger)
  {
    _baseUrl = baseUrl;
    _logger = logger;

    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
  }

  /// <summary>
  /// Get 
  /// </summary>
  /// <returns></returns>
  public async Task<IEnumerable<CatImage>> GetRandomCatImagesAsync()
  {
    try
    {
      var limit = 25;
      var response = await _httpClient.GetAsync($"{_baseUrl}/images/search?size=med&mime_types=jpg&format=json&has_breeds=true&order=RANDOM&page=0&limit={limit}");

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to fetch with code {0}", response.StatusCode);
        return Array.Empty<CatImage>();
      }

      string json = await response.Content.ReadAsStringAsync();
      var cats = JsonSerializer.Deserialize<CatImage[]>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      return cats?.Length > 0 ? cats : Array.Empty<CatImage>();
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to fetch with code {0}", ex.Message);
      return Array.Empty<CatImage>();
    }
  }
}

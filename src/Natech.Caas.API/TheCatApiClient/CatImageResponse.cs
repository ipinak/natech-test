using System.Text.Json.Serialization;

namespace Natech.Caas.API.TheCatApiClient;

public class CatImageResponse
{
  [JsonPropertyName("id")]
  public string Id { get; set; }

  [JsonPropertyName("url")]
  public string Url { get; set; }

  [JsonPropertyName("width")]
  public int Width { get; set; }

  [JsonPropertyName("height")]
  public int Height { get; set; }

  [JsonPropertyName("breeds")]
  public List<Breed> Breeds { get; set; }
}
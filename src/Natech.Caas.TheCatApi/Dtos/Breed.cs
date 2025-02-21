using System.Text.Json.Serialization;

namespace Natech.Caas.TheCatApi.Dtos;

public class Breed
{
  [JsonPropertyName("id")]
  public string Id { get; set; }

  [JsonPropertyName("temperament")]
  public string Temperament { get; set; } // e.g.: "Affectionate, Intelligent, Playful",
}
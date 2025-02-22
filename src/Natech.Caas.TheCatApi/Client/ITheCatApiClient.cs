namespace Natech.Caas.TheCatApi.Client;

using Natech.Caas.TheCatApi.Dtos;

public interface ITheCatApiClient
{
  Task<IEnumerable<CatImage>> GetRandomCatImagesAsync();
}

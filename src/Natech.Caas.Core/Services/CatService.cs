using Natech.Caas.Database.Entities;
using Natech.Caas.Dtos;
using Natech.Caas.Database.Repository;
using Natech.Caas.TheCatApi.Client;
using Natech.Caas.TheCatApi.Dtos;
using Microsoft.Extensions.Configuration;

namespace Natech.Caas.Core.Services;

public interface ICatService
{
  Task SaveCats();
  Task<CatDto> GetCat(int id);
  Task<ListResponse<CatDto>> ListCats(string tag, int page, int pageSize);
}

public class CatService : ICatService
{
  private readonly ITheCatApiClient _catApiClient;
  private readonly IDownloader _downloader;
  private readonly ICatRepository _catRepository;
  private readonly ITagRepository _tagRepository;
  private readonly string _host;

  public CatService(
    IConfiguration configuration,
    ICatRepository catRepository,
    ITagRepository tagRepository,
    ITheCatApiClient theCatApiClient,
    IDownloader downloader)
  {
    // configuration.ThrowIfNull("Configuration is null");

    _catApiClient = theCatApiClient;
    _downloader = downloader;
    _catRepository = catRepository;
    _tagRepository = tagRepository;

    _host = configuration.GetSection("Kestrel:Endpoints:Http:Url").Value;
  }

  /// <summary>
  /// Saves cats adn tags avoiding duplicate tags.
  /// </summary>
  /// <returns></returns>
  public async Task SaveCats()
  {
    var cats = await _catApiClient.GetRandomCatImagesAsync();
    foreach (var c in cats)
    {
      var fileName = $"{Guid.NewGuid()}{Utils.GetFileExtension(c.Url)}";
      await _downloader.Download(c.Url, fileName);

      var catEntity = new CatEntity
      {
        CatId = c.Id,
        Width = c.Width,
        Height = c.Height,
        Image = $"{_host}/downloads/{fileName}",
        Created = DateTime.UtcNow,
        Tags = new List<TagEntity>()
      };

      await _catRepository.Add(catEntity);

      var tags = ExtractTags(c.Breeds);

      // Fetch existing tags
      var existingTags = await _tagRepository.List(tags);

      // Find only tags except the ones that match by name
      var newTags = tags.Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
          .Select(tagName => new TagEntity { Name = tagName, Created = DateTime.UtcNow })
          .ToList();

      // Add new tags to the database
      if (newTags.Any())
      {
        await _tagRepository.AddAll(newTags);
      }

      // Merge existing and newly created tags
      var allTags = existingTags.Concat(newTags).ToList();

      // Assign tags to the cat
      catEntity.Tags.AddRange(allTags);
      await _catRepository.Update(catEntity);
    }
  }

  private static IEnumerable<string> ExtractTags(IEnumerable<Breed> breeds)
  {
    return breeds.SelectMany(b => b.Temperament.Split(", "));
  }

  public async Task<CatDto> GetCat(int id)
  {
    var result = await _catRepository.GetByIdAsync(id);
    return result?.ToDto();
  }

  public async Task<ListResponse<CatDto>> ListCats(string tag, int page, int pageSize)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : pageSize;

    var (cats, totalCount) = await _catRepository.ListAsync(tag, page, pageSize);

    return new ListResponse<CatDto>
    {
      TotalCount = totalCount,
      Page = page,
      Data = cats.Select(c => c.ToDto())
    };
  }
}

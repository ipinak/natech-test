using Natech.Caas.API.Database.Entities;
using Natech.Caas.API.Dtos;
using Natech.Caas.API.Extensions;
using Natech.Caas.API.Database.Repository;
using Natech.Caas.API.Request;
using Natech.Caas.API.TheCatApiClient;

namespace Natech.Caas.API.Services;

public class CatService
{
  private readonly ITheCatApiClient _catApiClient;
  private readonly IDownloader _downloader;
  private readonly ICatRepository _catRepository;
  private readonly ITagRepository _tagRepository;

  public CatService(
    ICatRepository catRepository,
    ITagRepository tagRepository,
    ITheCatApiClient theCatApiClient,
    IDownloader downloader)
  {
    _catApiClient = theCatApiClient;
    _downloader = downloader;
    _catRepository = catRepository;
    _tagRepository = tagRepository;
  }

  /// <summary>
  /// Saves cats adn tags avoiding duplicate tags.
  /// </summary>
  /// <returns></returns>
  public async Task SaveCats()
  {
    var cats = await _catApiClient.GetRandomCatImages();
    foreach (var c in cats)
    {
      var fileName = $"{Guid.NewGuid()}{Utils.GetFileExtension(c.Url)}";
      await _downloader.Download(c.Url, fileName);

      var catEntity = new CatEntity
      {
        CatId = c.Id,
        Width = c.Width,
        Height = c.Height,
        Image = $"/downloads/{fileName}",
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

  static IEnumerable<string> ExtractTags(IEnumerable<Breed> breeds)
  {
    return breeds.SelectMany(b => b.Temperament.Split(", "));
  }

  public async Task<CatDto> GetCat(int id)
  {
    var result = await _catRepository.GetByIdAsync(id);
    return result?.ToDto();
  }

  public async Task<(IEnumerable<CatDto> Cats, int TotalCount)> ListCats(ListCatsRequest request)
  {
    request.ThrowIfNull("ListCatRequest object is null");

    // TODO: don't use this validation
    var page = request.Page < 1 ? 1 : request.Page;
    var pageSize = request.Page < 1 ? 10 : request.PageSize;

    var (cats, totalCount) = await _catRepository.ListAsync(request.Tag, request.Page, request.PageSize);

    return (cats.Select(c => c.ToDto()), totalCount);
  }
}

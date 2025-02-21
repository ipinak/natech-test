using Microsoft.EntityFrameworkCore;
using Natech.Caas.API.Database;
using Natech.Caas.API.Database.Entities;
using Natech.Caas.API.Dtos;
using Natech.Caas.API.Extensions;
using Natech.Caas.API.Request;
using Natech.Caas.API.TheCatApiClient;

namespace Natech.Caas.API.Services;

public class CatService
{
  private readonly ApplicationDbContext _dbContext;
  private readonly ITheCatApiClient _catApiClient;
  private readonly IDownloader _downloader;

  public CatService(
    ApplicationDbContext dbContext,
    ITheCatApiClient theCatApiClient,
    IDownloader downloader)
  {
    _dbContext = dbContext;
    _catApiClient = theCatApiClient;
    _downloader = downloader;
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
      // await _downloader.Download(c.Url, fileName);

      var catEntity = new CatEntity
      {
        CatId = c.Id,
        Width = c.Width,
        Height = c.Height,
        Image = $"/downloads/{fileName}",
        Created = DateTime.UtcNow,
        Tags = new List<TagEntity>()
      };

      _dbContext.Cats.Add(catEntity);

      var tags = ExtractTags(c.Breeds);

      // Fetch existing tags
      var existingTags = await _dbContext.Tags
          .Where(t => tags.Contains(t.Name))
          .ToListAsync();

      // Find only tags except the ones that match by name
      var newTags = tags.Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
          .Select(tagName => new TagEntity { Name = tagName, Created = DateTime.UtcNow })
          .ToList();

      // Add new tags to the database
      if (newTags.Any())
      {
        _dbContext.Tags.AddRange(newTags);
        await _dbContext.SaveChangesAsync();
      }

      // Merge existing and newly created tags
      var allTags = existingTags.Concat(newTags).ToList();

      // Assign tags to the cat
      catEntity.Tags.AddRange(allTags);
    }

    await _dbContext.SaveChangesAsync();
  }

  static IEnumerable<string> ExtractTags(IEnumerable<Breed> breeds)
  {
    return breeds.SelectMany(b => b.Temperament.Split(", "));
  }

  public async Task<CatDto> GetCat(int id)
  {
    // TODO: add result, error tuple
    var result = await _dbContext.Cats.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == id);

    return result?.ToDto();
  }

  public async Task<(IEnumerable<CatDto> Cats, int TotalCount)> ListCats(ListCatsRequest request)
  {
    request.ThrowIfNull("ListCatRequest object is null");

    // TODO: don't use this validation
    var page = request.Page < 1 ? 1 : request.Page;
    var pageSize = request.Page < 1 ? 10 : request.PageSize;

    var query = _dbContext.Cats
        .Include(c => c.Tags)
        .AsQueryable();

    if (!string.IsNullOrEmpty(request.Tag))
    {
      query = query.Where(c => c.Tags.Any(t => t.Name == request.Tag));
    }

    var totalCount = await query.CountAsync();
    var cats = await query
        .Skip(CalculateSkip(page, pageSize))
        .Take(pageSize)
        .ToListAsync();

    if (totalCount <= 0)
    {
      return (Array.Empty<CatDto>(), 0);
    }

    return (cats.Select(c => c.ToDto()), totalCount);
  }

  private static int CalculateSkip(int page, int pageSize)
  {
    return (page - 1) * pageSize;
  }
}

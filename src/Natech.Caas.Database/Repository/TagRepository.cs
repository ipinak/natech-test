using Microsoft.EntityFrameworkCore;
using Natech.Caas.Database.Entities;

namespace Natech.Caas.Database.Repository;

public class TagRepository : ITagRepository
{
  private readonly ApplicationDbContext _dbContext;

  public TagRepository(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task AddAll(IEnumerable<TagEntity> entities)
  {
    _dbContext.Tags.AddRange(entities);
    await _dbContext.SaveChangesAsync();
  }

  public async Task<IEnumerable<TagEntity>> List(IEnumerable<string> tags)
  {
    return await _dbContext.Tags
      .Where(t => tags.Contains(t.Name))
      .ToListAsync();
  }
}
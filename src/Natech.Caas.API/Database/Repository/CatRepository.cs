using Microsoft.EntityFrameworkCore;
using Natech.Caas.API.Database.Entities;
using Natech.Caas.API.Extensions;

namespace Natech.Caas.API.Database.Repository;

public interface ICatRepository
{
  Task Add(CatEntity entity);
  Task<CatEntity> GetByIdAsync(int id);
  Task<(IEnumerable<CatEntity> Cats, int TotalCount)> ListAsync(string tag, int page, int pageSize);
  Task Update(CatEntity entity);
}

public class CatRepository : ICatRepository
{
  private readonly ApplicationDbContext _dbContext;

  public CatRepository(ApplicationDbContext dbContext)
  {
    dbContext.ThrowIfNull("dbContext should not be null");
    _dbContext = dbContext;
  }

  public async Task<CatEntity> GetByIdAsync(int id)
  {
    return await _dbContext.Cats
      .Include(c => c.Tags)
      .FirstOrDefaultAsync(c => c.Id == id);
  }

  public async Task<(IEnumerable<CatEntity> Cats, int TotalCount)> ListAsync(string tag, int page, int pageSize)
  {
    var query = _dbContext.Cats
            .Include(c => c.Tags)
            .AsQueryable();

    if (!string.IsNullOrEmpty(tag))
    {
      query = query.Where(c => c.Tags.Any(t => t.Name == tag));
    }

    var totalCount = await query.CountAsync();
    var cats = await query
        .Skip(CalculateSkip(page, pageSize))
        .Take(pageSize)
        .ToListAsync();

    if (cats.Count <= 0)
    {
      return (Array.Empty<CatEntity>(), 0);
    }

    return (cats, totalCount);
  }

  public async Task Add(CatEntity entity)
  {
    _dbContext.Cats.Add(entity);
    await _dbContext.SaveChangesAsync();
  }

  public async Task Update(CatEntity entity)
  {
    _dbContext.Cats.Update(entity);
    await _dbContext.SaveChangesAsync();
  }

  private static int CalculateSkip(int page, int pageSize)
  {
    return (page - 1) * pageSize;
  }
}
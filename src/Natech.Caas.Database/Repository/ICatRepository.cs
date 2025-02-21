namespace Natech.Caas.Database.Repository;

using Natech.Caas.Database.Entities;

public interface ICatRepository
{
  Task Add(CatEntity entity);
  Task<CatEntity> GetByIdAsync(int id);
  Task<(IEnumerable<CatEntity> Cats, int TotalCount)> ListAsync(string tag, int page, int pageSize);
  Task Update(CatEntity entity);
}
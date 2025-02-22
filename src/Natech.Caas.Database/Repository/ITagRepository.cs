namespace Natech.Caas.Database.Repository;

using Natech.Caas.Database.Entities;

public interface ITagRepository
{
  Task AddAll(IEnumerable<TagEntity> entities);

  Task<IEnumerable<TagEntity>> List(IEnumerable<string> tags);
}
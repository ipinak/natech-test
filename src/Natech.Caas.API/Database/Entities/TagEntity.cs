namespace Natech.Caas.API.Database.Entities;

using System.Collections.Generic;

public class TagEntity
{
  public int Id { get; set; }
  public string Name { get; set; }
  public DateTime Created { get; set; }

  public List<CatEntity> Cats { get; set; } = [];
}

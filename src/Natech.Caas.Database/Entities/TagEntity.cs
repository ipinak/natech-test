namespace Natech.Caas.Database.Entities;

using System.Collections.Generic;

public class TagEntity
{
  public int Id { get; set; }
  public string Name { get; set; }
  public DateTime Created { get; set; }

  public List<CatEntity> Cats { get; set; } = [];
}

using Natech.Caas.API.Database.Entities;

namespace Natech.Caas.API.Dtos;

public class CatDto
{
  public int Id { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }
  public string ImageUrl { get; set; }
  public string[] Tags { get; set; }
  public DateTime Created { get; set; }
}

public static class CatExtensions
{
  public static CatDto ToDto(this CatEntity entity)
  {
    return new CatDto
    {
      Id = entity.Id,
      Width = entity.Width,
      Height = entity.Height,
      ImageUrl = entity.Image,
      Created = entity.Created,
      Tags = entity.Tags.Select(x => x.Name).ToArray()
    };
  }
}
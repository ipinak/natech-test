using Natech.Caas.API.Database.Entities;

namespace Natech.Caas.API.Dtos;

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
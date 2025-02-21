namespace Natech.Caas.Database.Entities;

public class CatEntity
{
  public int Id { get; set; }
  public string CatId { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }

  /// <summary>
  /// The path to an image locally or a full url where the image is hosted
  /// </summary>
  public string Image { get; set; }
  public DateTime Created { get; set; }

  public List<TagEntity> Tags { get; set; } = [];
}
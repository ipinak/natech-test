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

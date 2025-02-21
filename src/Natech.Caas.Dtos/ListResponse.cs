namespace Natech.Caas.Dtos;

public class ListResponse<T>
{
  public int TotalCount { get; set; }
  public int Page { get; set; }
  public IEnumerable<T> Data { get; set; } = new List<T>();
}
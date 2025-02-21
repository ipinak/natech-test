namespace Natech.Caas.API.Request;

public class ListCatsRequest
{
  public string? Tag { get; set; }
  public int Page { get; set; } = 1; // Defaults to the first page
  public int PageSize { get; set; } = 10; // Defaults to a page size of 10
}
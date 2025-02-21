using Microsoft.AspNetCore.Mvc;

namespace Natech.Caas.API.Request;

public class ListCatsRequest
{
  [FromQuery(Name = "tag")]
  public string? Tag { get; set; }

  [FromQuery(Name = "page")]
  public int Page { get; set; } = 1; // Defaults to the first page

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; set; } = 10; // Defaults to a page size of 10
}

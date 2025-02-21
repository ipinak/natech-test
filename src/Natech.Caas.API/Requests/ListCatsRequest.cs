using Microsoft.AspNetCore.Mvc;

namespace Natech.Caas.API.Request;

public class ListCatsRequest
{
  [FromQuery]
  public string? Tag { get; set; }
  [FromQuery]
  public int Page { get; set; } = 1; // Defaults to the first page
  [FromQuery]
  public int PageSize { get; set; } = 10; // Defaults to a page size of 10
}

public class GetCatRequest
{
  [FromRoute]
  public int Id { get; set; }
}
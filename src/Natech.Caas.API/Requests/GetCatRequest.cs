using Microsoft.AspNetCore.Mvc;

namespace Natech.Caas.API.Request;

public class GetCatRequest
{
  [FromRoute(Name = "id")]
  public int Id { get; set; }
}
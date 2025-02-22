using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Natech.Caas.API.Request;
using Natech.Caas.Core.Services;
using Natech.Caas.Dtos;

namespace Natech.Caas.API.Controllers;

[ApiController]
[Route("api/cats")]
public class CatController : ControllerBase
{
  private readonly ICatService _catService;
  private readonly IBackgroundJobClient _backgroundJobClient;

  public CatController(ICatService catService, IBackgroundJobClient backgroundJobClient)
  {
    _catService = catService ?? throw new ArgumentNullException(nameof(catService));
    _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
  }

  [HttpPost]
  [Route("fetch")]
  [ProducesResponseType(typeof(string), 200, contentType: "application/text")]
  [ProducesResponseType(typeof(string), 500, contentType: "application/json")]
  public async Task<ActionResult> Fetch()
  {
    // NOTE: runs the job in the background because it's kind of heavy
    _backgroundJobClient.Enqueue<ICatService>(catService => catService.SaveCats());
    return Ok();
  }

  [HttpGet]
  [Route("{id}")]
  [ProducesResponseType(typeof(CatDto), 200, contentType: "application/json")]
  [ProducesResponseType(typeof(string), 400, contentType: "application/json")]
  [ProducesResponseType(typeof(string), 500, contentType: "application/json")]
  public async Task<ActionResult<CatDto>> Get([FromRoute] GetCatRequest request)
  {
    var response = await _catService.GetCat(request.Id);
    if (response == null)
    {
      return NotFound($"No cat with id {request.Id} was found");
    }
    return response;
  }

  [HttpGet]
  [Route("")]
  [ProducesResponseType(typeof(IEnumerable<CatDto>), 200, contentType: "application/json")]
  [ProducesResponseType(typeof(string), 400, contentType: "application/json")]
  [ProducesResponseType(typeof(string), 500, contentType: "application/json")]
  public async Task<ActionResult<IEnumerable<CatDto>>> List([FromQuery] ListCatsRequest request)
  {
    var listCatsResponse = await _catService.ListCats(request.Tag, request.Page, request.PageSize);
    return Ok(listCatsResponse);
  }
}

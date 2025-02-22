using Natech.Caas.Dtos;

namespace Natech.Caas.Core.Services;

public interface ICatService
{
  Task SaveCats();
  Task<CatDto> GetCat(int id);
  Task<ListResponse<CatDto>> ListCats(string tag, int page, int pageSize);
}

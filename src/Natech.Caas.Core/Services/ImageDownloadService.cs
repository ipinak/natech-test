namespace Natech.Caas.Core.Services;

public interface IDownloader
{
  Task Download(string url, string name);
}

public class ImageDownloadService : IDownloader
{
  private readonly string _savePath;

  public ImageDownloadService(string savePath)
  {
    _savePath = savePath;
  }

  public async Task Download(string url, string name)
  {
    using HttpClient httpClient = new HttpClient();

    HttpResponseMessage response = await httpClient.GetAsync(url);
    response.EnsureSuccessStatusCode(); // Throw an exception if the request fails

    await using FileStream fileStream = new FileStream(_savePath + "/" + name, FileMode.Create, FileAccess.Write, FileShare.None);
    await response.Content.CopyToAsync(fileStream);
  }
}

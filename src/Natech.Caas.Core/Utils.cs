namespace Natech.Caas.Core;

public static class Utils
{
  public static string GetFileExtension(string url)
  {
    return Path.GetExtension(new Uri(url).AbsolutePath);
  }
}
namespace Natech.Caas.API.Extensions;

public static class ObjectExtensions
{
  internal static void ThrowIfNull<T>(this T obj, string paramName)
  {
    if (obj == null)
    {
      throw new ArgumentNullException(paramName, $"Parameter {paramName} cannot be null.");
    }
  }
}
namespace AdvancedPlugin.Utils
{
  using System.IO;
  using System.Runtime.Serialization.Json;
  using System.Text;

  /// <summary>
  /// Simple JSON Helper
  /// <remarks>
  /// Fits for "manually decorated" classes only 
  /// </remarks>
  /// </summary>
  public static class JsonHelper
  {
    /// <summary>
    /// Serializes to json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    public static string SerializeToJson<T>(this T obj, DataContractJsonSerializerSettings settings = null)
    {
      using (var stream = new MemoryStream())
      {
        var serializer = new DataContractJsonSerializer(typeof(T), settings);
        serializer.WriteObject(stream, obj);
        return Encoding.UTF8.GetString(stream.ToArray());
      }
    }

    /// <summary>
    /// Deserializes from json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">The json.</param>
    /// <returns></returns>
    public static T DeserializeFromJson<T>(this string json)
    {
      using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
      {
        var serializer = new DataContractJsonSerializer(typeof(T));
        return (T)serializer.ReadObject(stream);
      }
    }
  }
}

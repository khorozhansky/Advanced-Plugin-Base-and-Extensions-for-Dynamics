namespace AdvancedPlugin.Exceptions
{
  using System;

  /// <summary>
  /// An exception to show a detailed error message to a user WITH trace details and WITH logging
  /// </summary>
  /// <seealso cref="PluginExceptionBase" />
  public sealed class PluginExceptionWithDetailsInUiAndWithLogging : PluginExceptionBase
  {
    public PluginExceptionWithDetailsInUiAndWithLogging(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }
  }
}

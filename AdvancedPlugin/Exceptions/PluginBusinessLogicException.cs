namespace AdvancedPlugin.Exceptions
{
  using System;

  /// <summary>
  /// An exception to show a "simple" error message to a user WITHOUT trace details and WITHOUT logging
  /// </summary>
  /// <seealso cref="PluginExceptionBase" />
  public sealed class PluginBusinessLogicException : PluginExceptionBase
  {
    public PluginBusinessLogicException(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }
  }
}

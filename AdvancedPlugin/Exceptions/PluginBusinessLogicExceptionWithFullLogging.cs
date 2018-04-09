namespace AdvancedPlugin.Exceptions
{
  using System;

  /// <summary>
  /// An exception to show a "simple" error message to a user WITHOUT trace details and WITH "Full" logging
  ///  </summary>
  /// <seealso cref="PluginExceptionBase" />
  public sealed class PluginBusinessLogicExceptionWithFullLogging : PluginExceptionBase
  {
    public PluginBusinessLogicExceptionWithFullLogging(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }
  }
}

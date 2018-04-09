namespace AdvancedPlugin.Exceptions
{
  using System;

  /// <summary>
  /// An exception to show a "simple" error message to a user WITHOUT trace details and WITH "SIMPLE" logging
  /// <remarks>
  /// "Simple logging" means the trace log will not include StackTrace, TraceText as well as Execution Context's collections (like Input Parameters, Output Parameters, etc.)
  /// </remarks>
  ///  </summary>
  /// <seealso cref="PluginExceptionBase" />
  public sealed class PluginBusinessLogicExceptionWithSimpleLogging : PluginExceptionBase
  {
    public PluginBusinessLogicExceptionWithSimpleLogging(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }
  }
}

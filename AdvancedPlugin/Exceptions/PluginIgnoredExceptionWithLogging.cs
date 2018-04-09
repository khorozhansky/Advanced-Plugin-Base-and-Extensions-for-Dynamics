namespace AdvancedPlugin.Exceptions
{
  using System;

  using Logging;

  /// <summary>
  /// An exception WITHOUT throwing any message/exception to a user but WITH logging error details (see also the note below).
  /// </summary>
  /// <remarks>
  /// ATTENTION! THE ERROR WILL NOT BE LOGGED IN CASE YOU USE <see cref="ExtendedOutOfBoxTraceLog"/> AND THE 'CustomVerboseLogSwitchedOn' FLAG IS NOT SPECIFIED FOR THE PLUGIN STEP
  /// </remarks>
  /// <seealso cref="PluginExceptionBase" />
  public sealed class PluginIgnoredExceptionWithLogging : PluginExceptionBase
  {
    public PluginIgnoredExceptionWithLogging(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }
  }
}

namespace AdvancedPlugin.Exceptions
{
  using System;

  /// <summary>
  /// Base exception for other custom plugin exceptions
  /// </summary>
  /// <seealso cref="System.ApplicationException" />
  public abstract class PluginExceptionBase : ApplicationException
  {
    protected PluginExceptionBase(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }
  }
}

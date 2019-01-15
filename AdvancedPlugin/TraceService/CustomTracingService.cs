namespace AdvancedPlugin.TraceService
{
  using System;
  using System.Globalization;
  using System.Text;

  using Microsoft.Xrm.Sdk;

  public class CustomTracingService : ITracingService
  {
    private readonly Lazy<StringBuilder> sb;
    private bool limitReached;
    private string cachedResult;

    public CustomTracingService()
    {
      this.sb = new Lazy<StringBuilder>(() => new StringBuilder());
    }

    private StringBuilder Sb => this.sb.Value;

    public void Trace(string format, params object[] args)
    {
      if (this.limitReached)
      {
        return;
      }

      var length = this.Sb.Length;
      if (length > 0)
      {
        this.Sb.AppendLine();
      }

      if (args.Length > 0)
      {
        format = string.Format(CultureInfo.InvariantCulture, format, args);
      }

      this.Sb.Append(format);

      const int CountLimit = 10240;
      if (length <= CountLimit)
      {
        return;
      }

      this.limitReached = true;
      this.Sb.Remove(length - 1, length - CountLimit);
    }

    /// <summary>
    /// Gets the trace information.
    /// </summary>
    /// <param name="takeCached">if set to <c>true</c> the cached value will be taken.</param>
    /// <returns>Accumulated Tracing Info</returns>
    public string GetTraceInfo(bool takeCached)
    {
      return 
        takeCached && this.cachedResult != null ? 
          this.cachedResult : 
          this.cachedResult = this.Sb.ToString();
    }
  }
}

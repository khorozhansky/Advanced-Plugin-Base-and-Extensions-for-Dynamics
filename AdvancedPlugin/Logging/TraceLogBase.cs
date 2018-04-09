namespace AdvancedPlugin.Logging
{
  using System;
  using System.ServiceModel;
  using System.Text;

  using Exceptions;

  using Microsoft.Xrm.Sdk;

  using Newtonsoft.Json;

  public abstract class TraceLogBase
  {
    private const int MaxMemoFieldLength = 1000000;

    protected readonly ITracingService SystemTracingService;

    private ITracingService tracingService;

    private readonly Lazy<StringBuilder> traceDetailsStringBuilder;

    protected TraceLogBase(IPluginContext pluginCtx, ITracingService systemTracingService)
    {
      this.PluginCtx = pluginCtx;
      this.SystemTracingService = systemTracingService;

      this.traceDetailsStringBuilder = new Lazy<StringBuilder>(() => new StringBuilder());
    }

    public ITracingService TracingService =>
      this.tracingService ??
        (this.tracingService = this.BuildTracingService());

    protected StringBuilder TraceDetailsStringBuilder => this.traceDetailsStringBuilder.Value;

    protected virtual void AddTraceDetailsInfo(Exception exc)
    {
      var includeStackTrace = true;
      var includeInnerFaultTraceText = true;

      if (!this.PluginCtx.CustomVerboseLogSwitchedOn)
      {
        if (exc is PluginBusinessLogicExceptionWithSimpleLogging)
        {
          includeStackTrace = false;
          includeInnerFaultTraceText = false;
        }
      }

      this.AddTraceDetailsInfo(exc, includeStackTrace, includeInnerFaultTraceText);
    }

    protected virtual void AddTraceDetailsInfo(Exception exc, bool includeStackTrace, bool includeInnerFaultTraceText)
    {
      StringBuilder detailsStringBuilder = this.TraceDetailsStringBuilder;
      if (exc != null)
      {
        detailsStringBuilder.AppendLine($"Message: {exc.Message}");

        if (includeStackTrace)
        {
          detailsStringBuilder.AppendLine("--- Stack Trace ---");
          detailsStringBuilder.AppendLine(exc.StackTrace);
          detailsStringBuilder.AppendLine("-------------------");
        }
      }
      else
      {
        detailsStringBuilder.AppendLine("Trace only (no exceptions)");
        return;
      }

      var faultException = exc as FaultException<OrganizationServiceFault>;
      if (faultException == null)
      {
        if (exc is TimeoutException)
        {
          detailsStringBuilder.Insert(0, "--- (Timeout Exception) ---");
        }

        return;
      }

      var orgServiceFault = faultException.Detail;
      this.AddTraceDetailsInfo(orgServiceFault, includeInnerFaultTraceText);
    }

    /// <summary>
    /// Adds the message description for <see cref="OrganizationServiceFault" />.
    /// </summary>
    /// <param name="orgServiceFault">The org service fault.</param>
    /// <param name="includeInnerFaultTraceText">if set to <c>true</c> [include inner fault trace text].</param>
    /// <param name="depth">The depth.</param>
    /// <exception cref="System.ArgumentNullException">orgServiceFault</exception>
    protected virtual void AddTraceDetailsInfo(OrganizationServiceFault orgServiceFault, bool includeInnerFaultTraceText, int depth = 0)
    {
      if (orgServiceFault == null)
      {
        throw new ArgumentNullException(nameof(orgServiceFault));
      }

      StringBuilder detailsStringBuilder = this.TraceDetailsStringBuilder;
      detailsStringBuilder.AppendLine($"Detail Error Code: {orgServiceFault.ErrorCode}");
      detailsStringBuilder.AppendLine($"Detail Timestamp: {orgServiceFault.Timestamp}");
      detailsStringBuilder.AppendLine($"Detail Message: {orgServiceFault.Message}");

      if (includeInnerFaultTraceText)
      {
        this.TracingService.Trace($"- INNER FAULT {depth} START -");
        this.TracingService.Trace(orgServiceFault.TraceText);
        this.TracingService.Trace($"- INNER FAULT {depth} END -");
      }

      if (orgServiceFault.ErrorDetails.Count > 0)
      {
        detailsStringBuilder.AppendLine("- ERROR DETAILS START -");
        foreach (var errorDetail in orgServiceFault.ErrorDetails)
        {
          detailsStringBuilder.AppendLine($" {errorDetail.Key,20} = {errorDetail.Value}");
        }

        detailsStringBuilder.AppendLine("- ERROR DETAILS END -");
      }

      if (orgServiceFault.InnerFault == null)
      {
        return;
      }

      depth++;
      detailsStringBuilder.AppendLine($"- INNER FAULT {depth} START -");
      this.AddTraceDetailsInfo(orgServiceFault.InnerFault, includeInnerFaultTraceText, depth);
      detailsStringBuilder.AppendLine($"- INNER FAULT {depth} END -");
    }

    protected virtual string GetTraceDetails(Exception e)
    {
      return PrepareMemoValue(this.TraceDetailsStringBuilder);
    }

    protected virtual ITracingService BuildTracingService()
    {
      return this.SystemTracingService;
    }

    protected IPluginContext PluginCtx { get; private set; }

    public void LogContext()
    {
      if (this.PluginCtx.CustomVerboseLogSwitchedOn)
      {
        this.LogContextAndHandleException(null);
      }
    }

    public void HandleException(Exception exc)
    {
      if (exc == null)
      {
        throw new ArgumentNullException(nameof(exc));
      }

      this.LogContextAndHandleException(exc);
    }

    protected abstract void LogContextAndHandleException(Exception exc);

    protected static string SerializeAndPrepareForMemoField(object value)
    {
      return PrepareMemoValue(JsonConvert.SerializeObject(value));
    }

    protected static string GuidToString(Guid value)
    {
      return value.ToString("D");
    }

    protected static string GuidToString(Guid? value)
    {
      return value?.ToString("D");
    }

    protected static string PrepareMemoValue(string value)
    {
      return value.Length > MaxMemoFieldLength ? value.Substring(0, MaxMemoFieldLength) : value;
    }

    protected static string PrepareMemoValue(StringBuilder value)
    {
      return value.Length > MaxMemoFieldLength ? value.ToString(0, MaxMemoFieldLength) : value.ToString();
    }

    protected static EntityReference GetUserEntityReference(Guid userId)
    {
      return new EntityReference("systemuser", userId);
    }
  }
}

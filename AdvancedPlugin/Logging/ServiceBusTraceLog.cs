namespace AdvancedPlugin.Logging
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using Exceptions;
  using TraceService;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Messages;

  public class ServiceBusTraceLog : CustomTracingServiceTraceLogBase
  {
    private readonly Guid traceServiceBusEndpointId;

    public ServiceBusTraceLog(IPluginContext pluginCtx, ITracingService systemTracingService, Guid traceServiceBusEndpointId)
      : base(pluginCtx, systemTracingService)
    {
      this.traceServiceBusEndpointId = traceServiceBusEndpointId;
    }

    protected override void LogContextAndHandleException(Exception exc)
    {
      this.AddTraceDetailsInfo(exc);
      var traceDetails = this.GetTraceDetails(exc);
      var includeTraceText = true;
      var customVerboseLogSwitchedOn = this.PluginCtx.CustomVerboseLogSwitchedOn;
      var saveLog = true;
      InvalidPluginExecutionException exceptionToThrow;
      if (exc == null)
      {
        exceptionToThrow = null;
        saveLog = customVerboseLogSwitchedOn;
      }
      else if (exc is InvalidPluginExecutionException)
      {
        exceptionToThrow = (InvalidPluginExecutionException)exc;
        saveLog = customVerboseLogSwitchedOn;
      }
      else if (exc is PluginBusinessLogicException)
      {
        exceptionToThrow = new InvalidPluginExecutionException(exc.Message);
        saveLog = customVerboseLogSwitchedOn;
      }
      else if (exc is PluginBusinessLogicExceptionWithSimpleLogging)
      {
        exceptionToThrow = new InvalidPluginExecutionException(exc.Message);
        includeTraceText = false;
      }
      else if (exc is PluginBusinessLogicExceptionWithFullLogging)
      {
        exceptionToThrow = new InvalidPluginExecutionException(exc.Message);
      }
      else if (exc is PluginIgnoredExceptionWithLogging)
      {
        exceptionToThrow = null;
      }
      else
      {
        var logId = Guid.NewGuid();
        traceDetails = $"\r\nSorry, an error occurred.\r\nError ID: {logId}.\r\n\r\n{traceDetails}";
        exceptionToThrow = new InvalidPluginExecutionException(traceDetails);
      }

      if (saveLog)
      {
        var pluginCtx = this.PluginCtx;
        includeTraceText = includeTraceText || customVerboseLogSwitchedOn;
        const string CustomTraceTextParamName = "CustomTraceTextParam";
        if (includeTraceText)
        {
          var traceText = this.CustomTracingService.GetTraceInfo(takeCached: true);
          pluginCtx.SetOutputParameter(CustomTraceTextParamName, traceText);
        }

        const string CustomTraceDetailsParamName = "CustomTraceDetailsParam";
        if (exceptionToThrow != null)
        {
          pluginCtx.SetOutputParameter(CustomTraceDetailsParamName, traceDetails); 
        }

        pluginCtx.NotifyServiceBusEndpoint(this.traceServiceBusEndpointId);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        var removeCustomTraceTextParamFromContext = exceptionToThrow == null && includeTraceText;
        if (removeCustomTraceTextParamFromContext)
        {
          pluginCtx.ExecContext.OutputParameters.Remove(CustomTraceTextParamName);
        }
      }

      if (exceptionToThrow != null)
      {
        throw exceptionToThrow;
      }
    }
  }
}
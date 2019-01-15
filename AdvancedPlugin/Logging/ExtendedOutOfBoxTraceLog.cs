namespace AdvancedPlugin.Logging
{
  using System;

  using AdvancedPlugin.Constants;
  using AdvancedPlugin.Plugins;

  using Exceptions;

  using Microsoft.Xrm.Sdk;

  public class ExtendedOutOfBoxTraceLog : TraceLogBase
  {
    public ExtendedOutOfBoxTraceLog(IPluginContextBase pluginCtx, ITracingService systemTracingService)
      : base(pluginCtx, systemTracingService)
    {
    }

    protected override void LogContextAndHandleException(Exception exc)
    {
      this.AddTraceDetailsInfo(exc, false, true);

      var traceDetails = this.GetTraceDetails(exc);
      var customVerboseLogSwitchedOn = this.PluginCtx.CustomVerboseLogSwitchedOn;
      var saveLog = true;
      InvalidPluginExecutionException exceptionToThrow;

      var saveTraceDetailsToTraceText = true;
      var saveParamsImagesSharedVarsToTraceText = true;

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
        saveParamsImagesSharedVarsToTraceText = customVerboseLogSwitchedOn;
        saveTraceDetailsToTraceText = false;
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
        var pluginExecCtx = pluginCtx.ExecContext;
        this.TracingService.Trace("- CURRENT CONTEXT INFO -");
        this.TracingService.Trace($"PrimaryEntityId: {pluginExecCtx.PrimaryEntityId}");
        this.TracingService.Trace($"PrimaryEntityName: {pluginExecCtx.PrimaryEntityName}");
        this.TracingService.Trace($"Stage: {pluginExecCtx.Stage}");
        this.TracingService.Trace($"Depth: {pluginExecCtx.Depth}");
        this.TracingService.Trace($"UserId: {pluginExecCtx.UserId}");
        this.TracingService.Trace($"InitiatingUserId: {pluginExecCtx.InitiatingUserId}");

        if (saveParamsImagesSharedVarsToTraceText)
        {
          this.TracingService.Trace(
            $"\r\nINPUT PARAMETERS\r\n: {SerializeAndPrepareForMemoField(pluginExecCtx.InputParameters)}");
          this.TracingService.Trace(
            $"OUTPUT PARAMETERS\r\n: {SerializeAndPrepareForMemoField(pluginExecCtx.OutputParameters)}");
          this.TracingService.Trace(
            $"POST IMAGES\r\n: {SerializeAndPrepareForMemoField(pluginExecCtx.PostEntityImages)}");
          this.TracingService.Trace(
            $"PRE IMAGES\r\n: {SerializeAndPrepareForMemoField(pluginExecCtx.PreEntityImages)}");
          this.TracingService.Trace(
            $"SHARED VARIABLES\r\n: {SerializeAndPrepareForMemoField(pluginExecCtx.SharedVariables)}");
        }

        var parentExecCtx = pluginExecCtx.ParentContext;
        if (parentExecCtx != null)
        {
          this.TracingService.Trace("\r\n- PARENT CONTEXT INFO -");
          this.TracingService.Trace($"MessageName: {parentExecCtx.MessageName}");
          this.TracingService.Trace($"PrimaryEntityName: {parentExecCtx.PrimaryEntityName}");
          this.TracingService.Trace($"PrimaryEntityId: {parentExecCtx.PrimaryEntityId}");
          this.TracingService.Trace($"Stage: {parentExecCtx.Stage}");
          this.TracingService.Trace($"Mode: {parentExecCtx.Mode}");
          this.TracingService.Trace($"UserId: {parentExecCtx.UserId}");
          this.TracingService.Trace($"InitiatingUserId: {parentExecCtx.InitiatingUserId}");
          this.TracingService.Trace($"IsInTransaction: {parentExecCtx.IsInTransaction}");
          this.TracingService.Trace($"OwningExtension: {parentExecCtx.OwningExtension?.Name}");
          this.TracingService.Trace(
            $"Shared Variables\r\n: {SerializeAndPrepareForMemoField(parentExecCtx.SharedVariables)}");

          var traceGrandParentCtx =
            parentExecCtx.Stage == (int)Stage.MainOperation && parentExecCtx.ParentContext != null;
          if (traceGrandParentCtx)
          {
            var grandParentCtx = parentExecCtx.ParentContext;
            this.TracingService.Trace("\r\n- GRAND PARENT CONTEXT INFO -");
            this.TracingService.Trace($"MessageName: {grandParentCtx.MessageName}");
            this.TracingService.Trace($"PrimaryEntityName: {grandParentCtx.PrimaryEntityName}");
            this.TracingService.Trace($"PrimaryEntityId: {grandParentCtx.PrimaryEntityId}");
            this.TracingService.Trace($"Stage: {grandParentCtx.Stage}");
            this.TracingService.Trace($"Mode: {grandParentCtx.Mode}");
            this.TracingService.Trace($"UserId: {grandParentCtx.UserId}");
            this.TracingService.Trace($"InitiatingUserId: {grandParentCtx.InitiatingUserId}");
            this.TracingService.Trace($"IsInTransaction: {grandParentCtx.IsInTransaction}");
            this.TracingService.Trace($"OwningExtension: {grandParentCtx.OwningExtension?.Name}");
            this.TracingService.Trace(
              $"Shared Variables\r\n: {SerializeAndPrepareForMemoField(grandParentCtx.SharedVariables)}");
          }
        }

        if (saveTraceDetailsToTraceText)
        {
          this.TracingService.Trace("\r\n- TRACE DETAILS -");
          this.TracingService.Trace(traceDetails);
        }
      }

      if (exceptionToThrow != null)
      {
        throw exceptionToThrow;
      }
    }
  }
}
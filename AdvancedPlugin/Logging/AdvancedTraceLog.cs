namespace AdvancedPlugin.Logging
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using Exceptions;
  
  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Messages;

  public class AdvancedTraceLog : CustomTracingServiceTraceLogBase
  {
    public AdvancedTraceLog(IPluginContext pluginCtx, ITracingService systemTracingService)
      : base(pluginCtx, systemTracingService)
    {
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    /// <summary>
    /// Builds the advanced plugin execute context log.
    /// </summary>
    /// <param name="pluginExecCtx">The plugin execute CTX.</param>
    /// <param name="execContextLogs">The execute context logs.</param>
    private static void BuildAdvancedPluginExecContextLog(IPluginExecutionContext pluginExecCtx, IList<Entity> execContextLogs)
    {
      if (pluginExecCtx == null)
      {
        return;
      }

      BuildAdvancedPluginExecContextLog(pluginExecCtx.ParentContext, execContextLogs);
      var parentCtxLogRef = execContextLogs.LastOrDefault()?.ToEntityReference();
      var pluginCtxLog = ConvertToAdvancedPluginTrace(pluginExecCtx, true);
      pluginCtxLog["pavelkh_isparentcontext"] = true;
      pluginCtxLog["pavelkh_parentcontextref"] = parentCtxLogRef;
      pluginCtxLog["pavelkh_subject"] =
        $"Ctx: {pluginExecCtx.MessageName} - Stage:{pluginExecCtx.Stage} - Mode:{pluginExecCtx.Mode} - {pluginExecCtx.PrimaryEntityName}";
      execContextLogs.Add(pluginCtxLog);
    }

    private static Entity ConvertToAdvancedPluginTrace(IPluginExecutionContext pluginExecCtx, bool saveParamsImagesSharedVars, Guid? logId = null)
    {
      const string EntityLogicalName = "pavelkh_advancedplugintrace";
      var id = logId ?? Guid.NewGuid();
      var result = new Entity(EntityLogicalName, id)
      {
        ["pavelkh_advancedplugintraceid"] = id,
        ["pavelkh_primaryentityid"] = GuidToString(pluginExecCtx.PrimaryEntityId),
        ["pavelkh_primaryentityname"] = pluginExecCtx.PrimaryEntityName,
        ["pavelkh_secondaryentityname"] = pluginExecCtx.SecondaryEntityName,
        ["pavelkh_message"] = pluginExecCtx.MessageName,
        ["pavelkh_stage"] = new OptionSetValue(pluginExecCtx.Stage),
        ["pavelkh_mode"] = new OptionSetValue(pluginExecCtx.Mode),
        ["pavelkh_depth"] = pluginExecCtx.Depth,
        ["pavelkh_userref"] = GetUserEntityReference(pluginExecCtx.UserId),
        ["pavelkh_initiatinguserref"] = GetUserEntityReference(pluginExecCtx.InitiatingUserId),
        ["pavelkh_isolationmode"] = pluginExecCtx.IsolationMode,
        ["pavelkh_correlationid"] = GuidToString(pluginExecCtx.CorrelationId),
        ["pavelkh_isintransaction"] = pluginExecCtx.IsInTransaction,
        ["pavelkh_requestid"] = GuidToString(pluginExecCtx.RequestId),
        ["pavelkh_operationid"] = GuidToString(pluginExecCtx.OperationId),
        ["pavelkh_operationcreatedon"] = pluginExecCtx.OperationCreatedOn,
        ["pavelkh_owningextensionid"] = GuidToString(pluginExecCtx.OwningExtension?.Id),
        ["pavelkh_owningextensionname"] = pluginExecCtx.OwningExtension?.Name,
        ["pavelkh_businessunitid"] = GuidToString(pluginExecCtx.BusinessUnitId)
      };
      
      if (!saveParamsImagesSharedVars)
      {
        return result;
      }

      result["pavelkh_inputparameters"] = SerializeAndPrepareForMemoField(pluginExecCtx.InputParameters);
      result["pavelkh_outputparameters"] = SerializeAndPrepareForMemoField(pluginExecCtx.OutputParameters);
      result["pavelkh_sharedvariables"] = SerializeAndPrepareForMemoField(pluginExecCtx.SharedVariables);
      result["pavelkh_preentityimages"] = SerializeAndPrepareForMemoField(pluginExecCtx.PreEntityImages);
      result["pavelkh_postentityimages"] = SerializeAndPrepareForMemoField(pluginExecCtx.PostEntityImages);

      return result;
    }

    protected override void LogContextAndHandleException(Exception exc)
    {
      this.AddTraceDetailsInfo(exc);
      var traceDetails = this.GetTraceDetails(exc);
      var includeTraceText = true;
      var customVerboseLogSwitchedOn = this.PluginCtx.CustomVerboseLogSwitchedOn;
      var error = exc != null;
      var saveLog = true;
      InvalidPluginExecutionException exceptionToThrow;
      Guid? logId = null;
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
        logId = Guid.NewGuid();
        traceDetails = $"\r\nSorry, an error occurred.\r\nError ID: {logId}.\r\n\r\n{traceDetails}";
        exceptionToThrow = new InvalidPluginExecutionException(traceDetails);
      }

      if (saveLog)
      {
        var pluginCtx = this.PluginCtx;
        var pluginExecCtx = pluginCtx.ExecContext;
        var logs = new List<Entity>();
        string traceText = null;
        includeTraceText = includeTraceText || customVerboseLogSwitchedOn;
        if (includeTraceText)
        {
          traceText = this.CustomTracingService.GetTraceInfo(takeCached: true);
        }

        var includeParentContextInfo = includeTraceText;
        if (includeParentContextInfo)
        {
          BuildAdvancedPluginExecContextLog(pluginExecCtx.ParentContext, logs);
        }

        // ReSharper disable RedundantArgumentName
        var currentCtxLog = 
          ConvertToAdvancedPluginTrace(pluginExecCtx, saveParamsImagesSharedVars: includeTraceText, logId: logId);
        // ReSharper restore RedundantArgumentName
        var parentCtxLogRef = logs.LastOrDefault()?.ToEntityReference();
        currentCtxLog["pavelkh_traceid"] = GuidToString(currentCtxLog.Id);
        currentCtxLog["pavelkh_parentcontextref"] = parentCtxLogRef;
        currentCtxLog["pavelkh_subject"] = 
          $"Plugin {(error ? "Error" : "Trace")} on {pluginCtx.OrganizationName} ({pluginCtx.OrganizationId})";
        currentCtxLog["pavelkh_tracedetails"] = traceDetails;
        currentCtxLog["pavelkh_tracetext"] = traceText;

        logs.Add(currentCtxLog);

        this.SaveLogs(logs);
      }

      if (exceptionToThrow != null)
      {
        throw exceptionToThrow;
      }
    }

    private void SaveLogs(IList<Entity> logs)
    {
      var executeMultipleRequest = new ExecuteMultipleRequest
      {
        Requests = new OrganizationRequestCollection(),
        Settings = new ExecuteMultipleSettings
        {
          ReturnResponses = true,
          ContinueOnError = true
        }
      };

      var orgService = this.PluginCtx.OrgServiceOnBehalfOfSystemUser;
      var clearParentRef = false;
      foreach (var log in logs)
      {
        if (clearParentRef)
        {
          log["pavelkh_parentcontextref"] = null;
          clearParentRef = false;
        }

        try
        {
          executeMultipleRequest.Requests.Clear();
          executeMultipleRequest.Requests.Add(new CreateRequest { Target = log });
          var executeMultipleResponse = (ExecuteMultipleResponse)orgService.Execute(executeMultipleRequest);
          if (executeMultipleResponse.IsFaulted)
          {
            clearParentRef = true;
          }
        }
        catch (Exception)
        {
          clearParentRef = true;
          // ignored
        }
      }

    }
  }
}
namespace AdvancedPlugin.Managers
{
  using System;
  using System.Linq;

  using AdvancedPlugin.Constants;
  using AdvancedPlugin.Exceptions;
  using AdvancedPlugin.Logging;
  using AdvancedPlugin.Utils;
  using AdvancedPlugin.Plugins;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Client;

  /// <summary>
  /// Base Manager class
  /// </summary>
  /// <typeparam name="TEntity">A type that inherits from the <see cref="Entity" /></typeparam>
  /// <typeparam name="TContext">A type that inherits from the <see cref="OrganizationServiceContext" /></typeparam>
  /// <typeparam name="TTraceLog">The type of the trace log inherited from the <see cref="TraceLogBase"/>.</typeparam>
  public abstract class ManagerBase<TEntity, TContext, TTraceLog> 
    where TEntity : Entity, new()
    where TContext : OrganizationServiceContext
    where TTraceLog : TraceLogBase
  {
    protected readonly PluginBase<TEntity, TContext, TTraceLog>.PluginContext PluginCtx;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerBase{TEntity, TContext, TTraceLog}" /> class.
    /// </summary>
    /// <param name="pluginCtx">The local context.</param>
    protected ManagerBase(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginCtx)
    {
      this.PluginCtx = pluginCtx;
    }

    protected virtual int DefaultStateCode => 0;

    protected virtual int DefaultStatusCode => 1;

    /// <summary>
    /// Defines the protected field validation rules.
    /// </summary>
    /// <returns>A set of protected field validation rules.</returns>
    protected virtual ProtectedFieldValidationRule[] GetProtectedFieldValidationRules()
    {
      return null;
    }

    /// <summary>
    /// Rejects the protected fields change.
    /// </summary>
    /// <exception cref="PluginBusinessLogicExceptionWithFullLogging">It is not allowed to set a value for the field directly.</exception>
    protected void RejectProtectedFieldsChange()
    {
      this.PluginCtx.TraceWithElapsedTimeEnter(nameof(this.RejectProtectedFieldsChange));
      var rules = this.GetProtectedFieldValidationRules();
      if (rules == null)
      {
        return;
      }

      var pluginCtx = this.PluginCtx;
      pluginCtx.Trace("Validating Protected Fields change.");
      rules = pluginCtx.IsCreateMode
                ? rules.Where(r => r.OnCreate).ToArray() 
                : rules.Where(r => r.OnUpdate).ToArray();

      foreach (var rule in rules)
      {
        this.RejectFieldChange(rule.LogicalName, rule.GetAllowUpdate);
      }

      this.PluginCtx.TraceWithElapsedTimeExit(nameof(this.RejectProtectedFieldsChange));
    }

    /// <summary>
    /// Rejects the field change.
    /// </summary>
    /// <param name="logicalName">Field logical name.</param>
    /// <param name="getAllowUpdate">An additional function returning <c>true</c> if the field update is allowed.</param>
    /// <exception cref="PluginBusinessLogicExceptionWithFullLogging">It is not allowed to set a value for the '{logicalName }' field directly.</exception>
    protected void RejectFieldChange(string logicalName, Func<bool> getAllowUpdate = null)
    {
      var fieldSpecified = this.PluginCtx.TargetEntityAsBaseEntity.GetIsFieldSpecified(logicalName);
      if (!fieldSpecified)
      {
        return;
      }

      var allowUpdate = getAllowUpdate?.Invoke() ?? false;
      if (!allowUpdate)
      {
        throw new PluginBusinessLogicExceptionWithFullLogging($"It is not allowed to set a value for the '{ logicalName }' field directly.");
      }
    }

    /// <summary>
    /// Rejects the field change.
    /// </summary>
    /// <param name="logicalNames">List of fields logical names.</param>
    /// <param name="getAllowUpdate">An additional function returning <c>true</c> if the field update is allowed.</param>
    /// <exception cref="PluginBusinessLogicExceptionWithFullLogging">It is not allowed to set a value for the '{logicalName }' field directly.</exception>
    protected void RejectFieldsChange(string[] logicalNames, Func<bool> getAllowUpdate = null)
    {
      var isAnyFieldSpecified = this.PluginCtx.TargetEntityAsBaseEntity.GetIsAnyFieldSpecified(logicalNames);
      if (!isAnyFieldSpecified)
      {
        return;
      }

      var allowUpdate = getAllowUpdate?.Invoke() ?? false;
      if (allowUpdate)
      {
        return;
      }

      var fieldNames = string.Join("', '", logicalNames);
      throw new PluginBusinessLogicExceptionWithFullLogging($"It is not allowed to set a value for the '{ fieldNames }' field(s) directly.");
    }
    /// <summary>
    /// Rejects the Owner change.
    /// </summary>
    /// <param name="getAllowUpdate">An optional additional function returning <c>true</c> if the field update is allowed.</param>
    protected void RejectOwnerChange(Func<bool> getAllowUpdate = null)
    {
      this.RejectFieldChange("ownerid", getAllowUpdate);
    }

    /// <summary>
    /// Rejects the owner change.
    /// </summary>
    /// <param name="getAllowUpdate">The get allow update.</param>
    protected void RejectOwnerChange(Func<IPluginContext<TContext>, bool> getAllowUpdate)
    {
      this.RejectOwnerChange(() => getAllowUpdate(this.PluginCtx));
    }

    /// <summary>
    /// Rejects the Status change.
    /// </summary>
    /// <param name="getAllowUpdate">An optional additional function returning <c>true</c> if the field update is allowed.</param>
    protected void RejectStatusChange(Func<bool> getAllowUpdate = null)
    {
      this.RejectFieldChange("statecode", getAllowUpdate);
    }

    /// <summary>
    /// Rejects the Status Reason change.
    /// </summary>
    /// <param name="getAllowUpdate">An optional additional function returning <c>true</c> if the field update is allowed.</param>
    protected void RejectStatusReasonChange(Func<bool> getAllowUpdate = null)
    {
      this.RejectFieldChange("statuscode", getAllowUpdate);
    }

    /// <summary>
    /// Rejects the Status and Status Reason change.
    /// </summary>
    /// <param name="getAllowUpdate">An optional additional function returning <c>true</c> if the field update is allowed.</param>
    protected virtual void RejectStatusAndStatusReasonChange(Func<bool> getAllowUpdate = null)
    {
      var fields = new[]{"statecode", "statuscode"};
      if (getAllowUpdate == null)
      {
        getAllowUpdate = this.GetIsStatusAndStatusReasonUpdateIsAllowed;
      }

      this.RejectFieldsChange(fields, getAllowUpdate);
    }

    /// <summary>
    /// Gets the is status and status reason update is allowed.
    /// </summary>
    /// <returns></returns>
    protected virtual bool GetIsStatusAndStatusReasonUpdateIsAllowed()
    {
      var pluginCtx = this.PluginCtx;
      if (!pluginCtx.IsCreateMode)
      {
        return false;
      }

      var targetEntity = pluginCtx.TargetEntityAsBaseEntity;
      var status = targetEntity.GetAttrValue<OptionSetValue>("statecode");
      var statusReason = targetEntity.GetAttrValue<OptionSetValue>("statuscode");
      return status != null 
              && status.Value == this.DefaultStateCode
              && statusReason != null 
              && statusReason.Value == this.DefaultStatusCode;
    }

    /// <summary>
    /// Validates if the mandatory fields are specified. 
    /// </summary>
    /// <remarks>
    /// Validation is based on the data in the Target and PreImage
    /// </remarks>
    /// <param name="fields">The mandatory fields.</param>
    /// <exception cref="PluginBusinessLogicException"></exception>
    protected void ValidateForMandatoryFields(params string[] fields)
    {
      var pluginCtx = this.PluginCtx;
      pluginCtx.Trace(nameof(this.ValidateForMandatoryFields));
      var target = pluginCtx.TargetEntityAsBaseEntity;
      var preImage = pluginCtx.PreImageAsBaseEntity;

      foreach (var field in fields)
      {
        var isSpecified = target.Attributes.ContainsKey(field);
        bool invalid;
        if (isSpecified)
        {
          var value = target.GetAttributeValue<object>(field);
          var isSetToNull = value == null || value.ToString() == string.Empty;
          invalid = isSetToNull;
        }
        else
        {
          var preImageValueIsNull = preImage?.GetAttributeValue<object>(field) == null;
          invalid = preImageValueIsNull;
        }

        if (!invalid)
        {
          continue;
        }
        
        throw new PluginBusinessLogicException(
          $"It is not possible to save the record as the following mandatory field is not provided: {field}");
      }
    }

    /// <summary>
    /// Validates for expected empty fields.
    /// </summary>
    /// <param name="fields">The fields.</param>
    /// <exception cref="PluginBusinessLogicException"></exception>
    protected void ValidateForExpectedEmptyFields(params string[] fields)
    {
      var pluginCtx = this.PluginCtx;
      pluginCtx.Trace(nameof(this.ValidateForExpectedEmptyFields));
      var target = pluginCtx.TargetEntityAsBaseEntity;
      var preImage = pluginCtx.PreImageAsBaseEntity;

      foreach (var field in fields)
      {
        var isSpecified = target.Attributes.ContainsKey(field);
        bool invalid;
        if (isSpecified)
        {
          var value = target.GetAttributeValue<object>(field);
          var isSetToNull = value == null || value.ToString() == string.Empty;
          invalid = !isSetToNull;
        }
        else
        {
          var value = preImage?.GetAttributeValue<object>(field);
          var preImageValueIsNull = value == null;
          invalid = !preImageValueIsNull;
        }

        if (!invalid)
        {
          continue;
        }

        throw new PluginBusinessLogicException(
          $"It is not possible to save the record as the following field is expected to be empty: {field}");
      }
    }

    /// <summary>
    /// Adds the field to log record.
    /// </summary>
    /// <param name="logEntity">The log entity.</param>
    /// <param name="targetFieldLogicalName">Name of the target field logical.</param>
    /// <param name="logCoreFieldLogicalName">Name of the log core field logical.</param>
    /// <param name="logOld">if set to <c>true</c> [log old].</param>
    /// <param name="logNew">if set to <c>true</c> [log new].</param>
    protected void AddFieldToLogRecord(
      Entity logEntity, string targetFieldLogicalName, string logCoreFieldLogicalName = null, 
      bool logOld = true, bool logNew = true)
    {
      this.AddFieldToLogRecord<object, object>(logEntity, targetFieldLogicalName, logCoreFieldLogicalName, null, logOld, logNew);
    }
    /// <summary>
    /// Adds the field to log record.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TK">The type of the k.</typeparam>
    /// <param name="logEntity">The log entity.</param>
    /// <param name="targetFieldLogicalName">Name of the target field logical.</param>
    /// <param name="logCoreFieldLogicalName">Name of the log core field logical.</param>
    /// <param name="converter">The converter.</param>
    /// <param name="logOld">if set to <c>true</c> [log old].</param>
    /// <param name="logNew">if set to <c>true</c> [log new].</param>
    protected void AddFieldToLogRecord<T, TK>(
      Entity logEntity, string targetFieldLogicalName, string logCoreFieldLogicalName = null, Func<T, TK> converter = null,  
      bool logOld = true, bool logNew = true) {
      var pluginCtx = this.PluginCtx;
      pluginCtx.Trace(nameof(AddFieldToLogRecord));
      pluginCtx.TraceValue(nameof(logCoreFieldLogicalName), logCoreFieldLogicalName, nameof(targetFieldLogicalName), targetFieldLogicalName);

      var oldValueBase = pluginCtx.PreImageEntity.GetAttributeValue<T>(targetFieldLogicalName);
      var oldValue = converter == null ? (object)oldValueBase : converter(oldValueBase);
      if (logOld)
      {
        var attrName = $"{logCoreFieldLogicalName ?? targetFieldLogicalName}_old";
        logEntity.SetAttrValue(attrName, oldValue);
      }

      // ReSharper disable once InvertIf
      if (logNew)
      {
        var targetEntity = pluginCtx.TargetEntityAsBaseEntity;
        var newValueBase = targetEntity.GetIsFieldSpecified(targetFieldLogicalName)
                             ? targetEntity.GetAttributeValue<T>(targetFieldLogicalName)
                             : oldValueBase;
        var newValue = converter == null ? (object)newValueBase : converter(newValueBase);
        var attrName = $"{logCoreFieldLogicalName ?? targetFieldLogicalName}_new";
        logEntity.SetAttrValue(attrName, newValue);
      }
    }

    /// <summary>
    /// Validates the by base validators.
    /// </summary>
    protected virtual void ValidateByBaseValidators()
    {
      switch (this.PluginCtx.MessageName)
      {
        case MessageName.Update:
          this.ValidateTargetEntityAsBaseEntity();
          break;
        case MessageName.Create:
          this.ValidateTargetEntityAsBaseEntity();
          break;
        case MessageName.Delete:
          this.ValidateTargetEntityReference();
          break;
        case MessageName.Retrieve:
          this.ValidateTargetEntityReference();
          this.ValidateColumnSet();
          if (this.PluginCtx.IsPostOperation)
          {
            this.ValidateOutputBusinessEntityAsBaseEntity();
          }

          break;

          // TODO: Add base validators for other messages
      }
    }

    /// <summary>
    /// Validates the target entity as base entity.
    /// </summary>
    protected void ValidateTargetEntityAsBaseEntity()
    {
      ValidateTargetEntityAsBaseEntity(this.PluginCtx);
    }

    /// <summary>
    /// Validates the target entity reference.
    /// </summary>
    protected void ValidateTargetEntityReference()
    {
      ValidateTargetEntityReference(this.PluginCtx);
    }

    /// <summary>
    /// Validates the pre image.
    /// </summary>
    protected void ValidatePreImage()
    {
      ValidatePreImage(this.PluginCtx);
    }

    /// <summary>
    /// Validates the post image.
    /// </summary>
    protected void ValidatePostImage()
    {
      ValidatePostImage(this.PluginCtx);
    }

    /// <summary>
    /// Validates the column set.
    /// </summary>
    protected void ValidateColumnSet()
    {
      ValidateColumnSet(this.PluginCtx);
    }

    /// <summary>
    /// Validates the output business entity as base entity.
    /// </summary>
    protected void ValidateOutputBusinessEntityAsBaseEntity()
    {
      ValidateOutputBusinessEntityAsBaseEntity(this.PluginCtx);
    }

    /// <summary>
    /// Validates the output business entity collection.
    /// </summary>
    protected void ValidateOutputBusinessEntityCollection()
    {
      ValidateOutputBusinessEntityCollection(this.PluginCtx);
    }

    /// <summary>
    /// Validates the unsecure configuration.
    /// </summary>
    protected void ValidateUnsecureConfiguration()
    {
      ValidateUnsecureConfiguration(this.PluginCtx);
    }

    /// <summary>
    /// Wraps a method in trace with elapsed time.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="name">The name of the "point".</param>
    protected void WrapInTraceWithElapsedTime(Action action, string name)
    {
      this.PluginCtx.WrapInTraceWithElapsedTime(action, name);
    }

    /// <summary>
    /// Validates the target as entity.
    /// </summary>
    /// <param name="pluginContext">The local context.</param>
    /// <exception cref="NullReferenceException">TargetEntityAsBaseEntity</exception>
    private static void ValidateTargetEntityAsBaseEntity(IPluginContextBase pluginContext)
    {
      if (pluginContext.TargetEntityAsBaseEntity == null)
      {
        throw new NullReferenceException(nameof(pluginContext.TargetEntityAsBaseEntity));
      }
    }
    
    /// <summary>
    /// Validates the target entity reference.
    /// </summary>
    /// <param name="pluginContext">The local context.</param>
    /// <exception cref="System.NullReferenceException">TargetAsEntityReference</exception>
    private static void ValidateTargetEntityReference(IPluginContextBase pluginContext)
    {
      if (pluginContext.TargetAsEntityReference == null)
      {
        throw new NullReferenceException(nameof(pluginContext.TargetAsEntityReference));
      }
    }

    /// <summary>
    /// Validates the pre image.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">PreImage</exception>
    private static void ValidatePreImage(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.PreImageEntity == null)
      {
        throw new NullReferenceException(nameof(pluginContext.PreImageEntity));
      }
    }

    /// <summary>
    /// Validates the post image.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">PostImage</exception>
    private static void ValidatePostImage(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.PostImageEntity == null)
      {
        throw new NullReferenceException(nameof(pluginContext.PostImageEntity));
      }
    }

    /// <summary>
    /// Validates the column set.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">PostImage</exception>
    private static void ValidateColumnSet(IPluginContextBase pluginContext)
    {
      if (pluginContext.ColumnSet == null)
      {
        throw new NullReferenceException(nameof(pluginContext.ColumnSet));
      }
    }

    /// <summary>
    /// Validates the output business entity as base entity.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">OutputBusinessEntityAsBaseEntity</exception>
    private static void ValidateOutputBusinessEntityAsBaseEntity(IPluginContextBase pluginContext)
    {
      if (pluginContext.OutputBusinessEntityAsBaseEntity == null)
      {
        throw new NullReferenceException(nameof(pluginContext.OutputBusinessEntityAsBaseEntity));
      }
    }

    /// <summary>
    /// Validates the output business entity collection.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">OutputBusinessEntityCollection</exception>
    private static void ValidateOutputBusinessEntityCollection(IPluginContextBase pluginContext)
    {
      if (pluginContext.OutputBusinessEntityCollection == null)
      {
        throw new NullReferenceException(nameof(pluginContext.OutputBusinessEntityCollection));
      }
    }

    /// <summary>
    /// Validates the unsecure configuration.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">OutputBusinessEntityCollection</exception>
    private static void ValidateUnsecureConfiguration(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.UnsecureConfig == null)
      {
        throw new NullReferenceException(nameof(pluginContext.UnsecureConfig));
      }
    }

    protected class ProtectedFieldValidationRule
    {
      public ProtectedFieldValidationRule(
        string logicalName, Func<bool> getAllowUpdate = null, bool onCreate = true, bool onUpdate = true)
      {
        this.LogicalName = logicalName;
        this.GetAllowUpdate = getAllowUpdate;
        this.OnCreate = onCreate;
        this.OnUpdate = onUpdate;
      }

      public string LogicalName { get; }

      public Func<bool> GetAllowUpdate { get; }

      public bool OnCreate { get; set; }

      public bool OnUpdate { get; set; }
    }

    /// <summary>
    /// Gets the if the current action is initiated by Merge.
    /// </summary>
    /// <returns></returns>
    protected bool GetIsInitiatedByMerge()
    {
      bool result;
      var pluginCtx = this.PluginCtx;
      if (pluginCtx.IsCreateMode)
      {
        result = false;
      }
      else
      {
        result =
          // ReSharper disable once RedundantToStringCall
          pluginCtx.ParentExecContext?.MessageName == MessageName.Merge.ToString();
      }

      return result;
    }

    /// <summary>Locks the record.</summary>
    /// <param name="entityReference">The entity reference.</param>
    /// <param name="dummyUpdatedFieldName">Name of the dummy updated field.</param>
    /// <exception cref="ArgumentNullException">entityReference</exception>
    protected void LockRecord(EntityReference entityReference, string dummyUpdatedFieldName)
    {
      this.PluginCtx.LockRecord(entityReference, dummyUpdatedFieldName);
    }

    /// <summary>
    /// Gets whether the current operation is initiated 'indirectly' (invoked by some other operation).
    /// </summary>
    /// <returns></returns>
    protected bool GetIsInitiatedByIndirectOperation()
    {
      return !this.PluginCtx.DepthIs1;
    }
  }
}

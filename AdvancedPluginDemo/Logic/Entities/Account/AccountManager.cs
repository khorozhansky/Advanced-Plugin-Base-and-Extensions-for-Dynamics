namespace AdvancedPluginDemo.Logic.Entities.Account
{
  using System;

  using AdvancedPlugin.Exceptions;
  using AdvancedPlugin.Utils;

  using AdvancedPluginDemo.Logic.Base;
  using AdvancedPluginDemo.Proxy;

  using Microsoft.Xrm.Sdk;

  public class AccountManager: ExtendedOutOfBoxTraceLogManagerBase<Account>
  {
    private Lazy<string> cachedDemoIndicatorSharedVariableValue;

    public AccountManager(Plugins.Bound.Account.Base.PluginContext pluginCtx)
      : base(pluginCtx)
    {
      this.cachedDemoIndicatorSharedVariableValue = new Lazy<string>(
        () =>
          {
            this.PluginCtx.Trace("Getting DemoIndicatorSharedVariableValue for the first time and before any reset...");
            this.PluginCtx.TryGetSharedVariableValue(nameof(this.DemoIndicatorSharedVariableValue), out string value);
            return value;
          });
    }

    private string DemoIndicatorSharedVariableValue
    {
      get
      {
        if (this.cachedDemoIndicatorSharedVariableValue.IsValueCreated)
        {
          this.PluginCtx.Trace("Getting cached DemoIndicatorSharedVariableValue...");
        }
        
        return this.cachedDemoIndicatorSharedVariableValue.Value;
      }
      set
      {
        this.PluginCtx.Trace("Resetting DemoIndicatorSharedVariableValue...");
        this.PluginCtx.SetSharedVariable(nameof(this.DemoIndicatorSharedVariableValue), value);
        this.cachedDemoIndicatorSharedVariableValue = new Lazy<string>(
          delegate
            {
              this.PluginCtx.Trace("Getting DemoIndicatorSharedVariableValue the first time after cache reset...");
              return value;
            });
      }
    }

    #region Create Message

    public void CreatePreValidationSync()
    {
      this.ValidateByBaseValidators();

      this.ValidateAccountName();

      this.ProcessDemoExceptionScenarios();
    }

    public void CreatePreOperationSync()
    {
      this.ValidateByBaseValidators();
      
      this.AdjustFaxValueWrapper();

      this.ProcessDemoExceptionScenarios();
    }

    public void CreatePostOperationSync()
    {
      this.ValidateByBaseValidators();

      this.ProcessDemoExceptionScenarios();
    }

    public void CreatePostOperationAsync()
    {
      this.ValidateByBaseValidators();

      this.ProcessDemoExceptionScenarios();
    }

    #endregion

    #region Update Message

    public void UpdatePreValidationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();

      this.ValidateAccountName();
      this.PreventUpdateInactive();

      this.ReadDemoSharedVariable();
      this.ReadDemoSharedVariable();
      this.SetDemoSharedVariable();
      this.ReadDemoSharedVariable();
      this.ReadDemoSharedVariable();

      this.ProcessDemoExceptionScenarios();
    }

    public void UpdatePreOperationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();

      this.AdjustFaxValueWrapper();

      this.ProcessDemoExceptionScenarios();

      this.WrapInTraceWithElapsedTime(
        this.PreventAssignToTeamIfParentAccountIsNotSpecified, nameof(this.PreventAssignToTeamIfParentAccountIsNotSpecified));

      this.WrapInTraceWithElapsedTime(
        this.PreventDeactivationIfOwnerIsAnotherUser, nameof(this.PreventDeactivationIfOwnerIsAnotherUser));

      this.WrapInTraceWithElapsedTime(this.TrackParentAccountSet, nameof(this.TrackParentAccountSet));
    }

    public void UpdatePostOperationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();

      this.ReadDemoSharedVariable();
      this.ReadDemoSharedVariable();
      this.SetDemoSharedVariable();
      this.ReadDemoSharedVariable();
      this.ReadDemoSharedVariable();

      this.ProcessDemoExceptionScenarios();

      this.WrapInTraceWithElapsedTime(
        this.CreatePostOnAssignToTeam, nameof(this.CreatePostOnAssignToTeam));

    }

    public void UpdatePostOperationAsync()
    {
      this.ValidateByBaseValidators();

      this.ReadDemoSharedVariable();
      this.ReadDemoSharedVariable();

      this.ProcessDemoExceptionScenarios();
    }

    #endregion

    private void SetDemoSharedVariable()
    {
      this.DemoIndicatorSharedVariableValue = $"{DateTime.Now:O} This is a set of a demo shared variable... (set on Stage:{this.PluginCtx.Stage})";
    }

    private void ReadDemoSharedVariable()
    {
      this.PluginCtx.TraceWithElapsedTime($"Reading {nameof(this.DemoIndicatorSharedVariableValue)}...");
      var value = this.DemoIndicatorSharedVariableValue;
      this.PluginCtx.TraceWithElapsedTime($"{nameof(this.DemoIndicatorSharedVariableValue)} = {value}");
    }

    private void ValidateAccountName()
    {
      var pluginCtx = this.PluginCtx;
      pluginCtx.TraceWithElapsedTime("Entering {0}", nameof(this.ValidateAccountName));
      var entityExt = pluginCtx.TargetEntityExt;
      var currentEntity = entityExt.CurrentEntity;
      var nameExt = entityExt.GetValue(() => currentEntity.Name);
      if (!nameExt.IsModified)
      {
        return;
      }

      const int MinNameLength = 4;
      var name = nameExt.NewValue;
      var invalid = string.IsNullOrWhiteSpace(name) || name.Length < MinNameLength;
      if (invalid)
      {
        throw new InvalidPluginExecutionException($"Account name \"{name}\" is incorrect. It should contain at least {MinNameLength} characters.");
      }
    }

    private void ProcessDemoExceptionScenarios()
    {
      var pluginCtx = this.PluginCtx;
      pluginCtx.TraceWithElapsedTime("Entering {0}", nameof(this.ProcessDemoExceptionScenarios));
      var target = pluginCtx.TargetEntity;
      if (string.IsNullOrWhiteSpace(target.Name))
      {
        return;
      }

      var value = target.Name;
      var stage = pluginCtx.Stage;
      var stageTitle = $"Stage:{stage}";
      if (!value.Contains(stageTitle))
      {
        return;
      }

      var mode = pluginCtx.Mode;
      var modeTitle = $"Mode:{mode}";
      if (!value.Contains(modeTitle))
      {
        return;
      }

      if (value.Contains("Generate InvalidPluginExecutionException"))
      {
        throw new InvalidPluginExecutionException(
          $"Hi, this is a demo InvalidPluginExecutionException exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate PluginBusinessLogicExceptionWithSimpleLogging"))
      {
        throw new PluginBusinessLogicExceptionWithSimpleLogging(
          $"Hi, this is a demo PluginBusinessLogicExceptionWithSimpleLogging exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate PluginBusinessLogicExceptionWithFullLogging"))
      {
        throw new PluginBusinessLogicExceptionWithFullLogging(
          $"Hi, this is a demo PluginBusinessLogicExceptionWithFullLogging exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate PluginBusinessLogicException"))
      {
        throw new PluginBusinessLogicException(
          $"Hi, this is a demo PluginBusinessLogicException exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate PluginExceptionWithDetailsInUiAndWithLogging"))
      {
        throw new PluginExceptionWithDetailsInUiAndWithLogging(
          $"Hi, this is a demo PluginExceptionWithDetailsInUiAndWithLogging exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate PluginIgnoredExceptionWithLogging"))
      {
        throw new PluginIgnoredExceptionWithLogging(
          $"Hi, this is a demo PluginIgnoredExceptionWithLogging exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate ApplicationException"))
      {
        throw new ApplicationException(
          $"Hi, this is a demo ApplicationException exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate Exception"))
      {
        throw new Exception($"Hi, this is a demo Exception exception message. ({stageTitle}, {modeTitle})");
      }
      else if (value.Contains("Generate FaultException<OrganizationServiceFault>"))
      {
        var record = new Entity("_DummyEntityName", Guid.Empty);
        this.PluginCtx.OrgService.Create(record);
      }
    }

    private void PreventUpdateInactive()
    {
      var pluginCtx = this.PluginCtx;
      pluginCtx.TraceWithElapsedTime("Entering {0}", nameof(this.PreventUpdateInactive));
      var targetEntityExt = pluginCtx.TargetEntityExt;
      var currentEntity = targetEntityExt.CurrentEntity;
      var validatedFields = new[] { Account.Fields.Name, Account.Fields.PrimaryContactId };
      var skipValidation = !currentEntity.GetIsAnyFieldSpecified(validatedFields);
      if (skipValidation)
      {
        return;
      }

      var stateCodeExt = targetEntityExt.GetValue(() => currentEntity.StateCode);
      var accountState = stateCodeExt.OldValue;
      switch (accountState)
      {
        case null:
          throw new NullReferenceException(nameof(accountState));
        case AccountState.Active:
          return;
      }

      var nameExt = targetEntityExt.GetValue(() => currentEntity.Name);
      var primaryContactRefExt = targetEntityExt.GetValue(() => currentEntity.PrimaryContactId);
      var keyFieldsModified = nameExt.IsModified || primaryContactRefExt.IsModified;
      if (keyFieldsModified)
      {
        throw new InvalidPluginExecutionException("It is not possible to change key fields of an inactive account.");
      }

      pluginCtx.TraceWithElapsedTime("Exiting {0}", nameof(this.PreventUpdateInactive));
    }

    private void PreventDeactivationIfOwnerIsAnotherUser()
    {
      var pluginCtx = this.PluginCtx;
      var entityExt = pluginCtx.TargetEntityExt;
      var stateCodeExt = entityExt.GetValue<AccountState?>(Account.Fields.StateCode);
      if (!stateCodeExt.OldValue.HasValue)
      {
        throw new NullReferenceException($"{Account.Fields.StateCode} field is not specified for the Pre-Image");
      }

      var deactivateAction = stateCodeExt.IsModified && stateCodeExt.NewValue == AccountState.Inactive;
      if (deactivateAction)
      {
        var currentEntity = entityExt.CurrentEntity;
        var ownerRefExt = entityExt.GetValue(() => currentEntity.OwnerId);
        if (ownerRefExt.OldValue == null)
        {
          throw new NullReferenceException($"{Account.Fields.OwnerId} field is not specified for the Pre-Image");
        }
           
        var currentUserRecord = ownerRefExt.Value != null && ownerRefExt.Value.Id.Equals(pluginCtx.UserId);
        if (!currentUserRecord)
        {
          throw new InvalidPluginExecutionException("You must be owner of the account record to deactivate it.");
        }
      }
    }

    private void AdjustFaxValueWrapper()
    {
      var pluginCtx = this.PluginCtx;
      const string MethodName = nameof(this.AdjustFaxValue);
      pluginCtx.TraceWithElapsedTime("Entering {0}", MethodName);
      this.AdjustFaxValue();
      pluginCtx.TraceWithElapsedTime("Exiting {0}", MethodName);
    }

    private void AdjustFaxValue()
    {
      var pluginCtx = this.PluginCtx;
      var entityExt = pluginCtx.TargetEntityExt;
      var currentEntity = entityExt.CurrentEntity;
      var faxExt = entityExt.GetValue(() => currentEntity.Fax);
      if (!faxExt.IsModified)
      {
        return;
      }

      var value = faxExt.Value;
      if (string.IsNullOrWhiteSpace(value))
      {
        currentEntity.Fax = "no info";
      }
      else
      {
        const string FaxPrefix = "***";
        if (!value.StartsWith(FaxPrefix))
        {
          currentEntity.Fax = $"{FaxPrefix}{value}";
        }
      }
    }

    private void TrackParentAccountSet()
    {
      var pluginCtx = this.PluginCtx;
      var targetEntityExt = pluginCtx.TargetEntityExt;
      var currentEntity = targetEntityExt.CurrentEntity;
      var parentAccountExt = targetEntityExt.GetValue(() => currentEntity.ParentAccountId);
      if (!parentAccountExt.IsModified)
      {
        return;
      }

      var newValue = parentAccountExt.NewValue;
      var text = newValue == null ? "Parent account has been removed from the account." : $"Parent account has been set to {newValue.Name}.";
      var post = new Post
                    {
                     RegardingObjectId = currentEntity.ToEntityReference(),
                     SourceEnum = Post_Source.ManualPost,
                     TypeEnum = Post_Type.News,
                     Text = text
                    };

      pluginCtx.OrgServiceOnBehalfOfSystemUser.Create(post);
    }

    private void PreventAssignToTeamIfParentAccountIsNotSpecified()
    {
      var pluginCtx = this.PluginCtx;
      var entityExt = pluginCtx.TargetEntityExt;
      var ownerIdValueExt = entityExt.GetValue<EntityReference>(Account.Fields.OwnerId);
      if (!ownerIdValueExt.IsModified)
      {
        return;
      }

      var ownerRef = ownerIdValueExt.NewValue;
      var ownerIsTeam = ownerRef.LogicalName.Equals(Team.EntityLogicalName);
      if (!ownerIsTeam)
      {
        return;
      }

      var parentAccountExt = entityExt.GetValue<EntityReference>(Account.Fields.ParentAccountId);
      if (parentAccountExt.IsNull)
      {
        throw new PluginBusinessLogicExceptionWithSimpleLogging("You cannot assign account to a team unless parent account is specified.");
      }
    }

    private void CreatePostOnAssignToTeam()
    {
      var pluginCtx = this.PluginCtx;
      var entityExt = pluginCtx.TargetEntityExt;
      var ownerIdValueExt = entityExt.GetValue<EntityReference>(Account.Fields.OwnerId);
      if (!ownerIdValueExt.IsModified)
      {
        return;
      }

      var ownerRef = ownerIdValueExt.NewValue;
      var ownerIsTeam = ownerRef.LogicalName.Equals(Team.EntityLogicalName);
      if (!ownerIsTeam)
      {
        return;
      }

      var post = new Post
                   {
                     RegardingObjectId = entityExt.ToEntityReference(),
                     SourceEnum = Post_Source.ManualPost,
                     TypeEnum = Post_Type.News,
                     Text = "News on behalf of current System User: assigned to a team."
                   };

      pluginCtx.OrgService.Create(post);

      post.Text = "News on behalf of Initiating User: assigned to a team.";
      pluginCtx.OrgServiceOnBehalfOfInitiatingUser.Create(post);
    }

  }
}

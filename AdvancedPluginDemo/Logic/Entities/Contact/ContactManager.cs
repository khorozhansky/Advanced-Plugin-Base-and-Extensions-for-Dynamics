namespace AdvancedPluginDemo.Logic.Entities.Contact
{
  using System;
  using System.Linq;
  using System.Text;

  using AdvancedPlugin.Extensions;
  using AdvancedPlugin.Utils;

  using AdvancedPluginDemo.Logic.Base;
  using AdvancedPluginDemo.Proxy;

  using Microsoft.Xrm.Sdk;

  public class ContactManager: ExtendedOutOfBoxTraceLogManagerBase<Contact>
  {
    private FieldExt<EntityReference, Contact> parentAccountRefExt;

    public ContactManager(Plugins.Bound.Contact.Base.PluginContext pluginCtx)
      : base(pluginCtx)
    {
    }

    private FieldExt<EntityReference, Contact> ParentAccountRefExt
    {
      get
      {
        if (this.parentAccountRefExt != null)
        {
          return this.parentAccountRefExt;
        }

        var pluginCtx = this.PluginCtx;
        var entityExt = pluginCtx.TargetEntityExt;
        this.parentAccountRefExt = entityExt.GetValue<EntityReference>(Contact.Fields.ParentCustomerId);
        return this.parentAccountRefExt;
      }
    }

    #region Create Message

    public void CreatePreValidationSync()
    {
      this.ValidateByBaseValidators();

      this.RejectProtectedFieldsChange();
      this.ValidateMandatoryFields();
    }

    public void CreatePreOperationSync()
    {
      this.ValidateByBaseValidators();
      this.WrapInTraceWithElapsedTime(
        this.SetCountryAndStateBasedOnParentAccount, nameof(this.SetCountryAndStateBasedOnParentAccount));
    }

    public void CreatePostOperationSync()
    {
      this.ValidateByBaseValidators();
      this.WrapInTraceWithElapsedTime(
        this.CreateTaskWithTopAccountsStat, nameof(this.CreateTaskWithTopAccountsStat));
    }

    #endregion

    #region Update Message

    public void UpdatePreValidationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();

      this.RejectProtectedFieldsChange();
      this.ValidateMandatoryFields();
    }

    public void UpdatePreOperationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();
      this.WrapInTraceWithElapsedTime(
        this.SetCountryAndStateBasedOnParentAccount, nameof(this.SetCountryAndStateBasedOnParentAccount));
    }

    public void UpdatePostOperationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();
      this.WrapInTraceWithElapsedTime(
        this.CreateTaskWithTopAccountsStat, nameof(this.CreateTaskWithTopAccountsStat));
    }

    #endregion

    protected override ProtectedFieldValidationRule[] GetProtectedFieldValidationRules()
    {
      return new []
               {
                 new ProtectedFieldValidationRule(
                   Contact.Fields.TransactionCurrencyId,
                   this.GetIsInitiatedByIndirectOperation,
                   // ReSharper disable once ArgumentsStyleLiteral
                   onCreate:false,
                   // ReSharper disable once ArgumentsStyleLiteral
                   // ReSharper disable once RedundantArgumentDefaultValue
                   onUpdate:true
                   ),

                 new ProtectedFieldValidationRule(
                   Contact.Fields.JobTitle,
                   this.GetIsInitiatedByIndirectOperation
                 ),
               };
    }

    private void ValidateMandatoryFields()
    {
      var mandatoryFields = new []
                              {
                                Contact.Fields.MobilePhone,
                                Contact.Fields.EMailAddress1
                              };

      this.ValidateForMandatoryFields(mandatoryFields);
    }

    private void SetCountryAndStateBasedOnParentAccount()
    {
      var pluginCtx = this.PluginCtx;
      var skipProcessing = !this.ParentAccountRefExt.IsModified || this.ParentAccountRefExt.IsSetToNull;
      if (skipProcessing)
      {
        return;
      }

      var parentAccountId = this.ParentAccountRefExt.Value.Id;
      var parentAccount = pluginCtx.OrgCtx.AccountSet
        .Where(r => r.AccountId == parentAccountId)
        .Select(r => new Account
                       {
                         AccountId = r.AccountId,
                         Address1_StateOrProvince = r.Address1_StateOrProvince,
                         Address1_Country = r.Address1_Country
                       })
        .FirstOrDefault();
      if (parentAccount == null)
      {
        throw new NullReferenceException(nameof(parentAccount));
      }

      var targetEntity = pluginCtx.TargetEntity;
      targetEntity.Address1_StateOrProvince = parentAccount.Address1_StateOrProvince;
      targetEntity.Address1_Country = parentAccount.Address1_Country;
    }

    private void CreateTaskWithTopAccountsStat()
    {
      var pluginCtx = this.PluginCtx;
      var createTaskWithTopAccountsStat =
        this.ParentAccountRefExt.IsModified && (
             this.ParentAccountRefExt.OldValue?.LogicalName == Account.EntityLogicalName
             ||
             this.ParentAccountRefExt.NewValue?.LogicalName == Account.EntityLogicalName)
        ;
      if (!createTaskWithTopAccountsStat)
      {
        pluginCtx.Trace("Parent Account is not changed.");
        return;
      }

      const int TopAccountQt = 3;
      const string QtAliasName = "Qt";
      var fetchXml = $@"
        <fetch top='3' no-lock='true' aggregate='true'>
          <entity name='{Contact.EntityLogicalName}'>
            <attribute name='{Contact.Fields.ParentCustomerId}' alias='{Contact.Fields.ParentCustomerId}' groupby='true' />
            <attribute name='{Contact.Fields.ContactId}' alias='{QtAliasName}' aggregate='count' />
            <filter>
              <condition attribute='{Contact.Fields.AccountId}' operator='not-null' />
            </filter>
            <order alias='{QtAliasName}' descending='true' />
          </entity>
        </fetch>";
      var orgService = pluginCtx.OrgService;

      pluginCtx.TraceValue(nameof(fetchXml), fetchXml);

      var entityCollection = orgService.GetFetchXmlQueryResultSet(fetchXml);
      var entities = entityCollection.Entities;
      var description = new StringBuilder("Account list:");
      description.AppendLine();
      foreach (var entity in entities)
      {
        var accountRef = entity.GetSimpleAttrValue<EntityReference>(Contact.Fields.ParentCustomerId, aliased: true);
        var name = accountRef?.Name;
        pluginCtx.TraceValue(nameof(name), name);
        var qt = entity.GetSimpleAttrValue<int>(QtAliasName, aliased: true);
        pluginCtx.TraceValue(nameof(qt), qt);
        description.AppendLine($"Name: {name} - Contacts count: {qt}");
      }

      var task = new Task
                   {
                     PriorityCodeEnum = Task_PriorityCode.Low,
                     Subject = $"Please review top {TopAccountQt} accounts.",
                     Description = description.ToString()
                   };

      orgService.Create(task);
    }
  }
}

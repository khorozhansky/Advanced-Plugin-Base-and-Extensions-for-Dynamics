namespace AdvancedPluginDemo.Logic.Entities.Contact
{
  using System.Text;

  using AdvancedPlugin.Utils;

  using AdvancedPluginDemo.Logic.Base;
  using AdvancedPluginDemo.Proxy;

  using Microsoft.Xrm.Sdk;

  public class ContactManager: ExtendedOutOfBoxTraceLogManagerBase<Contact>
  {
    public ContactManager(Plugins.Bound.Contact.Base.PluginContext pluginCtx)
      : base(pluginCtx)
    {
    }

    #region Create Message

    public void CreatePreValidationSync()
    {
      this.ValidateByBaseValidators();

      this.RejectProtectedFieldsChange();
      this.ValidateMandatoryFields();
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

    private void CreateTaskWithTopAccountsStat()
    {
      var pluginCtx = this.PluginCtx;
      var entityExt = pluginCtx.TargetEntityExt;
      var parentAccountRefExt = entityExt.GetValue<EntityReference>(Contact.Fields.ParentCustomerId);
      var createTaskWithTopAccountsStat =
        parentAccountRefExt.IsModified && (
             parentAccountRefExt.OldValue?.LogicalName == Account.EntityLogicalName
             ||
             parentAccountRefExt.NewValue?.LogicalName == Account.EntityLogicalName)
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
                     Subject = $"Please review top {TopAccountQt} contacts.",
                     Description = description.ToString()
      };

      orgService.Create(task);
    }
  }
}

namespace AdvancedPluginDemo.Logic.Contact
{
  using System;
  using System.Text;

  using AdvancedPlugin.Exceptions;
  using AdvancedPlugin.Logging;
  using AdvancedPlugin.Utils;

  using CrmProxy;

  using Microsoft.Xrm.Sdk;

  public class ContactManager: ManagerBase<Contact, ExtendedOutOfBoxTraceLog>
  {
    public ContactManager(Plugins.Bound.Contact.Base.PluginContext pluginCtx)
      : base(pluginCtx)
    {
    }

    #region Create Message

    public void CreatePreValidationSync()
    {
      this.ValidateByBaseValidators();
    }

    public void CreatePreOperationSync()
    {
      this.ValidateByBaseValidators();
    }

    public void CreatePostOperationSync()
    {
      this.ValidateByBaseValidators();
      this.CreateTaskWithTopAccountsStat();
    }

    public void CreatePostOperationAsync()
    {
      this.ValidateByBaseValidators();
    }

    #endregion

    #region Update Message

    public void UpdatePreValidationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();
    }

    public void UpdatePreOperationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();
    }

    public void UpdatePostOperationSync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();
      this.ValidatePostImage();
      this.CreateTaskWithTopAccountsStat();
    }

    public void UpdatePostOperationAsync()
    {
      this.ValidateByBaseValidators();
      this.ValidatePreImage();

      this.ProcessContactAccountRelationshipChange();
    }

    #endregion

    private void ProcessContactAccountRelationshipChange()
    {
      var pluginCtx = this.PluginCtx;
      var entityExt = pluginCtx.TargetEntityExt;
      var parentAccountRefExt = entityExt.GetValue<EntityReference>(Contact.Fields.AccountId);
      var createTaskWithTopAccountsStat = parentAccountRefExt.IsModified;
    }

    private void CreateTaskWithTopAccountsStat()
    {
      var pluginCtx = this.PluginCtx;
      const int TopAccountQt = 3;
      const string QtAliasName = "Qt";
      var fetchXml = $@"
        <fetch top='3' no-lock='true' aggregate='true'>
          <entity name='{Contact.EntityLogicalName}'>
            <attribute name='{Contact.Fields.AccountId}' alias='{Contact.Fields.AccountId}' groupby='true' />
            <attribute name='{Contact.Fields.ContactId}' alias='{QtAliasName}' aggregate='count' />
            <order alias='{QtAliasName}' descending='true' />
          </entity>
        </fetch>";
      var orgService = pluginCtx.OrgService;
      var entityCollection = orgService.GetFetchXmlQueryResultSet(fetchXml);
      var entities = entityCollection.Entities;
      var description = new StringBuilder("Account list:");
      foreach (var entity in entities)
      {
        var accountRef = entity.GetSimpleAttrValue<EntityReference>(Contact.Fields.AccountId, aliased: true);
        var qt = entity.GetSimpleAttrValue<int>(QtAliasName, aliased: true);
        description.AppendLine($"Name: {accountRef.Name} - Contacts count: {qt}");
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

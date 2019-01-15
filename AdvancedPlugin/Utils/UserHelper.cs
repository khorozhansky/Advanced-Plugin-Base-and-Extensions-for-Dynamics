namespace AdvancedPlugin.Utils
{
  using System;

  using AdvancedPlugin.Plugins;

  using Microsoft.Xrm.Sdk.Client;
  using Microsoft.Xrm.Sdk;

  public static class UserHelper
  {
    public static Guid GetSystemUserId<TContext>(IPluginContext<TContext> pluginCtx)
      where TContext : OrganizationServiceContext
    {
      var fetchXml = $@"
        <fetch top='1' no-lock='true'>
          <entity name='systemuser'>
            <attribute name='systemuserid' />
            <filter>
              <condition attribute='fullname' operator='eq' value='SYSTEM'/>
            </filter>
            <order attribute='createdon' />
          </entity>
        </fetch>";

      var orgService = pluginCtx.OrgServiceOnBehalfOfSystemUser;
      return orgService.GetFetchXmlQueryResultAsSingleEntity(fetchXml).Id;
    }

    public static EntityReference GetSystemUserRef<TContext>(IPluginContext<TContext> pluginCtx)
      where TContext : OrganizationServiceContext
    {
      return new EntityReference("systemuser", GetSystemUserId(pluginCtx));
    }

    public static void SetOwnerAsSystemOnCreate<TContext>(IPluginContext<TContext> pluginCtx)
      where TContext : OrganizationServiceContext
    {
      SetOwnerOnCreate(pluginCtx, GetSystemUserRef(pluginCtx));
    }

    public static void SetOwnerOnCreate<TContext>(IPluginContext<TContext> pluginCtx, EntityReference userRef)
      where TContext : OrganizationServiceContext
    {
      pluginCtx.TargetEntityAsBaseEntity.SetAttrValue("ownerid", userRef);
    }
  }
}

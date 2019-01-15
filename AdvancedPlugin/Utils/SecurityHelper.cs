using Microsoft.Xrm.Sdk.Client;

namespace AdvancedPlugin.Utils
{
  using System;
  using System.Linq;

  using AdvancedPlugin.Plugins;

  public static class SecurityHelper
  {
    /// <summary>
    /// Gets if the user is a system administrator.
    /// </summary>
    /// <param name="pluginCtx">The plugin context.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns></returns>
    public static bool GetIsAdminUser(this IPluginContextBase pluginCtx, Guid userId)
    {
      return pluginCtx.GetIsUserDirectlyAssociatedWithSecurityRole(
        userId,
        templateRoleId: TemplateSecurityRoleIds.SystemAdministratorRoleTemplateId);
    }

    /// <summary>
    /// Gets if the initiating user is a system administrator.
    /// </summary>
    /// <param name="pluginCtx">The plugin context.</param>
    /// <returns></returns>
    public static bool GetIsAdminUser(this IPluginContextBase pluginCtx)
    {
      return pluginCtx.GetIsAdminUser(pluginCtx.InitiatingUserId);
    }

    public static bool GetIsUserDirectlyAssociatedWithSecurityRole(
      this IPluginContextBase pluginCtx,
      string rootRoleId = null,
      string templateRoleId = null)
    {
      return pluginCtx.GetIsUserDirectlyAssociatedWithSecurityRole(
        pluginCtx.InitiatingUserId,
        rootRoleId,
        templateRoleId);
    }

    public static bool GetIsUserDirectlyAssociatedWithSecurityRole(
      this IPluginContextBase pluginCtx,
      Guid userId,
      string rootRoleId = null,
      string templateRoleId = null)
    {
      var invalidAgrs = rootRoleId == null && templateRoleId == null;
      if (invalidAgrs)
      {
        throw new ArgumentOutOfRangeException($"Neither {nameof(rootRoleId)} nor {nameof(templateRoleId)} is provided.");
      }

      string[] rootRoleIds = null;
      if (rootRoleId != null)
      {
        rootRoleIds = new[] { rootRoleId };
      }

      string[] templateRoleIds = null;
      if (templateRoleId != null)
      {
        templateRoleIds = new[] { templateRoleId };
      }

      return pluginCtx.GetIsUserDirectlyAssociatedWithSecurityRole(userId, rootRoleIds, templateRoleIds);
    }

    public static bool GetIsUserDirectlyAssociatedWithSecurityRole(
      this IPluginContextBase pluginCtx,
      string[] rootRoleIds = null,
      string[] templateRoleIds = null)
    {
      return pluginCtx.GetIsUserDirectlyAssociatedWithSecurityRole(
        pluginCtx.InitiatingUserId,
        rootRoleIds,
        templateRoleIds);
    }

    public static bool GetIsUserDirectlyAssociatedWithSecurityRole(
      this IPluginContextBase pluginCtx,
      Guid userId,
      string[] rootRoleIds = null,
      string[] templateRoleIds = null)
    {
      var invalidAgrs = rootRoleIds == null && templateRoleIds == null;
      if (invalidAgrs)
      {
        throw new ArgumentOutOfRangeException($"Neither {nameof(rootRoleIds)} nor {nameof(templateRoleIds)} is provided.");
      }

      string rootRolesCondition; 
      if (rootRoleIds == null)
      {
        rootRolesCondition = string.Empty;
      }
      else
      {
        if (!rootRoleIds.Any())
        {
          throw new ArgumentOutOfRangeException(nameof(rootRoleIds));
        }

        var values = FetchXmlQueryHelper.ConvertToFetchXmlInValuesString(rootRoleIds);
        rootRolesCondition = $@"
              <condition attribute='parentrootroleid' operator='in'>
                {values}
              </condition>";
      }

      string templateRolesCondition;
      if (templateRoleIds == null)
      {
        templateRolesCondition = string.Empty;
      }
      else
      {
        if (!templateRoleIds.Any())
        {
          throw new ArgumentOutOfRangeException(nameof(rootRoleIds));
        }

        var values = FetchXmlQueryHelper.ConvertToFetchXmlInValuesString(templateRoleIds);
        templateRolesCondition = $@"
              <condition attribute='roletemplateid' operator='in'>
                {values}
              </condition>";
      }

      var fetchXml = $@"
        <fetch top='1' no-lock='true'>
          <entity name='role'>
            <attribute name='roleid' />
            <filter type='or' >
              {templateRolesCondition}
              {rootRolesCondition}
            </filter>
            <link-entity name='systemuserroles' from='roleid' to='roleid' link-type='inner'>
              <filter>
                <condition attribute='systemuserid' operator='eq' value='{userId:D}'/>
              </filter>
            </link-entity>
          </entity>
        </fetch>";

      return pluginCtx.OrgServiceOnBehalfOfSystemUser.GetFetchXmlQueryResultAsSingleEntity(fetchXml) != null;
    }

    public static class TemplateSecurityRoleIds
    {
      public const string SystemAdministratorRoleTemplateId = "627090FF-40A3-4053-8790-584EDC5BE201";
    }
  }
}

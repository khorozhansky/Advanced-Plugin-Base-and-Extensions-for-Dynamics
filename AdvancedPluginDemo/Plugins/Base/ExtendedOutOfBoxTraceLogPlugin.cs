namespace AdvancedPluginDemo.Plugins.Base
{
  using AdvancedPlugin.Logging;
  using AdvancedPlugin.Plugins;

  using AdvancedPluginDemo.Plugins.Base.Ext;
  using AdvancedPluginDemo.Proxy;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Client;

  /// <summary>
  /// A base class for all the rest Plugin classes inside the project
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <seealso cref="ExtendedOutOfBoxTraceLogPluginBase{TEntity,TContext}" />
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="CrmContext" /> as Organization Service Context.
  /// In such a base class of concrete plugin assembly we should implement the <see cref="BuildOrgCtx" /> to give the system a way of building <see cref="CrmContext" /> instance.
  /// <seealso cref="TraceLogBase"/>
  /// </remarks>
  public abstract class ExtendedOutOfBoxTraceLogPlugin<TEntity> : ExtendedOutOfBoxTraceLogPluginBase<TEntity, CrmContext>
    where TEntity : Entity, new()
  {
    protected ExtendedOutOfBoxTraceLogPlugin(string unsecureConfig, string secureConfig) 
      : base(unsecureConfig, secureConfig)
    {
    }

    /// <summary>
    /// Implements concrete builder of the <see cref="OrganizationServiceContext"/> instance
    /// </summary>
    /// <param name="orgService">The org service.</param>
    /// <returns>An instance of <see cref="CrmContext"/></returns>
    protected override CrmContext BuildOrgCtx(IOrganizationService orgService)
    {
      return new CrmContext(orgService);
    }

    protected override bool GetIsExecutionAllowed(IPluginContextBase pluginCtx)
    {
      return PluginCtxExt.GetIsExecutionAllowed(pluginCtx);
    }
  }
}

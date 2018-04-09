namespace AdvancedPluginDemo.Plugins
{
  using AdvancedPlugin.Logging;

  using Microsoft.Xrm.Sdk;

  using AdvancedPlugin.TraceService;

  using CrmProxy;

  using Microsoft.Xrm.Sdk.Client;

  /// <summary>
  /// A base class for all the rest Plugin classes inside the project
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <typeparam name="TTraceLog">The type of the trace log.</typeparam>
  /// <seealso cref="AdvancedPlugin.PluginBase{TEntity, CrmContext, TTraceLog}" />
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="CrmContext" /> as Organization Service Context.
  /// In such a base class of concrete plugin assembly we should implement the <see cref="OrganizationContextBuilder" /> to give the system a way of building <see cref="CrmContext" /> instance.
  /// <seealso cref="TraceLogBase"/>
  /// </remarks>
  public abstract class PluginInCrmContextBase<TEntity, TTraceLog> : AdvancedPlugin.PluginBase<TEntity, CrmContext, TTraceLog>
    where TEntity : Entity, new()
    where TTraceLog : TraceLogBase
  {
    protected PluginInCrmContextBase(string unsecureConfig, string secureConfig) 
      : base(unsecureConfig, secureConfig)
    {
    }

    /// <summary>
    /// Implements concrete builder of the <see cref="OrganizationServiceContext"/> instance
    /// </summary>
    /// <param name="orgService">The org service.</param>
    /// <returns>An instance of <see cref="CrmContext"/></returns>
    protected override CrmContext OrganizationContextBuilder(IOrganizationService orgService)
    {
      return new CrmContext(orgService);
    }
  }
}

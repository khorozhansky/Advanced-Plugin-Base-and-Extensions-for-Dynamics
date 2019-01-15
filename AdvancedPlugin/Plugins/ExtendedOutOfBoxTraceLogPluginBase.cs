namespace AdvancedPlugin.Plugins
{
  using AdvancedPlugin.Logging;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Client;

  /// <summary>
  /// A base class for all the Plugin classes inside the project which will use "Extended Out-Of-Box Trace Log Engine"
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <typeparam name="TContext">A type that inherits from the <see cref="OrganizationServiceContext" /></typeparam>
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="ExtendedOutOfBoxTraceLog" /> as a "Trace Log Engine"
  /// In such a base class of concrete plugin assembly we should implement the <see cref="BuildTraceLog" /> to give the system a way of building "Trace Log"
  /// <seealso cref="ExtendedOutOfBoxTraceLog" />
  /// </remarks>
  public abstract class ExtendedOutOfBoxTraceLogPluginBase<TEntity, TContext> : PluginBase<TEntity, TContext, ExtendedOutOfBoxTraceLog>
    where TEntity : Entity, new()
    where TContext : OrganizationServiceContext
  {
    protected ExtendedOutOfBoxTraceLogPluginBase(
      string unsecureConfig,
      string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override ExtendedOutOfBoxTraceLog BuildTraceLog(IPluginContextBase pluginCtx, ITracingService tracingService)
    {
      return new ExtendedOutOfBoxTraceLog(pluginCtx, tracingService);
    }
  }
}

namespace AdvancedPluginDemo.Plugins
{
  using AdvancedPlugin;
  using AdvancedPlugin.Logging;

  using Microsoft.Xrm.Sdk;

  /// <summary>
  /// A base class for all the Plugin classes inside the project which will use "Advanced Trace Log Engine" (detailed info will be saved in the "Advanced Plugin Trace" custom entity)
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="AdvancedTraceLog" /> as a "Trace Log Engine"
  /// In such a base class of concrete plugin assembly we should implement <see cref="TraceLogBuilder" /> to give the system a way of building "Trace Log"
  /// <seealso cref="AdvancedTraceLog" />
  /// </remarks>
  public abstract class AdvancedTraceLogPluginBase<TEntity> : PluginInCrmContextBase<TEntity, AdvancedTraceLog>
    where TEntity : Entity, new()
  {
    protected AdvancedTraceLogPluginBase(string unsecureConfig, string secureConfig) 
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override AdvancedTraceLog TraceLogBuilder(IPluginContext pluginCtx, ITracingService systemTracingService)
    {
      return new AdvancedTraceLog(pluginCtx, systemTracingService);
    }
  }
}

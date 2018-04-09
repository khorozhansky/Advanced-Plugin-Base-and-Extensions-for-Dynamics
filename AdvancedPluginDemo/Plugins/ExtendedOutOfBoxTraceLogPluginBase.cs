namespace AdvancedPluginDemo.Plugins
{
  using AdvancedPlugin;
  using AdvancedPlugin.Logging;

  using Microsoft.Xrm.Sdk;

  /// <summary>
  /// A base class for all the Plugin classes inside the project which will use "Extended Out-Of-Box Trace Log Engine"
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="ExtendedOutOfBoxTraceLog" /> as a "Trace Log Engine"
  /// In such a base class of concrete plugin assembly we should implement the <see cref="TraceLogBuilder" /> to give the system a way of building "Trace Log"
  /// <seealso cref="ExtendedOutOfBoxTraceLog" />
  /// </remarks>
  public abstract class ExtendedOutOfBoxTraceLogPluginBase<TEntity> : PluginInCrmContextBase<TEntity, ExtendedOutOfBoxTraceLog>
    where TEntity : Entity, new()
  {
    protected ExtendedOutOfBoxTraceLogPluginBase(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override ExtendedOutOfBoxTraceLog TraceLogBuilder(IPluginContext pluginCtx, ITracingService systemTracingService)
    {
      return new ExtendedOutOfBoxTraceLog(pluginCtx, systemTracingService);
    }
  }
}

namespace AdvancedPluginDemo.Logic
{
  using AdvancedPlugin.Logging;

  using Plugins;

  using CrmProxy;
  using Microsoft.Xrm.Sdk;

  /// <summary>
  /// A base class for all Manager classes
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <typeparam name="TTraceLog">The type of the trace log.</typeparam>
  /// <seealso cref="AdvancedPlugin.ManagerBase{TEntity, CrmContext, TTraceLog}" />
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="CrmContext" /> as Organization Service Context.
  /// <seealso cref="AdvancedPlugin.ManagerBase{TEntity, TContext, TTraceLog}" />
  /// </remarks>
  public abstract class ManagerBase<TEntity, TTraceLog> :
    AdvancedPlugin.ManagerBase<TEntity, CrmContext, TTraceLog>
    where TEntity : Entity, new()
    where TTraceLog : TraceLogBase
  {
    protected ManagerBase(PluginInCrmContextBase<TEntity, TTraceLog>.PluginContext pluginCtx)
      : base(pluginCtx)
    {
    }
  }
}

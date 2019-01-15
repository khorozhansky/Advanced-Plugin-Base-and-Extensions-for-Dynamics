namespace AdvancedPluginDemo.Logic.Base
{
  using AdvancedPlugin.Logging;
  using AdvancedPlugin.Managers;
  using AdvancedPlugin.Plugins;

  using AdvancedPluginDemo.Proxy;

  using Microsoft.Xrm.Sdk;

  /// <summary>
  /// A base class for all Manager classes
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <seealso cref="ManagerBase{TEntity,TContext,TTraceLog}" />
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="CrmContext" /> as Organization Service Context.
  /// </remarks>
  public abstract class AdvancedTraceLogManagerBase<TEntity> : ManagerBase<TEntity, CrmContext, AdvancedTraceLog>
    where TEntity : Entity, new()
  {
    protected AdvancedTraceLogManagerBase(PluginBase<TEntity, CrmContext, AdvancedTraceLog>.PluginContext pluginCtx)
      : base(pluginCtx)
    {
    }
  }
}

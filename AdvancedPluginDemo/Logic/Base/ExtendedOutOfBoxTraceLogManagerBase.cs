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
  public abstract class ExtendedOutOfBoxTraceLogManagerBase<TEntity> : ManagerBase<TEntity, CrmContext, ExtendedOutOfBoxTraceLog>
    where TEntity : Entity, new()
  {
    protected ExtendedOutOfBoxTraceLogManagerBase(PluginBase<TEntity, CrmContext, ExtendedOutOfBoxTraceLog>.PluginContext pluginCtx)
      : base(pluginCtx)
    {
    }
  }
}

namespace AdvancedPluginDemo.Plugins
{
  using System;

  using AdvancedPlugin;
  using AdvancedPlugin.Logging;

  using Microsoft.Xrm.Sdk;

  /// <summary>
  /// A base class for all the Plugin classes inside the project which will use "Advanced Trace Log Engine" (detailed info will be saved in the "Advanced Plugin Trace" custom entity)
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <remarks>
  /// This definition states that all derived manager classes will use <see cref="ServiceBusTraceLog" /> as a "Trace Log Engine"
  /// In such a base class of concrete plugin assembly we should implement <see cref="TraceLogBuilder" /> to give the system a way of building "Trace Log"
  /// <seealso cref="ServiceBusTraceLog" />
  /// </remarks>
  public abstract class ServiceBusTraceLogPluginBase<TEntity> : PluginInCrmContextBase<TEntity, ServiceBusTraceLog>
    where TEntity : Entity, new()
  {
    /// <summary>
    /// The ID of Trace Service Bus Endpoint.
    /// </summary>
    /// <remarks>
    /// ! ATTENTION SET THE ID OF TRACE SERVICE BUS ENDPOINT REGISTERED IN THE SYSTEM
    /// </remarks>
    private readonly Guid traceServiceBusEndpointId = Guid.Empty;

    protected ServiceBusTraceLogPluginBase(string unsecureConfig, string secureConfig) 
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override ServiceBusTraceLog TraceLogBuilder(IPluginContext pluginCtx, ITracingService systemTracingService)
    {
      return new ServiceBusTraceLog(pluginCtx, systemTracingService, this.traceServiceBusEndpointId);
    }
  }
}

namespace AdvancedPlugin.Logging
{
  using System;

  using AdvancedPlugin.Plugins;

  using TraceService;

  using Microsoft.Xrm.Sdk;

  public abstract class CustomTracingServiceTraceLog : TraceLogBase
  {
    private readonly Lazy<CustomTracingService> customTracingService;

    protected CustomTracingServiceTraceLog(IPluginContextBase pluginCtx, ITracingService systemTracingService)
      : base(pluginCtx, systemTracingService)
    {
      this.customTracingService = new Lazy<CustomTracingService>(
        () => (CustomTracingService)this.TracingService);
    }

    protected virtual CustomTracingService CustomTracingService 
      => this.customTracingService.Value;

    protected override ITracingService BuildTracingService()
    {
      return new CustomTracingService();
    }
  }
}
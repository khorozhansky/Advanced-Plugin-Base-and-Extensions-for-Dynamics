namespace AdvancedPlugin.Logging
{
  using System;

  using TraceService;

  using Microsoft.Xrm.Sdk;

  public abstract class CustomTracingServiceTraceLogBase : TraceLogBase
  {
    private readonly Lazy<CustomTracingService> customTracingService;

    protected CustomTracingServiceTraceLogBase(IPluginContext pluginCtx, ITracingService systemTracingService)
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
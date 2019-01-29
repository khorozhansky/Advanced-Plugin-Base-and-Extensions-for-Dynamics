namespace AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx
{
  using FakeXrmEasy;

  using Microsoft.Xrm.Sdk;

  public class WithTargetEntityReferenceBase : FakedPluginExecCtxBase
  {
    public EntityReference TargetEntityReference
    {
      set => this.SetInputParameter("Target", value);
    }

    public WithTargetEntityReferenceBase(XrmFakedContext xrmFakedContext)
      : base(xrmFakedContext)
    {
    }
  }
}

namespace AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx
{
  using FakeXrmEasy;

  using Microsoft.Xrm.Sdk;

  public class WithTargetEntityBase : FakedPluginExecCtxBase
  {
    public Entity TargetEntity
    {
      set => this.SetInputParameter("Target", value);
    }

    public Entity PreImage
    {
      set => this.PreEntityImages["PreImage"] = value;
    }

    public Entity PostImage
    {
      set => this.PreEntityImages["PostImage"] = value;
    }

    public WithTargetEntityBase(XrmFakedContext xrmFakedContext)
      : base(xrmFakedContext)
    {
    }
  }
}

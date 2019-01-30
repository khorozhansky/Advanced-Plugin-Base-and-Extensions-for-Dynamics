namespace AdvancedPluginDemoUnitTests.Ext
{
  using System.Reflection;

  using AdvancedPluginDemo.Proxy;

  using FakeXrmEasy;

  public class XrmFakedContextWithProxy : XrmFakedContext
  {
    public XrmFakedContextWithProxy()
    {
      this.ProxyTypesAssembly = Assembly.GetAssembly(typeof(SystemUser));
    }
  }
}

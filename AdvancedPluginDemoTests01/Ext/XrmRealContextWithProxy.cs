namespace AdvancedPluginDemoUnitTests.Ext
{
  using System.Reflection;

  using AdvancedPluginDemo.Proxy;

  using FakeXrmEasy;

  using Microsoft.Xrm.Sdk.Client;

  public class XrmRealContextWithProxy : XrmRealContext
  {
    public XrmRealContextWithProxy(string connectionStringName) : base(connectionStringName)
    {
      this.ProxyTypesAssembly = Assembly.GetAssembly(typeof(SystemUser));
    }

    public OrganizationServiceContext OrgCtx => new OrganizationServiceContext(this.GetOrganizationService());
  }
}

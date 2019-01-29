namespace AdvancedPluginDemoUnitTests.Demo.Account
{
  using System.Reflection;

  using AdvancedPluginDemo.Proxy;

  using AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx;

  //using AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx;

  using FakeXrmEasy;

  using Microsoft.Xrm.Sdk;

  using Xunit;

  public class NameTest
  {
    [Fact]
    // ReSharper disable once InconsistentNaming
    public void When_AccountNameIsTooShort_Expect_PluginToFail()
    {
      var ctx = new XrmFakedContext { ProxyTypesAssembly = Assembly.GetAssembly(typeof(Account)) };
      const string AccountName = "AAA";
      var target = new Account
                     {
                       Name = AccountName
      };

      var pluginCtx = new CreatePreValidationSyncBase(ctx)
                        {
                          TargetEntity = target
                        };

      var exc = Assert.Throws<InvalidPluginExecutionException>(
        () =>
          ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Account.Create>(
            pluginCtx, string.Empty, string.Empty));
      var message = $"Account name \"{AccountName}\" is incorrect. It should contain at least 4 characters.";
      Assert.Equal(message, exc.Message);
    }

    [Fact]
    // ReSharper disable once InconsistentNaming
    public void When_AccountNameIsNotShort_Expect_PluginNotToFail()
    {
      var ctx = new XrmFakedContext { ProxyTypesAssembly = Assembly.GetAssembly(typeof(Account)) };
      const string AccountName = "AAAA";
      var target = new Account
                     {
                       Name = AccountName
                     };

      var pluginCtx = new CreatePreValidationSyncBase(ctx)
                        {
                          TargetEntity = target
                        };

      var exc = Record.Exception(
        () =>
          ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Account.Create>(
            pluginCtx, string.Empty, string.Empty));
      Assert.Null(exc);
    }
  }
}

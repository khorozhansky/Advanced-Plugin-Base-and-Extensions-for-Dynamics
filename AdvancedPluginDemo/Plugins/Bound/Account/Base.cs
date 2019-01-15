namespace AdvancedPluginDemo.Plugins.Bound.Account
{
  using System;

  using AdvancedPluginDemo.Logic.Entities.Account;
  using AdvancedPluginDemo.Plugins.Base;
  using AdvancedPluginDemo.Proxy;

  public abstract class Base : ExtendedOutOfBoxTraceLogPlugin<Account>
  {
    protected virtual void RunInManager(PluginContext pluginContext, Action<AccountManager> action)
    {
      var manager = new AccountManager(pluginContext);
      action(manager);
    }

    protected Base(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }
  }
}

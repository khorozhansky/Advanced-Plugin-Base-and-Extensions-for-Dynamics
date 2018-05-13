namespace AdvancedPluginDemo.Plugins.Bound.Email
{
  using System;

  using AdvancedPluginDemo.Logic.Email;

  using Logic.Account;

  using CrmProxy;

  public abstract class Base : ExtendedOutOfBoxTraceLogPluginBase<Email>
  {
    protected virtual void RunInManager(PluginContext pluginContext, Action<EmailManager> action)
    {
      var manager = new EmailManager(pluginContext);
      action(manager);
    }

    protected Base(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }
  }
}

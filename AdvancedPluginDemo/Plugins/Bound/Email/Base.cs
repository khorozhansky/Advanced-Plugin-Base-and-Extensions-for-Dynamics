namespace AdvancedPluginDemo.Plugins.Bound.Email
{
  using System;

  using AdvancedPlugin.Plugins;

  using AdvancedPluginDemo.Logic.Entities.Email;
  using AdvancedPluginDemo.Plugins.Base;
  using AdvancedPluginDemo.Proxy;

  public abstract class Base : ExtendedOutOfBoxTraceLogPlugin<Email>
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

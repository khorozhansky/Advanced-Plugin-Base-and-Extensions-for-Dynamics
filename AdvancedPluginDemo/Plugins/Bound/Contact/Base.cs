namespace AdvancedPluginDemo.Plugins.Bound.Contact
{
  using System;

  using AdvancedPluginDemo.Logic.Entities.Contact;

  using Proxy;

  using AdvancedPluginDemo.Plugins.Base;

  public abstract class Base : ExtendedOutOfBoxTraceLogPlugin<Contact>
  {
    protected virtual void RunInManager(PluginContext pluginContext, Action<ContactManager> action)
    {
      var manager = new ContactManager(pluginContext);
      action(manager);
    }

    protected Base(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }
  }
}

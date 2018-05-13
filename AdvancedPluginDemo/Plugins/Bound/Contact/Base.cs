namespace AdvancedPluginDemo.Plugins.Bound.Contact
{
  using System;

  using AdvancedPluginDemo.CrmProxy;
  using AdvancedPluginDemo.Logic.Contact;

  public abstract class Base : ExtendedOutOfBoxTraceLogPluginBase<Contact>
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

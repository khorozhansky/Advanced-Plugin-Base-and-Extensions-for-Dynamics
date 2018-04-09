namespace AdvancedPluginDemo.Plugins.Bound.Account
{
  using AdvancedPlugin;

  using Microsoft.Xrm.Sdk;

  // ReSharper disable once RedundantExtendsListEntry
  public class SetStateAsUpdate : Base, IPlugin
  {
    public SetStateAsUpdate(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override void InitializeStepsDefinition()
    {
      this.RegisterPluginStep(
        Stage.PreOperation,
        Mode.Synchronous,
        PluginMessage.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.SetStateAsUpdatePreOperationSync()));
    }
  }
}
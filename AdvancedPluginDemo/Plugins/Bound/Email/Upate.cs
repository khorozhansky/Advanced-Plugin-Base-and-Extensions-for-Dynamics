namespace AdvancedPluginDemo.Plugins.Bound.Email
{
  using AdvancedPlugin;

  using Microsoft.Xrm.Sdk;

  // ReSharper disable once RedundantExtendsListEntry
  public class Upate : Base, IPlugin
  {
    public Upate(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override void InitializeStepsDefinition()
    {
      this.RegisterPluginStep(
        Stage.PreValidate,
        Mode.Synchronous,
        PluginMessage.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePreValidationSync()));

      this.RegisterPluginStep(
        Stage.PreOperation,
        Mode.Synchronous,
        PluginMessage.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePreOperationSync()));
    }
  }
}
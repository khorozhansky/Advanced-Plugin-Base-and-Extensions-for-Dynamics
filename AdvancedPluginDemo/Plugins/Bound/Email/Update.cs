namespace AdvancedPluginDemo.Plugins.Bound.Email
{
  using AdvancedPlugin.Constants;

  using Microsoft.Xrm.Sdk;

  // ReSharper disable once RedundantExtendsListEntry
  public class Update : Base, IPlugin
  {
    public Update(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override void InitializeStepsDefinition()
    {
      this.RegisterPluginStep(
        Stage.PreOperation,
        Mode.Synchronous,
        MessageName.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePreOperationSync()));
    }
  }
}
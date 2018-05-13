namespace AdvancedPluginDemo.Plugins.Bound.Contact
{
  using AdvancedPlugin;

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
      //this.RegisterPluginStep(
      //  Stage.PreValidate,
      //  Mode.Synchronous,
      //  PluginMessage.Update,
      //  pluginCtx => this.RunInManager(
      //    pluginCtx, m => m.UpdatePreValidationSync()));

      //this.RegisterPluginStep(
      //  Stage.PreOperation,
      //  Mode.Synchronous,
      //  PluginMessage.Update,
      //  pluginCtx => this.RunInManager(
      //    pluginCtx, m => m.UpdatePreOperationSync()));

      this.RegisterPluginStep(
        Stage.PostOperation,
        Mode.Synchronous,
        PluginMessage.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePostOperationSync()));

      //this.RegisterPluginStep(
      //  Stage.PostOperation,
      //  Mode.Asynchronous,
      //  PluginMessage.Update,
      //  pluginCtx => this.RunInManager(
      //    pluginCtx, m => m.UpdatePostOperationAsync()));
    }
  }
}
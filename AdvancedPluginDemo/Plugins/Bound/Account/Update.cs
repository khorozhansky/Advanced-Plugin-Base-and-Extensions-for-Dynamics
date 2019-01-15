namespace AdvancedPluginDemo.Plugins.Bound.Account
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
        Stage.PreValidate,
        Mode.Synchronous,
        MessageName.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePreValidationSync()));

      this.RegisterPluginStep(
        Stage.PreOperation,
        Mode.Synchronous,
        MessageName.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePreOperationSync()));

      this.RegisterPluginStep(
        Stage.PostOperation,
        Mode.Synchronous,
        MessageName.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePostOperationSync()));

      this.RegisterPluginStep(
        Stage.PostOperation,
        Mode.Asynchronous,
        MessageName.Update,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.UpdatePostOperationAsync()));
    }
  }
}
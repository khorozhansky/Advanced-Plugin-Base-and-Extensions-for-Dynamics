namespace AdvancedPluginDemo.Plugins.Bound.Account
{
  using AdvancedPlugin.Constants;

  using Microsoft.Xrm.Sdk;

  // ReSharper disable once RedundantExtendsListEntry
  public class Create : Base, IPlugin
  {
    public Create(string unsecureConfig, string secureConfig)
      : base(unsecureConfig, secureConfig)
    {
    }

    protected override void InitializeStepsDefinition()
    {
      this.RegisterPluginStep(
        Stage.PreValidate,
        Mode.Synchronous,
        MessageName.Create,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.CreatePreValidationSync()));

      this.RegisterPluginStep(
        Stage.PreOperation,
        Mode.Synchronous,
        MessageName.Create,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.CreatePreOperationSync()));

      this.RegisterPluginStep(
        Stage.PostOperation,
        Mode.Synchronous,
        MessageName.Create,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.CreatePostOperationSync()));

      this.RegisterPluginStep(
        Stage.PostOperation,
        Mode.Asynchronous,
        MessageName.Create,
        pluginCtx => this.RunInManager(
          pluginCtx, m => m.CreatePostOperationAsync()));
    }
  }
}
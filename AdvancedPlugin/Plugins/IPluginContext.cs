namespace AdvancedPlugin.Plugins
{
  using Microsoft.Xrm.Sdk.Client;

  public interface IPluginContext<TContext> : IPluginContextBase
    where TContext : OrganizationServiceContext
  {
    /// <summary>
    /// Gets the organization context on behalf of the "current user".
    /// </summary>
    /// <value>The organization context.</value>
    TContext OrgCtx { get; }

    /// <summary>
    /// Gets the organization context on behalf of the "system user".
    /// </summary>
    /// <remarks>
    /// Use it when the current context is not the system user's context but you need to perform some operation on behalf of the system user
    /// </remarks>
    /// <value>The organization context.</value>
    TContext OrgCtxOnBehalfOfSystemUser { get; }

    /// <summary>
    /// Gets the organization context on behalf of the "Initiating user".
    /// </summary>
    /// <remarks>
    /// Use it when the current context is the system user's context but you need to perform some operation on behalf of the initiating user
    /// </remarks>
    /// <value>The organization context.</value>
    TContext OrgCtxOnBehalfOfInitiatingUser { get; }
  }
}

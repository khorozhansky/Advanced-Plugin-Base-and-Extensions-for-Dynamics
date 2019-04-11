namespace AdvancedPlugin.Plugins
{
  using System;

  using Microsoft.Crm.Sdk.Messages;
  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Query;

  public interface IPluginContextBase : IDisposable
  {
    bool CustomVerboseLogSwitchedOn { get; }

    /// <summary>
    /// Gets the plugin execution context.
    /// </summary>
    /// <value>The plugin execution context.</value>
    IPluginExecutionContext ExecContext { get; }

    /// <summary>
    /// Gets the organization service for the "current context user".
    /// </summary>
    /// <value>The organization service.</value>
    IOrganizationService OrgService { get; }

    /// <summary>
    /// Gets the organization service for a System User.
    /// </summary>
    /// <value>
    /// The organization service for system user.
    /// </value>
    IOrganizationService OrgServiceOnBehalfOfSystemUser { get; }

    /// <summary>
    /// Gets the organization service for an Initiating User.
    /// </summary>
    /// <value>
    /// The organization service for system user.
    /// </value>
    IOrganizationService OrgServiceOnBehalfOfInitiatingUser { get; }

    /// <summary>
    /// Gets the notification service bus endpoint.
    /// </summary>
    /// <value>
    /// The notification service bus endpoint.
    /// </value>
    IServiceEndpointNotificationService ServiceBusNotificationEndpoint { get; }

    /// <summary>
    /// Gets the primary entity identifier.
    /// </summary>
    /// <value>
    /// The primary entity identifier.
    /// </value>
    Guid PrimaryEntityId { get; }

    /// <summary>
    /// Gets the name of the primary entity.
    /// </summary>
    /// <value>
    /// The name of the primary entity.
    /// </value>
    string PrimaryEntityName { get; }

    /// <summary>
    /// Gets the primary entity reference.
    /// </summary>
    /// <value>
    /// The primary entity reference.
    /// </value>
    EntityReference PrimaryEntityRef { get; }

    /// <summary>
    /// Gets the name of the secondary entity that has a relationship with the primary entity.
    /// </summary>
    /// <value>
    /// The name of the secondary entity.
    /// </value>
    string SecondaryEntityName { get; }

    /// <summary>
    /// Gets the name of the Web service message that is being processed by the event execution pipeline.
    /// </summary>
    /// <value>
    /// The name of the message.
    /// </value>
    string MessageName { get; }

    /// <summary>
    /// Gets the stage of plug-in execution..
    /// </summary>
    /// <value>
    /// The stage code.
    /// </value>
    int Stage { get; }

    /// <summary>
    /// Gets the mode of plug-in execution..
    /// </summary>
    /// <value>
    /// The mode code.
    /// </value>
    int Mode { get; }

    /// <summary>
    /// Gets the current depth of execution in the call stack.
    /// </summary>
    /// <value>
    /// The depth.
    /// </value>
    int Depth { get; }

    /// <summary>
    /// Gets the global unique identifier of the system user for whom the plug-in invokes web service methods on behalf of.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    Guid UserId { get; }

    /// <summary>
    /// Gets the global unique identifier of the system user account under which the current pipeline is executing.
    /// </summary>
    /// <value>
    /// The initiating user identifier.
    /// </value>
    Guid InitiatingUserId { get; }

    /// <summary>
    /// Gets a value indicating if the plug-in is executing in the sandbox.
    /// </summary>
    /// <value>
    /// The isolation mode.
    /// </value>
    int IsolationMode { get; }

    /// <summary>
    /// Gets the parent execution context.
    /// </summary>
    /// <value>
    /// The parent execute context.
    /// </value>
    IPluginExecutionContext ParentExecContext { get; }

    /// <summary>
    /// Gets the global unique identifier for tracking plug-in or custom workflow activity execution.
    /// </summary>
    /// <value>
    /// The correlation identifier.
    /// </value>
    Guid CorrelationId { get; }

    /// <summary>
    /// Gets a value indicating if the plug-in is executing from the Microsoft Dynamics CRM for Microsoft Office Outlook with Offline Access client while it is offline.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is executing offline; otherwise, <c>false</c>.
    /// </value>
    bool IsExecutingOffline { get; }

    /// <summary>
    /// Gets a value indicating if the plug-in is executing within the database transaction.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is in transaction; otherwise, <c>false</c>.
    /// </value>
    bool IsInTransaction { get; }

    /// <summary>
    /// Gets a value indicating if the plug-in is executing as a result of the Microsoft Dynamics CRM for Microsoft Office Outlook with Offline Access client transitioning from offline to online and synchronizing with the Microsoft Dynamics CRM server.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is offline playback; otherwise, <c>false</c>.
    /// </value>
    bool IsOfflinePlayback { get; }

    /// <summary>
    /// Gets the global unique identifier of the request being processed by the event execution pipeline.
    /// </summary>
    /// <value>
    /// The request identifier.
    /// </value>
    Guid? RequestId { get; }

    /// <summary>
    /// Gets the global unique identifier of the related System Job.
    /// </summary>
    /// <value>
    /// The operation identifier.
    /// </value>
    Guid OperationId { get; }

    /// <summary>
    /// Gets the date and time that the related System Job was created.
    /// </summary>
    /// <value>
    /// The operation created on.
    /// </value>
    DateTime OperationCreatedOn { get; }

    /// <summary>
    /// Gets a reference to the related SdkMessageProcessingingStep or ServiceEndpoint.
    /// </summary>
    /// <value>
    /// The owning extension.
    /// </value>
    EntityReference OwningExtension { get; }

    /// <summary>
    /// Gets the global unique identifier of the business unit that the user making the request, also known as the calling user, belongs to.
    /// </summary>
    /// <value>
    /// The business unit identifier.
    /// </value>
    Guid BusinessUnitId { get; }

    /// <summary>
    /// Gets the global unique identifier of the organization that the entity belongs to and the plug-in executes under.
    /// </summary>
    /// <value>
    /// The organization identifier.
    /// </value>
    Guid OrganizationId { get; }

    /// <summary>
    /// Gets the unique name of the organization that the entity currently being processed belongs to and the plug-in executes under.
    /// </summary>
    /// <value>
    /// The name of the organization.
    /// </value>
    string OrganizationName { get; }

    /// <summary>
    /// Gets a value indicating whether the Stage is Pre Validation.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is pre validation; otherwise, <c>false</c>.
    /// </value>
    bool IsPreValidation { get; }

    /// <summary>
    /// Gets a value indicating whether the Stage is Pre Operation.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is pre operation; otherwise, <c>false</c>.
    /// </value>
    bool IsPreOperation { get; }

    /// <summary>
    /// Gets a value indicating whether the Stage is Post Operation.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is post operation; otherwise, <c>false</c>.
    /// </value>
    bool IsPostOperation { get; }

    /// <summary>
    /// Gets a value indicating whether the Mode is Asynchronous.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is asynchronous; otherwise, <c>false</c>.
    /// </value>
    bool IsAsynchronous { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is synchronous.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is synchronous; otherwise, <c>false</c>.
    /// </value>
    bool IsSynchronous { get; }

    /// <summary>
    /// Gets a value indicating whether the Mode is Synchronous.
    /// </summary>
    /// <value>
    /// The name of the base entity logical.
    /// </value>
    // ReSharper disable once ConvertToAutoProperty
    string RegisteredEntityLogicalName { get; }

    /// <summary>
    /// Gets or sets the input target as Entity type.
    /// </summary>
    /// <value>The input target entity.</value>
    Entity TargetEntityAsBaseEntity { get; set; }

    /// <summary>
    /// Gets the input column set.
    /// </summary>
    /// <value>
    /// The input column set.
    /// </value>
    ColumnSet ColumnSet { get; }

    /// <summary>
    /// Gets or sets the output business entity (as Entity).
    /// </summary>
    /// <value>The output business entity.</value>
    Entity OutputBusinessEntityAsBaseEntity { get; set; }

    /// <summary>
    /// Gets the output business entity collection.
    /// </summary>
    /// <value>
    /// The output business entity collection.
    /// </value>
    EntityCollection OutputBusinessEntityCollection { get; }

    /// <summary>
    /// Gets the input target as entity reference.
    /// </summary>
    /// <value>The input target entity reference.</value>
    EntityReference TargetAsEntityReference { get; }

    /// <summary>
    /// Gets the principal.
    /// </summary>
    /// <value>The principal.</value>
    EntityReference Principal { get; }

    /// <summary>
    /// Gets or sets the access rights.
    /// </summary>
    /// <value>The access rights.</value>
    AccessRights? AccessRights { get; set; }

    /// <summary>
    /// Gets the state of the input.
    /// </summary>
    /// <value>The state of the input.</value>
    OptionSetValue State { get; }

    /// <summary>
    /// Gets the input status.
    /// </summary>
    /// <value>The input status.</value>
    OptionSetValue Status { get; }

    /// <summary>
    /// Gets the relationship.
    /// </summary>
    /// <value>The relationship.</value>
    Relationship Relationship { get; }

    /// <summary>
    /// Gets the related entities.
    /// </summary>
    /// <value>The related entities.</value>
    EntityReferenceCollection RelatedEntities { get; }

    /// <summary>
    /// Gets the ParameterXML parameter.
    /// </summary>
    /// <value>The parameter XML.</value>
    string ParameterXml { get; }

    /// <summary>
    /// Gets the subordinate reference.
    /// </summary>
    /// <value>
    /// The subordinate reference.
    /// </value>
    Guid? SubordinateId { get; }

    /// <summary>
    /// Gets the entity moniker.
    /// </summary>
    /// <value>
    /// The entity moniker.
    /// </value>
    EntityReference EntityMoniker { get; }

    /// <summary>
    /// Gets the query base.
    /// </summary>
    /// <value>
    /// The query base.
    /// </value>
    QueryBase Query { get; }

    /// <summary>
    /// Gets the query expression.
    /// </summary>
    /// <value>
    /// The query expression.
    /// </value>
    QueryExpression QueryExpr { get; }

    /// <summary>
    /// Gets the fetch expression.
    /// </summary>
    /// <value>
    /// The fetch expression.
    /// </value>
    FetchExpression FetchExpr { get; }

    /// <summary>
    /// Gets the assignee.
    /// </summary>
    /// <value>The assignee.</value>
    EntityReference Assignee { get; }

    /// <summary>
    /// Gets or sets the update content entity.
    /// </summary>
    /// <value>
    /// The update content entity.
    /// </value>
    Entity UpdateContentEntity { get; set; }

    /// <summary>
    /// Gets the initiating user reference.
    /// </summary>
    /// <value>
    /// The initiating user reference.
    /// </value>
    EntityReference InitiatingUserRef { get; }

    /// <summary>
    /// Gets a value indicating whether Depth == 1.
    /// </summary>
    /// <value>
    ///   <c>true</c> if Depth == 1; otherwise, <c>false</c>.
    /// </value>
    bool DepthIs1 { get; }

    /// <summary>
    /// Notifies the service bus endpoint sending the content of the current execution context.
    /// </summary>
    /// <param name="serviceBusEndpointId">Id of the Service Bus endpoint</param>
    void NotifyServiceBusEndpoint(Guid serviceBusEndpointId);

    /// <summary>
    /// Determines whether the specified plugin message name is message.
    /// </summary>
    /// <param name="pluginMessageName">Name of the plugin message.</param>
    /// <returns>
    ///   <c>true</c> if the specified plugin message name is message; otherwise, <c>false</c>.
    /// </returns>
    bool GetIsMessage(string pluginMessageName);

    /// <summary>
    /// Gets the pre image entity.
    /// </summary>
    /// <param name="imageName">Name of the image.</param>
    /// <returns>
    /// Pre Image Entity
    /// </returns>
    Entity GetPreImageAsBaseEntity(string imageName = null);

    /// <summary>
    /// Gets the post image entity.
    /// </summary>
    /// <param name="imageName">Name of the image.</param>
    /// <returns>
    /// Post Image Entity
    /// </returns>
    Entity GetPostImageAsBaseEntity(string imageName = null);

    /// <summary>
    /// Sets the shared variable.
    /// </summary>
    /// <param name="name">
    /// The name.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    void SetSharedVariable(string name, object value);

    /// <summary>
    /// Gets the input parameter.
    /// </summary>
    /// <typeparam name="TK">The type of the TK.</typeparam>
    /// <param name="key">The key.</param>
    /// <returns>
    /// An instance of the TK
    /// </returns>
    TK GetInputParameter<TK>(string key);

    /// <summary>
    /// Gets the input parameter.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// Parameter value
    /// </returns>
    object GetInputParameter(string key);

    /// <summary>
    /// Gets the output parameter.
    /// </summary>
    /// <typeparam name="TK">The type of the TK.</typeparam>
    /// <param name="key">The key.</param>
    /// <returns>
    /// An instance of the TK
    /// </returns>
    TK GetOutputParameter<TK>(string key);

    /// <summary>
    /// Gets the output parameter.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// Parameter value
    /// </returns>
    object GetOutputParameter(string key);

    /// <summary>
    /// Sets the input parameter.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    void SetInputParameter(string key, object value);

    /// <summary>
    /// Sets the output parameter.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    void SetOutputParameter(string key, object value);

    /// <summary>
    /// Tries to get a shared variable value.
    /// </summary>
    /// <param name="name">The name of the shared variable.</param>
    /// <param name="execCtx">An instance of <see cref="IPluginExecutionContext" />.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if the shared variable is found in the collection; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetSharedVariableValue(string name, IPluginExecutionContext execCtx, out object value);

    /// <summary>
    /// Tries to get a shared variable value.
    /// </summary>
    /// <param name="name">The name of the shared variable.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if the shared variable is found in the collection; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetSharedVariableValue(string name, out object value);

    /// <summary>
    /// Tries to get a shared variable value.
    /// </summary>
    /// <typeparam name="T">Expected type of the shared variable value</typeparam>
    /// <param name="name">The name of the shared variable.</param>
    /// <param name="execCtx">An instance of <see cref="IPluginExecutionContext" />.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if the shared variable is found in the collection; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetSharedVariableValue<T>(string name, IPluginExecutionContext execCtx, out T value);

    /// <summary>
    /// Tries to get a shared variable value.
    /// </summary>
    /// <typeparam name="T">Expected type of the shared variable value</typeparam>
    /// <param name="name">The name of the shared variable.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if the shared variable is found in the collection; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetSharedVariableValue<T>(string name, out T value);

    /// <summary>
    /// Logs trace info.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    void Trace(string message, params object[] args);

    /// <summary>
    /// Traces the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    void TraceValue(string name, object value);

    /// <summary>
    /// Traces the value.
    /// </summary>
    /// <param name="name1">The name1.</param>
    /// <param name="value1">The value1.</param>
    /// <param name="name2">The name2.</param>
    /// <param name="value2">The value2.</param>
    void TraceValue(string name1, object value1, string name2, object value2);

    /// <summary>
    /// Logs trace info via and includes an "elapsed time" stamp 
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    void TraceWithElapsedTime(string message, params object[] args);

    void TraceWithElapsedTimeEnter(string name);

    void TraceWithElapsedTimeExit(string name);

    void WrapInTraceWithElapsedTime(Action action, string name);

    /// <summary>
    /// Traces the value.
    /// </summary>
    /// <param name="name1">The name1.</param>
    /// <param name="value1">The value1.</param>
    /// <param name="name2">The name2.</param>
    /// <param name="value2">The value2.</param>
    /// <param name="name3">The name3.</param>
    /// <param name="value3">The value3.</param>
    void TraceValue(string name1, object value1, string name2, object value2, string name3, object value3);
  }
}
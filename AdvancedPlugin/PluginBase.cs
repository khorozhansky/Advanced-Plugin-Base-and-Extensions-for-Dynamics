namespace AdvancedPlugin
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  using Extensions;

  using Logging;

  using Microsoft.Crm.Sdk.Messages;
  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Client;
  using Microsoft.Xrm.Sdk.Query;

  /// <summary>
  /// Extended Base class for Plug-ins.
  /// </summary>
  /// <typeparam name="TEntity">A type that inherits from the <see cref="Entity" /></typeparam>
  /// <typeparam name="TContext">A type that inherits from the <see cref="OrganizationServiceContext" /></typeparam>
  /// <typeparam name="TTraceLog">The type of the trace log inherited from the <see cref="TraceLogBase"/>.</typeparam>
  /// <seealso cref="Microsoft.Xrm.Sdk.IPlugin" />
  /// <remarks>
  /// Just set <typeparamref name="TEntity" /> equals Entity in case the plugin is to handle different entities in a single plugin
  /// </remarks>
  public abstract class PluginBase<TEntity, TContext, TTraceLog> : IPlugin
    where TEntity : Entity, new()
    where TContext: OrganizationServiceContext
    where TTraceLog: TraceLogBase
  {
    /// <summary>
    /// The entity logical name registered in the plugin
    /// </summary>
    private readonly string registeredEntityLogicalName;

    /// <summary>
    /// The secure config
    /// </summary>
    private readonly string secureConfig;

    /// <summary>
    /// The unsecure config
    /// </summary>
    private readonly string unsecureConfig;

    /// <summary>
    /// The flag reflecting if the CustomVerboseLogSwitchedOn key is specified.
    /// </summary>
    private readonly bool customVerboseLogSwitchedOn;

    /// <summary>
    /// The Backing Field for <seealso cref="RegisteredPluginSteps"/>
    /// </summary>
    private List<PluginStep> registeredPluginSteps;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginBase{TEntity,TContext,TTracingService}" /> class.
    /// </summary>
    /// <param name="unsecureConfig">The unsecure config.</param>
    /// <param name="secureConfig">The secure config.</param>
    // ReSharper disable once PublicConstructorInAbstractClass
    protected PluginBase(string unsecureConfig, string secureConfig)
    {
      this.registeredEntityLogicalName =
        typeof(TEntity) == typeof(Entity) ? string.Empty : typeof(TEntity).Name.ToLower();

      this.unsecureConfig = unsecureConfig;
      const string CustomVerboseLogSwitchedOnKeyword = "CustomVerboseLogSwitchedOn";
      this.customVerboseLogSwitchedOn = unsecureConfig != null
                                        && unsecureConfig.Contains(CustomVerboseLogSwitchedOnKeyword);

      this.secureConfig = secureConfig;

      // ReSharper disable once VirtualMemberCallInContructor
      // This is a by design "exception" of a rule for VirtualMemberCallInContructor
      this.InitializeStepsDefinition();
    }

    /// <summary>
    /// Implements concrete builder of the <see cref="OrganizationServiceContext"/> instance.
    /// <remarks>
    /// When you implement a concrete plugin assembly, allocate the implementation of this method in the derived base plugin class,
    /// and then use that class as a base one for all your other plugins.
    /// </remarks>
    /// </summary>
    /// <param name="orgService">The org service.</param>
    /// <returns>An instance of <see cref="TContext"/> organization context.</returns>
    protected abstract TContext OrganizationContextBuilder(IOrganizationService orgService);

    /// <summary>
    /// Implements concrete builder of the <see cref="TraceLogBase"/> instance.
    /// <remarks>
    /// When you implement a concrete plugin assembly, allocate the implementation of this method in the derived base plugin class,
    /// and then use that class as a base one for all your other plugins.
    /// </remarks>
    /// </summary>
    /// <returns>An instance of <see cref="TTraceLog"/> type.</returns>
    protected abstract TTraceLog TraceLogBuilder(IPluginContext pluginCtx, ITracingService systemTracingService);

    /// <summary>
    /// Gets the name of the child class.
    /// </summary>
    /// <value>The name of the child class.</value>
    protected virtual string PluginName => this.GetType().FullName;

    /// <summary>
    /// Gets the registered plugin steps.
    /// </summary>
    /// <value>
    /// The registered plugin steps.
    /// </value>
    private List<PluginStep> RegisteredPluginSteps => 
      this.registeredPluginSteps ?? (this.registeredPluginSteps = new List<PluginStep>());

    /// <summary>
    /// Registers the plugin step.
    /// </summary>
    /// <param name="pluginStep">The plugin step.</param>
    protected void RegisterPluginStep(PluginStep pluginStep)
    {
      this.RegisteredPluginSteps.Add(pluginStep);
    }

    /// <summary>
    /// Registers the plugin step.
    /// </summary>
    /// <param name="pluginSteps">The plugin steps.</param>
    protected void RegisterPluginStep(IEnumerable<PluginStep> pluginSteps)
    {
      this.RegisteredPluginSteps.AddRange(pluginSteps);
    }

    /// <summary>
    /// Registers the plugin step.
    /// </summary>
    /// <param name="stage">The stage.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="pluginMessage">The plugin message.</param>
    /// <param name="handler">The handler.</param>
    protected void RegisterPluginStep(Stage? stage, Mode? mode, PluginMessage? pluginMessage, Action<PluginContext> handler)
    {
      this.RegisterPluginStep(new PluginStep(stage, mode, pluginMessage, handler));
    }

    /// <summary>
    /// Executes the plug-in.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider.
    /// </param>
    /// <remarks>
    /// For improved performance, Microsoft Dynamics CRM caches plug-in instances. 
    /// The plug-in's Execute method should be written to be stateless as the constructor 
    /// is not called for every invocation of the plug-in. Also, multiple system threads 
    /// could execute the plug-in at the same time. All per invocation state information 
    /// is stored in the context. This means that you should not use global variables in plug-ins.
    /// </remarks>
    public void Execute(IServiceProvider serviceProvider)
    {
      if (serviceProvider == null)
      {
        throw new ArgumentNullException(nameof(serviceProvider));
      }

      using (var pluginCtx = new PluginContext(serviceProvider, 
                                              this.registeredEntityLogicalName, 
                                              this.OrganizationContextBuilder, 
                                              this.TraceLogBuilder, 
                                              this.unsecureConfig, 
                                              this.secureConfig, 
                                              this.customVerboseLogSwitchedOn))
      {
        var stage = pluginCtx.Stage;
        var mode = pluginCtx.Mode;
        var messageName = pluginCtx.MessageName;
        var primaryEntityName = pluginCtx.PrimaryEntityName;

        pluginCtx.Trace(
          $"[PLUGIN START] Name: {this.PluginName}");

        Func<PluginStep, bool> stepSearchCondition =
          step =>
            (step.Stage == null || (int)step.Stage == stage) 
            && (step.Mode == null || (int)step.Mode == mode)
            && (
              string.IsNullOrEmpty(step.PluginMessageName) 
              || step.PluginMessageName.Equals(messageName, StringComparison.InvariantCultureIgnoreCase))
            && (
              string.IsNullOrWhiteSpace(this.registeredEntityLogicalName) 
              || primaryEntityName == "none" 
              || this.registeredEntityLogicalName.Equals(primaryEntityName, StringComparison.InvariantCultureIgnoreCase));

        var entityAction =
          this.RegisteredPluginSteps.Where(stepSearchCondition).Select(step => step.Handler).FirstOrDefault();

        if (entityAction == null)
        {
          pluginCtx.Trace("No appropriate registered action found.");
          pluginCtx.Trace(
            $"[PLUGIN SKIPPED] Entity: {primaryEntityName}, Message: {messageName}, Stage: {stage}, Mode: {mode}, Depth: {pluginCtx.Depth}, UserId: {pluginCtx.UserId}, InitiatingUserId: {pluginCtx.InitiatingUserId}");
          return;
        }

        pluginCtx.Trace("An appropriate registered action has been found. Executing the action...");
        try
        {
          entityAction(pluginCtx);
          TracePluginExitNotes(pluginCtx);
          pluginCtx.TraceLog.LogContext();
        }
        catch (Exception e)
        {
          TracePluginExitNotes(pluginCtx);
          pluginCtx.TraceLog.HandleException(e);
        }
      }
    }

    private static void TracePluginExitNotes(PluginContext pluginCtx)
    {
      pluginCtx.Trace("[PLUGIN EXIT] Exec Time: {0} ms", pluginCtx.StopWatch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Initializes the steps definition.
    /// <remarks>[IMPORTANT!] It's invoked from the <see cref="PluginBase{TEntity,TContext,TTracingService}"/> base constructor</remarks>
    /// </summary>
    protected abstract void InitializeStepsDefinition();

    /// <summary>
    /// Defines the local plugin context class.
    /// </summary>
    /// <seealso cref="Microsoft.Xrm.Sdk.IPlugin" />
    public class PluginContext : IPluginContext
    {
      /// <summary>
      /// The secured configuration
      /// </summary>
      internal readonly string SecureConfig;

      /// <summary>
      /// The unsecured configuration
      /// </summary>
      internal readonly string UnsecureConfig;

      #region PluginParameterKeyNames

      /// <summary>
      /// The target key.
      /// </summary>
      private const string TargetKey = "Target";

      /// <summary>
      /// The business entity key.
      /// </summary>
      private const string BusinessEntityKey = "BusinessEntity";

      /// <summary>
      /// The entity moniker key.
      /// </summary>
      private const string EntityMonikerKey = "EntityMoniker";

      /// <summary>
      /// The State key.
      /// </summary>
      private const string StateKey = "State";

      /// <summary>
      /// The Status key.
      /// </summary>
      private const string StatusKey = "Status";

      /// <summary>
      /// The ParameterXml key.
      /// </summary>
      private const string ParameterXmlKey = "ParameterXml";

      /// <summary>
      /// The RelatedEntities key.
      /// </summary>
      private const string RelatedEntitiesKey = "RelatedEntities";

      /// <summary>
      /// The Relationship key.
      /// </summary>
      private const string RelationshipKey = "Relationship";

      /// <summary>
      /// The SubordinateId key.
      /// </summary>
      private const string SubordinateIdKey = "SubordinateId";

      /// <summary>
      /// The ColumnSet key.
      /// </summary>
      private const string ColumnSetKey = "ColumnSet";

      /// <summary>
      /// The Query key.
      /// </summary>
      private const string QueryKey = "Query";

      /// <summary>
      /// The BusinessEntityCollection key.
      /// </summary>
      private const string BusinessEntityCollectionKey = "BusinessEntityCollection";

      /// <summary>
      /// The Assignee key.
      /// </summary>
      private const string AssigneeKey = "Assignee";

      /// <summary>
      /// The Principal key.
      /// </summary>
      private const string PrincipalKey = "Principal";

      /// <summary>
      /// The AccessRights key.
      /// </summary>
      private const string AccessRightsKey = "AccessRights";

      /// <summary>
      /// The UpdateContent key.
      /// </summary>
      private const string UpdateContentKey = "UpdateContent";

      #endregion

      /// <summary>
      /// The service provider
      /// </summary>
      // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
      private readonly IServiceProvider serviceProvider;

      /// <summary>
      /// Determines if the object is disposed
      /// </summary>
      private bool disposed;

      #region BackingFields

      /// <summary>
      /// Backing Field for <see cref="OrganizationServiceFactory" />
      /// </summary>
      private readonly Lazy<IOrganizationServiceFactory> organizationServiceFactory;

      /// <summary>
      /// Backing Field for <see cref="ExecContext" />
      /// </summary>
      private readonly Lazy<IPluginExecutionContext> executionContext;

      /// <summary>
      /// Backing Field for <see cref="TraceLog" />
      /// </summary>
      private readonly Lazy<TTraceLog> traceLog;

      /// <summary>
      /// Backing Field for <see cref="OrgService" />
      /// </summary>
      private readonly Lazy<IOrganizationService> orgService;

      /// <summary>
      /// Backing Field for <see cref="OrgServiceOnBehalfOfSystemUser" />
      /// </summary>
      private readonly Lazy<IOrganizationService> orgServiceOnBehalfOfSystemUser;

      /// <summary>
      /// Backing Field for <see cref="OrgServiceOnBehalfOfInitiatingUser" />
      /// </summary>
      private readonly Lazy<IOrganizationService> orgServiceOnBehalfOfInitiatingUser;

      /// <summary>
      /// Backing Field for <see cref="ServiceBusNotificationEndpoint" />
      /// </summary>
      private readonly Lazy<IServiceEndpointNotificationService> serviceBusNotificationEndpoint;

      /// <summary>
      /// Backing Field for <see cref="StopWatch"/>
      /// </summary>
      private readonly Lazy<Stopwatch> stopWatch;

      /// <summary>
      /// Backing Field for <see cref="PrimaryEntityRef" />
      /// </summary>
      private EntityReference primaryEntityRef;

      /// <summary>
      /// Backing Field for <see cref="OrgCtx" />
      /// </summary>
      private readonly Lazy<TContext> orgCtx;

      /// <summary>
      /// Backing Field for <see cref="OrgCtxOnBehalfOfSystemUser" />
      /// </summary>
      private readonly Lazy<TContext> orgCtxOnBehalfOfSystemUser;

      /// <summary>
      /// Backing Field for <see cref="OrgCtxOnBehalfOfInitiatingUser" />
      /// </summary>
      private readonly Lazy<TContext> orgCtxOnBehalfOfInitiatingUser;

      /// <summary>
      /// Backing Field for <see cref="TargetEntity"/>
      /// </summary>
      private TEntity targetEntity;

      /// <summary>
      /// Backing Field for  <see cref="RegisteredEntityLogicalName" />
      /// </summary>
      private readonly string registeredEntityLogicalName;

      /// <summary>
      /// Backing Field for  <see cref="TargetEntityAsBaseEntity"/>
      /// </summary>
      private Entity targetEntityAsBaseEntity;

      /// <summary>
      /// Backing Field for <see cref="TargetEntityExt"/>
      /// </summary>
      private EntityExt<TEntity> targetExt;

      /// <summary>
      /// Backing Field for <see cref="PostImageExt"/>
      /// </summary>
      private EntityExt<TEntity> postImageExt;

      /// <summary>
      /// Backing Field for  <see cref="OutputBusinessEntityAsBaseEntity"/>
      /// </summary>
      private Entity outputBusinessEntityAsBaseEntity;

      /// <summary>
      /// Backing Field for  <see cref="OutputBusinessEntity"/>
      /// </summary>
      private TEntity outputBusinessEntity;

      /// <summary>
      /// Backing Field for  <see cref="Relationship"/>
      /// </summary>
      private Relationship relationship;

      /// <summary>
      /// Backing Field for  <see cref="RelatedEntities"/>
      /// </summary>
      private EntityReferenceCollection relatedEntities;

      /// <summary>
      /// Backing Field for  <see cref="TargetAsEntityReference"/>
      /// </summary>      
      private EntityReference targetAsEntityReference;

      /// <summary>
      /// Backing Field for <see cref="Principal"/>
      /// </summary>
      private EntityReference principal;

      /// <summary>
      /// Backing Field for <see cref="ColumnSet"/>
      /// </summary>
      private ColumnSet columnSet;

      /// <summary>
      /// Backing Field for <see cref="State"/>
      /// </summary>
      private OptionSetValue state;

      /// <summary>
      /// Backing Field for <see cref="Status"/>
      /// </summary>
      private OptionSetValue status;

      /// <summary>
      /// Backing Field for <see cref="ParameterXml"/>
      /// </summary>
      private string parameterXml;

      /// <summary>
      /// Backing Field for  <see cref="SubordinateId"/>
      /// </summary>
      private Guid? subordinateReference;

      /// <summary>
      /// Backing Field for  <see cref="EntityMoniker"/>
      /// </summary>
      private EntityReference entityMoniker;

      /// <summary>
      /// Backing Field for  <see cref="PreImage"/>
      /// </summary>
      private TEntity preImage;

      /// <summary>
      /// Backing Field for  <see cref="PostImage"/>
      /// </summary>
      private TEntity postImage;

      /// <summary>
      /// Backing field for <see cref="QueryExpr"/>
      /// </summary>
      private QueryExpression query;

      /// <summary>
      /// Backing field for <see cref="Assignee"/>
      /// </summary>
      private EntityReference assignee;

      /// <summary>
      /// Backing Field for  <see cref="UpdateContentEntity"/>
      /// </summary>
      private Entity updateContentEntity;

      /// <summary>
      /// Backing Field for  <see cref="UpdateContent"/>
      /// </summary>
      private TEntity updateContent;

      /// <summary>
      /// Backing Field for  <see cref="AccessRights"/>
      /// </summary>
      private AccessRights? accessRights;

      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="PluginContext" /> class.
      /// </summary>
      /// <param name="serviceProvider">The service provider.</param>
      /// <param name="registeredEntityLogicalName">Name of the registered entity logical.</param>
      /// <param name="orgContextBuilder">The org context builder.</param>
      /// <param name="traceLogBuilder"></param>
      /// <param name="unsecureConfig">The unsecure config.</param>
      /// <param name="secureConfig">The secure config.</param>
      /// <param name="customVerboseLogSwitchedOn">if set to <c>true</c> [custom verbose log switched on].</param>
      /// <exception cref="System.ArgumentNullException">serviceProvider</exception>
      /// <exception cref="ArgumentNullException">serviceProvider</exception>
      public PluginContext(IServiceProvider serviceProvider, string registeredEntityLogicalName, Func<IOrganizationService, TContext> orgContextBuilder, Func<IPluginContext, ITracingService, TTraceLog> traceLogBuilder, string unsecureConfig, string secureConfig, bool customVerboseLogSwitchedOn)
      {
        if (serviceProvider == null)
        {
          throw new ArgumentNullException(nameof(serviceProvider));
        }

        this.registeredEntityLogicalName = registeredEntityLogicalName;
        this.serviceProvider = serviceProvider;
        this.UnsecureConfig = unsecureConfig;
        this.SecureConfig = secureConfig;
        this.CustomVerboseLogSwitchedOn = customVerboseLogSwitchedOn;

        this.stopWatch = new Lazy<Stopwatch>(Stopwatch.StartNew);

        this.executionContext = new Lazy<IPluginExecutionContext>(
          () => (IPluginExecutionContext)this.serviceProvider.GetService(typeof(IPluginExecutionContext)));

        this.organizationServiceFactory =
          new Lazy<IOrganizationServiceFactory>(
            () => (IOrganizationServiceFactory)this.serviceProvider.GetService(typeof(IOrganizationServiceFactory)));

        this.orgService = new Lazy<IOrganizationService>(
          () => this.OrganizationServiceFactory.CreateOrganizationService(this.UserId));

        this.orgServiceOnBehalfOfSystemUser = new Lazy<IOrganizationService>(
          () => this.OrganizationServiceFactory.CreateOrganizationService(null));

        this.orgServiceOnBehalfOfInitiatingUser = new Lazy<IOrganizationService>(
          () => this.OrganizationServiceFactory.CreateOrganizationService(this.InitiatingUserId));

        this.serviceBusNotificationEndpoint = new Lazy<IServiceEndpointNotificationService>(
          () => (IServiceEndpointNotificationService)this.serviceProvider.GetService(typeof(IServiceEndpointNotificationService)));

        this.traceLog = new Lazy<TTraceLog>(
          () =>
            {
              var systemTracingService = (ITracingService)this.serviceProvider.GetService(typeof(ITracingService));
              return traceLogBuilder(this, systemTracingService);
            });

        this.orgCtx = new Lazy<TContext>(
          () => {
            var c = orgContextBuilder(this.OrgService);
            c.MergeOption = MergeOption.NoTracking;
            return c;
          });

        this.orgCtxOnBehalfOfSystemUser = new Lazy<TContext>(
          () => {
            var c = orgContextBuilder(this.OrgServiceOnBehalfOfSystemUser);
            c.MergeOption = MergeOption.NoTracking;
            return c;
          });

        this.orgCtxOnBehalfOfInitiatingUser = new Lazy<TContext>(
          () => {
            var c = orgContextBuilder(this.OrgServiceOnBehalfOfInitiatingUser);
            c.MergeOption = MergeOption.NoTracking;
            return c;
          });
      }

      public bool CustomVerboseLogSwitchedOn { get; }

      /// <summary>
      /// Gets the plugin execution context.
      /// </summary>
      /// <value>The plugin execution context.</value>
      public IPluginExecutionContext ExecContext => this.executionContext.Value;

      /// <summary>
      /// Gets the organization service for the "current context user".
      /// </summary>
      /// <value>The organization service.</value>
      public IOrganizationService OrgService => this.orgService.Value;

      /// <summary>
      /// Gets the organization service for a system user.
      /// </summary>
      /// <remarks>
      /// Use it when the current context is not the system user's context but you need to perform some operation on behalf of the system user
      /// </remarks>
      /// <value>
      /// The organization service for system user.
      /// </value>
      public IOrganizationService OrgServiceOnBehalfOfSystemUser => this.orgServiceOnBehalfOfSystemUser.Value;

      /// <summary>
      /// Gets the organization service for a system user.
      /// </summary>
      /// <remarks>
      /// Use it when the current context is the system user's context but you need to perform some operation on behalf of the initiating user
      /// </remarks>
      /// <value>
      /// The organization service for system user.
      /// </value>
      public IOrganizationService OrgServiceOnBehalfOfInitiatingUser => this.orgServiceOnBehalfOfInitiatingUser.Value;

      /// <summary>
      /// Gets the notification service bus endpoint.
      /// </summary>
      /// <value>
      /// The notification service bus endpoint.
      /// </value>
      public IServiceEndpointNotificationService ServiceBusNotificationEndpoint => this.serviceBusNotificationEndpoint.Value;

      /// <summary>
      /// Gets the Trace Log.
      /// </summary>
      /// <value>The trace log.</value>
      public TTraceLog TraceLog => this.traceLog.Value;

      /// <summary>
      /// Gets the organization context on behalf of the "current user".
      /// </summary>
      /// <value>The organization context.</value>
      public TContext OrgCtx => this.orgCtx.Value;

      /// <summary>
      /// Gets the organization context on behalf of the "system user".
      /// </summary>
      /// <remarks>
      /// Use it when the current context is not the system user's context but you need to perform some operation on behalf of the system user
      /// </remarks>
      /// <value>The organization context.</value>
      public TContext OrgCtxOnBehalfOfSystemUser => this.orgCtxOnBehalfOfSystemUser.Value;

      /// <summary>
      /// Gets the organization context on behalf of the "Initiating user".
      /// </summary>
      /// <remarks>
      /// Use it when the current context is the system user's context but you need to perform some operation on behalf of the initiating user
      /// </remarks>
      /// <value>The organization context.</value>
      public TContext OrgCtxOnBehalfOfInitiatingUser => this.orgCtxOnBehalfOfInitiatingUser.Value;

      /// <summary>
      /// Gets the Stopwatch.
      /// </summary>
      /// <value>
      /// The Stopwatch.
      /// </value>
      public Stopwatch StopWatch => this.stopWatch.Value;

      #region Execution Context Properties

      /// <summary>
      /// Gets the primary entity identifier.
      /// </summary>
      /// <value>
      /// The primary entity identifier.
      /// </value>
      public Guid PrimaryEntityId => this.ExecContext.PrimaryEntityId;

      /// <summary>
      /// Gets the name of the primary entity.
      /// </summary>
      /// <value>
      /// The name of the primary entity.
      /// </value>
      public string PrimaryEntityName => this.ExecContext.PrimaryEntityName;

      /// <summary>
      /// Gets the primary entity reference.
      /// </summary>
      /// <value>
      /// The primary entity reference.
      /// </value>
      public EntityReference PrimaryEntityRef =>
        this.primaryEntityRef ??
          (this.primaryEntityRef = new EntityReference(this.PrimaryEntityName, this.PrimaryEntityId));

      /// <summary>
      /// Gets the name of the secondary entity that has a relationship with the primary entity.
      /// </summary>
      /// <value>
      /// The name of the secondary entity.
      /// </value>
      public string SecondaryEntityName => this.ExecContext.SecondaryEntityName;

      /// <summary>
      /// Gets the name of the Web service message that is being processed by the event execution pipeline.
      /// </summary>
      /// <value>
      /// The name of the message.
      /// </value>
      public string MessageName => this.ExecContext.MessageName;

      /// <summary>
      /// Gets the stage of plug-in execution..
      /// </summary>
      /// <value>
      /// The stage code.
      /// </value>
      public int Stage => this.ExecContext.Stage;

      /// <summary>
      /// Gets the mode of plug-in execution..
      /// </summary>
      /// <value>
      /// The mode code.
      /// </value>
      public int Mode => this.ExecContext.Mode;

      /// <summary>
      /// Gets the current depth of execution in the call stack.
      /// </summary>
      /// <value>
      /// The depth.
      /// </value>
      public int Depth => this.ExecContext.Depth;

      /// <summary>
      /// Gets the global unique identifier of the system user for whom the plug-in invokes web service methods on behalf of.
      /// </summary>
      /// <value>
      /// The user identifier.
      /// </value>
      public Guid UserId => this.ExecContext.UserId;

      /// <summary>
      /// Gets the global unique identifier of the system user account under which the current pipeline is executing.
      /// </summary>
      /// <value>
      /// The initiating user identifier.
      /// </value>
      public Guid InitiatingUserId => this.ExecContext.InitiatingUserId;

      /// <summary>
      /// Gets a value indicating if the plug-in is executing in the sandbox.
      /// </summary>
      /// <value>
      /// The isolation mode.
      /// </value>
      public int IsolationMode => this.ExecContext.IsolationMode;

      /// <summary>
      /// Gets the parent execution context.
      /// </summary>
      /// <value>
      /// The parent execute context.
      /// </value>
      public IPluginExecutionContext ParentExecContext => this.ExecContext.ParentContext;

      /// <summary>
      /// Gets the global unique identifier for tracking plug-in or custom workflow activity execution.
      /// </summary>
      /// <value>
      /// The correlation identifier.
      /// </value>
      public Guid CorrelationId => this.ExecContext.CorrelationId;

      /// <summary>
      /// Gets a value indicating if the plug-in is executing from the Microsoft Dynamics CRM for Microsoft Office Outlook with Offline Access client while it is offline.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is executing offline; otherwise, <c>false</c>.
      /// </value>
      public bool IsExecutingOffline => this.ExecContext.IsExecutingOffline;

      /// <summary>
      /// Gets a value indicating if the plug-in is executing within the database transaction.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is in transaction; otherwise, <c>false</c>.
      /// </value>
      public bool IsInTransaction => this.ExecContext.IsInTransaction;

      /// <summary>
      /// Gets a value indicating if the plug-in is executing as a result of the Microsoft Dynamics CRM for Microsoft Office Outlook with Offline Access client transitioning from offline to online and synchronizing with the Microsoft Dynamics CRM server.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is offline playback; otherwise, <c>false</c>.
      /// </value>
      public bool IsOfflinePlayback => this.ExecContext.IsOfflinePlayback;

      /// <summary>
      /// Gets the parameters of the request message that triggered the event that caused the plug-in to execute.
      /// </summary>
      /// <value>
      /// The input parameters.
      /// </value>
      public ParameterCollection InputParameters => this.ExecContext.InputParameters;

      /// <summary>
      /// Gets the parameters of the response message after the core platform operation has completed.
      /// </summary>
      /// <value>
      /// The output parameters.
      /// </value>
      public ParameterCollection OutputParameters => this.ExecContext.OutputParameters;

      /// <summary>
      /// Gets the custom properties that are shared between plug-ins.
      /// </summary>
      /// <value>
      /// The shared variables.
      /// </value>
      public ParameterCollection SharedVariables => this.ExecContext.SharedVariables;

      /// <summary>
      /// Gets the properties of the primary entity before the core platform operation has begins.
      /// </summary>
      /// <value>
      /// The pre entity images.
      /// </value>
      public EntityImageCollection PreEntityImages => this.ExecContext.PreEntityImages;

      /// <summary>
      /// Gets the properties of the primary entity after the core platform operation has been completed.
      /// </summary>
      /// <value>
      /// The post entity images.
      /// </value>
      public EntityImageCollection PostEntityImages => this.ExecContext.PostEntityImages;

      /// <summary>
      /// Gets the global unique identifier of the request being processed by the event execution pipeline.
      /// </summary>
      /// <value>
      /// The request identifier.
      /// </value>
      public Guid? RequestId => this.ExecContext.RequestId;

      /// <summary>
      /// Gets the global unique identifier of the related System Job.
      /// </summary>
      /// <value>
      /// The operation identifier.
      /// </value>
      public Guid OperationId => this.ExecContext.OperationId;

      /// <summary>
      /// Gets the date and time that the related System Job was created.
      /// </summary>
      /// <value>
      /// The operation created on.
      /// </value>
      public DateTime OperationCreatedOn => this.ExecContext.OperationCreatedOn;

      /// <summary>
      /// Gets a reference to the related SdkMessageProcessingingStep or ServiceEndpoint.
      /// </summary>
      /// <value>
      /// The owning extension.
      /// </value>
      public EntityReference OwningExtension => this.ExecContext.OwningExtension;

      /// <summary>
      /// Gets the global unique identifier of the business unit that the user making the request, also known as the calling user, belongs to.
      /// </summary>
      /// <value>
      /// The business unit identifier.
      /// </value>
      public Guid BusinessUnitId => this.ExecContext.BusinessUnitId;

      /// <summary>
      /// Gets the global unique identifier of the organization that the entity belongs to and the plug-in executes under.
      /// </summary>
      /// <value>
      /// The organization identifier.
      /// </value>
      public Guid OrganizationId => this.ExecContext.OrganizationId;

      /// <summary>
      /// Gets the unique name of the organization that the entity currently being processed belongs to and the plug-in executes under.
      /// </summary>
      /// <value>
      /// The name of the organization.
      /// </value>
      public string OrganizationName => this.ExecContext.OrganizationName;

      #endregion

      /// <summary>
      /// Gets a value indicating whether the Stage is Pre Validation.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is pre validation; otherwise, <c>false</c>.
      /// </value>
      public bool IsPreValidation => this.Stage == (int)AdvancedPlugin.Stage.PreValidate;

      /// <summary>
      /// Gets a value indicating whether the Stage is Pre Operation.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is pre operation; otherwise, <c>false</c>.
      /// </value>
      public bool IsPreOperation => this.Stage == (int)AdvancedPlugin.Stage.PreOperation;

      /// <summary>
      /// Gets a value indicating whether the Stage is Post Operation.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is post operation; otherwise, <c>false</c>.
      /// </value>
      public bool IsPostOperation => this.Stage == (int)AdvancedPlugin.Stage.PostOperation;

      /// <summary>
      /// Gets a value indicating whether the Mode is Asynchronous.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is asynchronous; otherwise, <c>false</c>.
      /// </value>
      public bool IsAsynchronous => this.Mode == (int)AdvancedPlugin.Mode.Asynchronous;

      /// <summary>
      /// Gets a value indicating whether this instance is synchronous.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance is synchronous; otherwise, <c>false</c>.
      /// </value>
      public bool IsSynchronous => !this.IsAsynchronous;

      /// <summary>
      /// Gets a value indicating whether the Mode is Synchronous.
      /// </summary>
      /// <value>
      /// The name of the base entity logical.
      /// </value>
      // ReSharper disable once ConvertToAutoProperty
      public string RegisteredEntityLogicalName => this.registeredEntityLogicalName;

      /// <summary>
      /// Gets or sets the input target.
      /// </summary>
      /// <value>The input target.</value>
      public TEntity TargetEntity
      {
        get
        {
          if (this.targetEntity != null)
          {
            return this.targetEntity;
          }

          if (this.TargetEntityAsBaseEntity != null)
          {
            this.targetEntity = this.TargetEntityAsBaseEntity.ToEntity<TEntity>();
          }

          return this.targetEntity;
        }

        set
        {
          this.targetEntityAsBaseEntity = value;
        }
      }

      /// <summary>
      /// Gets or sets the input target as Entity type.
      /// </summary>
      /// <value>The input target entity.</value>
      public Entity TargetEntityAsBaseEntity
      {
        get
        {
          return this.targetEntityAsBaseEntity ?? (this.targetEntityAsBaseEntity = this.GetInputParameter<Entity>(TargetKey));
        }

        set
        {
          this.targetEntityAsBaseEntity = value;
          this.SetInputParameter(TargetKey, value);
        }
      }

      /// <summary>
      /// Gets the input target extended.
      /// </summary>
      /// <value>
      /// The post image extended as <see cref="EntityExt{TEntity}"/>.
      /// </value>
      public EntityExt<TEntity> TargetEntityExt => 
        this.targetExt ?? (this.targetExt = new EntityExt<TEntity>(this.TargetEntity, this.PreImage, true));

      /// <summary>
      /// Gets the post image extended.
      /// </summary>
      /// <value>
      /// The post image extended as <see cref="EntityExt{TEntity}"/>.
      /// </value>
      public EntityExt<TEntity> PostImageExt => 
        this.postImageExt ?? (this.postImageExt = new EntityExt<TEntity>(this.PostImage, this.PreImage, false));

      /// <summary>
      /// Gets the input column set.
      /// </summary>
      /// <value>
      /// The input column set.
      /// </value>
      public ColumnSet ColumnSet => 
        this.columnSet ?? (this.columnSet = this.GetInputParameter<ColumnSet>(ColumnSetKey));

      /// <summary>
      /// Gets or sets the output business entity.
      /// </summary>
      /// <value>The entity.</value>
      public TEntity OutputBusinessEntity
      {
        get
        {
          if (this.outputBusinessEntity != null)
          {
            return this.outputBusinessEntity;
          }

          if (this.OutputBusinessEntityAsBaseEntity != null)
          {
            this.outputBusinessEntity = this.OutputBusinessEntityAsBaseEntity.ToEntity<TEntity>();
          }

          return this.outputBusinessEntity;
        }

        set
        {
          this.OutputBusinessEntityAsBaseEntity = value;
        }
      }

      /// <summary>
      /// Gets or sets the output business entity (as <see cref="Entity"/>).
      /// </summary>
      /// <value>The entity.</value>
      public Entity OutputBusinessEntityAsBaseEntity
      {
        get
        {
          return this.outputBusinessEntityAsBaseEntity
                 ?? (this.outputBusinessEntityAsBaseEntity = this.GetOutputParameter<Entity>(BusinessEntityKey));
        }

        set
        {
          this.outputBusinessEntityAsBaseEntity = value;
          this.SetOutputParameter(BusinessEntityKey, value);
        }
      }

      /// <summary>
      /// Gets the output business entity collection.
      /// </summary>
      /// <value>
      /// The output business entity collection.
      /// </value>
      public EntityCollection OutputBusinessEntityCollection => 
        this.GetOutputParameter<EntityCollection>(BusinessEntityCollectionKey);

      /// <summary>
      /// Gets the input target as entity reference.
      /// </summary>
      /// <value>The input target entity reference.</value>
      public EntityReference TargetAsEntityReference => 
        this.targetAsEntityReference ?? 
          (this.targetAsEntityReference = this.GetInputParameter<EntityReference>(TargetKey));

      /// <summary>
      /// Gets the principal.
      /// </summary>
      /// <value>The principal.</value>
      public EntityReference Principal => 
        this.principal ?? 
          (this.principal = this.GetInputParameter<EntityReference>(PrincipalKey));

      /// <summary>
      /// Gets or sets the access rights.
      /// </summary>
      /// <value>The access rights.</value>
      public AccessRights? AccessRights
      {
        get
        {
          if (this.accessRights != null)
          {
            return this.accessRights;
          }

          var value = this.GetInputParameter(AccessRightsKey);
          if (value == null)
          {
            this.accessRights = null;
          }
          else
          {
            this.accessRights = (AccessRights)value;
          }

          return this.accessRights;
        }

        set
        {
          this.accessRights = value;
          this.SetInputParameter(AccessRightsKey, value);
        }
      }

      /// <summary>
      /// Gets the state of the input.
      /// </summary>
      /// <value>The state of the input.</value>
      public OptionSetValue State => 
        this.state ?? (this.state = this.GetInputParameter<OptionSetValue>(StateKey));

      /// <summary>
      /// Gets the input status.
      /// </summary>
      /// <value>The input status.</value>
      public OptionSetValue Status => 
        this.status ?? (this.status = this.GetInputParameter<OptionSetValue>(StatusKey));

      /// <summary>
      /// Gets the relationship.
      /// </summary>
      /// <value>The relationship.</value>
      public Relationship Relationship => 
        this.relationship ?? (this.relationship = this.GetInputParameter<Relationship>(RelationshipKey));

      /// <summary>
      /// Gets the related entities.
      /// </summary>
      /// <value>The related entities.</value>
      public EntityReferenceCollection RelatedEntities => 
        this.relatedEntities ?? 
          (this.relatedEntities = this.GetInputParameter<EntityReferenceCollection>(RelatedEntitiesKey));

      /// <summary>
      /// Gets the ParameterXML parameter.
      /// </summary>
      /// <value>The parameter XML.</value>
      public string ParameterXml => 
        this.parameterXml ?? (this.parameterXml = this.GetInputParameter<string>(ParameterXmlKey));

      /// <summary>
      /// Gets the subordinate reference.
      /// </summary>
      /// <value>
      /// The subordinate reference.
      /// </value>
      public Guid? SubordinateId => 
        this.subordinateReference ?? 
          (this.subordinateReference = this.GetInputParameter<Guid>(SubordinateIdKey));

      /// <summary>
      /// Gets the entity moniker.
      /// </summary>
      /// <value>
      /// The entity moniker.
      /// </value>
      public EntityReference EntityMoniker => 
        this.entityMoniker ?? (this.entityMoniker = this.GetInputParameter<EntityReference>(EntityMonikerKey));

      /// <summary>
      /// Gets the query.
      /// </summary>
      /// <value>
      /// The query.
      /// </value>
      public QueryExpression QueryExpr => 
        this.query ?? (this.query = this.GetInputParameter(QueryKey) as QueryExpression);

      /// <summary>
      /// Gets the pre image.
      /// </summary>
      /// <value>
      /// The pre image.
      /// </value>
      public TEntity PreImage => 
        this.preImage ?? (this.preImage = this.GetPreImage());

      /// <summary>
      /// Gets the post image.
      /// </summary>
      /// <value>
      /// The post image.
      /// </value>
      public TEntity PostImage => 
        this.postImage ?? (this.postImage = this.GetPostImage());

      /// <summary>
      /// Gets the assignee.
      /// </summary>
      /// <value>The assignee.</value>
      public EntityReference Assignee => 
        this.assignee ?? (this.assignee = this.GetInputParameter<EntityReference>(AssigneeKey));

      /// <summary>
      /// Gets or sets the content of the update.
      /// </summary>
      /// <value>
      /// The content of the update.
      /// </value>
      public TEntity UpdateContent
      {
        get
        {
          if (this.updateContent != null)
          {
            return this.updateContent;
          }

          if (this.UpdateContentEntity != null)
          {
            this.updateContent = this.UpdateContentEntity.ToEntity<TEntity>();
          }

          return this.updateContent;
        }

        set
        {
          this.updateContentEntity = value;
        }
      }

      /// <summary>
      /// Gets or sets the update content entity.
      /// </summary>
      /// <value>
      /// The update content entity.
      /// </value>
      public Entity UpdateContentEntity
      {
        get
        {
          return this.updateContentEntity
                 ?? (this.updateContentEntity = this.GetInputParameter<Entity>(UpdateContentKey));
        }

        set
        {
          this.updateContentEntity = value;
          this.SetInputParameter(UpdateContentKey, value);
        }
      }

      /// <summary>
      /// Gets the organization service factory.
      /// </summary>
      /// <value>
      /// The organization service factory.
      /// </value>
      private IOrganizationServiceFactory OrganizationServiceFactory => this.organizationServiceFactory.Value;

      /// <summary>
      /// Notifies the service bus endpoint sending the content of the current execution context.
      /// </summary>
      /// <param name="serviceBusEndpointId">Id of the Service Bus endpoint</param>
      public void NotifyServiceBusEndpoint(Guid serviceBusEndpointId)
      {
        this.TraceWithElapsedTime($"Notifying Service Bus Endpoint (ID: {serviceBusEndpointId})...");
        // ReSharper disable once RedundantCast
        const string EntityLogicalName = "serviceendpoint";
        var serviceBusEndpointRef = new EntityReference(EntityLogicalName, serviceBusEndpointId);
        this.ServiceBusNotificationEndpoint.Execute(serviceBusEndpointRef, (IExecutionContext)this.ExecContext);
        this.TraceWithElapsedTime("Service Bus notified.");
      }

      /// <summary>
      /// Determines whether the specified pluginMessage name contains pluginMessage.
      /// </summary>
      /// <param name="pluginMessage">
      /// Name of the pluginMessage.
      /// </param>
      /// <returns>
      /// true in case it contains the pluginMessage
      /// </returns>
      public bool GetIsMessage(PluginMessage pluginMessage)
      {
        return this.GetIsMessage(pluginMessage.ToString());
      }

      /// <summary>
      /// Determines whether the specified plugin message name is message.
      /// </summary>
      /// <param name="pluginMessageName">Name of the plugin message.</param>
      /// <returns>
      ///   <c>true</c> if the specified plugin message name is message; otherwise, <c>false</c>.
      /// </returns>
      public bool GetIsMessage(string pluginMessageName)
      {
        return this.MessageName != null && this.MessageName.Equals(pluginMessageName, StringComparison.InvariantCultureIgnoreCase);
      }

      /// <summary>
      /// Logs trace info.
      /// </summary>
      /// <param name="message">The message.</param>
      /// <param name="args">The arguments.</param>
      public void Trace(string message, params object[] args)
      {
        this.TraceLog.TracingService.Trace(message, args);
      }

      /// <summary>
      /// Logs trace info via and includes an "elapsed time" stamp 
      /// </summary>
      /// <param name="message">The message.</param>
      /// <param name="args">The arguments.</param>
      public void TraceWithElapsedTime(string message, params object[] args)
      {
        this.TraceLog.TracingService.Trace($"{message} [{this.StopWatch.Elapsed.TotalMilliseconds} ms]", args);
      }

      /// <summary>
      /// Gets the pre image entity.
      /// </summary>
      /// <param name="name">Name of the image.</param>
      /// <returns>
      /// Pre Image Entity
      /// </returns>
      public Entity GetPreImageEntity(string name = null)
      {
        return this.GetImageEntity(this.PreEntityImages, name ?? "PreImage");
      }

      /// <summary>
      /// Gets the pre image.
      /// </summary>
      /// <param name="name">Name of the image.</param>
      /// <returns>
      /// Pre Image
      /// </returns>
      public TEntity GetPreImage(string name = null)
      {
        var entity = this.GetPreImageEntity(name);
        return entity?.ToEntity<TEntity>();
      }

      /// <summary>
      /// Gets the post image entity.
      /// </summary>
      /// <param name="name">Name of the image.</param>
      /// <returns>
      /// Post Image Entity
      /// </returns>
      public Entity GetPostImageEntity(string name = null)
      {
        return this.GetImageEntity(this.PostEntityImages, name ?? "PostImage");
      }

      /// <summary>
      /// Gets the post image.
      /// </summary>
      /// <param name="name">Name of the image.</param>
      /// <returns>
      /// Post Image
      /// </returns>
      public TEntity GetPostImage(string name = null)
      {
        var entity = this.GetPostImageEntity(name);
        return entity?.ToEntity<TEntity>();
      }

      /// <summary>
      /// Tries to get a shared variable value.
      /// </summary>
      /// <param name="name">The name of the shared variable.</param>
      /// <param name="execCtx">An instance of <see cref="IPluginExecutionContext" />.</param>
      /// <param name="value">The value.</param>
      /// <returns>
      ///   <c>true</c> if the shared variable is found in the collection; otherwise, <c>false</c>.
      /// </returns>
      public bool TryGetSharedVariableValue(string name, IPluginExecutionContext execCtx, out object value)
      {
        if (execCtx == null || string.IsNullOrWhiteSpace(name))
        {
          value = null;
          return false;
        }

        var vars = execCtx.SharedVariables;
        var result = vars.ContainsKey(name);
        value = result ? vars[name] : null;
        return result;
      }

      /// <summary>
      /// Tries to get a shared variable value.
      /// </summary>
      /// <param name="name">The name of the shared variable.</param>
      /// <param name="value">The value.</param>
      /// <returns>
      ///   <c>true</c> if the shared variable is found in the collection; otherwise, <c>false</c>.
      /// </returns>
      public bool TryGetSharedVariableValue(string name, out object value)
      {
        var execCtx = this.ExecContext;
        return 
          this.TryGetSharedVariableValue(name, execCtx, out value) 
            || this.TryGetSharedVariableValue(name, execCtx.ParentContext, out value);
      }

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
      public bool TryGetSharedVariableValue<T>(string name, IPluginExecutionContext execCtx, out T value)
      {
        object objValue;
        var result = this.TryGetSharedVariableValue(name, execCtx, out objValue);
        value = result ? (T)objValue : default(T);
        return result;
      }

      /// <summary>
      /// Tries to get a shared variable value.
      /// </summary>
      /// <typeparam name="T">Expected type of the shared variable value</typeparam>
      /// <param name="name">The name of the shared variable.</param>
      /// <param name="value">The value.</param>
      /// <returns>
      ///   <c>true</c> if the shared variable is found in the collection; otherwise, <c>false</c>.
      /// </returns>
      public bool TryGetSharedVariableValue<T>(string name, out T value)
      {
        object objValue;
        var result = this.TryGetSharedVariableValue(name, out objValue);
        value = result ? (T)objValue : default(T);
        return result;
      }

      /// <summary>
      /// Sets the shared variable.
      /// </summary>
      /// <param name="name">
      /// The name.
      /// </param>
      /// <param name="value">
      /// The value.
      /// </param>
      public void SetSharedVariable(string name, object value)
      {
        var vars = this.SharedVariables;
        if (vars.Contains(name))
        {
          vars[name] = value;
        }
        else
        {
          vars.Add(name, value);
        }
      }

      /// <summary>
      /// Gets the input parameter.
      /// </summary>
      /// <typeparam name="TK">The type of the TK.</typeparam>
      /// <param name="key">The key.</param>
      /// <returns>
      /// An instance of the TK
      /// </returns>
      public TK GetInputParameter<TK>(string key)
      {
        var parameters = this.InputParameters;
        return this.GetParameter<TK>(parameters, key);
      }

      /// <summary>
      /// Gets the input parameter.
      /// </summary>
      /// <param name="key">The key.</param>
      /// <returns>
      /// Parameter value
      /// </returns>
      public object GetInputParameter(string key)
      {
        var parameters = this.InputParameters;
        return this.GetParameter(parameters, key);
      }

      /// <summary>
      /// Gets the output parameter.
      /// </summary>
      /// <typeparam name="TK">The type of the TK.</typeparam>
      /// <param name="key">The key.</param>
      /// <returns>
      /// An instance of the TK
      /// </returns>
      public TK GetOutputParameter<TK>(string key)
      {
        var parameters = this.OutputParameters;
        return this.GetParameter<TK>(parameters, key);
      }

      /// <summary>
      /// Gets the output parameter.
      /// </summary>
      /// <param name="key">The key.</param>
      /// <returns>
      /// Parameter value
      /// </returns>
      public object GetOutputParameter(string key)
      {
        var parameters = this.OutputParameters;
        return this.GetParameter(parameters, key);
      }

      /// <summary>
      /// Gets the parameter.
      /// </summary>
      /// <typeparam name="TK">The type of the TK.</typeparam>
      /// <param name="parameters">The parameters.</param>
      /// <param name="key">The key.</param>
      /// <returns>
      /// An instance of the TK
      /// </returns>
      public TK GetParameter<TK>(ParameterCollection parameters, string key)
      {
        if (!parameters.ContainsKey(key))
        {
          return default(TK);
        }

        return (TK)parameters[key];
      }

      /// <summary>
      /// Gets the parameter.
      /// </summary>
      /// <param name="parameters">The parameters.</param>
      /// <param name="key">The key.</param>
      /// <returns>
      /// Parameter value
      /// </returns>
      public object GetParameter(ParameterCollection parameters, string key)
      {
        return !parameters.ContainsKey(key) ? null : parameters[key];
      }

      /// <summary>
      /// Sets the input parameter.
      /// </summary>
      /// <param name="key">The key.</param>
      /// <param name="value">The value.</param>
      public void SetInputParameter(string key, object value)
      {
        this.SetParameter(this.InputParameters, key, value);
      }

      /// <summary>
      /// Sets the output parameter.
      /// </summary>
      /// <param name="key">The key.</param>
      /// <param name="value">The value.</param>
      public void SetOutputParameter(string key, object value)
      {
        this.SetParameter(this.OutputParameters, key, value);
      }

      /// <summary>
      /// Sets the parameter.
      /// </summary>
      /// <param name="parameters">The parameters.</param>
      /// <param name="key">The key.</param>
      /// <param name="value">The value.</param>
      public void SetParameter(ParameterCollection parameters, string key, object value)
      {
        parameters[key] = value;
      }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        this.Dispose(true);
        GC.SuppressFinalize(this);
      }

      /// <summary>
      /// Releases unmanaged and - optionally - managed resources.
      /// </summary>
      /// <param name="disposing">
      /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
      /// </param>
      protected virtual void Dispose(bool disposing)
      {
        if (this.disposed)
        {
          return;
        }

        if (disposing)
        {
          if (this.orgCtx.IsValueCreated)
          {
            this.orgCtx.Value.Dispose();
          }

          if (this.orgCtxOnBehalfOfSystemUser.IsValueCreated)
          {
            this.orgCtxOnBehalfOfSystemUser.Value.Dispose();
          }

          if (this.stopWatch.IsValueCreated)
          {
            if (this.StopWatch.IsRunning)
            {
              try
              {
                this.StopWatch.Stop();
              }
              catch
              {
                // ignored
              }
            }
          }
        }

        this.disposed = true;
      }

      /// <summary>
      /// Gets the entity image.
      /// </summary>
      /// <param name="images">An instance of <see cref="EntityImageCollection"/>.</param>
      /// <param name="name">Name of the image.</param>
      /// <returns>
      /// The entity image.
      /// </returns>
      /// <exception cref="ArgumentNullException">
      /// name
      /// or
      /// images
      /// </exception>
      private Entity GetImageEntity(EntityImageCollection images, string name)
      {
        if (string.IsNullOrEmpty(name))
        {
          throw new ArgumentNullException(nameof(name));
        }

        if (images == null)
        {
          throw new ArgumentNullException(nameof(images));
        }

        Entity result;
        return images.TryGetValue(name, out result) ? result : null;
      }
    }

    /// <summary>
    /// Defines the base Plugin Step class
    /// <remarks>
    /// Use it to register step event in the derived plugin class
    /// </remarks>
    /// </summary>
    /// <seealso cref="Microsoft.Xrm.Sdk.IPlugin" />
    public class PluginStep
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="PluginStep"/> class.
      /// </summary>
      public PluginStep()
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="PluginStep"/> class.
      /// </summary>
      /// <param name="stage">The stage.</param>
      /// <param name="mode">The mode.</param>
      /// <param name="pluginMessage">The plugin message.</param>
      /// <param name="handler">The handler.</param>
      public PluginStep(Stage? stage, Mode? mode, PluginMessage? pluginMessage, Action<PluginContext> handler)
        : this(stage, mode, pluginMessage.ToString(), handler)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="PluginStep"/> class.
      /// </summary>
      /// <param name="stage">The stage.</param>
      /// <param name="mode">The mode.</param>
      /// <param name="pluginMessageName">Name of the plugin message.</param>
      /// <param name="handler">The handler.</param>
      public PluginStep(Stage? stage, Mode? mode, string pluginMessageName, Action<PluginContext> handler)
      {
        this.Stage = stage;
        this.Mode = mode;
        this.PluginMessageName = pluginMessageName;
        this.Handler = handler;
      }

      /// <summary>
      /// Gets or sets the pipeline stage.
      /// </summary>
      /// <value>
      /// The pipeline stage.
      /// </value>
      public Stage? Stage { get; set; }

      /// <summary>
      /// Gets or sets the execution mode.
      /// </summary>
      /// <value>
      /// The mode.
      /// </value>
      public Mode? Mode { get; set; }

      /// <summary>
      /// Gets or sets the Plugin Message Name.
      /// </summary>
      /// <value>
      /// The Plugin Message Name.
      /// </value>
      public string PluginMessageName { get; set; }

      /// <summary>
      /// Sets the Plugin Message.
      /// </summary>
      /// <value>
      /// The plugin message.
      /// </value>
      public PluginMessage PluginMessage
      {
        set
        {
          this.PluginMessageName = value.ToString();
        }
      }

      /// <summary>
      /// Gets or sets the action that fires for the event.
      /// </summary>
      /// <value>
      /// The handler.
      /// </value>
      public readonly Action<PluginContext> Handler;
    }
  }
}
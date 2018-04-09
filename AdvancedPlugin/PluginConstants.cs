namespace AdvancedPlugin
{
  using System.Diagnostics.CodeAnalysis;

  /// <summary>
  /// Pipeline Stage
  /// </summary>
  public enum Stage
  {
    PreValidate = 10,
    PreOperation = 20,
    MainOperation = 30,
    PostOperation = 40
  }

  /// <summary>
  /// Execution Mode
  /// </summary>
  public enum Mode
  {
    Synchronous = 0,
    Asynchronous = 1
  }

  /// <summary>
  /// Execution Message Name
  /// <remarks>
  /// Add additional messages here as needed
  /// </remarks>
  /// </summary>
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public enum PluginMessage
  {
    AddItem,
    AddListMembers,
    AddMember,
    AddMembers,
    AddPrincipalToQueue,
    AddPrivileges,
    AddProductToKit,
    AddRecurrence,
    AddToQueue,
    AddUserToRecordTeam,
    ApplyRecordCreationAndUpdateRule,
    Assign,
    Associate,
    BackgroundSend,
    Book,
    CalculatePrice,
    Cancel,
    CheckIncoming,
    CheckPromote,
    Clone,
    CloneMobileOfflineProfile,
    CloneProduct,
    Close,
    CopyDynamicListToStatic,
    CopySystemForm,
    Create,
    CreateException,
    CreateInstance,
    CreateKnowledgeArticleTranslation,
    CreateKnowledgeArticleVersion,
    Delete,
    DeleteOpenInstances,
    DeliverIncoming,
    DeliverPromote,
    Disassociate,
    Execute,
    ExecuteById,
    Export,
    GenerateSocialProfile,
    GetDefaultPriceLevel,
    GrantAccess,
    Import,
    LockInvoicePricing,
    LockSalesOrderPricing,
    Lose,
    Merge,
    ModifyAccess,
    PickFromQueue,
    Publish,
    PublishAll,
    PublishTheme,
    QualifyLead,
    Recalculate,
    ReleaseToQueue,
    RemoveFromQueue,
    RemoveItem,
    RemoveMember,
    RemoveMembers,
    RemovePrivilege,
    RemoveProductFromKit,
    RemoveRelated,
    RemoveUserFromRecordTeam,
    ReplacePrivileges,
    Reschedule,
    Retrieve,
    RetrieveExchangeRate,
    RetrieveFilteredForms,
    RetrieveMultiple,
    RetrievePersonalWall,
    RetrievePrincipalAccess,
    RetrieveRecordWall,
    RetrieveSharedPrincipalsAndAccess,
    RetrieveUnpublished,
    RetrieveUnpublishedMultiple,
    RetrieveUserQueues,
    RevokeAccess,
    RouteTo,
    Send,
    SendFromTemplate,
    SetLocLabels,
    SetRelated,
    SetState,
    TriggerServiceEndpointCheck,
    UnlockInvoicePricing,
    UnlockSalesOrderPricing,
    Update,
    ValidateRecurrenceRule,
    Win,
  }
}


namespace AdvancedPlugin.Utils
{
  using System;
  using System.Collections.Generic;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Metadata;
  using Microsoft.Xrm.Sdk.Metadata.Query;
  using Microsoft.Xrm.Sdk.Organization;
  using Microsoft.Xrm.Sdk.Query;

  public static class ExecutionContextHelper
  {
    private static readonly List<Type> ProcessedTypes;

    static ExecutionContextHelper()
    {
      ProcessedTypes = new List<Type>
                         {
                           typeof(bool),
                           typeof(bool[]),
                           typeof(int),
                           typeof(int[]),
                           typeof(string),
                           typeof(string[]),
                           typeof(string[][]),
                           typeof(double),
                           typeof(double[]),
                           typeof(Decimal),
                           typeof(Decimal[]),
                           typeof(Guid),
                           typeof(Guid[]),
                           typeof(DateTime),
                           typeof(DateTime[]),
                           typeof(Money),
                           typeof(Money[]),
                           typeof(EntityReference),
                           typeof(EntityReference[]),
                           typeof(OptionSetValue),
                           typeof(OptionSetValue[]),
                           typeof(EntityCollection),
                           typeof(Money),
                           typeof(Label),
                           typeof(LocalizedLabel),
                           typeof(LocalizedLabelCollection),
                           typeof(EntityMetadata[]),
                           typeof(EntityMetadata),
                           typeof(AttributeMetadata[]),
                           typeof(AttributeMetadata),
                           typeof(RelationshipMetadataBase[]),
                           typeof(RelationshipMetadataBase),
                           typeof(EntityFilters),
                           typeof(OptionSetMetadataBase),
                           typeof(OptionSetMetadataBase[]),
                           typeof(OptionSetMetadata),
                           typeof(BooleanOptionSetMetadata),
                           typeof(OptionSetType),
                           typeof(ManagedPropertyMetadata),
                           typeof(ManagedPropertyMetadata[]),
                           typeof(EntityKeyMetadata[]),
                           typeof(EntityKeyMetadata),
                           typeof(BooleanManagedProperty),
                           typeof(AttributeRequiredLevelManagedProperty),
                           typeof(EndpointAccessType),
                           typeof(AliasedValue),
                           typeof(Dictionary<string, string>),
                           typeof(Entity),
                           typeof(Entity[]),
                           typeof(ColumnSet),
                           typeof(EntityReferenceCollection),
                           typeof(QueryBase),
                           typeof(QueryExpression),
                           typeof(QueryExpression[]),
                           typeof(LocalizedLabel[]),
                           typeof(PagingInfo),
                           typeof(Relationship),
                           typeof(AttributePrivilegeCollection),
                           typeof(RelationshipQueryCollection),
                           typeof(EntityMetadataCollection),
                           typeof(OneToManyRelationshipMetadata[]),
                           typeof(MetadataFilterExpression),
                           typeof(MetadataConditionExpression),
                           typeof(MetadataQueryBase),
                           typeof(DeletedMetadataFilters),
                           typeof(DeletedMetadataCollection),
                           typeof(ExecuteMultipleSettings),
                           typeof(OrganizationRequestCollection),
                           typeof(OrganizationResponseCollection),
                           typeof(ExecuteMultipleResponseItemCollection),
                           typeof(QuickFindResultCollection),
                           typeof(QuickFindConfigurationCollection),
                           typeof(AttributeMappingCollection),
                           typeof(MailboxTrackingFolderMappingCollection),
                           typeof(OrganizationDetail),
                           typeof(KeyAttributeCollection),
                           typeof(BusinessEntityChanges),
                           typeof(BusinessEntityChangesCollection),
                           typeof(ConcurrencyBehavior),
                           typeof(ChangeType),
                           typeof(NewOrUpdatedItem),
                           typeof(RemovedOrDeletedItem),
                           typeof(EntityAttributeCollection)
                         };
    }

    public static void CleanUpContextCollections(this IPluginExecutionContext execCtx)
    {
      execCtx.InputParameters.Clear();
      execCtx.OutputParameters.Clear();
      execCtx.PostEntityImages.Clear();
      execCtx.PreEntityImages.Clear();
      execCtx.SharedVariables.Clear();
      if (execCtx.ParentContext == null)
      {
        return;
      }
        
      CleanUpContextCollections(execCtx.ParentContext);
    }

    public static void UpdateContext(this IPluginExecutionContext currentContext, IPluginExecutionContext contextUpdate)
    {
      if (currentContext == null)
        throw new ArgumentNullException(nameof(currentContext));
      if (contextUpdate == null)
        throw new ArgumentNullException(nameof(contextUpdate));
      CopyParameterCollection(contextUpdate.InputParameters, currentContext.InputParameters);
      CopyParameterCollection(contextUpdate.OutputParameters, currentContext.OutputParameters);
      CopyEntityImageCollection(contextUpdate.PreEntityImages, currentContext.PreEntityImages);
      CopyEntityImageCollection(contextUpdate.PostEntityImages, currentContext.PostEntityImages);
      CopyParameterCollection(contextUpdate.SharedVariables, currentContext.SharedVariables);
      if (currentContext.ParentContext == null || contextUpdate.ParentContext == null)
      {
        return;
      }
        
      UpdateContext(currentContext.ParentContext, contextUpdate.ParentContext);
    }

    public static RemoteExecutionContext CopyContext(this IPluginExecutionContext context)
    {
      if (context == null)
        return null;
      var executionContext = new RemoteExecutionContext {
        BusinessUnitId = context.BusinessUnitId,
        CorrelationId = context.CorrelationId,
        Depth = context.Depth,
        InitiatingUserId = context.InitiatingUserId
      };

      CopyParameterCollection(context.InputParameters, executionContext.InputParameters);
      executionContext.IsExecutingOffline = context.IsExecutingOffline;
      executionContext.IsOfflinePlayback = context.IsOfflinePlayback;
      executionContext.IsInTransaction = context.IsInTransaction;
      executionContext.IsolationMode = context.IsolationMode;
      executionContext.MessageName = context.MessageName;
      executionContext.Mode = context.Mode;
      executionContext.OperationCreatedOn = context.OperationCreatedOn;
      executionContext.OperationId = context.OperationId;
      executionContext.OrganizationId = context.OrganizationId;
      executionContext.OrganizationName = context.OrganizationName;
      CopyParameterCollection(context.OutputParameters, executionContext.OutputParameters);
      executionContext.OwningExtension = context.OwningExtension;
      CopyEntityImageCollection(context.PreEntityImages, executionContext.PreEntityImages);
      CopyEntityImageCollection(context.PostEntityImages, executionContext.PostEntityImages);
      executionContext.PrimaryEntityId = context.PrimaryEntityId;
      executionContext.PrimaryEntityName = context.PrimaryEntityName;
      executionContext.SecondaryEntityName = context.SecondaryEntityName;
      CopyParameterCollection(context.SharedVariables, executionContext.SharedVariables);
      executionContext.UserId = context.UserId;
      executionContext.RequestId = context.RequestId;
      executionContext.Stage = context.Stage;
      executionContext.ParentContext = CopyContext(context.ParentContext);
      return executionContext;
    }

    private static void CopyParameterCollection(ParameterCollection source, ParameterCollection destination)
    {
      if (source == null || destination == null)
        return;
      foreach (KeyValuePair<string, object> keyValuePair in source)
      {
        if (keyValuePair.Value == null || IsProcessedType(keyValuePair.Value.GetType()))
          destination[keyValuePair.Key] = keyValuePair.Value;
      }
    }

    private static void CopyEntityImageCollection(EntityImageCollection source, EntityImageCollection destination)
    {
      if (source == null || destination == null)
        return;
      foreach (KeyValuePair<string, Entity> keyValuePair in source)
        destination.Add(keyValuePair);
    }

    private static bool IsProcessedType(Type valueType)
    {
      return ProcessedTypes.Contains(valueType);
    }
  }
}

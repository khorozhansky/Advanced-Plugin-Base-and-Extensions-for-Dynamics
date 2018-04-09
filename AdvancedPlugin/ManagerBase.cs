namespace AdvancedPlugin
{
  using System;
  using System.Collections.Generic;

  using Logging;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Client;

  /// <summary>
  /// Base class for entity manager classes
  /// </summary>
  /// <typeparam name="TEntity">Entity</typeparam>
  /// <typeparam name="TContext">OrganizationServiceContext</typeparam>
  /// <typeparam name="TTraceLog">A type that inherits from the <see cref="TTraceLog"/></typeparam>
  /// TODO: Add the rest basic validators for other PluginContext properties
  public abstract class ManagerBase<TEntity, TContext, TTraceLog> 
    where TEntity : Entity, new()
    where TContext : OrganizationServiceContext
    where TTraceLog : TraceLogBase
  {
    protected readonly PluginBase<TEntity, TContext, TTraceLog>.PluginContext PluginCtx;

    /// <summary>
    /// Gets or sets the target validators.
    /// </summary>
    /// <value>
    /// The target validators.
    /// </value>
    private static readonly Dictionary<string, Action<PluginBase<TEntity, TContext, TTraceLog>.PluginContext>[]> BaseMessageValidators;

    /// <summary>
    /// Initializes static members of the <see cref="ManagerBase{TEntity,TContext,TTraceLog}"/> class. 
    /// TODO: Refactor this
    /// </summary>
    static ManagerBase()
    {
      BaseMessageValidators = new Dictionary<string, Action<PluginBase<TEntity, TContext, TTraceLog>.PluginContext>[]>
      {
        {
          PluginMessage.Create.ToString(), new Action<PluginBase<TEntity, TContext, TTraceLog>.PluginContext>[]
          {
            ValidateTargetEntityAsBaseEntity
          }
        },
        {
          PluginMessage.Update.ToString(), new Action<PluginBase<TEntity, TContext, TTraceLog>.PluginContext>[]
          {
            ValidateTargetEntityAsBaseEntity
          }
        },
        {
          PluginMessage.Delete.ToString(), new Action<PluginBase<TEntity, TContext, TTraceLog>.PluginContext>[]
          {
            ValidateTargetEntityReference
          }
        },

        // TODO: add other basic validators
      };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerBase{TEntity,TContext,TTraceLog}" /> class.
    /// </summary>
    /// <param name="pluginCtx">The local context.</param>
    protected ManagerBase(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginCtx)
    {
      this.PluginCtx = pluginCtx;
    }

    /// <summary>
    /// Validates the by base validators.
    /// </summary>
    protected void ValidateByBaseValidators()
    {
      var messageName = this.PluginCtx.MessageName;
      if (!BaseMessageValidators.ContainsKey(messageName))
      {
        return;
      }

      var validators = BaseMessageValidators[messageName];
      foreach (var validator in validators)
      {
        validator(this.PluginCtx);
      }
    }

    /// <summary>
    /// Validates the target entity as base entity.
    /// </summary>
    protected void ValidateTargetEntityAsBaseEntity()
    {
      ValidateTargetEntityAsBaseEntity(this.PluginCtx);
    }

    /// <summary>
    /// Validates the target entity reference.
    /// </summary>
    protected void ValidateTargetEntityReference()
    {
      ValidateTargetEntityReference(this.PluginCtx);
    }

    /// <summary>
    /// Validates the pre image.
    /// </summary>
    protected void ValidatePreImage()
    {
      ValidatePreImage(this.PluginCtx);
    }

    /// <summary>
    /// Validates the post image.
    /// </summary>
    protected void ValidatePostImage()
    {
      ValidatePostImage(this.PluginCtx);
    }

    /// <summary>
    /// Validates the column set.
    /// </summary>
    protected void ValidateColumnSet()
    {
      ValidateColumnSet(this.PluginCtx);
    }

    /// <summary>
    /// Validates the output business entity as base entity.
    /// </summary>
    protected void ValidateOutputBusinessEntityAsBaseEntity()
    {
      ValidateOutputBusinessEntityAsBaseEntity(this.PluginCtx);
    }

    /// <summary>
    /// Validates the output business entity collection.
    /// </summary>
    protected void ValidateOutputBusinessEntityCollection()
    {
      ValidateOutputBusinessEntityCollection(this.PluginCtx);
    }

    /// <summary>
    /// Validates the target as entity.
    /// </summary>
    /// <param name="pluginContext">The local context.</param>
    /// <exception cref="NullReferenceException">TargetEntityAsBaseEntity</exception>
    private static void ValidateTargetEntityAsBaseEntity(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.TargetEntityAsBaseEntity == null)
      {
        throw new NullReferenceException(nameof(pluginContext.TargetEntityAsBaseEntity));
      }
    }
    
    /// <summary>
    /// Validates the target entity reference.
    /// </summary>
    /// <param name="pluginContext">The local context.</param>
    /// <exception cref="System.NullReferenceException">TargetAsEntityReference</exception>
    private static void ValidateTargetEntityReference(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.TargetAsEntityReference == null)
      {
        throw new NullReferenceException(nameof(pluginContext.TargetAsEntityReference));
      }
    }

    /// <summary>
    /// Validates the pre image.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">PreImage</exception>
    private static void ValidatePreImage(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.PreImage == null)
      {
        throw new NullReferenceException(nameof(pluginContext.PreImage));
      }
    }

    /// <summary>
    /// Validates the post image.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">PostImage</exception>
    private static void ValidatePostImage(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.PostImage == null)
      {
        throw new NullReferenceException(nameof(pluginContext.PostImage));
      }
    }

    /// <summary>
    /// Validates the column set.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">PostImage</exception>
    private static void ValidateColumnSet(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.ColumnSet == null)
      {
        throw new NullReferenceException(nameof(pluginContext.ColumnSet));
      }
    }

    /// <summary>
    /// Validates the output business entity as base entity.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">OutputBusinessEntityAsBaseEntity</exception>
    private static void ValidateOutputBusinessEntityAsBaseEntity(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.OutputBusinessEntityAsBaseEntity == null)
      {
        throw new NullReferenceException(nameof(pluginContext.OutputBusinessEntityAsBaseEntity));
      }
    }

    /// <summary>
    /// Validates the output business entity collection.
    /// </summary>
    /// <param name="pluginContext">The plugin context.</param>
    /// <exception cref="System.NullReferenceException">OutputBusinessEntityCollection</exception>
    private static void ValidateOutputBusinessEntityCollection(PluginBase<TEntity, TContext, TTraceLog>.PluginContext pluginContext)
    {
      if (pluginContext.OutputBusinessEntityCollection == null)
      {
        throw new NullReferenceException(nameof(pluginContext.OutputBusinessEntityCollection));
      }
    }
  }
}

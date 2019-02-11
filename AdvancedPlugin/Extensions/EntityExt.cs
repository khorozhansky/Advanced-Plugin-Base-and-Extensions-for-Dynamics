namespace AdvancedPlugin.Extensions
{
  using System;
  using System.Linq.Expressions;

  using AdvancedPlugin.Utils;

  using Microsoft.Xrm.Sdk;

  /// <summary>
  /// Extended Entity
  /// <remarks>
  /// Keeps info about the "current" and "previous" "states" of the processed entity
  /// and assists in identifying changes 
  /// </remarks>
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  public class EntityExt<TEntity> where TEntity : Entity, new()
  {
    private readonly bool currentIsTarget;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityExt{TEntity}" /> class.
    /// </summary>
    /// <param name="currentEntity">The Current Entity (usually either Target entity or PostImage)</param>
    /// <param name="initialEntity">The Initial Entity (usually PreImage)</param>
    /// <param name="currentIsTarget">if set to <c>true</c> [current is target].</param>
    public EntityExt(TEntity currentEntity, TEntity initialEntity, bool currentIsTarget)
    {
      this.CurrentEntity = currentEntity;
      this.InitialEntity = initialEntity;
      this.currentIsTarget = currentIsTarget;
    }

    /// <summary>
    /// Gets the change entity.
    /// </summary>
    /// <value>
    /// The change entity.
    /// </value>
    public TEntity CurrentEntity { get; }

    /// <summary>
    /// Gets the pre image.
    /// </summary>
    /// <value>
    /// The pre image.
    /// </value>
    public TEntity InitialEntity { get; }

    /// <summary>
    /// Gets the id.
    /// </summary>
    /// <value>The id.</value>
    public Guid Id => this.InitialEntity?.Id ?? this.CurrentEntity.Id;

    /// <summary>
    /// Gets the name of the logical.
    /// </summary>
    /// <value>The name of the logical.</value>
    public string LogicalName => this.InitialEntity == null ? this.CurrentEntity.LogicalName : this.InitialEntity.LogicalName;

    /// <summary>
    /// To the entity reference.
    /// </summary>
    /// <returns>EntityReference.</returns>
    public EntityReference ToEntityReference()
    {
      return new EntityReference(this.LogicalName, this.Id);
    }

    /// <summary>
    /// Gets the "extended" value.
    /// </summary>
    /// <typeparam name="T">Entity Field Type</typeparam>
    /// <param name="propertyExpression">The property expression.</param>
    /// <returns>
    /// Extended Field Value
    /// </returns>
    public FieldExt<T, TEntity> GetValue<T>(Expression<Func<T>> propertyExpression)
    {
      var attributeName = propertyExpression.ExtractEntityAttributeName(LogicalNameExtractMethod.ByPropertyOrClassName);
      return this.GetValue<T>(attributeName);
    }

    /// <summary>
    /// Gets the "extended" value.
    /// </summary>
    /// <typeparam name="T">Entity Field Type</typeparam>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <returns>
    /// Extended Field Value
    /// </returns>
    public FieldExt<T, TEntity> GetValue<T>(string attributeName)
    {
      var ignoreIsSpecifiedProperty = !this.currentIsTarget;
      return new FieldExt<T, TEntity>(this.InitialEntity, this.CurrentEntity, attributeName, ignoreIsSpecifiedProperty);
    }

    public CollectionFieldExt<T, TEntity> GetCollectionValue<T>(Expression<Func<T>> propertyExpression) where T : Entity
    {
      var attributeName = propertyExpression.ExtractEntityAttributeName(LogicalNameExtractMethod.ByPropertyOrClassName);
      return this.GetCollectionValue<T>(attributeName);
    }

    public CollectionFieldExt<T, TEntity> GetCollectionValue<T>(string attributeName) where T: Entity
    {
      return new CollectionFieldExt<T, TEntity>(this.InitialEntity, this.CurrentEntity, attributeName);
    }

    public SimpleFieldExt<T, TEntity> GetSimpleValueExt<T>(Expression<Func<T>> propertyExpression)
    {
      var attributeName = propertyExpression.ExtractEntityAttributeName(LogicalNameExtractMethod.ByPropertyOrClassName);
      return this.GetSimpleValueExt<T>(attributeName);
    }

    public SimpleFieldExt<T, TEntity> GetSimpleValueExt<T>(string attributeName)
    {
      return new SimpleFieldExt<T, TEntity>(this.InitialEntity, this.CurrentEntity, attributeName);
    }

    // TODO: Find a workaround to make GetNullableEnumValueExt<T>(Expression<Func<T>> propertyExpression) working
    //public NullableEnumValueExt<T, TEntity> GetNullableEnumValueExt<T>(Expression<Func<T>> propertyExpression)
    //{
    //  var attributeName = propertyExpression.ExtractEntityAttributeName(LogicalNameExractMethod.ByPropertyOrClassName);
    //  return this.GetNullableEnumValueExt<T>(attributeName);
    //}

    public NullableEnumFieldExt<T, TEntity> GetNullableEnumValueExt<T>(string attributeName)
    {
      return new NullableEnumFieldExt<T, TEntity>(this.InitialEntity, this.CurrentEntity, attributeName);
    }

    public BoxedFieldExt GetBoxedValueExt(string attributeName, string attributeTypeCode)
    {
      return new BoxedFieldExt(this.InitialEntity, this.CurrentEntity, attributeName, attributeTypeCode);
    }
  }
}

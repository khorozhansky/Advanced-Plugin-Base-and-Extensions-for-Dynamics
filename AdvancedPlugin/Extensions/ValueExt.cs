namespace AdvancedPlugin.Extensions
{
  using System;
  using System.Collections;
  using System.Linq;

  using Utils;

  using Microsoft.Xrm.Sdk;

  public class ValueExt<T, TEntity> : IValueExt
    where TEntity : Entity
  {
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly TEntity initial;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly TEntity current;

    private readonly bool ignoreIsSpecifiedProperty;

    private readonly Lazy<bool> isSpecified;

    private readonly Lazy<bool> isModified;

    private readonly Lazy<T> oldValue;

    private readonly Lazy<T> newValue;

    private readonly Lazy<string> oldValueFormatted;

    private readonly Lazy<string> newValueFormatted;

    public ValueExt(TEntity initial, TEntity current, string attributeName, bool ignoreIsSpecifiedProperty = false)
    {
      this.initial = initial;
      this.current = current;
      this.ignoreIsSpecifiedProperty = ignoreIsSpecifiedProperty;
      this.oldValue = new Lazy<T>(
        () =>
          {
            if (this.initial == null)
            {
              return default(T);
            }

            var attributeExists = this.initial.Contains(attributeName);
            return attributeExists ? this.AttributeValueFactory(this.initial, attributeName) : default(T);
          });

      this.oldValueFormatted = new Lazy<string>(
        // ReSharper disable once InvokeAsExtensionMethod (! NOTE: otherwise "Operation could destabilize the runtime" error appears)
        () => EntityHelper.GetFormattedAttrValue(this.initial, attributeName));
      this.isSpecified = new Lazy<bool>(
        () => ignoreIsSpecifiedProperty ? this.IsModified : this.current.Contains(attributeName));
          
      this.newValue =
        new Lazy<T>(
          () => ignoreIsSpecifiedProperty || this.IsSpecified ? this.AttributeValueFactory(this.current, attributeName) : default(T));
      this.newValueFormatted = new Lazy<string>(
        // ReSharper disable once InvokeAsExtensionMethod (! NOTE: otherwise "Operation could destabilize the runtime" error appears)
        () => EntityHelper.GetFormattedAttrValue(this.current, attributeName));
      this.isModified = new Lazy<bool>(this.GetIsModifed);
    }

    protected virtual Func<TEntity, string, T> AttributeValueFactory 
      => (entity, attributeName) => entity.GetAttrValue<T>(attributeName);

    /// <summary>
    /// Gets the old value.
    /// </summary>
    /// <value>The old value.</value>
    public T OldValue => this.oldValue.Value;

    /// <summary>
    /// Gets the new value.
    /// </summary>
    /// <value>The new value.</value>
    public T NewValue => this.newValue.Value;

    /// <summary>
    /// Gets whether the new value is specified.
    /// </summary>
    /// <remarks>
    /// ATTENTION! THIS PROPERTY WORKS AS EXPECTED ONLY IN CASE THE "TARGET" ENTITY IS SPECIFIED AS THE <see cref="current"/>, OTHERWISE IT IS EQUAL TO <see cref="IsModified"/>
    /// </remarks>
    /// <value>
    ///   <c>true</c> if this instance is specified; otherwise, <c>false</c>.
    /// </value>
    public bool IsSpecified => this.isSpecified.Value;

    /// <summary>
    /// Gets the value (the new one if it's specified, otherwise the old one ).
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public T Value => this.IsSpecified ? this.NewValue : this.OldValue;

    /// <summary>
    /// Gets the old value formatted.
    /// </summary>
    /// <value>The old value formatted.</value>
    public string OldValueFormatted => this.oldValueFormatted.Value;

    /// <summary>
    /// Gets the new value formatted.
    /// </summary>
    /// <value>The new value formatted.</value>
    public string NewValueFormatted => this.newValueFormatted.Value;

    /// <summary>
    /// Gets the value formatted.
    /// </summary>
    /// <value>The value formatted.</value>
    public string ValueFormatted 
      => this.IsSpecified ? this.NewValueFormatted : this.OldValueFormatted;

    /// <summary>
    /// Gets a value indicating whether [the value is modified].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [the value is modified]; otherwise, <c>false</c>.
    /// </value>
    public bool IsModified => this.isModified.Value;

    /// <summary>
    /// Gets a value indicating whether [new value gets set to null].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [new value gets set to null]; otherwise, <c>false</c>.
    /// </value>
    public bool IsSetToNull => this.IsSpecified && this.NewValue == null;

    /// <summary>
    /// Gets a value indicating whether the value will be null after the action.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [it will be null]; otherwise, <c>false</c>.
    /// </value>
    public bool IsNull => this.Value == null;

    /// <summary>
    /// Gets value indication if the value is modified.
    /// </summary>
    /// <returns></returns>
    protected virtual bool GetIsModifed()
    {
      if (!this.ignoreIsSpecifiedProperty && !this.IsSpecified)
      {
        return false;
      }

      if (this.NewValue == null)
      {
        return this.OldValue != null;
      }

      if (this.OldValue == null)
      {
        return true;
      }

      var type = typeof(T);
      if (!TypeHelper.IsGenericEnumerable(type))
      {
        return !this.NewValue.Equals(this.OldValue);
      }

      var oldIdList = ((IEnumerable)this.OldValue)
        .Cast<Entity>()
        .Select(r => r.Id)
        .ToList();

      var newIdList = ((IEnumerable)this.NewValue)
        .Cast<Entity>()
        .Select(r => r.Id)
        .ToList();

      var difference =
        oldIdList.Except(newIdList)
          .Union(newIdList.Except(oldIdList));

      return difference.Any();
    }
  }
}
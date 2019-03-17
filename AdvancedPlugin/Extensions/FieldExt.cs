namespace AdvancedPlugin.Extensions
{
  using System;
  using System.Collections;
  using System.Linq;

  using Utils;

  using Microsoft.Xrm.Sdk;

  using AdvancedPlugin.Exceptions;

  public class FieldExt<T, TEntity> : IFieldExt
    where TEntity : Entity
  {
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    protected readonly TEntity Initial;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    protected readonly TEntity Current;

    private Lazy<bool> isSpecified;

    private Lazy<bool> isModified;

    private readonly Lazy<T> oldValue;

    private Lazy<T> newValue;

    private readonly Lazy<string> oldValueFormatted;

    private Lazy<string> newValueFormatted;

    protected readonly bool IgnoreIsSpecifiedProperty;

    public readonly string LogicalName;

    public FieldExt(TEntity initial, TEntity current, string attributeName, bool ignoreIsSpecifiedProperty = false)
    {
      this.Initial = initial;
      this.Current = current;
      this.LogicalName = attributeName;
      this.IgnoreIsSpecifiedProperty = ignoreIsSpecifiedProperty;
      this.oldValue = new Lazy<T>(
        () =>
          {
            if (this.Initial == null)
            {
              return default(T);
            }

            var attributeExists = this.Initial.Contains(attributeName);
            return attributeExists ? this.AttributeGetValueFactory(this.Initial) : default(T);
          });

      this.oldValueFormatted = new Lazy<string>(
        // ReSharper disable once InvokeAsExtensionMethod (! NOTE: otherwise "Operation could destabilize the runtime" error appears)
        () => EntityHelper.GetFormattedAttrValue(this.Initial, attributeName));

      this.ResetNewValueRelatedLazyFields();
    }

    protected virtual Func<TEntity, T> AttributeGetValueFactory 
      => (entity) => entity.GetAttrValue<T>(this.LogicalName);

    protected virtual Action<T> AttributeSetNewValueFactory
      => (value) => this.Current.SetAttrValue(this.LogicalName, value);

    /// <summary>
    /// Gets the old value.
    /// </summary>
    /// <value>The old value.</value>
    public T OldValue => this.oldValue.Value;

    /// <summary>
    /// Gets or sets the new value.
    /// </summary>
    /// <value>
    /// The new value.
    /// </value>
    /// <exception cref="PluginBusinessLogicExceptionWithFullLogging">It's not possible to set the new value as the current entity is not provided.</exception>
    public T NewValue
    {
      get => this.newValue.Value;
      set
      {
        if (this.Current == null)
        {
          throw new PluginBusinessLogicExceptionWithFullLogging("It's not possible to set the new value as the current entity is not provided.");
        }

        var isNewValueDifferent = this.GetIsModified(value, this.Value);
        if (!isNewValueDifferent)
        {
          return;
        }

        this.AttributeSetNewValueFactory(value);
        this.newValue = new Lazy<T>(() => value);
        this.isSpecified = new Lazy<bool>(() => true);
        this.isModified = new Lazy<bool>(() => true);
        this.newValueFormatted = new Lazy<string>(() => null);
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// Gets whether the new value is specified.
    /// </summary>
    /// <remarks>
    /// ATTENTION! THIS PROPERTY WORKS AS EXPECTED ONLY IN CASE THE "TARGET" ENTITY IS SPECIFIED AS THE <see cref="F:AdvancedPlugin.Extensions.FieldExt`2.current" />, OTHERWISE IT IS EQUAL TO <see cref="P:AdvancedPlugin.Extensions.FieldExt`2.IsModified" />
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
    protected virtual bool GetIsModified()
    {
      if (!this.IgnoreIsSpecifiedProperty && !this.IsSpecified)
      {
        return false;
      }

      return this.GetIsModified(this.NewValue, this.OldValue);
    }

    /// <summary>
    /// Gets value indication if the old value is actually modified.
    /// </summary>
    /// <param name="newVal">The new value.</param>
    /// <param name="oldVal">The old value.</param>
    /// <returns></returns>
    protected virtual bool GetIsModified(T newVal, T oldVal)
    {
      if (newVal == null)
      {
        return oldVal != null;
      }

      if (oldVal == null)
      {
        return true;
      }

      var type = typeof(T);
      if (type == typeof(OptionSetValueCollection))
      {
        var newIds = ((IEnumerable)newVal).Cast<OptionSetValue>().Select(r => r.Value).ToList();
        var oldIds = ((IEnumerable)oldVal).Cast<OptionSetValue>().Select(r => r.Value).ToList();
        return newIds.Except(oldIds).Union(oldIds.Except(newIds)).Any();
      }

      if (!TypeHelper.IsGenericEnumerable(type))
      {
        return !newVal.Equals(oldVal);
      }

      var oldIdList = ((IEnumerable)oldVal)
        .Cast<Entity>()
        .Select(r => r.Id)
        .ToList();

      var newIdList = ((IEnumerable)newVal)
        .Cast<Entity>()
        .Select(r => r.Id)
        .ToList();

      var difference =
        oldIdList.Except(newIdList)
          .Union(newIdList.Except(oldIdList));

      return difference.Any();
    }

    /// <summary>
    /// Resets / Initializes the "NewValue related" lazy fields: 
    /// <see cref="isSpecified"/>
    /// <see cref="newValue"/>
    /// <see cref="newValueFormatted"/>
    /// <see cref="isModified"/>
    /// </summary>
    private void ResetNewValueRelatedLazyFields()
    {
      this.isSpecified = new Lazy<bool>(
        () => this.IgnoreIsSpecifiedProperty ? this.IsModified : this.Current.Contains(this.LogicalName));

      this.newValue =
        new Lazy<T>(
          () => this.IgnoreIsSpecifiedProperty || this.IsSpecified ? this.AttributeGetValueFactory(this.Current) : default(T));

      this.newValueFormatted = new Lazy<string>(
        // ReSharper disable once InvokeAsExtensionMethod (! NOTE: otherwise "Operation could destabilize the runtime" error appears)
        () => EntityHelper.GetFormattedAttrValue(this.Current, this.LogicalName));

      this.isModified = new Lazy<bool>(this.GetIsModified);
    }

  }
}
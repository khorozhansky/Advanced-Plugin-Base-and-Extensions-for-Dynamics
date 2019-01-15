namespace AdvancedPlugin.Extensions
{
  using System;

  using Utils;

  using Microsoft.Xrm.Sdk;

  public class SimpleFieldExt<T, TEntity> : FieldExt<T, TEntity>
    where TEntity : Entity
  {
    public SimpleFieldExt(TEntity initial, TEntity current, string attributeName, bool ignoreIsSpecifiedProperty = false)
      : base(initial, current, attributeName, ignoreIsSpecifiedProperty)
    {
    }

    protected override Func<TEntity, T> AttributeGetValueFactory => 
      (entity) => entity.GetSimpleAttrValue<T>(this.LogicalName);

    protected override bool GetIsModified(T newVal, T oldVal)
    {
      if (newVal == null)
      {
        return oldVal != null;
      }

      if (oldVal == null)
      {
        return true;
      }

      return !newVal.Equals(oldVal);
    }
  }
}

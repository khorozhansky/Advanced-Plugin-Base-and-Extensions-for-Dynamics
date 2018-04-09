namespace AdvancedPlugin.Extensions
{
  using System;

  using Utils;

  using Microsoft.Xrm.Sdk;

  public class SimpleValueExt<T, TEntity> : ValueExt<T, TEntity>
    where TEntity : Entity
  {
    public SimpleValueExt(TEntity initial, TEntity current, string attributeName)
      : base(initial, current, attributeName)
    {
    }

    protected override Func<TEntity, string, T> AttributeValueFactory => 
      (entity, attributeName) => entity.GetSimpleAttrValue<T>(attributeName);

    protected override bool GetIsModifed()
    {
      if (!this.IsSpecified)
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

      return !this.NewValue.Equals(this.OldValue);
    }
  }
}

namespace AdvancedPlugin.Extensions
{
  using System;

  using Utils;

  using Microsoft.Xrm.Sdk;

  public class NullableEnumFieldExt<T, TEntity> : SimpleFieldExt<T, TEntity>
    where TEntity : Entity
  {
    public NullableEnumFieldExt(TEntity initial, TEntity current, string attributeName, bool ignoreIsSpecifiedProperty = false)
      : base(initial, current, attributeName, ignoreIsSpecifiedProperty)
    {
    }

    protected override Func<TEntity, T> AttributeGetValueFactory
      => (entity) => entity.GetNullableEnumAttrValue<T>(this.LogicalName);

    protected override Action<T> AttributeSetNewValueFactory
      => (value) =>
        {
          if (value == null)
          {
            this.Current.SetAttrValue<OptionSetValue>(this.LogicalName, null);
          }
          else
          {
            this.Current.SetAttrValue(this.LogicalName, new OptionSetValue(Convert.ToInt32(value)));
          }
        };
  }
}

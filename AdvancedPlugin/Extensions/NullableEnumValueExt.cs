namespace AdvancedPlugin.Extensions
{
  using System;

  using Utils;

  using Microsoft.Xrm.Sdk;

  public class NullableEnumValueExt<T, TEntity> : SimpleValueExt<T, TEntity>
    where TEntity : Entity
  {
    public NullableEnumValueExt(TEntity initial, TEntity current, string attributeName)
      : base(initial, current, attributeName)
    {
    }

    protected override Func<TEntity, string, T> AttributeValueFactory
      => (entity, attributeName) => entity.GetNullableEnumAttrValue<T>(attributeName);
  }
}

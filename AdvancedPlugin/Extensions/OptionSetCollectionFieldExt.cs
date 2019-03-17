namespace AdvancedPlugin.Extensions
{
  using System;
  using System.Linq;

  using Utils;

  using Microsoft.Xrm.Sdk;

  public class OptionSetCollectionFieldExt<TEntity> : SimpleFieldExt<OptionSetValueCollection, TEntity>
    where TEntity : Entity
  {
    public OptionSetCollectionFieldExt(TEntity initial, TEntity current, string attributeName, bool ignoreIsSpecifiedProperty = false)
      : base(initial, current, attributeName, ignoreIsSpecifiedProperty)
    {
    }

    protected override Func<TEntity, OptionSetValueCollection> AttributeGetValueFactory
      => (entity) => entity.GetSimpleAttrValue<OptionSetValueCollection>(this.LogicalName);

    protected override bool GetIsModified(OptionSetValueCollection newVal, OptionSetValueCollection oldVal)
    {
      if (newVal == null)
      {
        return oldVal != null;
      }

      if (oldVal == null)
      {
        return true;
      }

      var newIds = newVal.Select(r => r.Value).ToList();
      var oldIds = oldVal.Select(r => r.Value).ToList();
      var difference = newIds.Except(oldIds).Union(oldIds.Except(newIds));
      return difference.Any();
    }
  }
}

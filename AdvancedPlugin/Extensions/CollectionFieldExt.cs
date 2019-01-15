namespace AdvancedPlugin.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using AdvancedPlugin.Utils;

  using Microsoft.Xrm.Sdk;

  public class CollectionFieldExt<T, TEntity> : FieldExt<IEnumerable<T>, TEntity>
    where TEntity : Entity
    where T: Entity
  {
    public CollectionFieldExt(TEntity initial, TEntity current, string attributeName, bool ignoreIsSpecifiedProperty = false)
      : base(initial, current, attributeName, ignoreIsSpecifiedProperty)
    {
    }

    protected override Func<TEntity, IEnumerable<T>> AttributeGetValueFactory => 
      (entity) => entity.GetCollectionAttrValue<T>(this.LogicalName);

    protected override bool GetIsModified(IEnumerable<T> newVal, IEnumerable<T> oldVal)
    {
      if (newVal == null)
      {
        return oldVal != null;
      }

      if (oldVal == null)
      {
        return true;
      }

      var oldIdList = oldVal.Select(r => r.Id).ToList();
      var newIdList = newVal.Select(r => r.Id).ToList();
      var difference =
        oldIdList.Except(newIdList)
          .Union(newIdList.Except(oldIdList));

      return difference.Any();
    }
  }
}

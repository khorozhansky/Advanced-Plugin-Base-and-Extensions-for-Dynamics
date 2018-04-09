namespace AdvancedPlugin.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using AdvancedPlugin.Utils;

  using Microsoft.Xrm.Sdk;

  public class CollectionValueExt<T, TEntity> : ValueExt<IEnumerable<T>, TEntity>
    where TEntity : Entity
    where T: Entity
  {
    public CollectionValueExt(TEntity initial, TEntity current, string attributeName)
      : base(initial, current, attributeName)
    {
    }

    protected override Func<TEntity, string, IEnumerable<T>> AttributeValueFactory => 
      (entity, attributeName) => entity.GetCollectionAttrValue<T>(attributeName);

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

      var oldIdList = this.OldValue.Select(r => r.Id).ToList();
      var newIdList = this.NewValue.Select(r => r.Id).ToList();
      var difference =
        oldIdList.Except(newIdList)
          .Union(newIdList.Except(oldIdList));

      return difference.Any();
    }
  }
}

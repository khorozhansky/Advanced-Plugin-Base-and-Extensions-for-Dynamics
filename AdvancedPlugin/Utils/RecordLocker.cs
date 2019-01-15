namespace AdvancedPlugin.Utils
{
  using System;
  using System.Linq;

  using AdvancedPlugin.Plugins;

  using Microsoft.Xrm.Sdk;

  public static class RecordLocker
  {
    /// <summary>
    /// Locks the given record
    /// </summary>
    /// <param name="pluginCtx">Plugin context</param>
    /// <param name="entityReference">The reference to the locked record.</param>
    /// <param name="dummyUpdatedFieldName">The logical name of the dummy updated DateTime field</param>
    public static void LockRecord(this IPluginContextBase pluginCtx, EntityReference entityReference, string dummyUpdatedFieldName)
    {
      if (entityReference == null)
      {
        throw new ArgumentNullException(nameof(entityReference));
      }

      var logicalName = entityReference.LogicalName;
      Entity entity;
      var id = entityReference.Id;
      if (id != Guid.Empty)
      {
        pluginCtx.Trace($"  LockRecord. {logicalName}. {id}");
        entity = new Entity(logicalName, id);
      }
      else
      {
        var keyAttrs = entityReference.KeyAttributes;
        var incorrectArgument = keyAttrs == null || keyAttrs.Count == 0;
        if (incorrectArgument)
        {
          throw new ArgumentOutOfRangeException(nameof(entityReference));
        }

        var keyAttrValues = keyAttrs
          .Select(r => $"{r.Key}:{r.Value}")
          .Aggregate((k1, k2) => $"{k1}, {k2}");

        pluginCtx.Trace($"  LockRecord. {logicalName}. {keyAttrValues}");
        entity = new Entity(logicalName, keyAttrs);
      }

      entity.SetAttrValue(dummyUpdatedFieldName, DateTime.UtcNow);
      var orgService = pluginCtx.OrgServiceOnBehalfOfSystemUser;
      orgService.Update(entity);
    }
  }
}

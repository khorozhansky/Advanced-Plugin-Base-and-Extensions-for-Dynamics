namespace AdvancedPluginDemo.Logic.Entities.Email
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using AdvancedPlugin.Extensions;

  using AdvancedPluginDemo.Logic.Base;
  using AdvancedPluginDemo.Proxy;

  using Microsoft.Xrm.Sdk;

  public class EmailManager: ExtendedOutOfBoxTraceLogManagerBase<Email>
  {
    public EmailManager(Plugins.Bound.Email.Base.PluginContext pluginCtx)
      : base(pluginCtx)
    {
    }

    #region Update Message

    public void UpdatePreOperationSync()
    {
      this.ValidateByBaseValidators();
      this.WrapInTraceWithElapsedTime(
        this.ProcessRecipientsChange, nameof(this.ProcessRecipientsChange));
    }

    #endregion

    private void ProcessRecipientsChange()
    {
      var pluginCtx = this.PluginCtx;
      var entityExt = pluginCtx.TargetEntityExt;

      // --------------- NOTE: entityExt.GetCollectionValue<ActivityParty> is faster then entityExt.GetValue<ActivityParty> for getting collections
      var toExt = entityExt.GetCollectionValue<ActivityParty>(Email.Fields.To);
      var ccExt = entityExt.GetValue<IEnumerable<ActivityParty>>(Email.Fields.Cc);
      // ---------------
      var process = toExt.IsModified || ccExt.IsModified;
      if (!process)
      {
        return;
      }

      var targetEntity = pluginCtx.TargetEntity;
      Func<IEnumerable<ActivityParty>, int> getCount = items => items?.Count() ?? 0;
      if (toExt.IsModified)
      {
        var toOldCount = getCount(toExt.OldValue);
        var toNewCount = getCount(toExt.NewValue);

        // example of using GetSimpleValueExt<>
        var descriptionExt = entityExt.GetSimpleValueExt<string>(Email.Fields.Description);

        // example of using GetNullableEnumValueExt<>
        var priorityEnumExt = entityExt.GetNullableEnumValueExt<Email_PriorityCode?>(Email.Fields.PriorityCode);
        var priorityEnum = priorityEnumExt.Value;
        var highPriority = priorityEnum != null && priorityEnum == Email_PriorityCode.High;
        var currentPriorityName = highPriority ? "HIGH !"
                                    : (priorityEnum == null ? "NOT SPECIFIED" :
                                         Enum.GetName(typeof(Email_PriorityCode), priorityEnum));

        var dueToExt = entityExt.GetSimpleValueExt<DateTime?>(Email.Fields.ScheduledEnd);
        var dueToText = dueToExt.Value?.ToUniversalTime();

        var durationExt = entityExt.GetSimpleValueExt<int?>(Email.Fields.ActualDurationMinutes);
        var durationText = durationExt.Value?.ToString();
        targetEntity.Description = $"{descriptionExt.Value}\r\n" 
                                   + $"'TO' has been modified (Qts): {toOldCount} - {toNewCount}\r\n"
                                   + $"Current Priority: {currentPriorityName}\r\n" 
                                   + $"Due To: {dueToText}\r\n" 
                                   + $"Duration: {durationText} min";
      }

      if (ccExt.IsModified)
      {
        var ccOldCount = getCount(ccExt.OldValue);
        var ccNewCount = getCount(ccExt.NewValue);

        // example of using GetValue<> for a simple field (try to use GetSimpleValueExt<> - it's a bit faster)
        var descriptionExt = entityExt.GetValue<string>(Email.Fields.Description);

        // example of using GetSimpleValueExt<> to get OptionSetValue
        SimpleFieldExt<OptionSetValue, Email> priorityExt = entityExt.GetSimpleValueExt(()=>targetEntity.PriorityCode);
        var priorityCode = priorityExt.Value?.Value;
        var highPriority = priorityCode != null && priorityCode == (int)Email_PriorityCode.High;
        var currentPriorityName = highPriority ? "HIGH !"
                                    : (priorityCode == null ? "NOT SPECIFIED" :
                                         Enum.GetName(typeof(Email_PriorityCode), priorityCode));

        var dueToExt = entityExt.GetValue<DateTime?>(Email.Fields.ScheduledEnd);
        var dueToText = dueToExt.Value?.ToUniversalTime();

        var durationExt = entityExt.GetValue<int?>(Email.Fields.ActualDurationMinutes);
        var durationText = durationExt.Value?.ToString();
        targetEntity.Description = $"{descriptionExt.Value}\r\n"
                                   + $"'Cc' has been modified (Qts): {ccOldCount} - {ccNewCount}\r\n"
                                   + $"Current Priority: {currentPriorityName}\r\n"
                                   + $"Due To: {dueToText}\r\n"
                                   + $"Duration: {durationText} min";

        targetEntity.Description = $"{descriptionExt.Value}\r\nCc has been modified (Qts): {ccOldCount} - {ccNewCount}";
      }
    }
  }
}

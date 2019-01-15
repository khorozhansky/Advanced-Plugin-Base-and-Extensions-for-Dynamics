namespace AdvancedPluginDemo.Plugins.Base.Ext
{
  using AdvancedPlugin.Constants;
  using AdvancedPlugin.Exceptions;
  using AdvancedPlugin.Plugins;
  using AdvancedPlugin.Utils;

  public static class PluginCtxExt
  {
    private const string ServiceOperationDateFieldName = "pavelkh_serviceoperationdate";
    private const string ServiceDeleteActionName = "pavelkh_ActionServiceRecordDelete";

    public static bool GetIsExecutionAllowed(IPluginContextBase pluginCtx)
    {
      var executeHandler = true;
      // ReSharper disable once SwitchStatementMissingSomeCases
      switch (pluginCtx.MessageName)
      {
        case MessageName.Create:
        case MessageName.Update:
          var target = pluginCtx.TargetEntityAsBaseEntity;
          var serviceOperationDateIsChanged =
            target.GetIsFieldSpecified(ServiceOperationDateFieldName);

          if (serviceOperationDateIsChanged)
          {
            if (pluginCtx.GetIsAdminUser() == false)
            {
              throw new PluginBusinessLogicExceptionWithSimpleLogging("Only System Administrator may initiate Service Operation.");
            }

            executeHandler = target.Attributes[ServiceOperationDateFieldName] == null;
          }

          break;

        case MessageName.Delete:
          var isServiceDeleteAction = false;
          var parentExecCtx = pluginCtx.ParentExecContext;
          if (parentExecCtx != null)
          {
            var grandParentExecCtx = parentExecCtx.ParentContext;
            if (grandParentExecCtx != null)
            {
              isServiceDeleteAction = ServiceDeleteActionName.Equals(grandParentExecCtx.MessageName);
            }

            if (!isServiceDeleteAction)
            {
              isServiceDeleteAction = ServiceDeleteActionName.Equals(parentExecCtx.MessageName);
            }
          }

          executeHandler = !isServiceDeleteAction;
          break;
      }

      return executeHandler;
    }
  }
}

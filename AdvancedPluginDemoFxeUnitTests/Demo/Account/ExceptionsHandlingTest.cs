// ReSharper disable InconsistentNaming
namespace AdvancedPluginDemoUnitTests.Demo.Account
{
  using System;

  using AdvancedPluginDemo.Plugins.Bound.Account;
  using AdvancedPluginDemo.Proxy;

  using AdvancedPluginDemoUnitTests.Ext;
  using AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx;

  using FakeXrmEasy.Extensions;

  using Microsoft.Xrm.Sdk;

  using Xunit;

  public class ExceptionsHandlingTest
  {
    public static TheoryData<string, int, int, string, string, Type> Data
    {
      get
      {
        var name = "Generate PluginBusinessLogicExceptionWithSimpleLogging";
        var exceptionMessageTemplate =
          "Hi, this is a demo PluginBusinessLogicExceptionWithSimpleLogging exception message. (Stage:{0}, Mode:{1})";
        var result = new TheoryData<string, int, int, string, string, Type>();
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate PluginBusinessLogicExceptionWithFullLogging";
        exceptionMessageTemplate = "Hi, this is a demo PluginBusinessLogicExceptionWithFullLogging exception message. (Stage:{0}, Mode:{1})";
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate InvalidPluginExecutionException";
        exceptionMessageTemplate = "Hi, this is a demo InvalidPluginExecutionException exception message. (Stage:{0}, Mode:{1})";
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate PluginBusinessLogicException";
        exceptionMessageTemplate = "Hi, this is a demo PluginBusinessLogicException exception message. (Stage:{0}, Mode:{1})";
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate PluginExceptionWithDetailsInUiAndWithLogging";
        exceptionMessageTemplate = "Hi, this is a demo PluginExceptionWithDetailsInUiAndWithLogging exception message. (Stage:{0}, Mode:{1})";
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate PluginIgnoredExceptionWithLogging";
        exceptionMessageTemplate = null;
        // ReSharper disable once ExpressionIsAlwaysNull
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate ApplicationException";
        exceptionMessageTemplate = "Hi, this is a demo ApplicationException exception message. (Stage:{0}, Mode:{1})";
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate Exception";
        exceptionMessageTemplate = "Hi, this is a demo Exception exception message. (Stage:{0}, Mode:{1})";
        AddTheoryData(result, name, exceptionMessageTemplate);

        name = "Generate FaultException<OrganizationServiceFault>";
        exceptionMessageTemplate = "Couldnt find reflected type for _DummyEntityName";
        AddTheoryData(result, name, exceptionMessageTemplate);

        return result;
      }
    }

    private static void AddTheoryData(
      TheoryData<string, int, int, string, string, Type> data,
      string name,
      string exceptionMessageTemplate) 
    {
      var message = "Create";
      var pluginType = typeof(Create);
      data.Add(message, 10, 0, name, exceptionMessageTemplate, pluginType);
      data.Add(message, 20, 0, name, exceptionMessageTemplate, pluginType);
      data.Add(message, 40, 0, name, exceptionMessageTemplate, pluginType);
      data.Add(message, 40, 1, name, exceptionMessageTemplate, pluginType);
      message = "Update";
      pluginType = typeof(Update);
      data.Add(message, 10, 0, name, exceptionMessageTemplate, pluginType);
      data.Add(message, 20, 0, name, exceptionMessageTemplate, pluginType);
      data.Add(message, 40, 0, name, exceptionMessageTemplate, pluginType);
      data.Add(message, 40, 1, name, exceptionMessageTemplate, pluginType);
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void When_CustomExceptionIsEmulated_Expect_PluginNotToFail(
      string messageName, int stage, int mode, string dummyAccountName, string exceptionMessageTemplate, Type pluginType)
    {
      var ctx = new XrmFakedContextWithProxy();
      var target = new Account
                     {
                       AccountId = Guid.NewGuid(),
                       Name = $"{dummyAccountName}. Stage:{stage} Mode:{mode}",
                       StateCode = AccountState.Active
                     };
      var pluginCtx = new WithTargetEntityBase(ctx)
                        {
                          MessageName = messageName,
                          Stage = stage,
                          Mode = mode,
                          TargetEntity = target
      };

      if (messageName == "Update")
      {
        pluginCtx.PreImage = target.Clone();
      }
      
      var pluginInstance =
        (IPlugin)Activator.CreateInstance(
          pluginType,
          FakedPluginExecCtxBase.CustomVerboseLogSwitchedOnKeyword,
          string.Empty);

      if (exceptionMessageTemplate == null)
      {
        var exc = Record.Exception(
          () => ctx.ExecutePluginWith(pluginCtx, pluginInstance));
        Assert.Null(exc);
      }
      else
      {
        var exc = Assert.Throws<InvalidPluginExecutionException>(
          () => ctx.ExecutePluginWith(pluginCtx, pluginInstance));
        var exceptionMessage = string.Format(exceptionMessageTemplate, stage, mode);
        Assert.Contains(exceptionMessage, exc.Message);
      }
    }
  }
}

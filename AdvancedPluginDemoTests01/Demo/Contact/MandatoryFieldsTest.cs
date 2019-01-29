// ReSharper disable InconsistentNaming
namespace AdvancedPluginDemoUnitTests.Demo.Contact
{
  using System;
  using AdvancedPluginDemo.Proxy;
  using AdvancedPluginDemoUnitTests.Demo.Common;
  using AdvancedPluginDemoUnitTests.Ext;
  using AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx;

  using FakeXrmEasy.Extensions;

  using Microsoft.Xrm.Sdk;

  using Xunit;

  public class MandatoryFieldsTest
  {
    [Theory]
    [InlineData("mobilephone")]
    [InlineData("emailaddress1")]
    [Trait("Category", "Unit")]
    public void When_ContactIsBeingCreatedAndMandatoryFieldIsNotSpecified_Expect_PluginToFail(string fieldName)
    {
      var ctx = new XrmFakedContextWithProxy();
      var target = TestContactBase;
      target[fieldName] = null;

      var pluginCtx = new CreatePreValidationSyncBase(ctx)
                        {
                          TargetEntity = target
                        };

      var exc = Assert.Throws<InvalidPluginExecutionException>(
          () =>
            ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Contact.Create>(
              pluginCtx, string.Empty, string.Empty));

      var expectedMessage = string.Format(CommonConstants.MandatoryFieldExceptionMessageTemplate, fieldName);
      Assert.Contains(expectedMessage, exc.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void When_ContactIsBeingCreatedAndMandatoryFieldsAreSpecified_Expect_PluginNotToFail()
    {
      var ctx = new XrmFakedContextWithProxy();
      var target = TestContactBase;
      var pluginCtx = new CreatePreValidationSyncBase(ctx)
                        {
                          TargetEntity = target
                        };

      var exc = Record.Exception(
        () =>
          ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Contact.Create>(
            pluginCtx, string.Empty, string.Empty));

      Assert.Null(exc);
    }

    [Theory]
    [InlineData("mobilephone")]
    [InlineData("emailaddress1")]
    [Trait("Category", "Unit")]
    public void When_ContactIsBeingUpdatedAndMandatoryFieldIsNotSpecified_Expect_PluginToFail(string fieldName)
    {
      var ctx = new XrmFakedContextWithProxy();
      var preImage = TestContactBase;
      var target = TestContactBase.Clone<Contact>();
      preImage[fieldName] = null;
      target.Attributes.Remove(fieldName);

      var pluginCtx = new UpdatePreValidationSyncBase(ctx)
                        {
                          TargetEntity = target,
                          PreImage = preImage
                        };

      var exc = Assert.Throws<InvalidPluginExecutionException>(
        () =>
          ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Contact.Update>(
            pluginCtx, string.Empty, string.Empty));

      var expectedMessage = string.Format(CommonConstants.MandatoryFieldExceptionMessageTemplate, fieldName);
      Assert.Contains(expectedMessage, exc.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void When_ContactIsBeingUpdatedAndMandatoryFieldsAreSpecified_Expect_PluginNotToFail()
    {
      var ctx = new XrmFakedContextWithProxy();
      var preImage = TestContactBase;
      preImage.Attributes.Remove("mobilephone");
      var target = TestContactBase.Clone<Contact>();
      target.Attributes.Remove("emailaddress");
      var pluginCtx = new UpdatePreValidationSyncBase(ctx)
                        {
                          TargetEntity = target,
                          PreImage = preImage
                        };

      var exc = Record.Exception(
        () =>
          ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Contact.Update>(
            pluginCtx, string.Empty, string.Empty));

      Assert.Null(exc);
    }

    private static Contact TestContactBase =>
      new Contact
        {
          ContactId = Guid.NewGuid(),
          FirstName = "John",
          LastName = "West",
          MobilePhone = "12312312",
          EMailAddress1 = "john@dummy_email_domain.com",
        };

  }
}

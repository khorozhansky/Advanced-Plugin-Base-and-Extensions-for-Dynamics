namespace AdvancedPluginDemoUnitTests.Demo.Contact
{
  using System;

  using AdvancedPluginDemo.Proxy;

  using AdvancedPluginDemoUnitTests.Ext;
  using AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx;

  using FakeXrmEasy.Extensions;

  using Xunit;

  public class ParentAccountAddressSyncTest
  {
    [Fact]
    [Trait("Category", "Unit")]
    // ReSharper disable once InconsistentNaming
    public void When_ContactIsCreatedAndParentAccountIsSet_Expect_CountryAndStateAreUpdated()
    {
      var ctx = new XrmFakedContextWithProxy();
      var service = ctx.GetOrganizationService();
      var parentAccount = TestAccountBase;

      service.Create(parentAccount);

      var target = TestContactBase;
      target.ParentCustomerId = parentAccount.ToEntityReference();

      var pluginCtx = new CreatePreOperationSyncBase(ctx)
                        {
                          TargetEntity = target
                        };

      ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Contact.Create>(
        pluginCtx, string.Empty, string.Empty);

      Assert.Equal(parentAccount.Address1_Country, target?.Address1_Country);
      Assert.Equal(parentAccount.Address1_StateOrProvince, target?.Address1_StateOrProvince);
    }

    [Fact]
    [Trait("Category", "Unit")]
    // ReSharper disable once InconsistentNaming
    public void When_ContactIsUpdatedAndParentAccountIsSet_Expect_CountryAndStateAreUpdated()
    {
      var ctx = new XrmFakedContextWithProxy();
      var service = ctx.GetOrganizationService();
      var parentAccount = TestAccountBase;
      service.Create(parentAccount);
      var contact = TestContactBase;
      service.Create(contact);

      var target = contact.Clone<Contact>();
      var preImage = contact.Clone();
      target.ParentCustomerId = parentAccount.ToEntityReference();
      var pluginCtx = new UpdatePreOperationSyncBase(ctx)
                        {
                          TargetEntity = target,
                          PreImage = preImage
                        };

      ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Contact.Update>(
        pluginCtx, string.Empty, string.Empty);

      Assert.Equal(parentAccount.Address1_Country, target.Address1_Country);
      Assert.Equal(parentAccount.Address1_StateOrProvince, target.Address1_StateOrProvince);
    }

    [Fact]
    [Trait("Category", "Unit")]
    // ReSharper disable once InconsistentNaming
    public void When_ContactIsUpdatedAndParentAccountIsCleanedUp_Expect_CountryAndStateAreNotChanged()
    {
      var ctx = new XrmFakedContextWithProxy();
      var service = ctx.GetOrganizationService();
      var contact = TestContactBase;
      var parentAccount = TestAccountBase;
      service.Create(parentAccount);
      contact.Address1_Country = parentAccount.Address1_Country;
      contact.Address1_StateOrProvince = parentAccount.Address1_StateOrProvince;
      contact.ParentCustomerId = parentAccount.ToEntityReference();
      service.Create(contact);

      var target = contact.Clone<Contact>();
      var preImage = target.Clone();
      target.ParentCustomerId = null;
      var pluginCtx = new UpdatePreOperationSyncBase(ctx)
                        {
                          TargetEntity = target,
                          PreImage = preImage
                        };

      ctx.ExecutePluginWithConfigurations<AdvancedPluginDemo.Plugins.Bound.Contact.Update>(
        pluginCtx, string.Empty, string.Empty);

      Assert.Equal(parentAccount.Address1_Country, target.Address1_Country);
      Assert.Equal(parentAccount.Address1_StateOrProvince, target.Address1_StateOrProvince);
    }

    private static Account TestAccountBase =>
      new Account
        {
          AccountId = Guid.NewGuid(),
          Name = "Some Company",
          Address1_Country = "USA",
          Address1_StateOrProvince = "IL"
        };

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

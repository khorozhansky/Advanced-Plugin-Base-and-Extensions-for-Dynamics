// ReSharper disable InconsistentNaming
namespace AdvancedPluginDemoUnitTests.Demo.Contact
{
  using System;
  using System.Linq;

  using AdvancedPluginDemo.Plugins.Bound.Contact;
  using AdvancedPluginDemo.Proxy;

  using AdvancedPluginDemoUnitTests.Ext;
  using AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx;

  using Microsoft.Xrm.Sdk;

  using Xunit;

  public class CreateTaskOnParentAccountChangeTest
  {
    [Fact]
    [Trait("Category", "Unit")]
    public void When_ContactIsCreatedAndParentAccountIsSet_Expected_TaskIsCreated()
    {
      var ctx = new XrmFakedContextWithProxy();
      var service = ctx.GetOrganizationService();
      var parentAccount = TestAccountBase;
      service.Create(parentAccount);
      var target = GetTestContactWithParentAccount(parentAccount.ToEntityReference());
      var pluginCtx = new CreatePostOperationSyncBase(ctx)
                        {
                          TargetEntity = target
                        };

      ctx.ExecutePluginWithConfigurations<Create>(pluginCtx, string.Empty, string.Empty);
      var task = ctx.CreateQuery<Task>().FirstOrDefault();
      Assert.NotNull(task);
      Assert.Equal(Task_PriorityCode.Low, task.PriorityCodeEnum);
      Assert.Equal("Please review top 3 accounts.", task.Subject);
    }

    //private void ArrangeContactAndAccountData(IOrganizationService service)
    //{
    //  var account1 = TestAccountBase;
    //  account1.Name = "Account #1";
    //  service.Create(account1);
    //  var account2 = TestAccountBase;
    //  account2.Name = "Account #2";
    //  account2.ParentAccountId = account1.ToEntityReference();

    //  service.Create(account2);
    //  var account3 = TestAccountBase;
    //  account3.Name = "Account #3";
    //  account3.ParentAccountId = account1.ToEntityReference();

    //  service.Create(account3);
    //  var account4 = TestAccountBase;
    //  account4.Name = "Account #4";
    //  account4.ParentAccountId = account2.ToEntityReference();

    //  service.Create(account4);
    //  var account5 = TestAccountBase;
    //  account5.Name = "Account #5";
    //  account5.ParentAccountId = account2.ToEntityReference();

    //  service.Create(account5);

    //  var account1Ref = new EntityReference(Account.EntityLogicalName,account1.Id) {Name = account1.Name} ;
    //  var contact11 = GetTestContactWithParentAccount(account1Ref);
    //  service.Create(contact11);
    //  var contact12 = GetTestContactWithParentAccount(account1Ref );
    //  service.Create(contact12);
    //  var contact13 = GetTestContactWithParentAccount(account1Ref );
    //  service.Create(contact13);
    //  var contact14 = GetTestContactWithParentAccount(account1Ref );
    //  service.Create(contact14);

    //  var account2Ref = new EntityReference(Account.EntityLogicalName, account2.Id) { Name = account2.Name };
    //  var contact21 = GetTestContactWithParentAccount(account2Ref);
    //  service.Create(contact21);
    //  var contact22 = GetTestContactWithParentAccount(account2Ref);
    //  service.Create(contact22);
    //  var contact23 = GetTestContactWithParentAccount(account2Ref);
    //  service.Create(contact23);

    //  var account3Ref = new EntityReference(Account.EntityLogicalName, account3.Id) { Name = account3.Name };
    //  var contact31 = GetTestContactWithParentAccount(account3Ref);
    //  service.Create(contact31);
    //  var contact32 = GetTestContactWithParentAccount(account3Ref);
    //  service.Create(contact32);
    //  var contact33 = GetTestContactWithParentAccount(account3Ref);
    //  service.Create(contact33);

    //  var account4Ref = new EntityReference(Account.EntityLogicalName, account4.Id) { Name = account4.Name };
    //  var contact41 = GetTestContactWithParentAccount(account4Ref);
    //  contact41.FirstName = "QQQQ";
    //  service.Create(contact41);
    //  var contact42 = GetTestContactWithParentAccount(account4Ref);
    //  contact42.FirstName = "QQQQ";
    //  service.Create(contact42);
    //}

    private static Contact GetTestContactWithParentAccount(EntityReference parentAccount)
    {
      return new Contact(new { accountid = parentAccount })
      {
        ContactId = Guid.NewGuid(),
        FirstName = "John",
        LastName = "West",
        MobilePhone = "12312312",
        EMailAddress1 = "john@dummy_email_domain.com",
        ParentCustomerId = parentAccount
      };
    }

    private static Account TestAccountBase =>
      new Account
        {
          AccountId = Guid.NewGuid(),
          Name = "Some Company",
          Address1_Country = "USA",
          Address1_StateOrProvince = "IL"
        };
  }
}
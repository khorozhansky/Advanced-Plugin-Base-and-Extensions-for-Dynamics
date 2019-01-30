// ReSharper disable InconsistentNaming
namespace AdvancedPluginDemoFxeIntTests.Demo.Contact
{
  extern alias AdvPlg;
  using AdvPlg::AdvancedPlugin.Utils;

  using System;
  using System.Collections.Generic;
  using System.Linq;

  using Microsoft.Xrm.Sdk;

  using Xunit;

  using AdvancedPluginDemo.Proxy;

  using AdvancedPluginDemoFxeIntTests.Demo.Common;
  using AdvancedPluginDemoFxeIntTests.Ext;

  [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
  public class CreateTaskOnParentAccountChangeIntegrationTest
  {
    [Fact]
    public void When_ContactIsCreatedAndParentAccountIsSet_Expected_TaskIsCreated()
    {
      var ctx = new XrmRealContextWithProxy(CommonConstants.CrmConn01);
      var service = ctx.GetOrganizationService();
      var orgCtx = ctx.OrgCtx;
      var parentAccount = TestAccountBase;
      service.Create(parentAccount);
      var contact = TestContactBase;
      contact.ParentCustomerId = parentAccount.ToEntityReference();
      service.Create(contact);
      var task = orgCtx.CreateQuery<Task>().OrderByDescending(r => r.CreatedOn).FirstOrDefault();
      Assert.NotNull(task);
      Assert.Equal(Task_PriorityCode.Low, task.PriorityCodeEnum);
      Assert.Equal("Please review top 3 contacts.", task.Subject);
      var description = task.Description;
      const string AccountInfoTemplate = "Name: {0} - Contacts count: {1}";
      var topAccounts = this.GetTopAccounts(service);
      foreach (var account in topAccounts)
      {
        var accountName = account.GetAttrValue<EntityReference>("parentcustomerid", true)?.Name;
        var qt = account.GetAttrValue<int?>("Qt", true);
        var accountInfoString = string.Format(AccountInfoTemplate, accountName, qt);
        Assert.Contains(accountInfoString, description);
      }
    }

    private IList<Entity> GetTopAccounts(IOrganizationService service)
    {
      var fetchXml = $@"
        <fetch top='3' no-lock='true' aggregate='true'>
          <entity name='contact'>
            <attribute name='parentcustomerid' alias='parentcustomerid' groupby='true' />
            <attribute name='contactid' alias='Qt' aggregate='count' />
            <filter>
              <condition attribute='accountid' operator='not-null' />
            </filter>
            <order alias='Qt' descending='true' />
          </entity>
        </fetch>";

      return service.GetFetchXmlQueryResultAsEntities(fetchXml);
    }

    private static Account TestAccountBase =>
      new Account
        {
          AccountId = Guid.NewGuid(),
          Name = $"Some Company { DateTime.UtcNow }" ,
          Address1_Country = "USA",
          Address1_StateOrProvince = "IL"
        };

    private static Contact TestContactBase =>
      new Contact
        {
          ContactId = Guid.NewGuid(),
          FirstName = "Some FN",
          LastName = $"Some LN { DateTime.UtcNow }",
          MobilePhone = "12312312",
          EMailAddress1 = "john@dummy_email_domain.com",
        };
  }
}
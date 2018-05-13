namespace AdvancedPlugin.Utils
{
  using System;
  using System.Linq;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Query;

  public static class FetchXmlQueryHelper
  {
    public static EntityCollection GetFetchXmlQueryResultSet(this IOrganizationService service, string fetchXml)
    {
      if (string.IsNullOrWhiteSpace(fetchXml))
      {
        throw new ArgumentException(nameof(fetchXml));
      }

      var fetchExpression = new FetchExpression(fetchXml);
      return service.RetrieveMultiple(fetchExpression);
    }

    public static Entity GetFetchXmlQueryResultAsSingleEntity(this IOrganizationService service, string fetchXml)
    {
      return service.GetFetchXmlQueryResultSet(fetchXml)?.Entities?.FirstOrDefault();
    }
  }
}

namespace AdvancedPlugin.Utils
{
  using System;
  using System.Collections.Generic;
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

    public static DataCollection<Entity> GetFetchXmlQueryResultAsEntities(this IOrganizationService service, string fetchXml)
    {
      return service.GetFetchXmlQueryResultSet(fetchXml)?.Entities;
    }

    public static IList<T> GetFetchXmlQueryResultAsEntities<T>(this IOrganizationService service, string fetchXml) where T: Entity
    {
      return service.GetFetchXmlQueryResultAsEntities(fetchXml)?.Select(r => r.ToEntity<T>()).ToList();
    }

    public static Entity GetFetchXmlQueryResultAsSingleEntity(this IOrganizationService service, string fetchXml)
    {
      return service.GetFetchXmlQueryResultAsEntities(fetchXml)?.FirstOrDefault();
    }

    public static T GetFetchXmlQueryResultAsSingleEntity<T>(this IOrganizationService service, string fetchXml) where T: Entity
    {
      var entity = service.GetFetchXmlQueryResultAsEntities(fetchXml)?.FirstOrDefault();
      return entity?.ToEntity<T>();
    }

    public static string ConvertToFetchXmlAttributesString(string[] attributeNames)
    {
      if (attributeNames == null)
      {
        return string.Empty;
      }

      return attributeNames.Aggregate(
        string.Empty, (current, attrName) => 
          current + $"<attribute name='{attrName}' />");
    }


    public static string ConvertToFetchXmlInValuesString(string[] values)
    {
      if (values == null)
      {
        return string.Empty;
      }

      return values.Aggregate(
        string.Empty, (current, value) =>
          current + $"<value>{value}</value>");
    }
  }
}

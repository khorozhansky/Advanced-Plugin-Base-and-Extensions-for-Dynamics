namespace AdvancedPlugin.Extensions
{
  using System;

  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Metadata;

  public class BoxedFieldExt: FieldExt<object, Entity>
  {
    private readonly string attributeTypeCodeString;

    public BoxedFieldExt(Entity initial, Entity current, string attributeName, string attributeTypeCodeString, bool ignoreIsSpecifiedProperty = false)
      : base(initial, current, attributeName, ignoreIsSpecifiedProperty)
    {
      this.attributeTypeCodeString = attributeTypeCodeString;
    }

    protected BoxedFieldExt(Entity initial, Entity current, string attributeName, bool ignoreIsSpecifiedProperty = false)
      : base(initial, current, attributeName, ignoreIsSpecifiedProperty)
    {
    }
    
    protected override bool GetIsModified(object newVal, object oldVal)
    {
      if (newVal == null)
      {
        return oldVal != null;
      }

      if (oldVal == null)
      {
        return true;
      }

      AttributeTypeCode attributeTypeCode;
      var parsed = Enum.TryParse(this.attributeTypeCodeString, out attributeTypeCode);
      if (!parsed)
      {
        throw new InvalidPluginExecutionException($"It's not possible to parse the {attributeTypeCode} attribute type code.");
      }


      var typedFieldExt = this.GetTypedFieldExt(attributeTypeCode);
      return typedFieldExt.IsModified;
    }
    
    private IFieldExt GetTypedFieldExt(AttributeTypeCode attributeTypeCode)
    {
      switch (attributeTypeCode)
      {
        case AttributeTypeCode.Boolean:
          return new FieldExt<bool?, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.Lookup:
        case AttributeTypeCode.Customer:
        case AttributeTypeCode.Owner:
          return new FieldExt<EntityReference, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.DateTime:
          return new FieldExt<DateTime?, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.Decimal:
          return new FieldExt<decimal?, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.Double:
          return new FieldExt<double?, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.Integer:
          return new FieldExt<int?, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.Money:
          return new FieldExt<Money, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.String:
          return new FieldExt<string, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.Picklist:
        case AttributeTypeCode.State:
        case AttributeTypeCode.Status:
          return new FieldExt<OptionSetValue, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.Memo:
          return new FieldExt<string, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.PartyList:
          return new CollectionFieldExt<Entity, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        case AttributeTypeCode.BigInt:
          return new FieldExt<long?, Entity>(this.Initial, this.Current, this.LogicalName, this.IgnoreIsSpecifiedProperty);

        default:
          throw new NotImplementedException();
      }
    }
  }
}

﻿namespace AdvancedPlugin.Utils
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  
  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Query;

  /// <summary>
  /// Defines a set of methods to extract Attribute/Field Name of CRM Entity
  /// </summary>
  public enum LogicalNameExtractMethod
  {
    /// <summary>
    /// Using property name
    /// </summary>
    ByPropertyOrClassName, 

    /// <summary>
    /// Using value of the AttributeLogicalNameAttribute of the property
    /// </summary>
    ByLogicalNameAttribute
  }

  /// <summary>
  /// The entity helper.
  /// </summary>
  public static class EntityHelper
  {
    /// <summary>
    /// Gets the attribute value as a <see cref="IEnumerable{T}"/> "collection".
    /// <remarks>
    /// For instance, you can use it to get values of "PartyList" attributes (e.g. like "To", "Cc", "Bcc" fields of Email entity, etc.)
    /// </remarks>
    /// </summary>
    /// <typeparam name="T">Any "entity type" inherited from <see cref="Entity"/></typeparam>
    /// <param name="entity">The entity.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="aliased"><c>true</c> if the attribute is an aliased attribute.</param>
    /// <returns>A list of entities of the <typeparamref name="T"/> type.</returns>
    public static IEnumerable<T> GetCollectionAttrValue<T>(this Entity entity, string attributeName, bool aliased = false) 
      where T: Entity
    {
      EntityCollection collection;
      if (aliased)
      {
        var value = entity.GetAttributeValue<AliasedValue>(attributeName)?.Value;
        if (value == null)
        {
          return null;
        }

        collection = value as EntityCollection;
      }
      else
      {
        collection = entity.GetAttributeValue<EntityCollection>(attributeName);
      }

      return collection?.Entities?.Select(e => e.ToEntity<T>());
    }

    /// <summary>
    /// Gets the attribute value as some <see cref="Enum"/> value.
    /// </summary>
    /// <remarks>
    /// Use it with OptionSet, State or Status fields ONLY IF YOU WORK WITH CORRESPONDING ENUM FIELDS
    /// </remarks>
    /// <typeparam name="T">An <see cref="Enum"/> defined for possible values of an OptionSet, State or Status field type</typeparam>
    /// <param name="entity">The entity.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="aliased"><c>true</c> if the attribute is an aliased attribute.</param>
    /// <returns>A Nullable value of <typeparamref name="T"/> type</returns>
    public static T GetNullableEnumAttrValue<T>(this Entity entity, string attributeName, bool aliased = false)
    {
      int? code;
      var defaultValue = default(T);
      if (aliased)
      {
        var value = entity.GetAttributeValue<AliasedValue>(attributeName)?.Value as OptionSetValue;
        code = value?.Value;
      }
      else
      {
        code = entity.GetAttributeValue<OptionSetValue>(attributeName)?.Value;
      }

      if (code == null)
      {
        return defaultValue;
      }

      var type = typeof(T);
      var underlyingType = Nullable.GetUnderlyingType(type);
      if (underlyingType?.IsEnum ?? false)
      {
        return (T)Enum.ToObject(underlyingType, code);
      }

      throw new InvalidCastException($"{type.Name} is not an Enum.");
    }

    /// <summary>
    /// Gets the attribute value of a "simple field" (like int?, Guid?, Money, DateTime? and so forth)
    /// ATTENTION! 
    ///   1. Do NOT use to get a value as collection (any <see cref="IEnumerable{T}"/>). Use <see cref="GetCollectionAttrValue{T}"/> for such a case.
    ///   2. Do NOT use to get a value as <see cref="Enum"/> for OptionSet, State or Status field IF YOU WORK WITH CORRESPONDING ENUM FIELDS. Use <see cref="GetNullableEnumAttrValue{T}"/> for such a case.    
    /// </summary>
    /// <typeparam name="T">A "simple type" (Do NOT use any <see cref="IEnumerable{T}"/> or <see cref="Enum"/>)</typeparam>
    /// <param name="entity">The entity.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="aliased"><c>true</c> if the attribute is an aliased attribute.</param>
    /// <returns>A value of T type</returns>
    /// <exception cref="InvalidCastException"></exception>
    public static T GetSimpleAttrValue<T>(this Entity entity, string attributeName, bool aliased = false)
    {
      try
      {
        if (!aliased)
        {
          return entity.GetAttributeValue<T>(attributeName);
        }

        var value = entity.GetAttributeValue<AliasedValue>(attributeName)?.Value;
        if (value == null)
        {
          return default(T);
        }

        return (T)value;
      }
      catch (InvalidCastException invalidCastException)
      {
        var message = $"InvalidCastException occurred while getting value with {nameof(GetSimpleAttrValue)} from the {attributeName} attribute of the {entity.LogicalName} entity. Base exception message: {invalidCastException.Message}";
        throw new InvalidCastException(message, invalidCastException);
      }
    }

    /// <summary>
    /// Gets the attribute value in a correct but a bit slow way for all types of T.
    /// <remarks>
    /// ATTENTION! This method uses reflection. If performance is very important consider using the following "less generic" methods.
    /// <seealso cref="GetSimpleAttrValue{T}" />, <seealso cref="GetCollectionAttrValue{T}" /> and <seealso cref="GetNullableEnumAttrValue{T}" /></remarks>
    /// </summary>
    /// <typeparam name="T">Type of the attribute</typeparam>
    /// <param name="entity">The entity.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="aliased"><c>true</c> if the attribute is an aliased attribute.</param>
    /// <returns>
    /// value of T type
    /// </returns>
    public static T GetAttrValue<T>(this Entity entity, string attributeName, bool aliased = false)
    {
      var defaultValue = default(T);
      var value = aliased ? 
        entity.GetAttributeValue<AliasedValue>(attributeName)?.Value : entity.GetAttributeValue<object>(attributeName);
      if (value == null)
      {
        return defaultValue;
      }

      var type = typeof(T);
      var underlyingType = Nullable.GetUnderlyingType(type);
      if (underlyingType != null && underlyingType.IsEnum)
      {
        // ReSharper disable once UsePatternMatching
        var optionSetValue = value as OptionSetValue;
        return optionSetValue == null ? defaultValue : (T)Enum.ToObject(underlyingType, optionSetValue.Value);
      }

      if (TypeHelper.IsGenericEnumerable(type))
      {
        if (typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
        {
          if (type == typeof(IEnumerable<Entity>))
          {
            return (T)entity.GetCollectionAttrValue<Entity>(attributeName, aliased);
          }

          var collection = value as EntityCollection;
          var entities = collection?.Entities;
          if (entities == null)
          {
            return defaultValue;
          }

          var enumerableType = TypeHelper.GetEnumerableType(type);
          var containerType = typeof(List<>).MakeGenericType(enumerableType);
          var containerList = (IList)Activator.CreateInstance(containerType);

          var toEntityMethod = typeof(Entity).GetMethod("ToEntity");
          // ReSharper disable once PossibleNullReferenceException
          var toEntityGenericMethod = toEntityMethod.MakeGenericMethod(enumerableType);

          foreach (var e in entities)
          {
            containerList.Add(toEntityGenericMethod.Invoke(e, null));
          }

          return (T)containerList;
        }

        throw new NotImplementedException($"Getting attribute value as {type.FullName} is not implemented.");
      }


      return (T)value;
    }


    /// <summary>
    /// Sets the attribute.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the attribute
    /// </typeparam>
    /// <param name="entity">
    /// The entity.
    /// </param>
    /// <param name="key">
    /// The key.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    public static void SetAttrValue<T>(this Entity entity, string key, T value)
    {
      if (entity.Attributes.ContainsKey(key))
      {
        entity[key] = value;
      }
      else
      {
        entity.Attributes.Add(key, value);
      }
    }

    /// <summary>
    /// Sets the attribute value.
    /// </summary>
    /// <typeparam name="T">Type of the attribute</typeparam>
    /// <param name="entity">The entity.</param>
    /// <param name="propertyExpression">The property expression.</param>
    /// <param name="value">The value.</param>
    /// <param name="method">The method.</param>
    public static void SetAttrValue<T>(this Entity entity, Expression<Func<T>> propertyExpression, T value, LogicalNameExtractMethod method = LogicalNameExtractMethod.ByPropertyOrClassName)
    {
      var entityAttributeName = propertyExpression.ExtractEntityAttributeName(method);
      entity.SetAttrValue(entityAttributeName, (object)value);
    }

    /// <summary>
    /// Removes the attribute.
    /// </summary>
    /// <param name="entity">
    /// The entity.
    /// </param>
    /// <param name="key">
    /// The key.
    /// </param>
    public static void RemoveAttr(this Entity entity, string key)
    {
      if (entity.Attributes.ContainsKey(key))
      {
        entity.Attributes.Remove(key);
      }
    }

    /// <summary>
    /// Removes the attribute.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the attribute
    /// </typeparam>
    /// <param name="entity">
    /// The entity.
    /// </param>
    /// <param name="propertyExpression">
    /// The property expression.
    /// </param>
    /// <param name="method">
    /// The method.
    /// </param>
    public static void RemoveAttr<T>(this Entity entity, Expression<Func<T>> propertyExpression, LogicalNameExtractMethod method = LogicalNameExtractMethod.ByPropertyOrClassName)
    {
      var entityAttributeName = propertyExpression.ExtractEntityAttributeName(method);
      entity.RemoveAttr(entityAttributeName);
    }

    /// <summary>
    /// Determines whether the ColumnSet contains the column specified by lambda expression.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the column
    /// </typeparam>
    /// <param name="columnSet">
    /// The column set.
    /// </param>
    /// <param name="propertyExpression">
    /// The property expression.
    /// </param>
    /// <param name="method">
    /// The method.
    /// </param>
    /// <returns>
    /// <c>True</c> if the column found
    /// </returns>
    public static bool Contains<T>(this ColumnSet columnSet, Expression<Func<T>> propertyExpression, LogicalNameExtractMethod method = LogicalNameExtractMethod.ByPropertyOrClassName)
    {
      var entityAttributeName = propertyExpression.ExtractEntityAttributeName(method);
      return columnSet.Columns.Contains(entityAttributeName);
    }

    /// <summary>
    /// Adds the column to the column set.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the column
    /// </typeparam>
    /// <param name="columnSet">
    /// The column set.
    /// </param>
    /// <param name="propertyExpression">
    /// The property expression.
    /// </param>
    /// <param name="method">
    /// The method.
    /// </param>
    public static void AddColumn<T>(this ColumnSet columnSet, Expression<Func<T>> propertyExpression, LogicalNameExtractMethod method = LogicalNameExtractMethod.ByPropertyOrClassName)
    {
      var entityAttributeName = propertyExpression.ExtractEntityAttributeName(method);
      columnSet.AddColumn(entityAttributeName);
    }

    /// <summary>
    /// Removes the attribute.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the column
    /// </typeparam>
    /// <param name="attributeCollection">
    /// The attribute collection.
    /// </param>
    /// <param name="propertyExpression">
    /// The property expression.
    /// </param>
    /// <param name="method">
    /// The method.
    /// </param>
    public static void Remove<T>(this AttributeCollection attributeCollection, Expression<Func<T>> propertyExpression, LogicalNameExtractMethod method = LogicalNameExtractMethod.ByPropertyOrClassName)
    {
      var entityAttributeName = propertyExpression.ExtractEntityAttributeName(method);
      attributeCollection.Remove(entityAttributeName);
    }

    /// <summary>
    /// Gets the formatted value.
    /// </summary>
    /// <param name="entity">
    /// The entity.
    /// </param>
    /// <param name="fieldName">
    /// Name of the field.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetFormattedAttrValue(this Entity entity, string fieldName)
    {
      var formattedValues = entity?.FormattedValues;
      var labelNotFound = formattedValues == null;
      if (labelNotFound)
      {
        return null;
      }

      return
        formattedValues.ContainsKey(fieldName) ? formattedValues[fieldName] : null;
    }

    /// <summary>
    /// Gets the formatted value.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the attribute
    /// </typeparam>
    /// <param name="entity">
    /// The entity.
    /// </param>
    /// <param name="propertyExpression">
    /// The property expression.
    /// </param>
    /// <param name="method">
    /// The method.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetFormattedAttrValue<T>(this Entity entity, Expression<Func<T>> propertyExpression, LogicalNameExtractMethod method = LogicalNameExtractMethod.ByPropertyOrClassName)
    {
      var propertyName = propertyExpression.ExtractPropertyName().ToLowerInvariant();
      return entity.GetFormattedAttrValue(propertyName);
    }

    /// <summary>
    /// Gets the name of the CRM Entity attribute.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the property
    /// </typeparam>
    /// <param name="propertyExpression">
    /// The property expression.
    /// </param>
    /// <param name="method">
    /// The method.
    /// </param>
    /// <returns>
    /// The name of the entity attribute
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Attribute name extraction method is not supported.
    /// </exception>
    public static string ExtractEntityAttributeName<T>(this Expression<Func<T>> propertyExpression, LogicalNameExtractMethod method)
    {
      switch (method)
      {
        case LogicalNameExtractMethod.ByPropertyOrClassName:
          return propertyExpression.ExtractPropertyName().ToLowerInvariant();

        case LogicalNameExtractMethod.ByLogicalNameAttribute:
          var propertyAttribute = propertyExpression.ExtractPropertyAttribute<T, AttributeLogicalNameAttribute>();
          return propertyAttribute.LogicalName.ToLowerInvariant();

        default:
          throw new NotImplementedException("Attribute name extraction method is not supported.");
      }
    }

    public static bool GetIsAnyFieldSpecified(this Entity entity, params Expression<Func<object>>[] fields)
    {
      if (entity == null || fields == null || fields.Length == 0)
      {
        return false;
      }

      var fieldNames = fields
        .Select(r => r.ExtractEntityAttributeName(LogicalNameExtractMethod.ByPropertyOrClassName))
        .ToArray();

      return entity.GetIsAnyFieldSpecified(fieldNames);
    }

    /// <summary>
    /// Gets a value indicating whether any of the given fields specified.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="fields">The monitored fields.</param>
    /// <returns>True in case any field is specified</returns>
    public static bool GetIsAnyFieldSpecified(this Entity entity, params string[] fields)
    {
      if (entity == null || fields == null || fields.Length == 0)
      {
        return false;
      }

      return fields.Any(entity.Attributes.ContainsKey);
    }

    /// <summary>
    /// Gets a value indicating whether all of the given fields are specified.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="fields">The monitored fields.</param>
    /// <returns>True in case all fields are specified</returns>
    public static bool GetAreAllFieldsSpecified(this Entity entity, params string[] fields)
    {
      if (entity == null || fields == null || fields.Length == 0)
      {
        return false;
      }

      return fields.All(entity.Attributes.ContainsKey);
    }

    /// <summary>
    /// Gets the unspecified fields.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="fields">The monitored fields.</param>
    /// <returns>The list of unspecified fields if any, otherwise null</returns>
    public static string[] GetUnspecifiedFields(this Entity entity, params string[] fields)
    {
      if (entity == null || fields == null || fields.Length == 0)
      {
        return null;
      }

      return fields
        .Where(r => !entity.Attributes.ContainsKey(r))
        .ToArray();
    }

    /// <summary>
    /// Gets a value indicating the given field specified.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The monitored field.</param>
    /// <returns>True in case the field is specified</returns>
    public static bool GetIsFieldSpecified(this Entity entity, string field)
    {
      if (entity == null || field == null)
      {
        return false;
      }

      return entity.Attributes.ContainsKey(field);
    }

    public static bool GetIsAnyFieldSpecifiedExceptExcluded(this Entity entity, params Expression<Func<object>>[] excludedFields)
    {
      if (entity == null || excludedFields == null || excludedFields.Length == 0)
      {
        return false;
      }

      var fieldNames = excludedFields
        .Select(r => r.ExtractEntityAttributeName(LogicalNameExtractMethod.ByPropertyOrClassName))
        .ToArray();

      return entity.GetIsAnyFieldSpecifiedExceptExcluded(fieldNames);
    }

    /// <summary>
    /// Gets a value indicating whether any of fields specified except the given list of excludedFields.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="excludedFields">The excluded fields.</param>
    /// <returns>True in case any field is specified excepting the given ones</returns>
    public static bool GetIsAnyFieldSpecifiedExceptExcluded(this Entity entity, params string[] excludedFields)
    {
      if (entity == null)
      {
        return false;
      }

      if (excludedFields == null)
      {
        excludedFields = new string[] {};
      }

      return entity.Attributes
        .Select(r => r.Key)
        .Except(excludedFields)
        .Any();
    }
  }
}

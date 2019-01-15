﻿namespace AdvancedPlugin.Utils
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using Microsoft.Xrm.Sdk;

  public static class TypeHelper
  {
    /// <summary>
    /// Gets the type of the enumerable.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Type.</returns>
    public static Type GetEnumerableType(Type type)
    {
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
      {
        return type.GetGenericArguments().Single();
      }

      return (from intType in type.GetInterfaces()
              where intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
              select intType.GetGenericArguments()[0]).FirstOrDefault();
    }

    /// <summary>
    /// Determines whether [is generic enumerable] [the specified type].
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if [is generic enumerable] [the specified type]; otherwise, <c>false</c>.</returns>
    public static bool IsGenericEnumerable(Type type)
    {
      return type.IsGenericType &&
         typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition());
    }

    /// <summary>
    /// Converts money value to nullable decimal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>nullable decimal</returns>
    public static decimal? ConvertToDecimal(Money value)
    {
      return value?.Value;
    }
  }
}

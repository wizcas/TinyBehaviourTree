/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

public static class TypeUtility
{
    #region Type Operations
    public static object DefaultValue(this Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        return null;
    }
    public static string Pretty(this bool value)
    {
        return value ? "<color=green>YES</color>" : "<color=red>NO</color>";
    }
    public static T[] GetAttributes<T>(this Type type, bool inherit) where T : Attribute
    {
        return (T[])type.GetCustomAttributes(typeof(T), inherit);
    }
    public static T GetAttribute<T>(this Type type, bool inherit) where T : Attribute
    {
        var attrs = GetAttributes<T>(type, inherit);
        if (attrs.Length == 0) return null;
        var attr = attrs[0];
        return attr;
    }
    #endregion

    #region Find Common Type
    public static Type FindCommonTypeWithin(params Type[] types)
    {
        if (types == null || types.Length == 0) return null;
        Type typeLeft = types[0];
        for(int i = 0; i < types.Length - 1; i++)
        {
            var typeRight = types[i + 1];
            typeLeft = typeLeft.FindCommonTypeWith(typeRight);
            if (typeLeft == null)
                return null;
        }
        return typeLeft;
    }

    // provide common base class or implemented interface
    public static Type FindCommonTypeWith(this Type typeLeft, Type typeRight)
    {
        if (typeLeft == null || typeRight == null) return null;

        var commonBaseClass = typeLeft.FindBaseClassWith(typeRight) ?? typeof(object);

        return commonBaseClass.Equals(typeof(object))
                ? typeLeft.FindInterfaceWith(typeRight)
                : commonBaseClass;
    }

    // searching for common base class (either concrete or abstract)
    public static Type FindBaseClassWith(this Type typeLeft, Type typeRight)
    {
        if (typeLeft == null || typeRight == null) return null;

        return typeLeft
                .GetClassHierarchy()
                .Intersect(typeRight.GetClassHierarchy())
                .FirstOrDefault(type => !type.IsInterface);
    }

    // searching for common implemented interface
    // it's possible for one class to implement multiple interfaces, 
    // in this case return first common based interface
    public static Type FindInterfaceWith(this Type typeLeft, Type typeRight)
    {
        if (typeLeft == null || typeRight == null) return null;

        return typeLeft
                .GetInterfaceHierarchy()
                .Intersect(typeRight.GetInterfaceHierarchy())
                .FirstOrDefault();
    }

    // iterate on interface hierarhy
    public static IEnumerable<Type> GetInterfaceHierarchy(this Type type)
    {
        if (type.IsInterface) return new[] { type }.AsEnumerable();

        return type
                .GetInterfaces()
                .OrderByDescending(current => current.GetInterfaces().Count())
                .AsEnumerable();
    }

    // interate on class hierarhy
    public static IEnumerable<Type> GetClassHierarchy(this Type type)
    {
        if (type == null) yield break;

        Type typeInHierarchy = type;

        do
        {
            yield return typeInHierarchy;
            typeInHierarchy = typeInHierarchy.BaseType;
        }
        while (typeInHierarchy != null && !typeInHierarchy.IsInterface);
    }
    #endregion

    #region Collection
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

    public static T[] Copy<T>(this T[] source)
    {
        var ret = new T[source.Length];
        source.CopyTo(ret, 0);
        return ret;
    }
    
    public static string Print<T>(this IEnumerable<T> collection, string delimiter = ",")
    {
        return string.Join(delimiter, collection.Select(item => item.ToString()).ToArray());
    }
    #endregion

    #region Flags Operation
    public static bool HasFlag(this Enum value, Enum criterion)
    {
        var intValue = Convert.ToInt32(value);
        var intCriterion = Convert.ToInt32(criterion);
        return (intValue & intCriterion) == intCriterion;
    }
    #endregion
}

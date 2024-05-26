using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;

namespace GraphProcessor
{
	public static class TypeExtension
    {
        private static Type[] _allTypes;

        public static Type[] AllTypes
        {
            get
            {
                return _allTypes ??= AppDomain.CurrentDomain.GetAllTypes().ToArray();
            }
        }

        public static IEnumerable<Type> GetAllTypesDerivedFrom<T>()
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesDerivedFrom<T>();
#else
            return AllTypes.Where(x => typeof(T).IsSubclassOf(x));
#endif
        }

        public static IEnumerable<(Type, T)> GetAllTypesWithAttribute<T>(bool inherit = false) where T : Attribute
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesWithAttribute<T>()
                .Where(x => x.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() is T attribute)
                .Select(x => (x, x.GetCustomAttribute(typeof(T), inherit) as T));
#else
            var list = new List<(Type, T)>();

            foreach (var type in AllTypes)
            {
                if(type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() is T attribute) list.Add((type, attribute));
            }

            return list;
#endif
        }

        public static bool IsReallyAssignableFrom(this Type type, Type otherType)
		{
			if (type.IsAssignableFrom(otherType))
				return true;
			if (otherType.IsAssignableFrom(type))
				return true;

			try
			{
				var v = Expression.Variable(otherType);
				var expr = Expression.Convert(v, type);
				return expr.Method != null && expr.Method.Name != "op_Implicit";
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}

	}
}
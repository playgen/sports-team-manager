using System;
using System.Collections.Generic;
using System.Reflection;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class ObjectExtensions
	{
		private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly ReferenceEqualityComparer ReferenceEqualityComparer = new ReferenceEqualityComparer();

		internal static bool IsPrimitive(this Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			return type.IsValueType & type.IsPrimitive;
		}

		internal static object Copy(this object originalObject)
		{
			return InternalCopy(originalObject, new Dictionary<object, object>(ReferenceEqualityComparer));
		}

		private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
		{
			if (originalObject == null)
			{
				return null;
			}
			var typeToReflect = originalObject.GetType();
			if (IsPrimitive(typeToReflect))
			{
				return originalObject;
			}
			if (visited.ContainsKey(originalObject))
			{
				return visited[originalObject];
			}
			if (typeof(Delegate).IsAssignableFrom(typeToReflect))
			{
				return originalObject;
			}
			var cloneObject = CloneMethod.Invoke(originalObject, null);
			visited.Add(originalObject, cloneObject);
			CopyFields(originalObject, visited, cloneObject, typeToReflect);
			RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
			return cloneObject;
		}

		private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
		{
			if (typeToReflect.BaseType != null)
			{
				RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
				if (typeToReflect.BaseType.BaseType != null)
				{
					CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic);
				}
			}
		}

		private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
		{
			foreach (var fieldInfo in typeToReflect.GetFields(bindingFlags))
			{
				if (IsPrimitive(fieldInfo.FieldType))
				{
					continue;
				}
				var originalFieldValue = fieldInfo.GetValue(originalObject);
				var clonedFieldValue = InternalCopy(originalFieldValue, visited);
				fieldInfo.SetValue(cloneObject, clonedFieldValue);
			}
		}

		internal static T Copy<T>(this T original)
		{
			return (T)Copy((object)original);
		}
	}

	internal class ReferenceEqualityComparer : EqualityComparer<object>
	{
		public override bool Equals(object x, object y)
		{
			return ReferenceEquals(x, y);
		}
		public override int GetHashCode(object obj)
		{
			return obj == null ? 0 : obj.GetHashCode();
		}
	}
}
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace System
{
	public static class ObjectExtensions
	{
		private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly ReferenceEqualityComparer ReferenceEqualityComparer = new ReferenceEqualityComparer();
		public static List<long> TimingList = new List<long>();

		public static object Copy(this object originalObject)
		{
			return InternalCopy(originalObject, new Dictionary<object, object>(ReferenceEqualityComparer));
		}
		private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
		{
			if (TimingList.Count == 0)
			{
				for (int i = 0; i < 10; i++)
				{
					TimingList.Add(0);
				}
			}
			if (originalObject == null) return null;
			var typeToReflect = originalObject.GetType();
			if (visited.ContainsKey(originalObject)) return visited[originalObject];
			if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return originalObject;
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
					CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
				}
			}
		}

		private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
		{
			foreach (var fieldInfo in typeToReflect.GetFields(bindingFlags))
			{
				var originalFieldValue = fieldInfo.GetValue(originalObject);
				var clonedFieldValue = InternalCopy(originalFieldValue, visited);
				fieldInfo.SetValue(cloneObject, clonedFieldValue);
			}
		}
		public static T Copy<T>(this T original)
		{
			return (T)Copy((object)original);
		}
	}

	public class ReferenceEqualityComparer : EqualityComparer<object>
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
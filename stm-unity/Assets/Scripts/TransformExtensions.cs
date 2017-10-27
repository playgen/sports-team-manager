using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
	/// <summary>
	/// Find child with name provided, including inactive objects
	/// </summary>
	public static Transform FindInactive(this Transform parent, string name)
	{
		var trs = parent.GetComponentsInChildren<Transform>(true);
		foreach (var t in trs)
		{
			if (t.name == name && t.parent == parent)
			{
				return t;
			}
		}
		return null;
	}

	/// <summary>
	/// Find all children with name provided, including inactive objects
	/// </summary>
	public static List<Transform> FindAll(this Transform parent, string name)
	{
		var found = new List<Transform>();
		var trs = parent.GetComponentsInChildren<Transform>(true);
		foreach (var t in trs)
		{
			if (t.name == name)
			{
				found.Add(t);
			}
		}
		return found;
	}

	/// <summary>
	/// Find if provided recttransform is currently visible within the provided rect
	/// </summary>
	public static bool IsRectTransformVisible(this RectTransform obj, RectTransform visibleRect)
	{
		var objCorners = new Vector3[4];
		obj.GetWorldCorners(objCorners);
		var isVisible = true;
		foreach (var corner in objCorners)
		{
		    if (!RectTransformUtility.RectangleContainsScreenPoint(visibleRect, corner, null))
		    {
		        isVisible = false;
		        break;
		    }
		}
		return isVisible;
	}
}
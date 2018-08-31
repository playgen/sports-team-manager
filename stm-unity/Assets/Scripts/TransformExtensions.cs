using System.Linq;
using UnityEngine;

public static class TransformExtensions
{
	/// <summary>
	/// Find if RectTransform is currently visible within the provided RectTransform
	/// </summary>
	public static bool IsRectTransformVisible(this RectTransform obj, RectTransform visibleRect)
	{
		var objCorners = new Vector3[4];
		obj.GetWorldCorners(objCorners);
		return objCorners.All(corner => RectTransformUtility.RectangleContainsScreenPoint(visibleRect, corner, null));
	}
}
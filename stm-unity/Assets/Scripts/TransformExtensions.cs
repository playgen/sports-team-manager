using UnityEngine;

public static class TransformExtensions
{
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
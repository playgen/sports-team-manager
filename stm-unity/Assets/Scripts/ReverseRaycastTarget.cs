using System;
using UnityEngine;

public class ReverseRaycastTarget : MonoBehaviour, ICanvasRaycastFilter
{
	public RectTransform MaskRect;

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		return !RectTransformUtility.RectangleContainsScreenPoint(MaskRect, sp, eventCamera);
	}
}
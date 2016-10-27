using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReverseRaycastTarget : MonoBehaviour, ICanvasRaycastFilter
{
	public List<RectTransform> MaskRect;

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		return !MaskRect.Any(mr => RectTransformUtility.RectangleContainsScreenPoint(mr, sp, eventCamera));
	}
}
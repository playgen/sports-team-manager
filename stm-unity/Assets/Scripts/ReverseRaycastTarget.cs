using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReverseRaycastTarget : MonoBehaviour, ICanvasRaycastFilter
{
	public List<RectTransform> MaskRect;
	public List<RectTransform> BlacklistRect;

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		return !(MaskRect.Any(mr => RectTransformUtility.RectangleContainsScreenPoint(mr, sp, eventCamera)) && !BlacklistRect.Any(br => RectTransformUtility.RectangleContainsScreenPoint(br, sp, eventCamera)));
	}

	public void UnblockWhitelisted()
	{
		var whitelisted = new List<RectTransform>();
		foreach (var trans in BlacklistRect)
		{
			if (MaskRect.Contains(trans))
			{
				whitelisted.Add(trans);
			}
		}
		foreach (var trans in whitelisted)
		{
			BlacklistRect.Remove(trans);
		}
	}
}
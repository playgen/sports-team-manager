using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used to prevent clicking on objects that do not fit within the allowed area
/// </summary>
public class ReverseRaycastTarget : MonoBehaviour, ICanvasRaycastFilter
{
	public List<RectTransform> MaskRect;
	public List<RectTransform> BlacklistRect;

	/// <summary>
	///If there are more valid objects clicked than invalid objects
	/// </summary>
	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		return !(MaskRect.Count(mr => RectTransformUtility.RectangleContainsScreenPoint(mr, sp, eventCamera)) > BlacklistRect.Count(br => RectTransformUtility.RectangleContainsScreenPoint(br, sp, eventCamera)));
	}

	/// <summary>
	/// Remove objects in the MaskRect list from the Blacklist
	/// </summary>
	public void UnblockWhitelisted()
	{
		var whitelisted = BlacklistRect.Where(trans => MaskRect.Contains(trans)).ToList();
		foreach (var trans in whitelisted)
		{
			BlacklistRect.Remove(trans);
		}
	}
}
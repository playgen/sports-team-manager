using System;
using UnityEngine;

public class PlatformPositioning : MonoBehaviour
{
	[Serializable]
	class Anchor
	{
		public Vector2 Min;
		public Vector2 Max;
	}
	[SerializeField]
	private Anchor _standalonePositioning;
	[SerializeField]
	private Anchor _mobilePositioning;

	private void OnEnable()
	{
		SetPosition();
	}

	public void SetPosition(bool forced = false, bool isForcedMobile = false)
	{
		if ((forced && isForcedMobile) || Application.isMobilePlatform)
		{
			transform.RectTransform().anchorMin = _mobilePositioning.Min;
			transform.RectTransform().anchorMax = _mobilePositioning.Max;
		}
		else
		{
			transform.RectTransform().anchorMin = _standalonePositioning.Min;
			transform.RectTransform().anchorMax = _standalonePositioning.Max;
		}
	}
}
using System.Collections.Generic;
using System.Reflection;
using PlayGen.Unity.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;
using TrackerAssetPackage;

/// <summary>
/// Used on hover text object to set text and reposition when needed
/// </summary>
public class HoverPopUpUI : MonoBehaviour
{
	private Transform _currentHovered;
	private bool _mobileReadyToHide;
	private string _currentText;
	private Vector2 _canvasSize;

	/// <summary>
	/// Triggered by PointerEnter on some UI objects. Sets the text on this object and trigger the HoverCheck method in 1 second
	/// </summary>
	public void DisplayHover(Transform trans, string text)
	{
		SetHoverObject(trans, text);
		Invoke(nameof(HoverCheck), 0.4f);
	}

	/// <summary>
	/// Triggered by OnClick on some UI objects. Sets the text on this object and triggers the HoverCheck method with no delay
	/// </summary>
	public void DisplayHoverNoDelay(Transform trans, string text)
	{
		SetHoverObject(trans, text);
		HoverCheck();
	}

	/// <summary>
	/// Triggered by PointerEnter/PointerClick on some UI objects. Stores position relative to pivot for the hovered object
	/// </summary>
	private void SetHoverObject(Transform trans, string text)
	{
		_currentHovered = trans;
		CancelInvoke(nameof(HoverCheck));
		_currentText = text;
	}

	/// <summary>
	/// If object is still being hovered over, display hover-over pop-up and text and position accordingly
	/// </summary>
	private void HoverCheck()
	{
		if (gameObject.activeInHierarchy)
		{
			return;
		}
		if (_currentHovered != null)
		{
			_mobileReadyToHide = false;
			var canvasScaler = GetComponentInParent<CanvasScaler>();
			_canvasSize = canvasScaler.RectTransform().rect.size;
			gameObject.Active(true);
			transform.SetAsLastSibling();
			GetComponentInChildren<Text>().text = Localization.Get(_currentText);
			var adjust = (Vector2.one * 0.5f) - _currentHovered.RectTransform().pivot;
			var hoverCenter = (Vector2)_currentHovered.position + new Vector2(_currentHovered.RectTransform().rect.width * adjust.x, _currentHovered.RectTransform().rect.height * adjust.y);

			var positionMultiplier = new Vector2(hoverCenter.x < Input.mousePosition.x ? 1 : -1, hoverCenter.y < Input.mousePosition.y ? 1 : -1);
			transform.position = Input.mousePosition;
			transform.RectTransform().anchoredPosition += new Vector2(transform.RectTransform().rect.width * 0.5f, transform.RectTransform().rect.height * 0.5f) * positionMultiplier;

			//reposition accordingly if pop-up would display partially off screen
			if (transform.RectTransform().anchoredPosition.x + (transform.RectTransform().rect.width * 0.5f) > _canvasSize.x * 0.5f)
			{
				transform.RectTransform().anchoredPosition -= new Vector2(transform.RectTransform().rect.width, 0);
			}
			else if(transform.RectTransform().anchoredPosition.x - (transform.RectTransform().rect.width * 0.5f) < -_canvasSize.x * 0.5f)
			{
				transform.RectTransform().anchoredPosition += new Vector2(transform.RectTransform().rect.width, 0);
			}
			if (transform.RectTransform().anchoredPosition.y + (transform.RectTransform().rect.height * 0.5f) > _canvasSize.y * 0.5f)
			{
				transform.RectTransform().anchoredPosition -= new Vector2(0, -transform.RectTransform().rect.height);
			}
			else if (transform.RectTransform().anchoredPosition.y - (transform.RectTransform().rect.height * 0.5f) < -_canvasSize.y * 0.5f)
			{
				transform.RectTransform().anchoredPosition -= new Vector2(0, -transform.RectTransform().rect.height);
			}
			TrackerEventSender.SendEvent(new TraceEvent("HoveredOver", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.HoverKey, _currentText }
			}, AccessibleTracker.Accessible.Accessible));
		    UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _currentText);
		}
	}

	/// <summary>
	/// Triggered by PointerExit on some objects. Hides the hover object and resets the expected position
	/// </summary>
	public void HideHover()
	{
		if (Application.isMobilePlatform && !_mobileReadyToHide)
		{
			_mobileReadyToHide = true;
			return;
		}
		gameObject.Active(false);
		_currentHovered = null;
	    UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	private void Update()
	{
		if (Application.isMobilePlatform && _mobileReadyToHide)
		{
			if (Input.GetMouseButton(0))
			{
				HideHover();
			}
		}
	}
}
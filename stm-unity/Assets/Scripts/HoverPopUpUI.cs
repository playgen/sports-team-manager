using System.Collections.Generic;
using System.Reflection;

using PlayGen.Unity.Utilities.Extensions;

using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;

/// <summary>
/// Used on hover text object to set text and reposition when needed
/// </summary>
public class HoverPopUpUI : MonoBehaviour {

	private Vector2 _currentHovered;
	private bool _mobileReadyToHide;
	private string _currentText;
	private Vector2 _canvasSize;

	/// <summary>
	/// Triggered by PointerEnter/PointerClick on some UI objects. Stores position relative to pivot for the hovered object
	/// </summary>
	public void SetHoverObject(Transform trans)
	{
		var adjust = (Vector2.one * 0.5f) - trans.RectTransform().pivot;
		_currentHovered = (Vector2)trans.position + new Vector2(trans.RectTransform().rect.width * adjust.x, trans.RectTransform().rect.height * adjust.y);
	}

	/// <summary>
	/// Triggered by PointerEnter on some UI objects. Sets the text on this object and trigger the HoverCheck method in 1 second
	/// </summary>
	public void DisplayHover(string text)
	{
		_currentText = text;
		Invoke("HoverCheck", 0.4f);
	}

	/// <summary>
	/// Triggered by OnClick on some UI objects. Sets the text on this object and triggers the HoverCheck method with no delay
	/// </summary>
	public void DisplayHoverNoDelay(string text)
	{
		_currentText = text;
		HoverCheck();
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
		if (_currentHovered != Vector2.zero)
		{
			_mobileReadyToHide = false;
			_canvasSize = GetComponentInParent<CanvasScaler>().RectTransform().rect.size;
			gameObject.Active(true);
			transform.SetAsLastSibling();
			GetComponentInChildren<Text>().text = Localization.Get(_currentText);
			transform.position = Input.mousePosition;
			//reposition accordingly if pop-up would display partially off screen
			var positionMultiplier = _currentHovered.x < transform.position.x ? 1 : -1;
			transform.RectTransform().anchoredPosition += new Vector2(transform.RectTransform().rect.width * 0.5f, 0) * positionMultiplier;


			if (_currentHovered.x < transform.position.x)
			{
				transform.RectTransform().anchoredPosition += new Vector2(transform.RectTransform().rect.width * 0.5f, 0);
				if (transform.RectTransform().anchoredPosition.x + transform.RectTransform().rect.width * 0.5f > _canvasSize.x * 0.5f)
				{
					transform.RectTransform().anchoredPosition -= new Vector2(transform.RectTransform().rect.width, 0);
				}
			}
			else
			{
				transform.RectTransform().anchoredPosition -= new Vector2(transform.RectTransform().rect.width * 0.5f, 0);
				if (transform.RectTransform().anchoredPosition.x - transform.RectTransform().rect.width * 0.5f < -_canvasSize.x * 0.5f)
				{
					transform.RectTransform().anchoredPosition += new Vector2(transform.RectTransform().rect.width, 0);
				}
			}
			if (_currentHovered.y < transform.position.y)
			{
				transform.RectTransform().anchoredPosition += new Vector2(0, transform.RectTransform().rect.height * 0.5f);
				if (transform.RectTransform().anchoredPosition.y + transform.RectTransform().rect.height * 0.5f > _canvasSize.y * 0.5f)
				{
					transform.RectTransform().anchoredPosition -= new Vector2(0, -transform.RectTransform().rect.height);
				}
			}
			else
			{
				transform.RectTransform().anchoredPosition -= new Vector2(0, transform.RectTransform().rect.height * 0.5f);
				if (transform.RectTransform().anchoredPosition.y - transform.RectTransform().rect.height * 0.5f < -_canvasSize.y * 0.5f)
				{
					transform.RectTransform().anchoredPosition -= new Vector2(0, -transform.RectTransform().rect.height);
				}
			}
			TrackerEventSender.SendEvent(new TraceEvent("HoveredOver", TrackerVerbs.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKeys.HoverKey.ToString(), _currentText }
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
		_currentHovered = Vector2.zero;
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
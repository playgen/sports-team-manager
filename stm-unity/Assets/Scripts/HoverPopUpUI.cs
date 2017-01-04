using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used on hover text object to set text and reposition when needed
/// </summary>
public class HoverPopUpUI : ObservableMonoBehaviour {

	private Vector2 _currentHovered;
	private string _currentText;
	private Vector2 _canvasSize;

	private void Awake()
	{
		_canvasSize = ((RectTransform)GetComponentInParent<CanvasScaler>().gameObject.transform).rect.size;
	}

	/// <summary>
	/// Triggered by PointerEnter on some UI objects. Stores position relative to pivot for the hovered object
	/// </summary>
	public void SetHoverObject(Transform trans)
	{
		var adjust = (Vector2.one * 0.5f) - ((RectTransform)trans.transform).pivot;
		_currentHovered = (Vector2)trans.position + new Vector2(((RectTransform)trans.transform).rect.width * adjust.x, ((RectTransform)trans.transform).rect.height * adjust.y);
	}

	/// <summary>
	/// Triggered by PointerEnter on some UI objects. Sets the text on this object and trigger the HoverCheck method in 1 second
	/// </summary>
	public void DisplayHover(string text)
	{
		_currentText = text;
		Invoke("HoverCheck", 1);
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
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
			GetComponentInChildren<Text>().text = Localization.Get(_currentText);
			transform.position = Input.mousePosition;
			if (_currentHovered.x < transform.position.x)
			{
				((RectTransform)transform).anchoredPosition += new Vector2(((RectTransform)transform).rect.width * 0.5f, 0);
				if (((RectTransform)transform).anchoredPosition.x + ((RectTransform)transform).rect.width * 0.5f > _canvasSize.x * 0.5f)
				{
					((RectTransform)transform).anchoredPosition -= new Vector2(((RectTransform)transform).rect.width, 0);
				}
			}
			else
			{
				((RectTransform)transform).anchoredPosition -= new Vector2(((RectTransform)transform).rect.width * 0.5f, 0);
				if (((RectTransform)transform).anchoredPosition.x - ((RectTransform)transform).rect.width * 0.5f < -_canvasSize.x * 0.5f)
				{
					((RectTransform)transform).anchoredPosition += new Vector2(((RectTransform)transform).rect.width, 0);
				}
			}
			if (_currentHovered.y < transform.position.y)
			{
				((RectTransform)transform).anchoredPosition += new Vector2(0, ((RectTransform)transform).rect.height * 0.5f);
				if (((RectTransform)transform).anchoredPosition.y + ((RectTransform)transform).rect.height * 0.5f > _canvasSize.y * 0.5f)
				{
					((RectTransform)transform).anchoredPosition -= new Vector2(0, -((RectTransform)transform).rect.height);
				}
			}
			else
			{
				((RectTransform)transform).anchoredPosition -= new Vector2(0, ((RectTransform)transform).rect.height * 0.5f);
				if (((RectTransform)transform).anchoredPosition.y - ((RectTransform)transform).rect.height * 0.5f < -_canvasSize.y * 0.5f)
				{
					((RectTransform)transform).anchoredPosition += new Vector2(0, -((RectTransform)transform).rect.height);
				}
			}
			ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, gameObject.name, transform.parent.name, _currentText, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "HoverOverIcon", _currentText, AlternativeTracker.Alternative.Menu));
		}
	}

	/// <summary>
	/// Triggered by PointerExit on some objects. Hides the hover object and resets the expected position
	/// </summary>
	public void HideHover()
	{
		gameObject.SetActive(false);
		_currentHovered = Vector2.zero;
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}
}

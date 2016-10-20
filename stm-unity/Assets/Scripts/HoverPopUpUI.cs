using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used on hover text object to set text and reposition when needed
/// </summary>
public class HoverPopUpUI : MonoBehaviour {

	private Vector2 _currentHovered;
	private string _currentText;
	private Vector2 _canvasSize;

	private void Start()
	{
		_canvasSize = GetComponentInParent<CanvasScaler>().gameObject.GetComponent<RectTransform>().rect.size;
	}

	/// <summary>
	/// Triggered by PointerEnter on some UI objects. Stores position relative to pivot for the hovered object
	/// </summary>
	public void SetHoverObject(Transform trans)
	{
		var adjust = (Vector2.one * 0.5f) - trans.GetComponent<RectTransform>().pivot;
		_currentHovered = (Vector2)trans.position + new Vector2(trans.GetComponent<RectTransform>().rect.width * adjust.x, trans.GetComponent<RectTransform>().rect.height * adjust.y);
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
		if (gameObject.activeSelf)
		{
			return;
		}
		if (_currentHovered != Vector2.zero)
		{
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
			GetComponentInChildren<Text>().text = _currentText.Localize();
			transform.position = Input.mousePosition;
			if (_currentHovered.x < transform.position.x)
			{
				GetComponent<RectTransform>().anchoredPosition += new Vector2(GetComponent<RectTransform>().rect.width * 0.5f, 0);
				if (GetComponent<RectTransform>().anchoredPosition.x + GetComponent<RectTransform>().rect.width * 0.5f > _canvasSize.x * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition -= new Vector2(GetComponent<RectTransform>().rect.width, 0);
				}
			}
			else
			{
				GetComponent<RectTransform>().anchoredPosition -= new Vector2(GetComponent<RectTransform>().rect.width * 0.5f, 0);
				if (GetComponent<RectTransform>().anchoredPosition.x - GetComponent<RectTransform>().rect.width * 0.5f < -_canvasSize.x * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition += new Vector2(GetComponent<RectTransform>().rect.width, 0);
				}
			}
			if (_currentHovered.y < transform.position.y)
			{
				GetComponent<RectTransform>().anchoredPosition += new Vector2(0, GetComponent<RectTransform>().rect.height * 0.5f);
				if (GetComponent<RectTransform>().anchoredPosition.y + GetComponent<RectTransform>().rect.height * 0.5f > _canvasSize.y * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, -GetComponent<RectTransform>().rect.height);
				}
			}
			else
			{
				GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, GetComponent<RectTransform>().rect.height * 0.5f);
				if (GetComponent<RectTransform>().anchoredPosition.y - GetComponent<RectTransform>().rect.height * 0.5f < -_canvasSize.y * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -GetComponent<RectTransform>().rect.height);
				}
			}
		}
	}

	/// <summary>
	/// Triggered by PointerExit on some objects. Hides the hover object and resets the expected position
	/// </summary>
	public void HideHover()
	{
		gameObject.SetActive(false);
		_currentHovered = Vector2.zero;

	}
}

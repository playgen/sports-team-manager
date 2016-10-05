using UnityEngine;
using UnityEngine.UI;

public class HoverPopUpUI : MonoBehaviour {

	private Vector2 _currentHovered;
	private string _currentText;

	public void SetHoverObject(Transform trans)
	{
		var adjust = (Vector2.one * 0.5f) - trans.GetComponent<RectTransform>().pivot;
		_currentHovered = (Vector2)trans.position + new Vector2(trans.GetComponent<RectTransform>().rect.width * adjust.x, trans.GetComponent<RectTransform>().rect.height * adjust.y);
	}

	public void DisplayHover(string text)
	{
		_currentText = text;
		Invoke("HoverCheck", 1);
	}

	private void HoverCheck()
	{
		if (_currentHovered != Vector2.zero)
		{
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
			GetComponentInChildren<Text>().text = _currentText;
			transform.position = Input.mousePosition;
			var canvasSize = GetComponentInParent<CanvasScaler>().referenceResolution;
			if (_currentHovered.x < transform.position.x)
			{
				GetComponent<RectTransform>().anchoredPosition += new Vector2(GetComponent<RectTransform>().rect.width * 0.5f, 0);
				if (GetComponent<RectTransform>().anchoredPosition.x + GetComponent<RectTransform>().rect.width * 0.5f > canvasSize.x * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition -= new Vector2(GetComponent<RectTransform>().rect.width, 0);
				}
			}
			else
			{
				GetComponent<RectTransform>().anchoredPosition -= new Vector2(GetComponent<RectTransform>().rect.width * 0.5f, 0);
				if (GetComponent<RectTransform>().anchoredPosition.x - GetComponent<RectTransform>().rect.width * 0.5f < -canvasSize.x * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition += new Vector2(GetComponent<RectTransform>().rect.width, 0);
				}
			}
			if (_currentHovered.y < transform.position.y)
			{
				GetComponent<RectTransform>().anchoredPosition += new Vector2(0, GetComponent<RectTransform>().rect.height * 0.5f);
				if (GetComponent<RectTransform>().anchoredPosition.y + GetComponent<RectTransform>().rect.height * 0.5f > canvasSize.y * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, -GetComponent<RectTransform>().rect.height);
				}
			}
			else
			{
				GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, GetComponent<RectTransform>().rect.height * 0.5f);
				if (GetComponent<RectTransform>().anchoredPosition.y - GetComponent<RectTransform>().rect.height * 0.5f < -canvasSize.y * 0.5f)
				{
					GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -GetComponent<RectTransform>().rect.height);
				}
			}
		}
	}

	public void HideHover()
	{
		gameObject.SetActive(false);
		_currentHovered = Vector2.zero;

	}
}

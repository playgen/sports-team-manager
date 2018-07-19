using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages Unity UI interaction for objects with hover over functionality
/// </summary>
public class HoverObject : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
	[SerializeField]
	private string _hoverText;

	/// <summary>
	/// Set text and pop-up for this object
	/// </summary>
	public void SetHoverText(string text)
	{
		_hoverText = text;
	}

	/// <summary>
	/// Triggered by Unity UI. If enabled instantly display text on pop-up
	/// </summary>
	public void OnPointerClick(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(_hoverText))
		{
		    UIManagement.Hover.DisplayHoverNoDelay(transform, _hoverText);
		}
	}

	/// <summary>
	/// Triggered by Unity UI. If enabled set text to be displayed if user continues to hover
	/// </summary>
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!string.IsNullOrEmpty(_hoverText))
		{
			if (Application.isMobilePlatform)
			{
				UIManagement.Hover.DisplayHoverNoDelay(transform, _hoverText);
			}
			else
			{
				UIManagement.Hover.DisplayHover(transform, _hoverText);
			}
		}
	}

	/// <summary>
	/// Triggered by Unity UI. If enabled hide hover pop-up
	/// </summary>
	public void OnPointerExit(PointerEventData eventData)
	{
		UIManagement.Hover.HideHover();
	}
}
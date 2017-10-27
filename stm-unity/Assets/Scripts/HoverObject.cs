using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages Unity UI interaction for objects with hover over functionality
/// </summary>
public class HoverObject : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
	private string _hoverText;
	public bool Enabled;

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
		if (Enabled)
		{
		    UIManagement.Hover.SetHoverObject(transform);
		    UIManagement.Hover.DisplayHoverNoDelay(_hoverText);
		}
	}

	/// <summary>
	/// Triggered by Unity UI. If enabled set text to be displayed if user continues to hover
	/// </summary>
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (Enabled)
		{
		    UIManagement.Hover.SetHoverObject(transform);
		    UIManagement.Hover.DisplayHover(_hoverText);
		}
	}

	/// <summary>
	/// Triggered by Unity UI. If enabled hide hover pop-up
	/// </summary>
	public void OnPointerExit(PointerEventData eventData)
	{
		if (Enabled)
		{
		    UIManagement.Hover.HideHover();
		}
	}
}
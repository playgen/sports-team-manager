using UnityEngine;
using UnityEngine.EventSystems;

public class HoverObject : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
	private string _hoverText;
	private HoverPopUpUI _hoverPopUp;

	public void SetHoverText(string text, HoverPopUpUI popUpUI)
	{
		_hoverText = text;
		_hoverPopUp = popUpUI;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_hoverPopUp.SetHoverObject(transform);
		_hoverPopUp.DisplayHoverNoDelay(_hoverText);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_hoverPopUp.SetHoverObject(transform);
		_hoverPopUp.DisplayHover(_hoverText);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_hoverPopUp.HideHover();
	}
}

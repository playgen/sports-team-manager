using UnityEngine;
using UnityEngine.UI;

public class DynamicPadding : MonoBehaviour {

	public void Adjust()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
		GetComponent<LayoutGroup>().padding.bottom = (int)(((RectTransform)transform).sizeDelta.y * 0.25f) + 16;
		((RectTransform)transform.parent.Find("Buttons").transform).anchoredPosition = new Vector2(0, GetComponent<LayoutGroup>().padding.bottom);
	}
}

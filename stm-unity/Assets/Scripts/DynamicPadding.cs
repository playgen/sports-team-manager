using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DynamicPadding : MonoBehaviour {

	public void OnEnable()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
		GetComponent<LayoutGroup>().padding.bottom = (int)(((RectTransform)transform).sizeDelta.y * 0.25f) + 20;
		((RectTransform)transform.parent.Find("Buttons").transform).anchoredPosition = new Vector2(0, GetComponent<LayoutGroup>().padding.bottom);
	}
}

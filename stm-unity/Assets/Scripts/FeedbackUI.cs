using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Feedback))]
public class FeedbackUI : MonoBehaviour {

	private Feedback _feedback;
	private int _pageNumber;
	[SerializeField]
	private GameObject[] _pages;
	[SerializeField]
	private GameObject _selectionGraph;
	[SerializeField]
	private GameObject _selectionPrefab;
	[SerializeField]
	private GameObject _descriptionPopUp;

	private void Awake()
	{
		_feedback = GetComponent<Feedback>();
	}

	private void OnEnable()
	{
		_pageNumber = 0;
		foreach (var page in _pages)
		{
			page.SetActive(false);
		}
		ChangePage(0);
		DrawGraph();
	}

	public void ChangePage(int amount)
	{
		_pages[_pageNumber].SetActive(false);
		_pageNumber += amount;
		_pages[_pageNumber].SetActive(true);
	}

	public void ShowDescription(string descriptionType)
	{
		_descriptionPopUp.transform.Find("Description Pop-Up/Header").GetComponent<Localization>().Key = descriptionType;
		_descriptionPopUp.transform.Find("Description Pop-Up/Text").GetComponent<Localization>().Key = descriptionType + "_Description";
		_descriptionPopUp.SetActive(true);
	}

	private void DrawGraph()
	{
		foreach (Transform child in _selectionGraph.transform)
		{
			Destroy(child.gameObject);
		}

		var styles = _feedback.GatherManagementStyles();
		var total = styles.Values.ToList().Sum();
		foreach (var style in styles)
		{
			var percentage = (float)style.Value / total;
			var styleObj = Instantiate(_selectionPrefab, _selectionGraph.transform, false);
			styleObj.transform.Find("Style").GetComponent<Text>().text = Localization.Get(style.Key);
			styleObj.transform.Find("Style").GetComponent<Localization>().Key = style.Key;
			styleObj.transform.Find("Amount").GetComponent<Image>().fillAmount = (float)style.Value / total;
			if (percentage >= 0.8f)
			{
				styleObj.transform.Find("Text Backer/Text").GetComponent<Text>().text = Localization.Get(style.Key + "_Questions_High");
				styleObj.transform.Find("Text Backer/Text").GetComponent<Localization>().Key = style.Key + "_Questions_High";
				styleObj.transform.Find("Text Backer").GetComponent<Image>().color = new Color(1, 0.5f, 0);
				styleObj.transform.Find("Text Backer").gameObject.SetActive(true);
			}
			else if (percentage <= 0.2f)
			{
				styleObj.transform.Find("Text Backer/Text").GetComponent<Text>().text = Localization.Get(style.Key + "_Questions_Low");
				styleObj.transform.Find("Text Backer/Text").GetComponent<Localization>().Key = style.Key + "_Questions_Low";
				styleObj.transform.Find("Text Backer").GetComponent<Image>().color = new Color(0, 1, 1);
				styleObj.transform.Find("Text Backer").gameObject.SetActive(true);
			}
			else
			{
				styleObj.transform.Find("Text Backer").gameObject.SetActive(false);
			}
		}
	}
}

using PlayGen.Unity.Utilities.Localization;
using System.Linq;

using PlayGen.Unity.Utilities.BestFit;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Feedback))]
public class FeedbackUI : MonoBehaviour {

	private Feedback _feedback;
	private int _pageNumber;
	[SerializeField]
	private GameObject[] _pages;
	[SerializeField]
	private GameObject[] _managementButtons;
	[SerializeField]
	private GameObject[] _leadershipButtons;
	[SerializeField]
	private GameObject _selectionGraph;
	[SerializeField]
	private GameObject _selectionPrefab;
	[SerializeField]
	private GameObject _descriptionPopUp;
	[SerializeField]
	private Text _finalResultText;

	private void Awake()
	{
		_feedback = GetComponent<Feedback>();
	}

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		_pageNumber = 0;
		foreach (var page in _pages)
		{
			page.SetActive(false);
		}
		ChangePage(0);
		DrawGraph();
		SetGraphPercentages();
		SetPrevalentStyleText();
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
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
		var orange = new Color(1, 0.5f, 0);
		var blue = new Color(0, 1, 1);
		foreach (var style in styles)
		{
			var styleObj = Instantiate(_selectionPrefab, _selectionGraph.transform, false);
			styleObj.transform.Find("Style").GetComponent<Text>().text = Localization.Get(style.Key);
			styleObj.transform.Find("Style").GetComponent<Localization>().Key = style.Key;
			styleObj.transform.Find("Amount").GetComponent<Image>().fillAmount = style.Value;
			styleObj.transform.Find("Percentage").GetComponent<Text>().text = (Mathf.Round(style.Value * 1000) * 0.1f).ToString(Localization.SelectedLanguage.GetSpecificCulture()) + "%";

			if (style.Value >= 0.6f)
			{
				styleObj.transform.Find("Text Backer/Text").GetComponent<Text>().text = Localization.Get(style.Key + "_Questions_High");
				styleObj.transform.Find("Text Backer/Text").GetComponent<Localization>().Key = style.Key + "_Questions_High";
				styleObj.transform.Find("Text Backer").GetComponent<Image>().color = orange;
				styleObj.transform.Find("Amount").GetComponent<Image>().color = orange;
				styleObj.transform.Find("Text Backer").gameObject.SetActive(true);
			}
			else if (style.Value <= 0.2f)
			{
				styleObj.transform.Find("Text Backer/Text").GetComponent<Text>().text = Localization.Get(style.Key + "_Questions_Low");
				styleObj.transform.Find("Text Backer/Text").GetComponent<Localization>().Key = style.Key + "_Questions_Low";
				styleObj.transform.Find("Text Backer").GetComponent<Image>().color = blue;
				styleObj.transform.Find("Amount").GetComponent<Image>().color = blue;
				styleObj.transform.Find("Text Backer").gameObject.SetActive(true);
			}
			else
			{
				styleObj.transform.Find("Text Backer/Text").GetComponent<Text>().text = string.Empty;
				styleObj.transform.Find("Text Backer/Text").GetComponent<Localization>().Key = string.Empty;
				styleObj.transform.Find("Text Backer").gameObject.SetActive(false);
				styleObj.transform.Find("Amount").GetComponent<Image>().color = Color.Lerp(blue, orange, (style.Value - 0.2f) * (0.4f/1f));
			}
		}
		Invoke("DoBestFit", 0);
	}

	private void SetGraphPercentages()
	{
		var style = _feedback.GetPrevalentLeadershipStyle();
		var styleString = string.Empty;
		foreach (var s in style)
		{
			styleString += Localization.Get(s);
			if (s != style.Last())
			{
				styleString += "\n";
			}
		}
		_finalResultText.text = styleString;
	}

	private void SetPrevalentStyleText()
	{
		var styles = _feedback.GatherManagementStyles();
		foreach (var button in _managementButtons)
		{
			if (styles.ContainsKey(button.name.ToLower()))
			{
				button.transform.Find("Percentage").GetComponent<Text>().text = (Mathf.Round(styles[button.name.ToLower()] * 1000) * 0.1f).ToString(Localization.SelectedLanguage.GetSpecificCulture()) + "%";
			}
			else
			{
				button.transform.Find("Percentage").GetComponent<Text>().text = "0%";
			}
		}

		var leaderStyles = _feedback.GatherLeadershipStyles();
		foreach (var button in _leadershipButtons)
		{
			if (leaderStyles.ContainsKey(button.name.ToLower()))
			{
				button.transform.Find("Percentage").GetComponent<Text>().text = (Mathf.Round(leaderStyles[button.name.ToLower()] * 1000) * 0.1f).ToString(Localization.SelectedLanguage.GetSpecificCulture()) + "%";
			}
			else
			{
				button.transform.Find("Percentage").GetComponent<Text>().text = "0%";
			}
		}
	}

	private void OnLanguageChange()
	{
		DrawGraph();
	}

	private void DoBestFit()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_selectionGraph.transform);
		_selectionGraph.BestFit();
	}
}

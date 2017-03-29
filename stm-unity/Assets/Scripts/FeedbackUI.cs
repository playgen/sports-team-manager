using System;

using PlayGen.Unity.Utilities.Localization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using PlayGen.SUGAR.Unity;
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
		if (_pageNumber == 0)
		{
			DoBestFit();
		}
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
		foreach (var style in styles)
		{
			var styleObj = Instantiate(_selectionPrefab, _selectionGraph.transform, false);
			styleObj.transform.Find("Style").GetComponent<Text>().text = Localization.Get(style.Key);
			styleObj.transform.Find("Style").GetComponent<Localization>().Key = style.Key;
			styleObj.transform.Find("Amount").GetComponent<Image>().fillAmount = style.Value;
			styleObj.transform.Find("Percentage").GetComponent<Text>().text = (Mathf.Round(style.Value * 1000) * 0.1f).ToString(Localization.SelectedLanguage.GetSpecificCulture()) + "%";
			if (style.Value >= 0.6f)
			{
				styleObj.transform.Find("Questions").GetComponent<Text>().text = Localization.Get(style.Key + "_Questions_High");
				styleObj.transform.Find("Questions").GetComponent<Localization>().Key = style.Key + "_Questions_High";
			}
			else if (style.Value <= 0.2f)
			{
				styleObj.transform.Find("Questions").GetComponent<Text>().text = Localization.Get(style.Key + "_Questions_Low");
				styleObj.transform.Find("Questions").GetComponent<Localization>().Key = style.Key + "_Questions_Low";
			}
			else
			{
				styleObj.transform.Find("Questions").GetComponent<Text>().text = string.Empty;
				styleObj.transform.Find("Questions").GetComponent<Localization>().Key = string.Empty;
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

	public void TriggerExternal()
	{
		if (SUGARManager.CurrentUser != null)
		{
			string url = "username=" + SUGARManager.CurrentUser.Name; 
			var styles = _feedback.GatherManagementStyles();
			url += "&par1=" + Mathf.Round(styles["competing"] * 100000f);
			url += "&par2=" + Mathf.Round(styles["avoiding"] * 100000f);
			url += "&par3=" + Mathf.Round(styles["accommodating"] * 100000f);
			url += "&par4=" + Mathf.Round(styles["collaborating"] * 100000f);
			url += "&par5=" + Mathf.Round(styles["compromising"] * 100000f);
			url += "&tstamp=" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
			byte[] intputBytes = Encoding.UTF8.GetBytes(url + "&hash=XWtliQQYvsK91kHGcEBg0FrRyOnj6h8w0DNtf5HrmYPSI8eq1fnryIFfLsai");
			var hashProvider = new SHA1Managed();
			byte[] hashed = hashProvider.ComputeHash(intputBytes);
			string hashValue = BitConverter.ToString(hashed).Replace("-", string.Empty);
			url += "&hash=" + hashValue;
			url = "http://comunitaonline.unitn.it/Modules/Games/Rage.aspx?" + url;
			Application.OpenURL(url);
			Application.Quit();
			Debug.Log(url);
		}
	}

	private void OnLanguageChange()
	{
		DrawGraph();
	}

	private void DoBestFit()
	{
		if (_selectionGraph.activeSelf) {
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_selectionGraph.transform);
			var text = _selectionGraph.GetComponentsInChildren<Text>().ToList();
			text.Where(t => t.name == "Style" || t.name == "Percentage").ToList().BestFit();
			text.Where(t => t.name == "Questions").ToList().BestFit();
		}
	}
}

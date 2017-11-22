using System;
using PlayGen.Unity.Utilities.Localization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.BestFit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Contains all logic relating to displaying post-game feedback
/// </summary>
public class FeedbackUI : MonoBehaviour {

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
	private VideoClip[] _videos;
	[SerializeField]
	private Text _finalResultText;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		//reset displayed UI
		_pageNumber = 0;
		foreach (var page in _pages)
		{
			page.Active(false);
		}
		ChangePage(0);
		DrawGraph();
		GetPrevalentLeadershipStyle();
		SetPrevalentStyleText();
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Change currently displayed feedback page
	/// </summary>
	public void ChangePage(int amount)
	{
		_pages[_pageNumber].Active(false);
		_pageNumber += amount;
		_pages[_pageNumber].Active(true);
		if (_pageNumber == 0)
		{
			DoBestFit();
		}
	}

	/// <summary>
	/// Display a pop-up that describes a management or leadership style. Triggered by Unity event
	/// </summary>
	public void ShowDescription(string descriptionType)
	{
		_descriptionPopUp.transform.FindComponent<TextLocalization>("Description Pop-Up/Header").Key = descriptionType;
		_descriptionPopUp.transform.FindComponent<TextLocalization>("Description Pop-Up/Text").Key = descriptionType + "_Description";
		_descriptionPopUp.transform.FindComponent<VideoPlayer>("Description Pop-Up/Video Display").clip = _videos.First(v => string.Equals(v.name, descriptionType, StringComparison.CurrentCultureIgnoreCase));
		_descriptionPopUp.Active(true);
		_descriptionPopUp.transform.FindObject("Description Pop-Up/Video Display").Active(false);
	}

	/// <summary>
	/// Draw the bar graph displayed on the first page.
	/// </summary>
	private void DrawGraph()
	{
		//destroy elements already on graph
		foreach (Transform child in _selectionGraph.transform)
		{
			Destroy(child.gameObject);
		}

		var styles = GameManagement.GameManager.GatherManagementStyles();
		foreach (var style in styles)
		{
			var styleObj = Instantiate(_selectionPrefab, _selectionGraph.transform, false);
			styleObj.transform.FindText("Style").text = Localization.Get(style.Key);
			styleObj.transform.FindComponent<TextLocalization>("Style").Key = style.Key;
			styleObj.transform.FindImage("Amount").fillAmount = style.Value;
			styleObj.transform.FindText("Percentage").text = (Mathf.Round(style.Value * 1000) * 0.1f).ToString(Localization.SpecificSelectedLanguage) + "%";
			if (Mathf.Approximately(style.Value, styles.Values.Max()))
			{
				styleObj.transform.FindText("Questions").text = Localization.Get(style.Key + "_Questions_High");
				styleObj.transform.FindComponent<TextLocalization>("Questions").Key = style.Key + "_Questions_High";
			}
			else if (Mathf.Approximately(style.Value, styles.Values.Min()))
			{
				styleObj.transform.FindText("Questions").text = Localization.Get(style.Key + "_Questions_Low");
				styleObj.transform.FindComponent<TextLocalization>("Questions").Key = style.Key + "_Questions_Low";
			}
			else
			{
				styleObj.transform.FindText("Questions").text = string.Empty;
				styleObj.transform.FindComponent<TextLocalization>("Questions").Key = string.Empty;
			}
		}
		Invoke("DoBestFit", 0);
	}

	/// <summary>
	/// Set the text for the prevalent leadership style selected on the final feedback page
	/// </summary>
	private void GetPrevalentLeadershipStyle()
	{
		var style = GameManagement.GameManager.GetPrevalentLeadershipStyle();
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

	/// <summary>
	/// Set the percentages displayed on the graphs on pages 2 and 3
	/// </summary>
	private void SetPrevalentStyleText()
	{
		var styles = GameManagement.GameManager.GatherManagementStyles();
		foreach (var button in _managementButtons)
		{
			if (styles.ContainsKey(button.name.ToLower()))
			{
				button.transform.FindText("Percentage").text = (Mathf.Round(styles[button.name.ToLower()] * 1000) * 0.1f).ToString(Localization.SpecificSelectedLanguage) + "%";
			}
			else
			{
				button.transform.FindText("Percentage").text = "0%";
			}
		}

		var leaderStyles = GameManagement.GameManager.GatherLeadershipStyles();
		foreach (var button in _leadershipButtons)
		{
			if (leaderStyles.ContainsKey(button.name.ToLower()))
			{
				button.transform.FindText("Percentage").text = (Mathf.Round(leaderStyles[button.name.ToLower()] * 1000) * 0.1f).ToString(Localization.SpecificSelectedLanguage) + "%";
			}
			else
			{
				button.transform.FindText("Percentage").text = "0%";
			}
		}
	}

	/// <summary>
	/// Create the URL for OKKAM and open this webpage
	/// </summary>
	public void TriggerExternal()
	{
		if (SUGARManager.CurrentUser != null)
		{
			var url = "username=" + SUGARManager.CurrentUser.Name; 
			var styles = GameManagement.GameManager.GatherManagementStyles();
			url += "&par1=" + Mathf.Round(styles["competing"] * 100000f);
			url += "&par2=" + Mathf.Round(styles["avoiding"] * 100000f);
			url += "&par3=" + Mathf.Round(styles["accommodating"] * 100000f);
			url += "&par4=" + Mathf.Round(styles["collaborating"] * 100000f);
			url += "&par5=" + Mathf.Round(styles["compromising"] * 100000f);
			url += "&tstamp=" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
			var intputBytes = Encoding.UTF8.GetBytes(url + "&hash=XWtliQQYvsK91kHGcEBg0FrRyOnj6h8w0DNtf5HrmYPSI8eq1fnryIFfLsai");
			var hashProvider = new SHA1Managed();
			var hashed = hashProvider.ComputeHash(intputBytes);
			var hashValue = BitConverter.ToString(hashed).Replace("-", string.Empty);
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
			LayoutRebuilder.ForceRebuildLayoutImmediate(_selectionGraph.RectTransform());
			var text = _selectionGraph.GetComponentsInChildren<Text>().ToList();
			text.Where(t => t.name == "Style" || t.name == "Percentage").ToList().BestFit();
			text.Where(t => t.name == "Questions").ToList().BestFit();
		}
	}
}
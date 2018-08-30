using System;
using System.Collections.Generic;
using PlayGen.Unity.Utilities.Localization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Text;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Extensions;
using PlayGen.Unity.Utilities.Video;
using UnityEngine.Video;

/// <summary>
/// Contains all logic relating to displaying post-game feedback
/// </summary>
public class FeedbackUI : MonoBehaviour
{
	private int _pageNumber;
	[SerializeField]
	private GameObject[] _pages;
	[SerializeField]
	private Text[] _managementButtonPercentageText;
	[SerializeField]
	private Text[] _leadershipButtonPercentageText;
	[SerializeField]
	private Transform _selectionGraph;
	[SerializeField]
	private GameObject _selectionPrefab;
	[SerializeField]
	private GameObject _descriptionPopUp;
	[SerializeField]
	private VideoClip[] _videos;
	private Dictionary<string, VideoClip> _videoDict = new Dictionary<string, VideoClip>();
	[SerializeField]
	private Text _finalResultText;

	private Dictionary<string, float> _managementStyles = new Dictionary<string, float>();

	private void OnEnable()
	{
		//set the array of video clips into a dictionary if not already done
		if (_videoDict.Count != _videos.Length)
		{
			_videoDict = _videos.ToDictionary(v => v.name, v => v, StringComparer.OrdinalIgnoreCase);
		}
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		//reset displayed UI
		_pageNumber = 0;
		foreach (var page in _pages)
		{
			page.Active(false);
		}
		ChangePage(0);
		_managementStyles = GameManagement.GameManager.GatherManagementStyles();
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
		DoBestFit();
	}

	/// <summary>
	/// Display a pop-up that describes a management or leadership style. Triggered by Unity event
	/// </summary>
	public void ShowDescription(string descriptionType)
	{
		var popUp = _descriptionPopUp.transform.Find("Description Pop-Up");
		popUp.FindComponent<TextLocalization>("Header").Key = descriptionType;
		popUp.FindComponent<TextLocalization>("Text").Key = descriptionType + "_Description";
		popUp.FindComponent<VideoPlayerUI>("Video Display").SetClip(_videoDict[descriptionType]);
		_descriptionPopUp.Active(true);
		popUp.FindObject("Video Display").Active(false);
	}

	/// <summary>
	/// Draw the bar graph displayed on the first page.
	/// </summary>
	private void DrawGraph()
	{
		//destroy elements already on graph
		foreach (Transform child in _selectionGraph)
		{
			Destroy(child.gameObject);
		}

		foreach (var style in _managementStyles)
		{
			var styleObj = Instantiate(_selectionPrefab, _selectionGraph, false).transform;
			var styleLocalization = styleObj.FindComponent<TextLocalization>("Style");
			styleLocalization.Key = style.Key;
			styleLocalization.Set();
			styleObj.FindImage("Amount").fillAmount = style.Value;
			styleObj.FindText("Percentage").text = (Mathf.Round(style.Value * 1000) * 0.1f).ToString(Localization.SpecificSelectedLanguage) + "%";
		}
		DoBestFit();
	}

	/// <summary>
	/// Set the text for the prevalent leadership style selected on the final feedback page
	/// </summary>
	private void GetPrevalentLeadershipStyle()
	{
		var style = GameManagement.GameManager.GetPrevalentLeadershipStyle();
		style = style.Select(s => Localization.Get(s)).ToArray();
		_finalResultText.text = string.Join("\n", style);
	}

	/// <summary>
	/// Set the percentages displayed on the graphs on pages 2 and 3
	/// </summary>
	private void SetPrevalentStyleText()
	{
		float value;
		foreach (var perText in _managementButtonPercentageText)
		{
			perText.text = $"{(Mathf.Round((_managementStyles.TryGetValue(perText.Parent().name.ToLower(), out value) ? value : 0) * 1000) * 0.1f).ToString(Localization.SpecificSelectedLanguage)}%";
		}

		var leaderStyles = GameManagement.GameManager.GatherLeadershipStyles();
		foreach (var perText in _leadershipButtonPercentageText)
		{
			perText.text = $"{(Mathf.Round((leaderStyles.TryGetValue(perText.Parent().name.ToLower(), out value) ? value : 0) * 1000) * 0.1f).ToString(Localization.SpecificSelectedLanguage)}%";
		}
	}

	/// <summary>
	/// Create the URL for OKKAM and open this webpage
	/// </summary>
	public void TriggerExternal()
	{
		if (SUGARManager.CurrentUser != null)
		{
			var url = $"username={SUGARManager.CurrentUser.Name}";
			url += $"&par1={Mathf.Round(_managementStyles["competing"] * 100000f)}";
			url += $"&par2={Mathf.Round(_managementStyles["avoiding"] * 100000f)}";
			url += $"&par3={Mathf.Round(_managementStyles["accommodating"] * 100000f)}";
			url += $"&par4={Mathf.Round(_managementStyles["collaborating"] * 100000f)}";
			url += $"&par5={Mathf.Round(_managementStyles["compromising"] * 100000f)}";
			url += $"&tstamp={DateTime.Now:yyyy-MM-ddTHH:mm}";
			var intputBytes = Encoding.UTF8.GetBytes(url + "&hash=XWtliQQYvsK91kHGcEBg0FrRyOnj6h8w0DNtf5HrmYPSI8eq1fnryIFfLsai");
			var hashProvider = new SHA1Managed();
			var hashed = hashProvider.ComputeHash(intputBytes);
			var hashValue = BitConverter.ToString(hashed).Replace("-", string.Empty);
			url += $"&hash={hashValue}";
			url = "http://comunitaonline.unitn.it/Modules/Games/Rage.aspx?" + url;
			Application.OpenURL(url);
			Application.Quit();
			Debug.Log(url);
		}
	}

	/// <summary>
	/// return to the Team Management UI. Triggered by Unity event
	/// </summary>
	public void GoToTeamManagement()
	{
		UIManagement.StateManager.GoToState(State.TeamManagement);
	}

	/// <summary>
	/// Redraw the graph when the language is changed
	/// </summary>
	private void OnLanguageChange()
	{
		DrawGraph();
	}

	/// <summary>
	/// Resize the graph text if the graph is currently active
	/// </summary>
	private void DoBestFit()
	{
		if (_selectionGraph.gameObject.activeSelf)
		{
			var text = _selectionGraph.GetComponentsInChildren<Text>().ToList();
			text.Where(t => t.name == "Style" || t.name == "Percentage").BestFit();
			text.Where(t => t.name == "Questions").BestFit();
		}
	}
}
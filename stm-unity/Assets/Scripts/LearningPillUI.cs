﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Localization;
using TrackerAssetPackage;

/// <summary>
/// Contains all logic relating to displaying post-race event 'learning pills'
/// </summary>
public class LearningPillUI : MonoBehaviour
{
	[SerializeField]
	private Text _helpText;
	[SerializeField]
	private Animation _popUpAnim;
	private string _currentHelp;
	private List<string> _furtherHelp;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
	}

	/// <summary>
	/// Set up displaying learning pills using the keys provided
	/// Further is used if the pill being displayed is following on directly from another
	/// </summary>
	public void SetHelp(List<string> keys, bool further = false)
	{
		_currentHelp = keys[0];
		var tip = keys[0].Split('_')[1].HelpText();
		keys.RemoveAt(0);
		_furtherHelp = keys;
		if (tip != null)
		{
			transform.EnableBlocker();
			StartCoroutine(Animate(true, further, tip));
		}
	}

	/// <summary>
	/// Display help after one is already being displayed
	/// </summary>
	public void SetFurtherHelp()
	{
		SetHelp(_furtherHelp, true);
	}

	/// <summary>
	/// Hide the learning pill. Logic varies slightly if there is further help to display.
	/// </summary>
	public void ClosePill(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			UIManagement.Blocker.onClick.RemoveAllListeners();
		}
		if (_furtherHelp.Count == 0)
		{
			StartCoroutine(Animate());
			UIManagement.Blocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			UIManagement.DisableBlocker();
		}
		if (_furtherHelp.Count > 0)
		{
			StartCoroutine(Animate(false, true));
			Invoke(nameof(SetFurtherHelp), 1.1f);
		}
		else
		{
			UIManagement.EventImpact.Display();
		}
		TrackerEventSender.SendEvent(new TraceEvent("LearningPillClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.LearningPillID, _currentHelp },
			{ TrackerContextKey.TriggerUI, source }
		}, AccessibleTracker.Accessible.Accessible));
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Animate displaying or hiding the learning pill
	/// </summary>
	private IEnumerator Animate(bool upward = false, bool keep = false, string tip = "")
	{
		_helpText.text = string.Empty;
		var endFrame = new WaitForEndOfFrame();
		var start = upward ? keep ? 1 : 0 : 2;
		var limit = keep ? 1 : 2;
		_popUpAnim["LearningPill"].speed = 1;
		_popUpAnim["LearningPill"].time = start;
		_popUpAnim.Play();
		while (_popUpAnim["LearningPill"].time <= start + limit)
		{
			yield return endFrame;
		}
		_popUpAnim["LearningPill"].speed = 0;
		_popUpAnim["LearningPill"].time = start + limit;
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, upward, keep, tip);
		if (upward)
		{
			_helpText.text = tip;
			TrackerEventSender.SendEvent(new TraceEvent("LearningPillDisplayed", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.LearningPillID, _currentHelp }
			}, AccessibleTracker.Accessible.Accessible));
			transform.EnableBlocker(() => ClosePill(TrackerTriggerSource.PopUpBlocker.ToString()));
		}
	}

	private void OnLanguageChange()
	{
		if (_currentHelp != null)
		{
			_helpText.text = _currentHelp.HelpText();
		}
	}
}
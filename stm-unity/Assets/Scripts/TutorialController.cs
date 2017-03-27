﻿using System.Collections.Generic;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using SimpleJSON;
using UnityEngine;
using System.Linq;

using PlayGen.SUGAR.Unity;

using UnityEngine.UI.Extensions;
using PlayGen.Unity.Utilities.Localization;
using System.Globalization;

public class TutorialController : MonoBehaviour
{
	private GameManager _gameManager;
	[SerializeField]
	private GameObject _tutorialSectionPrefab;
	[SerializeField]
	private GameObject _tutorialQuitButton;
	[SerializeField]
	private GameObject _tutorialExitBlocker;

	[ContextMenu("Create Tutorial")]
	public void CreateTutorial()
	{
		while (transform.childCount > 0)
		{
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
		var textAsset = (TextAsset)Resources.Load("Tutorial/Tutorial");
		var parsedAsset = JSON.Parse(textAsset.text);
		for (int i = 0; i < parsedAsset.Count; i++)
		{
			var tutorialSection = Instantiate(_tutorialSectionPrefab, transform, false);
			var section = tutorialSection.GetComponent<TutorialSectionUI>();
			var textDict = new Dictionary<string, string[]>();
			foreach (CultureInfo langName in Localization.Languages)
			{
				var lang = langName.EnglishName;
				if (parsedAsset[i]["Section Text " + lang] != null)
				{
					textDict.Add(langName.Name, parsedAsset[i]["Section Text " + lang].Value.Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).Where(tl => tl.Length > 0).ToArray());
				}
				else
				{
					textDict.Add(langName.Name, parsedAsset[i][0].Value.Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).Where(tl => tl.Length > 0).ToArray());
				}
			}
			var objectNames = parsedAsset[i]["Highlighted Object"].RemoveJSONNodeChars().Split('/');
			var objectHightlight = int.Parse(parsedAsset[i]["Highlight Text Trigger"].RemoveJSONNodeChars());
			var anchorObject = (RectTransform)transform.root;
			foreach (var obj in objectNames)
			{
				anchorObject = (RectTransform)anchorObject.FindInactive(obj) ?? anchorObject;
			}
			var blacklistObjectNames = parsedAsset[i]["Button Blacklist"].RemoveJSONNodeChars().Split('\n').ToList();
			var blacklistNames = blacklistObjectNames.Where(blon => blon.Length > 0).ToList();
			tutorialSection.GetComponentInChildren<SoftMaskScript>().maskScalingRect = anchorObject;
			tutorialSection.GetComponentInChildren<ReverseRaycastTarget>().MaskRect.Add(anchorObject);
			var reversed = bool.Parse(parsedAsset[i]["Reversed UI"].RemoveJSONNodeChars());
			var triggerSplit = parsedAsset[i]["Triggers"].RemoveJSONNodeChars().Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).ToList();
			var triggers = triggerSplit.Select(ts => ts.NoSpaces().Split(',')).Select(ts => new KeyValueMessage(ts[0], ts[1])).ToArray();
			var triggerCount = int.Parse(parsedAsset[i]["Trigger Count Required"].RemoveJSONNodeChars());
			var uniqueTriggers = bool.Parse(parsedAsset[i]["Unique Triggers"].RemoveJSONNodeChars());
			var saveToSection = int.Parse(parsedAsset[i]["Save Progress"].RemoveJSONNodeChars());
			var customAttributes = parsedAsset[i]["Custom Attributes"].RemoveJSONNodeChars().Split('\n').ToList();
			section.Construct(textDict, objectHightlight, reversed, triggers, triggerCount, uniqueTriggers, saveToSection, blacklistNames, customAttributes);
			tutorialSection.name = _tutorialSectionPrefab.name;
		}
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(false);
		}
	}

	public void Start()
	{
		_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		gameObject.SetActive(_gameManager.ShowTutorial);
		_tutorialQuitButton.SetActive(_gameManager.ShowTutorial);
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(child.GetSiblingIndex() == _gameManager.TutorialStage);
		}
		_tutorialExitBlocker.SetActive(transform.childCount == _gameManager.TutorialStage + 1);
	}

	public void AdvanceStage()
	{
		var stage = _gameManager.TutorialStage;
		transform.GetChild(stage).gameObject.SetActive(false);
		var saveAmount = transform.GetChild(stage).GetComponent<TutorialSectionUI>().SaveNextSection;
		_gameManager.SaveTutorialProgress(saveAmount, transform.childCount <= stage + 1);
		_tutorialExitBlocker.SetActive(transform.childCount == stage + 2);
		if (_gameManager.ShowTutorial)
		{
			transform.GetChild(stage + 1).gameObject.SetActive(true);
		}
		else
		{
			SUGARManager.GameData.Send("Tutorial Finished", true);
			gameObject.SetActive(false);
			_tutorialQuitButton.SetActive(false);
		}
	}

	public void QuitTutorial()
	{
		var stage = _gameManager.TutorialStage;
		transform.GetChild(stage).gameObject.SetActive(false);
		_gameManager.SaveTutorialProgress(0, true);
		gameObject.SetActive(false);
		_tutorialQuitButton.SetActive(false);
		_tutorialExitBlocker.SetActive(false);
	}

	public void CustomAttributes(Dictionary<string, string> attributes)
	{
		_gameManager.SetCustomTutorialAttributes(attributes);
	}
}

public static class JSONStringParse
{
	public static string RemoveJSONNodeChars(this string text)
	{
		text = text.Replace("\"", "");
		text = text.Replace("\\\\n", "\n");
		return text;
	}

	public static string RemoveJSONNodeChars(this JSONNode node)
	{
		var text = node.Value.Replace("\"", "");
		text = text.Replace("\\\\n", "\n");
		return text;
	}
}
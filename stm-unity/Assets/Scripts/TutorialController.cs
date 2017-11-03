using System.Collections.Generic;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using SimpleJSON;
using UnityEngine;
using System.Linq;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Localization;

using RAGE.Analytics.Formats;

/// <summary>
/// Connecting class between GameManager in logic and the Tutorial Section UIs
/// </summary>
public class TutorialController : MonoBehaviour
{
	[SerializeField]
	private GameObject _tutorialQuitButton;
	[SerializeField]
	private GameObject _tutorialExitBlocker;

	[SerializeField]
	private List<TutorialObject> _tutorialSections;
	private TutorialSectionUI _tutorialDisplay;
	public int SectionCount
	{
		get { return _tutorialSections.Count; }
	}

	/// <summary>
	/// Load and parse tutorial JSON, creating a new game object for each 
	/// </summary>
	[ContextMenu("Create Tutorial")]
	public void CreateTutorial()
	{
		var textAsset = (TextAsset)Resources.Load("Tutorial/Tutorial");
		var parsedAsset = JSON.Parse(textAsset.text);
		_tutorialSections = new List<TutorialObject>();
		var textDict = new Dictionary<string, List<string>>();
		Localization.Get("Tutorial");
		foreach (var langName in Localization.Languages)
		{
			textDict.Add(langName.Name, new List<string>());
		}
		var objectNames = new List<string[]>();
		var blacklistNames = new List<List<string>>();
		for (var i = 0; i < parsedAsset.Count; i++)
		{
			if (parsedAsset[i][0].Value.RemoveJSONNodeChars() == "BREAK")
			{
				continue;
			}
			foreach (var langName in Localization.Languages)
			{
				var lang = langName.EnglishName;
			    textDict[langName.Name].Add((parsedAsset[i]["Section Text " + lang] != null ? parsedAsset[i]["Section Text " + lang] : parsedAsset[i][0]).Value.RemoveJSONNodeChars());
			}
			objectNames.Add(parsedAsset[i]["Highlighted Object"].RemoveJSONNodeChars().Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray());
			var blacklistObjectNames = parsedAsset[i]["Button Blacklist"].RemoveJSONNodeChars().Split('\n').Where(s => !string.IsNullOrEmpty(s)).ToList();
			blacklistNames.Add(blacklistObjectNames.Where(blon => blon.Length > 0).ToList());
			var reversed = parsedAsset[i]["Reversed UI"].Value.Length > 0 && bool.Parse(parsedAsset[i]["Reversed UI"].RemoveJSONNodeChars());
			var triggerSplit = parsedAsset[i]["Triggers"].RemoveJSONNodeChars().Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).Where(s => !string.IsNullOrEmpty(s)).ToList();
			var triggers = triggerSplit.Count > 0 ? triggerSplit.Select(ts => ts.NoSpaces().Split(',')).Select(ts => new KeyValuePair<string, string>(ts[0], ts[1])).ToArray() : new KeyValuePair<string, string>[0];
			var triggerCount = parsedAsset[i]["Trigger Count Required"].Value.Length > 0 ? int.Parse(parsedAsset[i]["Trigger Count Required"].RemoveJSONNodeChars()) : 0;
			var uniqueTriggers = parsedAsset[i]["Unique Triggers"].Value.Length > 0 && bool.Parse(parsedAsset[i]["Unique Triggers"].RemoveJSONNodeChars());
			var saveToSection = parsedAsset[i]["Save Progress"].Value.Length > 0 ? int.Parse(parsedAsset[i]["Save Progress"].RemoveJSONNodeChars()) : 0;
			var customAttributes = parsedAsset[i]["Custom Attributes"].RemoveJSONNodeChars().Split('\n').Where(s => !string.IsNullOrEmpty(s)).ToList();
			if (i + 1 < parsedAsset.Count && parsedAsset[i + 1][0].Value.RemoveJSONNodeChars() == "BREAK")
			{
				_tutorialSections.Add(new TutorialObject(textDict, objectNames, reversed, triggers, triggerCount, uniqueTriggers, saveToSection, blacklistNames, customAttributes));
				foreach (var langName in Localization.Languages)
				{
					textDict[langName.Name] = new List<string>();
				}
				objectNames = new List<string[]>();
				blacklistNames = new List<List<string>>();
			}
		}
	}

	private void Start()
	{
		_tutorialDisplay = GetComponentsInChildren<TutorialSectionUI>(true).First();
		_tutorialDisplay.gameObject.Active(GameManagement.ShowTutorial);
		gameObject.Active(GameManagement.ShowTutorial);
		_tutorialQuitButton.Active(GameManagement.ShowTutorial);
		if (GameManagement.ShowTutorial)
		{
			_tutorialDisplay.Construct(_tutorialSections[GameManagement.TutorialStage]);
			for (var i = 0; i < SectionCount; i++)
			{
				if (_tutorialSections[i].CustomAttributes.Count > 0)
				{
					var attributeDict = _tutorialSections[i].CustomAttributes.Select(a => new KeyValuePair<string, string>(a.Split('=')[0], a.Split('=')[1])).ToDictionary(c => c.Key, c => c.Value);
					GameManagement.GameManager.SetCustomTutorialAttributes(i, attributeDict);
				}
			}
		}
		_tutorialExitBlocker.Active(SectionCount == GameManagement.TutorialStage + 1);
	}

	public void ShareEvent(string typeName, string methodName, params object[] passed)
	{
		if (GameManagement.ShowTutorial && _tutorialDisplay)
		{
			_tutorialDisplay.EventReceived(typeName, methodName, passed);
		}
	}

	/// <summary>
	/// Advance the tutorial one stage forward, finishing it if the last tutorial stage has been completed
	/// </summary>
	public void AdvanceStage()
	{
		var stage = GameManagement.TutorialStage;
		var saveAmount = _tutorialSections[stage].SaveNextSection;
		GameManagement.GameManager.SaveTutorialProgress(saveAmount, SectionCount <= stage + 1);
		_tutorialExitBlocker.Active(SectionCount == stage + 2);
		if (GameManagement.ShowTutorial)
		{
			_tutorialDisplay.Construct(_tutorialSections[GameManagement.TutorialStage]);
		}
		else
		{
			SUGARManager.GameData.Send("Tutorial Finished", true);
			gameObject.Active(false);
			_tutorialQuitButton.Active(false);
		}
	}

	/// <summary>
	/// Finish the tutorial early
	/// </summary>
	public void QuitTutorial()
	{
		if (SectionCount <= GameManagement.TutorialStage + 1)
		{
			TrackerEventSender.SendEvent(new TraceEvent("TutorialFinished", TrackerVerbs.Completed, new Dictionary<string, string>(), CompletableTracker.Completable.Completable));
		}
		else
		{
			TrackerEventSender.SendEvent(new TraceEvent("TutorialExited", TrackerVerbs.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKeys.TutorialStage.ToString(), (GameManagement.TutorialStage + 1).ToString() },
			}, CompletableTracker.Completable.Completable));
		}
		GameManagement.GameManager.SaveTutorialProgress(0, true);
		gameObject.Active(false);
		_tutorialQuitButton.Active(false);
		_tutorialExitBlocker.Active(false);
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
using System.Collections.Generic;
using System.IO;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using SimpleJSON;
using UnityEngine;
using System.Linq;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Loading;
using PlayGen.Unity.Utilities.Localization;

using TrackerAssetPackage;

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
	public int SectionCount => _tutorialSections.Count;

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
		Localization.Initialize();
		foreach (var langName in Localization.Languages)
		{
			textDict.Add(langName.Name, new List<string>());
		}
		var objectNames = new List<string>();
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
			objectNames.Add(parsedAsset[i]["Highlighted Object"].RemoveJSONNodeChars());
			var blacklistObjectNames = parsedAsset[i]["Button Blacklist"].RemoveJSONNodeChars().Split('\n').Where(s => !string.IsNullOrEmpty(s)).ToList();
			blacklistNames.Add(blacklistObjectNames.Where(blon => blon.Length > 0).ToList());
			var showOnLeft = parsedAsset[i]["Show Popup On Left"].Value.Length > 0 && bool.Parse(parsedAsset[i]["Show Popup On Left"].RemoveJSONNodeChars());
			var triggerSplit = parsedAsset[i]["Triggers"].RemoveJSONNodeChars().Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).Where(s => !string.IsNullOrEmpty(s)).ToList();
			var triggers = triggerSplit.Count > 0 ? triggerSplit.Select(ts => ts.NoSpaces().Split(',')).Select(ts => new KeyValuePair<string, string>(ts[0], ts[1])).ToArray() : new KeyValuePair<string, string>[0];
			var triggerCount = parsedAsset[i]["Trigger Count Required"].Value.Length > 0 ? int.Parse(parsedAsset[i]["Trigger Count Required"].RemoveJSONNodeChars()) : 0;
			var uniqueTriggers = parsedAsset[i]["Unique Triggers"].Value.Length > 0 && bool.Parse(parsedAsset[i]["Unique Triggers"].RemoveJSONNodeChars());
			var safeToSave = parsedAsset[i]["Safe To Save"].RemoveJSONNodeChars().ToLower();
			var customAttributes = parsedAsset[i]["Custom Attributes"].RemoveJSONNodeChars().Split('\n').Where(s => !string.IsNullOrEmpty(s)).ToList();
			var sectionName = parsedAsset[i]["Section Name"].RemoveJSONNodeChars();
			if (i + 1 < parsedAsset.Count && parsedAsset[i + 1][0].Value.RemoveJSONNodeChars() == "BREAK")
			{
				_tutorialSections.Add(new TutorialObject(textDict, objectNames, showOnLeft, triggers, triggerCount, uniqueTriggers, safeToSave, blacklistNames, customAttributes, sectionName));
				foreach (var langName in Localization.Languages)
				{
					textDict[langName.Name] = new List<string>();
				}
				objectNames = new List<string>();
				blacklistNames = new List<List<string>>();
			}
		}
	}

	private void Start()
	{
		_tutorialDisplay = GetComponentInChildren<TutorialSectionUI>(true);
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
		var saveIndex = GetLastSafeStage(stage);
		GameManagement.GameManager.SaveTutorialProgress(saveIndex, SectionCount <= stage + 1);
		_tutorialExitBlocker.Active(SectionCount == stage + 2);
		if (GameManagement.ShowTutorial)
		{
			_tutorialDisplay.Construct(_tutorialSections[GameManagement.TutorialStage]);
			TrackerEventSender.SendEvaluationEvent(TrackerEvalautionEvent.Support, new Dictionary<TrackerEvaluationKey, string> { { TrackerEvaluationKey.Event, "tutorialcontinued" } });
		}
		else
		{
			SUGARManager.GameData.Send("Tutorial Finished", true);
			QuitTutorial();
		}
	}

	private int GetLastSafeStage(int currentStage)
	{
		if (currentStage + 1 < SectionCount)
		{
			if (_tutorialSections[currentStage + 1].SafeToSave == "skip")
			{
				for (var i = currentStage + 1; i < SectionCount; i++)
				{
					if (_tutorialSections[i].SafeToSave == "true")
					{
						return i;
					}
				}
			}
			else
			{
				// fall back to the last stage that is safe to save from
				for (var i = currentStage + 1; i >= 0; i--)
				{
					if (_tutorialSections[i].SafeToSave == "true")
					{
						return i;
					}
				}
			}
		}
		return 0;
	}

	public void RestartGame()
	{
		var language = string.IsNullOrEmpty(Localization.SelectedLanguage.Parent.Name) ? Localization.SelectedLanguage.EnglishName : Localization.SelectedLanguage.Parent.EnglishName;
		var colorsPri = GameManagement.Team.TeamColorsPrimary;
		var colorsSec = GameManagement.Team.TeamColorsSecondary;
		Loading.Start();
		GameManagement.GameManager.NewGameTask(Path.Combine(Application.persistentDataPath, "GameSaves"), GameManagement.TeamName, colorsPri, colorsSec, GameManagement.ManagerName, false, language, success =>
		{
			Loading.Stop();
			if (success)
			{
				SUGARManager.GameData.Send("Tutorial Finished", true);
				QuitTutorial();
				UIManagement.StateManager.ReloadScene();
			}
		});

	}

	/// <summary>
	/// Finish the tutorial early
	/// </summary>
	public void QuitTutorial()
	{
		if (SectionCount <= GameManagement.TutorialStage + 1)
		{
			TrackerEventSender.SendEvent(new TraceEvent("TutorialFinished", TrackerAsset.Verb.Completed, new Dictionary<TrackerContextKey, object>(), CompletableTracker.Completable.Completable));
		}
		else
		{
			TrackerEventSender.SendEvent(new TraceEvent("TutorialExited", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.TutorialStage, GameManagement.TutorialStage + 1 }
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
		text = text.Replace("\"", string.Empty);
		text = text.Replace("\\\\n", "\n");
		return text;
	}

	public static string RemoveJSONNodeChars(this JSONNode node)
	{
		var text = node.Value.Replace("\"", string.Empty);
		text = text.Replace("\\\\n", "\n");
		return text;
	}
}
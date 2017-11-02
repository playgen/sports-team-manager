using System.Collections.Generic;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using SimpleJSON;
using UnityEngine;
using System.Linq;
using PlayGen.SUGAR.Unity;
using UnityEngine.UI.Extensions;
using PlayGen.Unity.Utilities.Localization;

using RAGE.Analytics.Formats;

/// <summary>
/// Connecting class between GameManager in logic and the Tutorial Section UIs
/// </summary>
public class TutorialController : MonoBehaviour
{
	[SerializeField]
	private GameObject _tutorialSectionPrefab;
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
        for (var i = 0; i < parsedAsset.Count; i++)
		{
			var textDict = new Dictionary<string, string[]>();
			foreach (var langName in Localization.Languages)
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
			var blacklistObjectNames = parsedAsset[i]["Button Blacklist"].RemoveJSONNodeChars().Split('\n').ToList();
			var blacklistNames = blacklistObjectNames.Where(blon => blon.Length > 0).ToList();
			var reversed = bool.Parse(parsedAsset[i]["Reversed UI"].RemoveJSONNodeChars());
			var triggerSplit = parsedAsset[i]["Triggers"].RemoveJSONNodeChars().Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).ToList();
			var triggers = triggerSplit.Select(ts => ts.NoSpaces().Split(',')).Select(ts => new KeyValuePair<string, string>(ts[0], ts[1])).ToArray();
			var triggerCount = int.Parse(parsedAsset[i]["Trigger Count Required"].RemoveJSONNodeChars());
			var uniqueTriggers = bool.Parse(parsedAsset[i]["Unique Triggers"].RemoveJSONNodeChars());
			var saveToSection = int.Parse(parsedAsset[i]["Save Progress"].RemoveJSONNodeChars());
			var customAttributes = parsedAsset[i]["Custom Attributes"].RemoveJSONNodeChars().Split('\n').ToList();
            _tutorialSections.Add(new TutorialObject(textDict, objectNames, objectHightlight, reversed, triggers, triggerCount, uniqueTriggers, saveToSection, blacklistNames, customAttributes));
		}
	}

	private void Start()
	{
        _tutorialDisplay = GetComponentsInChildren<TutorialSectionUI>(true).First();
        gameObject.Active(GameManagement.ShowTutorial);
		_tutorialQuitButton.Active(GameManagement.ShowTutorial);
        if (GameManagement.ShowTutorial)
        {
            _tutorialDisplay.Construct(_tutorialSections[GameManagement.TutorialStage]);
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

	/// <summary>
	/// Send custom tutorial attributes to the logic side of the code
	/// </summary>
	public void CustomAttributes(Dictionary<string, string> attributes)
	{
		GameManagement.GameManager.SetCustomTutorialAttributes(attributes);
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
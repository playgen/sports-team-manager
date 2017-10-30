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

	/// <summary>
	/// Load and parse tutorial JSON, creating a new game object for each 
	/// </summary>
	[ContextMenu("Create Tutorial")]
	public void CreateTutorial()
	{
		while (transform.childCount > 0)
		{
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
		var textAsset = (TextAsset)Resources.Load("Tutorial/Tutorial");
		var parsedAsset = JSON.Parse(textAsset.text);
		for (var i = 0; i < parsedAsset.Count; i++)
		{
			var tutorialSection = Instantiate(_tutorialSectionPrefab, transform, false);
			var section = tutorialSection.GetComponent<TutorialSectionUI>();
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
			var triggers = triggerSplit.Select(ts => ts.NoSpaces().Split(',')).Select(ts => new KeyValuePair<string, string>(ts[0], ts[1])).ToArray();
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

	private void Start()
	{
		gameObject.SetActive(GameManagement.ShowTutorial);
		_tutorialQuitButton.SetActive(GameManagement.ShowTutorial);
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(child.GetSiblingIndex() == GameManagement.TutorialStage);
		}
		_tutorialExitBlocker.SetActive(transform.childCount == GameManagement.TutorialStage + 1);
	}

	public void ShareEvent(string typeName, string methodName, params object[] passed)
	{
		if (GameManagement.ShowTutorial)
		{
			transform.GetChild(GameManagement.TutorialStage).GetComponent<TutorialSectionUI>().EventReceived(typeName, methodName, passed);
		}
	}

	/// <summary>
	/// Advance the tutorial one stage forward, finishing it if the last tutorial stage has been completed
	/// </summary>
	public void AdvanceStage()
	{
		var stage = GameManagement.TutorialStage;
		transform.GetChild(stage).gameObject.SetActive(false);
		var saveAmount = transform.GetChild(stage).GetComponent<TutorialSectionUI>().SaveNextSection;
		GameManagement.GameManager.SaveTutorialProgress(saveAmount, transform.childCount <= stage + 1);
		_tutorialExitBlocker.SetActive(transform.childCount == stage + 2);
		if (GameManagement.ShowTutorial)
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

	/// <summary>
	/// Finish the tutorial early
	/// </summary>
	public void QuitTutorial()
	{
		if (transform.childCount <= GameManagement.TutorialStage + 1)
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
		transform.GetChild(GameManagement.TutorialStage).gameObject.SetActive(false);
		GameManagement.GameManager.SaveTutorialProgress(0, true);
		gameObject.SetActive(false);
		_tutorialQuitButton.SetActive(false);
		_tutorialExitBlocker.SetActive(false);
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
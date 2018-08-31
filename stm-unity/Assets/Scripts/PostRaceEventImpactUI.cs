using System.Collections.Generic;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Reflection;
using TrackerAssetPackage;

/// <summary>
/// UI that displays the effects of a post-race event after it has been completed
/// </summary>
public class PostRaceEventImpactUI : MonoBehaviour
{
	private readonly List<KeyValuePair<string, List<string>>> _impacts = new List<KeyValuePair<string, List<string>>>();
	[SerializeField]
	private Text _impactText;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
	}

	/// <summary>
	/// Add an impact to the list to display when the pop-up is next shown
	/// </summary>
	public void AddImpact(string impactKey, List<string> additonal)
	{
		_impacts.Add(new KeyValuePair<string, List<string>>(impactKey, additonal));
	}

	/// <summary>
	/// Display pop-up which shows the impact of the post race event
	/// </summary>
	public void Display()
	{
		_impactText.text = string.Empty;
		//only display if there are any impacts
		if (_impacts.Any())
		{
			foreach (var impact in _impacts)
			{
				//add a space before capital letters and account for any double spacing created as a result
				var subList = impact.Value.Select(sub => Regex.Replace(sub, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0").Replace("  ", " ")).ToList();
				//set text according to what type of event the impact was for
				if (impact.Key.Contains(PostRaceEventImpact.MoodChange.ToString()))
				{
					_impactText.text += (_impactText.text.Length > 0 ? "\n\n" : string.Empty) + Localization.GetAndFormat("IMPACT_MOOD_" + (int.Parse(Regex.Match(impact.Key, @"-?\d+").Value) > 0 ? "BETTER" : "WORSE"), false, subList.ToArray());
				}
				if (Enum.IsDefined(typeof(PostRaceEventImpact), impact.Key))
				{
					switch ((PostRaceEventImpact)Enum.Parse(typeof(PostRaceEventImpact), impact.Key))
					{
						case PostRaceEventImpact.ImproveConflictOpinionGreatly:
							_impactText.text += (_impactText.text.Length > 0 ? "\n\n" : string.Empty) + Localization.GetAndFormat("IMPACT_IMPROVE_CONFLICT_OPINION_GREATLY", false, subList[1]);
							break;
						case PostRaceEventImpact.ImproveConflictTeamOpinion:
							_impactText.text += (_impactText.text.Length > 0 ? "\n\n" : string.Empty) + Localization.GetAndFormat("IMPACT_IMPROVE_CONFLICT_TEAM_OPINION", false, subList[1]);
							break;
						case PostRaceEventImpact.ImproveConflictKnowledge:
							_impactText.text += (_impactText.text.Length > 0 ? "\n\n" : string.Empty) + Localization.GetAndFormat("IMPACT_IMPROVE_CONFLICT_KNOWLEDGE", false, subList[1]);
							break;
						default:
							_impactText.text += (_impactText.text.Length > 0 ? "\n\n" : string.Empty) + Localization.GetAndFormat("IMPACT_" + Regex.Replace(impact.Key, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", "_$0").ToUpper(), false, subList.ToArray());
							break;
					}
				}
				gameObject.Active(true);
				transform.EnableBlocker(() => Close(TrackerTriggerSource.PopUpBlocker.ToString()));
			}
			TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventImpactPopUpDisplayed", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>(), AccessibleTracker.Accessible.Screen));
		}
		if (string.IsNullOrEmpty(_impactText.text))
		{
			Close(string.Empty);
		}
	}

	/// <summary>
	/// Clear and close the impact pop-up
	/// </summary>
	public void Close(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			_impacts.Clear();
			gameObject.Active(false);
			UIManagement.DisableBlocker();
			if (!string.IsNullOrEmpty(source))
			{
				TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventImpactPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.TriggerUI, source }
				}, AccessibleTracker.Accessible.Screen));
				UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
			}
		}
		UIManagement.PostRaceEvents.ToList().ForEach(e => e.Display());
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		Display();
	}
}
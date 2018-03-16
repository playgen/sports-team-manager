using System.Collections.Generic;
using PlayGen.Unity.Utilities.Localization;

using UnityEngine;
using System.Linq;

using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Reflection;

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

	public void AddImpact(string impactKey, List<string> additonal)
	{
		_impacts.Add(new KeyValuePair<string, List<string>>(impactKey, additonal));
	}

	/// <summary>
	/// Display pop-up which shows the cup result
	/// </summary>
	public void Display()
	{
		_impactText.text = string.Empty;
		if (_impacts.Any())
		{
			foreach (var impact in _impacts)
			{
				var subList = new List<string>();
				foreach (var sub in impact.Value)
				{
					subList.Add(Regex.Replace(sub, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0").Replace("  ", " "));
				}
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
				transform.EnableBlocker(() => Close(TrackerTriggerSources.PopUpBlocker.ToString()));
			}
			TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventImpactPopUpDisplayed", TrackerVerbs.Accessed, new Dictionary<string, string>(), AccessibleTracker.Accessible.Screen));
		}
		if (string.IsNullOrEmpty(_impactText.text))
		{
			Close(string.Empty);
		}
	}

	/// <summary>
	/// Close the promotion pop-up
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
				TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventImpactPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
				{
					{ TrackerContextKeys.TriggerUI.ToString(), source }
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
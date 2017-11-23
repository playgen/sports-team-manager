using System;
using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Unity;
using RAGE.Analytics;
using RAGE.Analytics.Formats;
using UnityEngine;

public class TraceEvent
{
	public string Key;
	public TrackerVerbs ActionType;
	public object[] Params;
	public Dictionary<string, string> Values;

	public TraceEvent(string key, TrackerVerbs verb, Dictionary<string, string> values, params object[] param)
	{
		Key = key;
		ActionType = verb;
		Values = values;
		Params = param;
	}
}

/// <summary>
/// Class used to handle events before passing them to the RAGE tracker
/// </summary>
public class TrackerEventSender {
	public static void SendEvent(TraceEvent trace)
	{
		if (!GameManagement.PlatformSettings.Rage)
		{
			return;
		}
		foreach (var v in trace.Values.OrderBy(v => v.Key))
		{
			if (v.Key == TrackerContextKeys.TriggerUI.ToString())
			{
				if (string.IsNullOrEmpty(v.Value))
				{
					Debug.LogWarning(trace.Key + " event not tracked due to null TriggerUI value. If not caused by internal game event, this is a bug!");
					return;
				}
				if (!Enum.IsDefined(typeof(TrackerTriggerSources), v.Value))
				{
					Debug.LogWarning("TrackerTriggerSources does not contain key " + v.Value + ". This is likely due to a typo in the inspector.");
				}
			}
			Tracker.T.setVar(v.Key, v.Value);
		}
		if (SUGARManager.CurrentUser != null)
		{
			Tracker.T.setVar("CurrentUser", SUGARManager.CurrentUser.Name);
		}
		switch (trace.ActionType)
		{
			case TrackerVerbs.Accessed:
				if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(AccessibleTracker.Accessible))
				{
					Tracker.T.accessible.Accessed(trace.Key, (AccessibleTracker.Accessible)Enum.Parse(typeof(AccessibleTracker.Accessible), trace.Params[0].ToString()));
					break;
				}
				Tracker.T.accessible.Accessed(trace.Key);
				break;
			case TrackerVerbs.Skipped:
				if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(AccessibleTracker.Accessible))
				{
					Tracker.T.accessible.Skipped(trace.Key, (AccessibleTracker.Accessible)Enum.Parse(typeof(AccessibleTracker.Accessible), trace.Params[0].ToString()));
					break;
				}
				Tracker.T.accessible.Skipped(trace.Key);
				break;
			case TrackerVerbs.Selected:
				if (trace.Params.Length > 1 && trace.Params[1].GetType() == typeof(AlternativeTracker.Alternative) && trace.Params[0] is string)
				{
					Tracker.T.alternative.Selected(trace.Key, trace.Params[0].ToString(), (AlternativeTracker.Alternative)Enum.Parse(typeof(AlternativeTracker.Alternative), trace.Params[1].ToString()));
					break;
				}
				if (trace.Params.Length > 0 && trace.Params[0] is string)
				{
					Tracker.T.alternative.Selected(trace.Key, trace.Params[0].ToString());
					break;
				}
				Tracker.T.trackedGameObject.Interacted(trace.Key);
				break;
			case TrackerVerbs.Unlocked:
				if (trace.Params.Length > 1 && trace.Params[1].GetType() == typeof(AlternativeTracker.Alternative) && trace.Params[0] is string)
				{
					Tracker.T.alternative.Unlocked(trace.Key, trace.Params[0].ToString(), (AlternativeTracker.Alternative)Enum.Parse(typeof(AlternativeTracker.Alternative), trace.Params[1].ToString()));
					break;
				}
				if (trace.Params.Length > 0 && trace.Params[0] is string)
				{
					Tracker.T.alternative.Unlocked(trace.Key, trace.Params[0].ToString());
					break;
				}
				Tracker.T.trackedGameObject.Interacted(trace.Key);
				break;
			case TrackerVerbs.Initialized:
				if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(CompletableTracker.Completable))
				{
					Tracker.T.completable.Initialized(trace.Key, (CompletableTracker.Completable)Enum.Parse(typeof(CompletableTracker.Completable), trace.Params[0].ToString()));
					break;
				}
				Tracker.T.completable.Initialized(trace.Key);
				break;
			case TrackerVerbs.Progressed:
				if (trace.Params.Length > 1 && trace.Params[0].GetType() == typeof(CompletableTracker.Completable) && trace.Params[1] is float)
				{
					Tracker.T.completable.Progressed(trace.Key, (CompletableTracker.Completable)Enum.Parse(typeof(CompletableTracker.Completable), trace.Params[0].ToString()), float.Parse(trace.Params[1].ToString()));
					break;
				}
				if (trace.Params.Length > 0 && trace.Params[0] is float)
				{
					Tracker.T.completable.Progressed(trace.Key, float.Parse(trace.Params[0].ToString()));
					break;
				}
				Tracker.T.trackedGameObject.Interacted(trace.Key);
				break;
			case TrackerVerbs.Completed:
				if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(CompletableTracker.Completable))
				{
					Tracker.T.completable.Completed(trace.Key, (CompletableTracker.Completable)Enum.Parse(typeof(CompletableTracker.Completable), trace.Params[0].ToString()));
					break;
				}
				Tracker.T.completable.Completed(trace.Key);
				break;
			case TrackerVerbs.Interacted:
				if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(GameObjectTracker.TrackedGameObject))
				{
					Tracker.T.trackedGameObject.Interacted(trace.Key, (GameObjectTracker.TrackedGameObject)Enum.Parse(typeof(GameObjectTracker.TrackedGameObject), trace.Params[0].ToString()));
					break;
				}
				Tracker.T.trackedGameObject.Interacted(trace.Key);
				break;
			case TrackerVerbs.Used:
				if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(GameObjectTracker.TrackedGameObject))
				{
					Tracker.T.trackedGameObject.Used(trace.Key, (GameObjectTracker.TrackedGameObject)Enum.Parse(typeof(GameObjectTracker.TrackedGameObject), trace.Params[0].ToString()));
					break;
				}
				Tracker.T.trackedGameObject.Used(trace.Key);
				break;
			default:
				Tracker.T.trackedGameObject.Interacted(trace.Key);
				break;
		}
		
		Tracker.T.RequestFlush();
	}
}
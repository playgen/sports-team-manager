using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PlayGen.SUGAR.Unity;
using RAGE.Analytics;
using RAGE.EvaluationAsset;

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
		try
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
						Tracker.T.Accessible.Accessed(trace.Key, (AccessibleTracker.Accessible)Enum.Parse(typeof(AccessibleTracker.Accessible), trace.Params[0].ToString()));
						if ((AccessibleTracker.Accessible)Enum.Parse(typeof(AccessibleTracker.Accessible), trace.Params[0].ToString()) == AccessibleTracker.Accessible.Accessible)
						{
							SendEvaluationEvent(TrackerEvalautionEvents.Support, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Event, trace.Key } });
						}
						else
						{
							SendEvaluationEvent(TrackerEvalautionEvents.GameActivity, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Event, trace.Key }, { TrackerEvaluationKeys.GoalOrientation, "neutral" }, { TrackerEvaluationKeys.Tool, "tool" } });
						}
						break;
					}
					Tracker.T.Accessible.Accessed(trace.Key);
					break;
				case TrackerVerbs.Skipped:
					if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(AccessibleTracker.Accessible))
					{
						Tracker.T.Accessible.Skipped(trace.Key, (AccessibleTracker.Accessible)Enum.Parse(typeof(AccessibleTracker.Accessible), trace.Params[0].ToString()));
						SendEvaluationEvent(TrackerEvalautionEvents.GameActivity, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Event, trace.Key }, { TrackerEvaluationKeys.GoalOrientation, "neutral" }, { TrackerEvaluationKeys.Tool, "tool" } });
						break;
					}
					Tracker.T.Accessible.Skipped(trace.Key);
					break;
				case TrackerVerbs.Selected:
					if (trace.Params.Length > 1 && trace.Params[1].GetType() == typeof(AlternativeTracker.Alternative) && trace.Params[0] is string)
					{
						Tracker.T.Alternative.Selected(trace.Key, trace.Params[0].ToString(), (AlternativeTracker.Alternative)Enum.Parse(typeof(AlternativeTracker.Alternative), trace.Params[1].ToString()));
						SendEvaluationEvent(TrackerEvalautionEvents.GameActivity, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Event, trace.Key }, { TrackerEvaluationKeys.GoalOrientation, "neutral" }, { TrackerEvaluationKeys.Tool, "tool" } });
						break;
					}
					if (trace.Params.Length > 0 && trace.Params[0] is string)
					{
						Tracker.T.Alternative.Selected(trace.Key, trace.Params[0].ToString());
						break;
					}
					Tracker.T.GameObject.Interacted(trace.Key);
					break;
				case TrackerVerbs.Unlocked:
					if (trace.Params.Length > 1 && trace.Params[1].GetType() == typeof(AlternativeTracker.Alternative) && trace.Params[0] is string)
					{
						Tracker.T.Alternative.Unlocked(trace.Key, trace.Params[0].ToString(), (AlternativeTracker.Alternative)Enum.Parse(typeof(AlternativeTracker.Alternative), trace.Params[1].ToString()));
						break;
					}
					if (trace.Params.Length > 0 && trace.Params[0] is string)
					{
						Tracker.T.Alternative.Unlocked(trace.Key, trace.Params[0].ToString());
						break;
					}
					Tracker.T.GameObject.Interacted(trace.Key);
					break;
				case TrackerVerbs.Initialized:
					if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(CompletableTracker.Completable))
					{
						Tracker.T.Completable.Initialized(trace.Key, (CompletableTracker.Completable)Enum.Parse(typeof(CompletableTracker.Completable), trace.Params[0].ToString()));
						SendEvaluationEvent(TrackerEvalautionEvents.GameUsage, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Event, trace.Key } });
					break;
					}
					Tracker.T.Completable.Initialized(trace.Key);
					break;
				case TrackerVerbs.Progressed:
					if (trace.Params.Length > 1 && trace.Params[0].GetType() == typeof(CompletableTracker.Completable) && trace.Params[1] is float)
					{
						Tracker.T.Completable.Progressed(trace.Key, (CompletableTracker.Completable)Enum.Parse(typeof(CompletableTracker.Completable), trace.Params[0].ToString()), float.Parse(trace.Params[1].ToString()));
						break;
					}
					if (trace.Params.Length > 0 && trace.Params[0] is float)
					{
						Tracker.T.Completable.Progressed(trace.Key, float.Parse(trace.Params[0].ToString()));
						break;
					}
					Tracker.T.GameObject.Interacted(trace.Key);
					break;
				case TrackerVerbs.Completed:
					if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(CompletableTracker.Completable))
					{
						Tracker.T.Completable.Completed(trace.Key, (CompletableTracker.Completable)Enum.Parse(typeof(CompletableTracker.Completable), trace.Params[0].ToString()));
						SendEvaluationEvent(TrackerEvalautionEvents.GameFlow, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Type, trace.Key }, { TrackerEvaluationKeys.Id, GameManagement.LineUpHistory.Count.ToString() }, { TrackerEvaluationKeys.Completed, "success" } });
						break;
					}
					Tracker.T.Completable.Completed(trace.Key);
					break;
				case TrackerVerbs.Interacted:
					if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(GameObjectTracker.TrackedGameObject))
					{
						Tracker.T.GameObject.Interacted(trace.Key, (GameObjectTracker.TrackedGameObject)Enum.Parse(typeof(GameObjectTracker.TrackedGameObject), trace.Params[0].ToString()));
						SendEvaluationEvent(TrackerEvalautionEvents.GameActivity, new Dictionary<TrackerEvaluationKeys, string> { { TrackerEvaluationKeys.Event, trace.Key }, { TrackerEvaluationKeys.GoalOrientation, "neutral" }, { TrackerEvaluationKeys.Tool, "tool" } });
						break;
					}
					Tracker.T.GameObject.Interacted(trace.Key);
					break;
				case TrackerVerbs.Used:
					if (trace.Params.Length > 0 && trace.Params[0].GetType() == typeof(GameObjectTracker.TrackedGameObject))
					{
						Tracker.T.GameObject.Used(trace.Key, (GameObjectTracker.TrackedGameObject)Enum.Parse(typeof(GameObjectTracker.TrackedGameObject), trace.Params[0].ToString()));
						break;
					}
					Tracker.T.GameObject.Used(trace.Key);
					break;
				default:
					Tracker.T.GameObject.Interacted(trace.Key);
					break;
			}

			Tracker.T.Flush();
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}
	}

	public static void SendEvaluationEvent(TrackerEvalautionEvents ev, Dictionary<TrackerEvaluationKeys, string> parameters)
	{
		try
		{
			if (!GameManagement.PlatformSettings.Rage)
			{
				return;
			}
			var valid = false;
			switch (ev)
			{
				case TrackerEvalautionEvents.GameUsage:
					valid = (parameters.Count == 1 && parameters.Keys.Contains(TrackerEvaluationKeys.Event));
					break;
				case TrackerEvalautionEvents.UserProfile:
					valid = (parameters.Count == 1 && parameters.Keys.Contains(TrackerEvaluationKeys.Event));
					break;
				case TrackerEvalautionEvents.GameActivity:
					valid = (parameters.Count == 3 && parameters.Keys.Contains(TrackerEvaluationKeys.Event) && parameters.Keys.Contains(TrackerEvaluationKeys.GoalOrientation) && parameters.Keys.Contains(TrackerEvaluationKeys.Tool));
					break;
				case TrackerEvalautionEvents.Gamification:
					valid = (parameters.Count == 1 && parameters.Keys.Contains(TrackerEvaluationKeys.Event));
					break;
				case TrackerEvalautionEvents.GameFlow:
					valid = (parameters.Count == 3 && parameters.Keys.Contains(TrackerEvaluationKeys.Type) && parameters.Keys.Contains(TrackerEvaluationKeys.Id) && parameters.Keys.Contains(TrackerEvaluationKeys.Completed));
					break;
				case TrackerEvalautionEvents.Support:
					valid = (parameters.Count == 1 && parameters.Keys.Contains(TrackerEvaluationKeys.Event));
					break;
				case TrackerEvalautionEvents.AssetActivity:
					valid = (parameters.Count == 2 && parameters.Keys.Contains(TrackerEvaluationKeys.Asset) && parameters.Keys.Contains(TrackerEvaluationKeys.Done));
					break;
			}
			if (!valid)
			{
				Debug.LogError("Invalid Evaluation Asset data");
				return;
			}
			var paraString = string.Empty;
			foreach (var para in parameters)
			{
				if (paraString.Length > 0)
				{
					paraString += "&";
				}
				paraString += para.Key.ToString().ToLower();
				paraString += "=";
				paraString += para.Value.ToLower();
			}
			if (!Application.isEditor)
			{
				SendEvaluationEventAsync(ev.ToString().ToLower(), paraString);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}
	}

	private static async void SendEvaluationEventAsync(string gameEvent, string parameter)
	{
		await Task.Factory.StartNew(() => EvaluationAsset.Instance.sensorData(gameEvent, parameter));
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

using RAGE.Analytics;

using UnityEngine;

public class TraceEvent
{
	public string Key;
	public Dictionary<string, string> Values;

	public TraceEvent(string key, Dictionary<string, string> values)
	{
		Key = key;
		Values = values;
	}
}

public class TrackerEventSender {
	public static void SendEvent(TraceEvent trace)
	{
		foreach (var v in trace.Values.OrderBy(v => v.Key))
		{
			if (v.Key == TrackerContextKeys.TriggerUI.ToString())
			{
				if (string.IsNullOrEmpty(v.Value))
				{
					Debug.LogWarning(trace.Key + " event not tracked due to null TriggerUI value. IF not caused by internal game event, this is a bug!");
					return;
				}
				if (!Enum.IsDefined(typeof(TrackerTriggerSources), v.Value))
				{
					Debug.LogWarning("TrackerTriggerSources does not contain key " + v.Value + ". This is likely due to a typo in the inspector.");
				}
			}
			Tracker.T.setVar(v.Key, v.Value);
		}
		Tracker.T.trackedGameObject.Used(trace.Key);
		Tracker.T.RequestFlush();
	}
}
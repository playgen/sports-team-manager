using System;
using System.Linq;

public class TrackerEventCatcher : ObserverMonoBehaviour {
	public override void OnNext(KeyValueMessage message)
	{
		KeyValueMessage track = null;
		foreach (var obj in message.Additional)
		{
		    var valueMessage = obj as KeyValueMessage;
		    if (valueMessage != null)
			{
				var kvm = valueMessage;
				var trackerType = Type.GetType(kvm.TypeName);
				if (trackerType != null)
				{
					track = kvm;
					break;
				}
			}
		}
		if (track != null)
		{
			var field = (Tracker.IGameObjectTracker)typeof(Tracker).GetFields().First(v => v.FieldType == Type.GetType(track.TypeName)).GetValue(Tracker.T);
			if (field != null)
			{
				var types = track.Additional.Select(a => a.GetType()).ToArray();
				var method = field.GetType().GetMethod(track.MethodName, types);
				if (method != null)
				{
					method.Invoke(field, track.Additional);
				}
			}
		}
	}
}
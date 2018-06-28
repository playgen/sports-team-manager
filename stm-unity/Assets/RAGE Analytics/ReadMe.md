# RAGE Analytics
The RAGE Analytics asset allows for tracking of player activity and performance.
- **[The GitHub for RAGE Analytics](https://github.com/e-ucm/unity-tracker)**


From this repo, the following should be copied in order to use RAGE Analytics:
- The '*Tracker.cs*', '*Tracker.prefab*' and '*UnityBridge.cs*' files in the root of the project.
- All of the C# files found in '*csharp-tracker\AssetManager\RageAssetManager*'.
- All of the C# files found in '*csharp-tracker\TrackerAsset*', including those in '*Exception/*', '*Interfaces/*' and '*Utils/*'.


In projects that use other RAGE Assets and as a result contain the RageAssetManager DLL or the RageAssetManager files, such as the FAtiMA Toolkit, the AssetPackage and AssetManagerPackage namespaces will need to be changed in order to avoid conflicts. It is suggested that they are simply prefixed with 'Tracker' or 'Analytics' in order to make the usage of this namespace clear.

- In order to use RAGE Analytics, the Tracker prefab should be contained in the start scene and set up as detailed in the [instructions on GitHub](https://github.com/e-ucm/unity-tracker).

## Tracker

### Recording Events
The following methods are used to record analytics events:
- Tracker.T.Accessible.Accessed(string reachableId)
- Tracker.T.Accessible.Accessed(string reachableId, Accessible type)
- Tracker.T.Accessible.Skipped(string reachableId)
- Tracker.T.Accessible.Skipped(string reachableId, Accessible type)
- Tracker.T.Alternative.Selected(string alternativeId, string optionId)
- Tracker.T.Alternative.Selected(string alternativeId, string optionId, Alternative type)
- Tracker.T.Alternative.Unlocked(string alternativeId, string optionId)
- Tracker.T.Alternative.Unlocked(string alternativeId, string optionId, Alternative type)
- Tracker.T.Completable.Initialized(string completableId)
- Tracker.T.Completable.Initialized(string completableId, Completable type)
- Tracker.T.Completable.Progressed(string completableId, float value)
- Tracker.T.Completable.Progressed(string completableId, Completable type, float value)
- Tracker.T.Completable.Completed(string completableId)
- Tracker.T.Completable.Completed(string completableId, Completable type)
- Tracker.T.Completable.Completed(string completableId, Completable type, bool success)
- Tracker.T.Completable.Completed(string completableId, Completable type, float score)
- Tracker.T.Completable.Completed(string completableId, Completable type, bool success, float score)
- Tracker.T.GameObject.Interacted(string gameobjectId)
- Tracker.T.GameObject.Interacted(string gameobjectId, TrackedGameObject type)
- Tracker.T.GameObject.Used(string gameobjectId)
- Tracker.T.GameObject.Used(string gameobjectId, TrackedGameObject type)

### Additional contextual information in events
Use this method to add additional contextual information to the next event.  

```c#
Tracker.T.setVar(string id, Dictionary<string, bool>/string/int/float/double/bool value)
```

### Force Event Sending
Force the sending of tracked events to the server using

```c#
Tracker.T.Flush()
```

# Usage in Space Modules Inc
## TrackerContextKey
An enum of all the additional contextual keys used for RAGE Analytics.

## TrackerTriggerSource
An enum of all the methods interfaces can be opened and closed. Used as a value of the TriggerUI ContextKey.

## TraceEvent
A TraceEvent contains all of the elements required for a RAGE Analytics event.
- **Key**: Event Name.
- **ActionType**: The type of event being passed (Interacted, Accessed etc)
- **Values**: Dictionary of all of the contextual information for this event.
- **Params**: Any other additional information for this event.

## TrackerEventSender
This class manages sending data for the Evaluation Asset component as well as for RAGE Analytics.

```c#
SendEvent(TraceEvent event)
```

- Method will not run if GameManagement.PlatformSettings.Rage is false.
- The appropriate method for recording an event is called depending on the ActonType passed in the TraceEvent.
- All of the Values provided in the TraceEvent are recorded as contextual information for the event through the use of Tracker.T.setVar on each one.
- For every event the currently signed in SUGAR USer Name is also recorded as contextual information, if a user is signed in.
- Different versions of a method for recording an event are called depending on the information provided in Params.
- At the end of the method recorded events must be sent to the server, see [Force Event Sending](#force-event-sending).

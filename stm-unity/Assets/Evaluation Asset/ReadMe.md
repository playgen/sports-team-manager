# Evaluation Asset
The Evaluation Asset is used to collect data from the RAGE games for evaluation purposes.
- **[GitHub for the EvaluationAsset](https://github.com/RAGE-TUGraz/EvaluationAsset/)**
- **[Sent Data](http://css-kti.tugraz.at/evaluationasset/web/index.html)**
- **[Documentation](http://css-kti.tugraz.at/projects/rage/assets/designdocuments/DesignDocument-EvaluationAsset.pdf)**


Three files from the GitHub (all in the A_Evaluation folder) are required in a game to use the Evaluation Asset: EvaluationAsset.cs, EvaluationAssetFunctionality.cs and EvaluationAssetSettings.cs.

## EvaluationAssetSettings
The information provided in this file needs to be updated to match the game it is being used in. Incorrect details can result in the data not being received or incorrect data being sent, which could result in recording data for the wrong game.

- **PostUrl**: Should be set to "http://css-kti.tugraz.at/evaluationasset/rest/sensordatapost".
- **GameId**: An identifier for the game.
- **GameVersion**: Game Version. 
- **PlayerId**: The Id of the player currently signed in. With SUGAR value should be SUGARManager.CurrentUser?.Name.
- **Language**: Use "Localization.SelectedLanguage.TwoLetterISOLanguageName" to get the code for the currently selected language.

The EvaluationAssetSettings can initialize before CurrentUser is set. As such, a new EvaluationAssetSettings object with accurate details can be created in code and will be used if EvaluationAsset.Instance.Settings is set to equal it.

## EvaluationAsset

### Sending Data

```c#
Instance.sensorData(String gameEvent, String parameter)
```

- **Note:** This method is not asynchronous.
- GameEvent is the event type (GameUsage, Support, Gamification etc).
- Parameter is a string of joined key value pairs, like how parameters are passed in a URL.

# Usage in Sports Team Manager
## TrackerEvaluationEvent
An enum of all the game events types used by the Evaluation Asset.
## TrackerEvaluationKey
An enum of all the parameter keys that are used by the Evaluation Asset.
## Event/Key Matches
Certain keys are expected and can only be used with certain events:

Event | Key Matches
--- | ---
GameUsage | Event
Gamification | Event
Support | Event
UserProfile | Event
GameActivity | Event, GoalOrientation, Tool
GameFlow | PieceType, PieceId, PieceCompleted
AssetActivity | AssetId, Action


## TrackerEventSender
This class manages sending data for the RAGE Analytics component as well as for the Evaluation Asset.

```c#
SendEvaluationEvent(TrackerEvalautionEvent ev, Dictionary<TrackerEvaluationKey, string> parameters)
```

- Events can be passed directly to the EvaluationAsset through the use of this static method.
- Data is only sent if not in editor (aka, builds only) and if GameManagement.PlatformSettings.Rage is true.
- This method checks that the correct TrackerEvaluationKeys have been provided for the provided TrackerEvalautionEvents ev, fulfilling the same check that will be performed by the asset itself later on.
- If valid, the parameter string is created using the parameters provided and SendEvaluationEventAsync() is called.
- SendEvaluationEventAsync is an asynchronous method which calls EvaluationAsset.Instance.sensorData, meaning that the sending of data will not pause gameplay.

### Sending Events 

```c#
SendEvent(TraceEvent trace)
```

Although primaily used for sending RAGE Analytics data, some events also trigger SendEvaluationEvent:

TrackerVerb | Params | Event Called
--- | ---  | ---
Accessed | AccessibleTracker.Accessible.Accessible | Support
Accessed | Not AccessibleTracker.Accessible.Accessible | Game Activity
Skipped | | GameActivity
Selected | | GameActivity
Initialized | | GameUsage
Completed | | GameFlow
Interacted | | GameActivity
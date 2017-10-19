using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PlayGen.RAGE.SportsTeamManager.Simulation;

using UnityEngine;
using UnityEngine.UI;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.BestFit;
using RAGE.Analytics.Formats;

/// <summary>
/// Contains UI logic related to the Post Race pop-up
/// </summary>
public class PostRaceEventUI : ObservableMonoBehaviour
{
	private CanvasGroup _canvasGroup;
	[SerializeField]
	private LearningPillUI _learningPill;
	[SerializeField]
	private GameObject _closeButton;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private PostRacePersonUI[] _postRacePeople;
	private List<string> _lastStates;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
        _canvasGroup = GetComponent<CanvasGroup>();
        //reorder pop-ups and blockers
        _popUpBlocker.transform.SetAsLastSibling();
		transform.parent.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		ResetDisplay();
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
		foreach (PostRacePersonUI person in _postRacePeople)
		{
			person.EnableQuestions();
			DoBestFit();
		}
		if (transform.parent.GetSiblingIndex() == transform.parent.parent.childCount - 1)
		{
			_popUpBlocker.transform.SetAsLastSibling();
            transform.parent.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Reset and populate the pop-up for a new event
	/// </summary>
	public void ResetDisplay()
	{
		_closeButton.SetActive(false);
		var current = GameManagement.PostRaceEvent.CurrentEvent;
		//if there is an event
		if (current != null && current.Count != 0 && current.Count == _postRacePeople.Length && GameManagement.PostRaceEvent.EnableCounter == 0)
		{
			GameManagement.PostRaceEvent.EnableCheck();
			for (int i = 0; i < _postRacePeople.Length; i++)
			{
				_postRacePeople[i].ResetDisplay(current[i]);
			}
			//set alpha to 1 (fully visible)
			GetComponent<CanvasGroup>().alpha = 1;
			_canvasGroup.interactable = true;
			_canvasGroup.blocksRaycasts = true;
			//set current NPC dialogue
			ResetQuestions();
			TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventPopUpOpened", TrackerVerbs.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKeys.EventID.ToString(), current[0].Dialogue.NextState },
			}, AccessibleTracker.Accessible.Screen));
			SUGARManager.GameData.Send("Post Race Event Start", current[0].Dialogue.NextState);
		}
		else
		{
			Hide(string.Empty);
		}
	}

	/// <summary>
	/// Hide this UI
	/// </summary>
	public void Hide(string source)
	{
		_canvasGroup.alpha = 0;
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
		GameManagement.PostRaceEvent.DisableCheck();
		if (!string.IsNullOrEmpty(source))
		{
			TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKeys.TriggerUI.ToString(), source },
				{ TrackerContextKeys.EventID.ToString(), GameManagement.PostRaceEvent.GetEventKey(_lastStates[0]) },
			}, AccessibleTracker.Accessible.Screen));
		}
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	private void ResetQuestions()
	{
		var current = GameManagement.PostRaceEvent.CurrentEvent;
		if (current != null && current.Count != 0 && current.Count == _postRacePeople.Length)
		{
			var replies = GameManagement.PostRaceEvent.GetEventReplies();
			for (int i = 0; i < _postRacePeople.Length; i++)
			{
				_postRacePeople[i].ResetQuestions(current[i], replies[current[i].CrewMember]);
			}
			if (replies.Values.Sum(dos => dos.Count) == 0)
			{
				SetBlockerOnClick();
				SUGARManager.GameData.Send("Post Event Crew Average Mood", GameManagement.PostRaceEvent.GetTeamAverageMood());
				SUGARManager.GameData.Send("Post Event Crew Average Manager Opinion", GameManagement.PostRaceEvent.GetTeamAverageManagerOpinion());
				SUGARManager.GameData.Send("Post Event Crew Average Opinion", GameManagement.PostRaceEvent.GetTeamAverageOpinion());
			}
			else
			{
				_popUpBlocker.onClick.RemoveAllListeners();
			}
			DoBestFit();
		}
	}

	/// <summary>
	/// Set-up the background blocker to allow for closing of the pop-up if the event has finished
	/// </summary>
	public void SetBlockerOnClick()
	{
		if (_postRacePeople.All(prp => prp.ActiveQuestions() == false))
		{
			_closeButton.SetActive(true);
			_popUpBlocker.onClick.AddListener(GetLearningPill);
			_popUpBlocker.onClick.AddListener(delegate { Hide(TrackerTriggerSources.PopUpBlocker.ToString()); });
			_popUpBlocker.onClick.AddListener(GameManagement.PostRaceEvent.GetEvent);
			var teamSelection = (TeamSelectionUI)FindObjectOfType(typeof(TeamSelectionUI));
			_popUpBlocker.onClick.AddListener(teamSelection.ResetCrew);
			_popUpBlocker.onClick.AddListener(SendLearningPill);
			ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Get the learning pill that will be displayed due to this event
	/// </summary>
	public void GetLearningPill()
	{
		_lastStates = _postRacePeople.Select(t => t.LastState).ToList();
	}

	/// <summary>
	/// Set-up the learning pill for this event
	/// </summary>
	public void SendLearningPill()
	{
		_learningPill.SetHelp(_lastStates);
	}

	/// <summary>
	/// Triggered by button. Send the selected dialogue to the character
	/// </summary>
	public void SendReply(PostRaceEventState reply)
	{
		var responses = GameManagement.PostRaceEvent.AddReply(reply);
		if (responses != null)
		{
			foreach (PostRacePersonUI person in _postRacePeople)
			{
				foreach (var res in responses)
				{
					if (res.Key == person.CurrentCrewMember)
					{
						person.UpdateDialogue(res.Value.Dialogue, res.Value.Subjects);
					}
				}
			}
			ResetQuestions();
		}
	}

	private void OnLanguageChange()
	{
		var current = GameManagement.PostRaceEvent.CurrentEvent;
		//if there is an event
		if (current != null && current.Count != 0 && current.Count == _postRacePeople.Length)
		{
			for (int i = 0; i < _postRacePeople.Length; i++)
			{
				_postRacePeople[i].ResetDisplay(current[i]);
			}
		}
		ResetQuestions();
	}

	private void DoBestFit()
	{
		var peopleLayout = _postRacePeople.Select(p => p.GetComponentInChildren<LayoutGroup>()).ToList();
		peopleLayout.ForEach(p => p.GetComponentsInChildren<Text>().ToList().ForEach(t => t.fontSize = 15));
		peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
		Invoke("BestFitCheckDelay", 0f);
	}

	private void BestFitCheckDelay()
	{
		var peopleLayout = _postRacePeople.Select(p => p.GetComponentInChildren<LayoutGroup>()).ToList();
		peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
		Invoke("BestFitCheck", 0f);
	}

	private void BestFitCheck()
	{
		var peopleLayout = _postRacePeople.Select(p => p.GetComponentInChildren<LayoutGroup>()).ToList();
		if (peopleLayout.Any(p => ((RectTransform)p.transform).sizeDelta.y > 0))
		{
			peopleLayout.ForEach(p => p.GetComponentsInChildren<Text>().ToList().ForEach(t => t.fontSize--));
			peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
			Invoke("BestFitCheckDelay", 0f);
		}
		peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
	}
}
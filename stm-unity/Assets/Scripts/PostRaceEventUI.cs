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
/// Contains all UI logic related to the Post Race pop-up
/// </summary>
public class PostRaceEventUI : ObservableMonoBehaviour
{
	private PostRaceEvent _postRaceEvent;
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
		if (_postRaceEvent == null)
		{
			_postRaceEvent = GetComponentInParent<PostRaceEvent>();
			_canvasGroup = GetComponent<CanvasGroup>();
		}
		//reorder pop-ups and blockers
		_popUpBlocker.transform.SetAsLastSibling();
		_postRaceEvent.transform.SetAsLastSibling();
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
		if (_postRaceEvent.transform.GetSiblingIndex() == _postRaceEvent.transform.parent.childCount - 1)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			_postRaceEvent.transform.SetAsLastSibling();
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
		var current = _postRaceEvent.CurrentEvent;
		//if there is an event
		if (current != null && current.Count != 0 && current.Count == _postRacePeople.Length && _postRaceEvent.EnableCounter == 0)
		{
			_postRaceEvent.EnableCheck();
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
			ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "PostRaceEvent", "PostRaceEventOpen", AlternativeTracker.Alternative.Dialog));
			SUGARManager.GameData.Send("Post Race Event Start", current[0].Dialogue.NextState);
		}
		else
		{
			Hide(true);
		}
	}

	public void Hide(bool autoHide = false)
	{
		_canvasGroup.alpha = 0;
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
		_postRaceEvent.DisableCheck();
		if (!autoHide)
		{
			ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "PostRaceEvent", "PostRaceEventClosed", AlternativeTracker.Alternative.Dialog));
		}
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	private void ResetQuestions()
	{
		var current = _postRaceEvent.CurrentEvent;
		if (current != null && current.Count != 0 && current.Count == _postRacePeople.Length)
		{
			var replies = _postRaceEvent.GetEventReplies();
			for (int i = 0; i < _postRacePeople.Length; i++)
			{
				_postRacePeople[i].ResetQuestions(current[i], replies[current[i].CrewMember]);
			}
			if (replies.Values.Sum(dos => dos.Count) == 0)
			{
				SetBlockerOnClick();
				SUGARManager.GameData.Send("Post Event Crew Average Mood", _postRaceEvent.GetTeamAverageMood());
				SUGARManager.GameData.Send("Post Event Crew Average Manager Opinion", _postRaceEvent.GetTeamAverageManagerOpinion());
				SUGARManager.GameData.Send("Post Event Crew Average Opinion", _postRaceEvent.GetTeamAverageOpinion());
			}
			else
			{
				_popUpBlocker.onClick.RemoveAllListeners();
			}
			DoBestFit();
		}
	}

	public void SetBlockerOnClick()
	{
		if (_postRacePeople.All(prp => prp.ActiveQuestions() == false))
		{
			_closeButton.SetActive(true);
			_popUpBlocker.onClick.AddListener(GetLearningPill);
			_popUpBlocker.onClick.AddListener(delegate { Hide(); });
			_popUpBlocker.onClick.AddListener(_postRaceEvent.GetEvent);
			var teamSelection = (TeamSelectionUI)FindObjectOfType(typeof(TeamSelectionUI));
			_popUpBlocker.onClick.AddListener(teamSelection.ResetCrew);
			_popUpBlocker.onClick.AddListener(SendLearningPill);
			ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	public void GetLearningPill()
	{
		_lastStates = _postRacePeople.Select(t => t.LastState).ToList();
	}

	public void SendLearningPill()
	{
		_learningPill.SetHelp(_lastStates);
	}

	/// <summary>
	/// Triggered by button. Send the selected dialogue to the character
	/// </summary>
	public void SendReply(PostRaceEventState reply)
	{
		var responses = _postRaceEvent.AddReply(reply);
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
		var current = _postRaceEvent.CurrentEvent;
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
using System.Collections.Generic;
using System.Linq;
using IntegratedAuthoringTool.DTOs;

using PlayGen.RAGE.SportsTeamManager.Simulation;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains all UI logic related to the Post Race pop-up
/// </summary>
public class PostRaceEventUI : MonoBehaviour
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

	private void OnEnable()
	{
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
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		_canvasGroup.alpha = 0;
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
		_postRaceEvent.DisableCheck();
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	private void ResetQuestions()
	{
		var current = _postRaceEvent.CurrentEvent;
		var replies = _postRaceEvent.GetEventReplies();
		for (int i = 0; i < _postRacePeople.Length; i++)
		{
			_postRacePeople[i].ResetQuestions(current[i], replies[current[i].Key]);
		}
		if (replies.Values.Sum(dos => dos.Count) == 0)
		{
			_closeButton.SetActive(true);
			SetBlockerOnClick();
		} 
		else
		{
			_popUpBlocker.onClick.RemoveAllListeners();
		}
	}

	public void SetBlockerOnClick()
	{
		if (_postRacePeople.All(prp => prp.ActiveQuestions() == false))
		{
			_popUpBlocker.onClick.AddListener(Hide);
			_popUpBlocker.onClick.AddListener(_postRaceEvent.GetEvent);
			var teamSelection = FindObjectOfType(typeof(TeamSelectionUI)) as TeamSelectionUI;
			_popUpBlocker.onClick.AddListener(teamSelection.ResetCrew);
			_popUpBlocker.onClick.AddListener(GetLearningPill);
		}
	}

	public void GetLearningPill()
	{
		var lastStates = new List<string>();
		for (int i = 0; i < _postRacePeople.Length; i++)
		{
			lastStates.Add(_postRacePeople[i].LastState);
		}
		_learningPill.SetHelp(lastStates);
	}

	/// <summary>
	/// Triggered by button. Send the selected dialogue to the character
	/// </summary>
	public void SendReply(CrewMember cm, DialogueStateActionDTO reply)
	{
		Tracker.T.alternative.Selected("Post Race Event", reply.NextState, AlternativeTracker.Alternative.Dialog);
		var responses = _postRaceEvent.AddReply(cm, reply);
		if (responses != null)
		{
			for (int i = 0; i < _postRacePeople.Length; i++)
			{
				foreach (var res in responses)
				{
					if (res.Key == _postRacePeople[i].CurrentCrewMember)
					{
						_postRacePeople[i].UpdateDialogue(res.Value);
					}
				}
			}
			ResetQuestions();
		}
	}
}
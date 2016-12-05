using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using IntegratedAuthoringTool.DTOs;

using SUGAR.Unity;

/// <summary>
/// Contains all logic to communicate between PostRaceEventUI and GameManager
/// </summary>
public class PostRaceEvent : ObservableMonoBehaviour
{
	private GameManager _gameManager;
	private List<KeyValuePair<CrewMember, DialogueStateActionDTO>> _currentEvent;
	public List<KeyValuePair<CrewMember, DialogueStateActionDTO>> CurrentEvent
	{
		get { return _currentEvent; }
	}
	private Dictionary<CrewMember, DialogueStateActionDTO> _selectedResponses;
	private int _disableCounter;
	private int _enableCounter;
	public int EnableCounter
	{
		get { return _enableCounter; }
	}

	/// <summary>
	/// Trigger chance for an event, display pop-up is one is returned
	/// </summary>
	public void GetEvent()
	{
		_disableCounter = 0;
		_enableCounter = 0;
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		if (_gameManager.EventController.PostRaceEvents.Count > 0)
		{
			_currentEvent = _gameManager.EventController.PostRaceEvents.First();
		}
		gameObject.SetActive(true);
	}

	public void EnableCheck()
	{
		_enableCounter++;
	}

	public void DisableCheck()
	{
		_disableCounter++;
		if (_disableCounter == transform.childCount)
		{
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Get player dialogue choices for the current situation
	/// </summary>
	public Dictionary<CrewMember, List<DialogueStateActionDTO>> GetEventReplies()
	{
		var replies = _gameManager.EventController.GetEventDialogues(_gameManager.Team.Manager);
		//if there are no replies, reset the current event
		if (replies.Values.Sum(dos => dos.Count) == 0)
		{
			_currentEvent = null;
		}
		else
		{
			foreach (var reply in replies)
			{
				if (reply.Value == null || reply.Value.Count == 0)
				{
					AddReply(reply.Key, null);
				}
			}
		}
		//if there is another event that can be set as current, do so
		if (_currentEvent == null && _gameManager.EventController.PostRaceEvents.Count > 0)
		{
			_currentEvent = _gameManager.EventController.PostRaceEvents.First();
		}
		return replies;
	}

	public Dictionary<CrewMember, DialogueStateActionDTO> AddReply(CrewMember cm, DialogueStateActionDTO response)
	{
		if (_selectedResponses == null)
		{
			_selectedResponses = new Dictionary<CrewMember, DialogueStateActionDTO>();
		}
		if (_selectedResponses.ContainsKey(cm))
		{
			_selectedResponses[cm] = response;
		}
		else
		{
			_selectedResponses.Add(cm, response);
		}
		if (_currentEvent != null && _selectedResponses.Count == _currentEvent.Count)
		{
			foreach (var res in _selectedResponses.Values)
			{
				ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, res.Utterance, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "PostRaceEvent", res.NextState, AlternativeTracker.Alternative.Dialog));
				//SUGARManager.GameData.Send("Post Race Event Reply", res.NextState);
			}
			float beforeValues = GetTeamAverageMood() + GetTeamAverageManagerOpinion() + GetTeamAverageOpinion();
			var replies = SendReply();
			float afterValues = GetTeamAverageMood() + GetTeamAverageManagerOpinion() + GetTeamAverageOpinion();
			if (afterValues > beforeValues)
			{
				SUGARManager.GameData.Send("Post Race Event Positive Outcome", true);
			}
			else if (afterValues < beforeValues)
			{
				SUGARManager.GameData.Send("Post Race Event Positive Outcome", false);
			}
			return replies;
		}
		return null;
	}

	/// <summary>
	/// Send player dialogue to the NPC involved in the event, get their reply in response
	/// </summary>
	private Dictionary<CrewMember, DialogueStateActionDTO> SendReply()
	{
		var replies = _gameManager.SendPostRaceEvent(_selectedResponses);
		_selectedResponses = null;
		return replies;
	}

	/// <summary>
	/// Get the average team mood
	/// </summary>
	public float GetTeamAverageMood()
	{
		return _gameManager.Team.AverageTeamMood();
	}

	/// <summary>
	/// Get the average team manager opinion
	/// </summary>
	public float GetTeamAverageManagerOpinion()
	{
		return _gameManager.Team.AverageTeamManagerOpinion();
	}

	/// <summary>
	/// Get the average team opinion
	/// </summary>
	public float GetTeamAverageOpinion()
	{
		return _gameManager.Team.AverageTeamOpinion();
	}
}

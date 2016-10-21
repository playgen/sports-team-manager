using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;

using IntegratedAuthoringTool.DTOs;

/// <summary>
/// Contains all logic to communicate between PostRaceEventUI and GameManager
/// </summary>
public class PostRaceEvent : MonoBehaviour
{
	private GameManager _gameManager;
	private List<KeyValuePair<CrewMember, DialogueStateActionDTO>> _currentEvent;
	public List<KeyValuePair<CrewMember, DialogueStateActionDTO>> CurrentEvent
	{
		get { return _currentEvent; }
	}
	private Dictionary<CrewMember, DialogueStateActionDTO> _selectedResponses;

	/// <summary>
	/// Trigger chance for an event, display pop-up is one is returned
	/// </summary>
	public void GetEvent()
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		if (_gameManager.EventController.PostRaceEvents.Count > 0)
		{
			_currentEvent = _gameManager.EventController.PostRaceEvents.First();
			gameObject.SetActive(true);
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
			return SendReply();
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
}

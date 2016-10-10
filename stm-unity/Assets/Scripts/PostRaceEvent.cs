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
	private List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> _currentEvents;
	private KeyValuePair<List<CrewMember>, DialogueStateActionDTO> _currentEvent;
	public KeyValuePair<List<CrewMember>, DialogueStateActionDTO> CurrentEvent
	{
		get { return _currentEvent; }
	}

	/// <summary>
	/// Trigger chance for an event, display pop-up is one is returned
	/// </summary>
	public void GetEvent()
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		_currentEvents = _gameManager.SelectPostRaceEvents();
		if (_currentEvents.Count > 0)
		{
			SetEvent();
			gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// Get player dialogue choices for the current situation
	/// </summary>
	public DialogueStateActionDTO[] GetEventReplies()
	{
		var replies = _gameManager.EventController.GetEvents();
		if (replies == null || replies.Length == 0)
		{
			_currentEvent = new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(null, null);
		}
		if (_currentEvent.Key == null && _currentEvents.Count > 0)
		{
			SetEvent();
		}
		return replies;
	}

	/// <summary>
	/// Set the event that the player is currently progressing through
	/// </summary>
	private void SetEvent()
	{
		_currentEvent = _currentEvents.First();
		_currentEvents.Remove(_currentEvent);
		_gameManager.EventController.SetPlayerState(_currentEvent.Value);
	}

	/// <summary>
	/// Send player dialogue to the NPC involved in the event, get their reply in response
	/// </summary>
	public Dictionary<CrewMember, string> SendReply(DialogueStateActionDTO selectedEvent)
	{
		var replies = _gameManager.SendPostRaceEvent(selectedEvent, _currentEvent.Key);
		return replies;
	}
}

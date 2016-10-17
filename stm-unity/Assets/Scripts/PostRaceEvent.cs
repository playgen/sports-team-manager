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
	private KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>> _currentEvent;
	public KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>> CurrentEvent
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
		if (_gameManager.EventController.PostRaceEvents.Count > 0)
		{
			_currentEvent = _gameManager.EventController.PostRaceEvents.First();
			gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// Get player dialogue choices for the current situation
	/// </summary>
	public DialogueStateActionDTO[] GetEventReplies()
	{
		var replies = _gameManager.EventController.GetEventDialogues(_gameManager.Team.Manager);
		//if there are no replies, reset the current event
		if (replies == null || replies.Length == 0)
		{
			_currentEvent = new KeyValuePair<List<CrewMember>, List<DialogueStateActionDTO>>(null, null);
		}
		//if there is another event that can be set as current, do so
		if (_currentEvent.Key == null && _gameManager.EventController.PostRaceEvents.Count > 0)
		{
			_currentEvent = _gameManager.EventController.PostRaceEvents.First();
		}
		return replies;
	}

	/// <summary>
	/// Send player dialogue to the NPC involved in the event, get their reply in response
	/// </summary>
	public Dictionary<CrewMember, DialogueStateActionDTO> SendReply(DialogueStateActionDTO selectedEvent)
	{
		var replies = _gameManager.SendPostRaceEvent(selectedEvent, _currentEvent.Key);
		return replies;
	}
}

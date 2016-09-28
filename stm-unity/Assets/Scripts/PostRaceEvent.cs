using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using IntegratedAuthoringTool.DTOs;

/// <summary>
/// Contains all logic to communicate between PostRaceEventUI and GameManager
/// </summary>
public class PostRaceEvent : MonoBehaviour
{
	private GameManager _gameManager;
	private KeyValuePair<List<CrewMember>, string> _currentEvent;
	public KeyValuePair<List<CrewMember>, string> CurrentEvent
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
		_currentEvent = _gameManager.SelectPostRaceEvent();
		if (_currentEvent.Key != null)
		{
			gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// Get player dialogue choices for the current situation
	/// </summary>
	public DialogueStateActionDTO[] GetEventReplies()
	{
		return _gameManager.GetPostRaceEvents();
	}

	/// <summary>
	/// Send player dialogue to the NPC involved in the event, get their reply in response
	/// </summary>
	public Dictionary<CrewMember, string> SendReply(DialogueStateActionDTO selectedEvent)
	{
		return _gameManager.SendPostRaceEvent(selectedEvent, _currentEvent.Key);
	}
}

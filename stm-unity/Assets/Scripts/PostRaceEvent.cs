using System;

using UnityEngine;
using System.Collections;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

using IntegratedAuthoringTool.DTOs;

public class PostRaceEvent : MonoBehaviour
{
	private GameManager _gameManager;
	private KeyValuePair<List<CrewMember>, string> _postRace;

	public void GetEvent()
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		_postRace = _gameManager.SelectPostRaceEvent();
		if (_postRace.Key != null)
		{
			gameObject.SetActive(true);
		}
	}

	public Boat GetBoat()
	{
		return _gameManager.Boat;
	}

	public KeyValuePair<List<CrewMember>, string> GetCurrentEvent()
	{
		return _postRace;
	}

	public DialogueStateActionDTO[] GetEventReplies()
	{
		return _gameManager.GetPostRaceEvents();
	}

	public Dictionary<CrewMember, string> SendReply(DialogueStateActionDTO selectedEvent)
	{
		return _gameManager.SendPostRaceEvent(selectedEvent, _postRace.Key);
	}
}

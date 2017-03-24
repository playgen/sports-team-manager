using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PlayGen.SUGAR.Unity;

using RAGE.Analytics.Formats;

/// <summary>
/// Contains all logic to communicate between PostRaceEventUI and GameManager
/// </summary>
public class PostRaceEvent : ObservableMonoBehaviour
{
	private GameManager _gameManager;
	private List<PostRaceEventState> _currentEvent;
	public List<PostRaceEventState> CurrentEvent
	{
		get { return _currentEvent; }
	}
	private Dictionary<CrewMember, PostRaceEventState> _selectedResponses;
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
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
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
	public Dictionary<CrewMember, List<PostRaceEventState>> GetEventReplies()
	{
		var replies = _gameManager.EventController.GetEventDialogues(_gameManager.Team.Manager);
		var replyDict = new Dictionary<CrewMember, List<PostRaceEventState>>();
		foreach (var ev in _currentEvent)
		{
			if (!replyDict.ContainsKey(ev.CrewMember))
			{
				replyDict.Add(ev.CrewMember, new List<PostRaceEventState>());
			}
		}
		//if there are no replies, reset the current event
		if (replies.Count == 0)
		{
			_currentEvent = null;
		}
		else
		{
			foreach (var reply in replies)
			{
				replyDict[reply.CrewMember].Add(reply);
			}
			foreach (var reply in replyDict)
			{
				if (reply.Value.Count == 0)
				{
					AddReply(new PostRaceEventState(reply.Key, null));
				}
			}
		}
		//if there is another event that can be set as current, do so
		if (_currentEvent == null && _gameManager.EventController.PostRaceEvents.Count > 0)
		{
			_currentEvent = _gameManager.EventController.PostRaceEvents.First();
		}
		return replyDict;
	}

	public Dictionary<CrewMember, PostRaceEventState> AddReply(PostRaceEventState response)
	{
		if (_selectedResponses == null)
		{
			_selectedResponses = new Dictionary<CrewMember, PostRaceEventState>();
		}
		if (_selectedResponses.ContainsKey(response.CrewMember))
		{
			_selectedResponses[response.CrewMember] = response;
		}
		else
		{
			_selectedResponses.Add(response.CrewMember, response);
		}
		if (_currentEvent != null && _selectedResponses.Count == _currentEvent.Count)
		{
			foreach (var res in _selectedResponses.Values)
			{
				TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventDialogueSelected", new Dictionary<string, string>
				{
					{ TrackerContextKeys.DialogueID.ToString(), res.Dialogue.NextState },
					{ TrackerContextKeys.EventID.ToString(), GetEventKey(res.Dialogue.NextState) },
				}));
				SUGARManager.GameData.Send("Post Race Event Reply", res.Dialogue.NextState);
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
			var replyDict = new Dictionary<CrewMember, PostRaceEventState>();
			foreach (var ev in _currentEvent)
			{
				if (!replyDict.ContainsKey(ev.CrewMember))
				{
					replyDict.Add(ev.CrewMember, null);
				}
			}
			foreach (var reply in replies)
			{
				replyDict[reply.CrewMember] = reply;
			}
			return replyDict;
		}
		return null;
	}

	/// <summary>
	/// Send player dialogue to the NPC involved in the event, get their reply in response
	/// </summary>
	private List<PostRaceEventState> SendReply()
	{
		var replies = _gameManager.SendPostRaceEvent(_selectedResponses.Values.ToList());
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

	public string GetEventKey (string state)
	{
		return _gameManager.GetPostRaceEventKeys().First(state.StartsWith);
	}
}

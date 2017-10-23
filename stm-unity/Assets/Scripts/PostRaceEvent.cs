using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Unity;

using RAGE.Analytics.Formats;
using UnityEngine;

/// <summary>
/// Contains all logic to communicate between PostRaceEventUI and GameManager
/// </summary>
public class PostRaceEvent
{
	private Dictionary<CrewMember, PostRaceEventState> _selectedResponses;
	private int _disableCounter;
	private int _enableCounter;
	public int EnableCounter
	{
		get { return _enableCounter; }
	}
	private GameObject _gameObject
	{
		get { return GameObject.Find("Canvas/Team Management/Pop-up Bounds/Event Pop-Up"); }
	}

	/// <summary>
	/// Display pop-up if there is an event to show
	/// </summary>
	public void GetEvent()
	{
		_disableCounter = 0;
		_enableCounter = 0;
		_gameObject.SetActive(true);
	}

	/// <summary>
	/// Increment _enableCounter by one
	/// </summary>
	public void EnableCheck()
	{
		_enableCounter++;
	}

	/// <summary>
	/// Increment _disableCounter by one. If new total matches childCount, disable gameObject
	/// </summary>
	public void DisableCheck()
	{
		_disableCounter++;
		if (_disableCounter == _gameObject.transform.childCount)
		{
			_gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Get player dialogue choices for the current situation
	/// </summary>
	public Dictionary<CrewMember, List<PostRaceEventState>> GetEventReplies()
	{
		var replies = GameManagement.GameManager.EventController.GetEventDialogues(GameManagement.Manager);
		var replyDict = new Dictionary<CrewMember, List<PostRaceEventState>>();
		foreach (var ev in GameManagement.CurrentEvent)
		{
			if (!replyDict.ContainsKey(ev.CrewMember))
			{
				replyDict.Add(ev.CrewMember, new List<PostRaceEventState>());
			}
		}
		//if there are no replies, reset the current event
		if (replies.Count != 0)
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
		return replyDict;
	}

	/// <summary>
	/// Add a reply to the dictionary of selected responses. If the number of responses matches the number expected, get replies from crew members.
	/// </summary>
	public Dictionary<CrewMember, PostRaceEventState> AddReply(PostRaceEventState response)
	{
		if (_selectedResponses == null)
		{
			_selectedResponses = new Dictionary<CrewMember, PostRaceEventState>();
		}
		//overwrite response if one had already been given for this crew member.
		if (_selectedResponses.ContainsKey(response.CrewMember))
		{
			_selectedResponses[response.CrewMember] = response;
		}
		else
		{
			_selectedResponses.Add(response.CrewMember, response);
		}
		if (GameManagement.CurrentEvent != null && _selectedResponses.Count == GameManagement.CurrentEvent.Count)
		{
			foreach (var res in _selectedResponses.Values)
			{
				TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventDialogueSelected", TrackerVerbs.Selected, new Dictionary<string, string>
				{
					{ TrackerContextKeys.DialogueID.ToString(), res.Dialogue.NextState },
					{ TrackerContextKeys.DialogueStyle.ToString(), res.Dialogue.Meaning[0] },
					{ TrackerContextKeys.EventID.ToString(), GetEventKey(res.Dialogue.NextState) },
				}, res.Dialogue.NextState, AlternativeTracker.Alternative.Dialog));
				SUGARManager.GameData.Send("Post Race Event Reply", res.Dialogue.NextState);
			}
			var beforeValues = GetTeamAverageMood() + GetTeamAverageManagerOpinion() + GetTeamAverageOpinion();
			foreach (var res in _selectedResponses)
			{
				ReactionSoundControl.PlaySound(res.Value.Dialogue.Meaning[0], res.Key.Gender == "Male", res.Key.Avatar.Height, res.Key.Avatar.Weight);
			}
			var replies = SendReply();
			var afterValues = GetTeamAverageMood() + GetTeamAverageManagerOpinion() + GetTeamAverageOpinion();
			if (afterValues > beforeValues)
			{
				SUGARManager.GameData.Send("Post Race Event Positive Outcome", true);
			}
			else if (afterValues < beforeValues)
			{
				SUGARManager.GameData.Send("Post Race Event Positive Outcome", false);
			}
			var replyDict = new Dictionary<CrewMember, PostRaceEventState>();
			foreach (var ev in GameManagement.CurrentEvent)
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
		var replies = GameManagement.GameManager.SendPostRaceEvent(_selectedResponses.Values.ToList());
		_selectedResponses = null;
		return replies;
	}

	/// <summary>
	/// Get the average team mood
	/// </summary>
	public float GetTeamAverageMood()
	{
		return GameManagement.Team.AverageTeamMood();
	}

	/// <summary>
	/// Get the average team manager opinion
	/// </summary>
	public float GetTeamAverageManagerOpinion()
	{
		return GameManagement.Team.AverageTeamManagerOpinion();
	}

	/// <summary>
	/// Get the average team opinion
	/// </summary>
	public float GetTeamAverageOpinion()
	{
		return GameManagement.Team.AverageTeamOpinion();
	}

	/// <summary>
	/// Get the event key for the current event
	/// </summary>
	public string GetEventKey (string state)
	{
		return GameManagement.GameManager.GetPostRaceEventKeys().First(state.StartsWith);
	}
}

﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.Text;
using TrackerAssetPackage;

/// <summary>
/// Contains UI logic related to the Post Race pop-up
/// </summary>
public class PostRaceEventUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _closeButton;
	private PostRacePersonUI[] _postRacePeople;
	private Dictionary<CrewMember, PostRaceEventState> _selectedResponses;
	private List<string> _lastStates;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		transform.parent.EnableBlocker();
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
		//if the close button is active, hide all dialogue options (helps with best fit sometimes) and disable the blocker if this pop-up is actually using it
		if (_closeButton.activeSelf)
		{
			foreach (var person in _postRacePeople)
			{
				person.EnableQuestions();
				DoBestFit();
			}
			if (transform.parent.GetSiblingIndex() == transform.parent.parent.childCount - 1)
			{
				UIManagement.DisableBlocker();
			}
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Show the post-race event pop-up if there is currently an event with the same number of characters involved as this pop-up can show
	/// </summary>
	public void Display()
	{
		if (_postRacePeople == null)
		{
			_postRacePeople = GetComponentsInChildren<PostRacePersonUI>(true);
		}
		if (GameManagement.CurrentEventCount == _postRacePeople.Length)
		{
			gameObject.Active(true);
			ResetDisplay();
		}
	}

	/// <summary>
	/// Reset and populate the pop-up for a new event
	/// </summary>
	public void ResetDisplay()
	{
		_closeButton.Active(false);
		//set current NPC dialogue and possible dialogue options
		for (var i = 0; i < _postRacePeople.Length; i++)
		{
			_postRacePeople[i].ResetDisplay(GameManagement.CurrentEvent[i]);
		}
		ResetQuestions();
		SUGARManager.GameData.Send("Post Race Event Start", GameManagement.CurrentEvent[0].Dialogue.NextState);
	}

	/// <summary>
	/// Close this UI
	/// </summary>
	public void Close(string source)
	{
		gameObject.Active(false);
		if (!string.IsNullOrEmpty(source))
		{
			TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.TriggerUI, source },
				{ TrackerContextKey.EventID, _lastStates[0].EventKeys() }
			}, AccessibleTracker.Accessible.Screen));
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	private void ResetQuestions()
	{
		//only do this if the current event if for this pop-up
		if (GameManagement.CurrentEventCount == _postRacePeople.Length)
		{
			var replies = GameManagement.EventController.GetEventDialogues(GameManagement.Manager);
			var replyDict = new Dictionary<CrewMember, List<PostRaceEventState>>();
			//if there is an ongoing event, add to the reply dictionary for each crew member involved
			if (GameManagement.OngoingEvent)
			{
				foreach (var ev in GameManagement.CurrentEvent)
				{
					if (!replyDict.ContainsKey(ev.CrewMember))
					{
						replyDict.Add(ev.CrewMember, new List<PostRaceEventState>());
					}
				}
			}
			//add all possible dialogue options to reply dictionary
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
			//reset dialogue option UI
			if (GameManagement.OngoingEvent)
			{
				for (var i = 0; i < _postRacePeople.Length; i++)
				{
					_postRacePeople[i].ResetQuestions(GameManagement.CurrentEvent[i], replyDict[GameManagement.CurrentEvent[i].CrewMember]);
				}
			}
			else
			{
				foreach (var p in _postRacePeople)
				{
					p.ResetQuestions(new PostRaceEventState(p.CurrentCrewMember, null), new List<PostRaceEventState>());
				}
			}
			//set the blocker to be clickable if there are no dialogue options (meaning the event has finished)
			if (replyDict.Values.Sum(dos => dos.Count) == 0)
			{
				SetBlockerOnClick();
				SUGARManager.GameData.Send("Post Event Crew Average Mood", GameManagement.AverageTeamMood);
				SUGARManager.GameData.Send("Post Event Crew Average Manager Opinion", GameManagement.AverageTeamManagerOpinion);
				SUGARManager.GameData.Send("Post Event Crew Average Opinion", GameManagement.AverageTeamOpinion);
			}
			else
			{
				UIManagement.Blocker.onClick.RemoveAllListeners();
			}
			DoBestFit();
		}
	}

	/// <summary>
	/// Triggered by button. Send the selected dialogue to the character
	/// </summary>
	public void SendReply(PostRaceEventState reply)
	{
		var responses = AddReply(reply);
		if (responses != null)
		{
			foreach (var person in _postRacePeople)
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

	/// <summary>
	/// Add a reply to the dictionary of selected responses. If the number of responses matches the number expected, get replies from crew members.
	/// </summary>
	public Dictionary<CrewMember, PostRaceEventState> AddReply(PostRaceEventState response)
	{
		//create dictionary if null
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
		if (_selectedResponses.Count == GameManagement.CurrentEventCount)
		{
			foreach (var res in _selectedResponses.Values)
			{
				TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventDialogueSelected", TrackerAsset.Verb.Selected, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.DialogueID, res.Dialogue.NextState },
					{ TrackerContextKey.DialogueStyle, res.Dialogue.Meaning.Split('_').First(sp => !string.IsNullOrEmpty(sp)) },
					{ TrackerContextKey.EventID, res.Dialogue.NextState.EventKeys() }
				}, res.Dialogue.NextState, AlternativeTracker.Alternative.Dialog));
				SUGARManager.GameData.Send("Post Race Event Reply", res.Dialogue.NextState);
			}
			var beforeValues = GameManagement.AverageTeamMood + GameManagement.AverageTeamManagerOpinion + GameManagement.AverageTeamOpinion;
			//send chosen dialogue and get response
			var replies = GameManagement.GameManager.SendPostRaceEvent(_selectedResponses.Values.ToList());
			_selectedResponses = null;
			var afterValues = GameManagement.AverageTeamMood + GameManagement.AverageTeamManagerOpinion + GameManagement.AverageTeamOpinion;
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
	/// Set-up the background blocker to allow for closing of the pop-up if the event has finished
	/// </summary>
	public void SetBlockerOnClick()
	{
		if (_postRacePeople.All(prp => prp.ActiveQuestions() == false))
		{
			_closeButton.Active(true);
			transform.parent.EnableBlocker(GetLearningPill, () => Close(TrackerTriggerSource.PopUpBlocker.ToString()), UIManagement.TeamSelection.ResetCrew, SendLearningPill);
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Get the learning pill that will be displayed due to this event
	/// </summary>
	public void GetLearningPill()
	{
		_lastStates = _postRacePeople.Select(t => t.LastState).ToList();
	}

	/// <summary>
	/// Set-up the learning pill for this event
	/// </summary>
	public void SendLearningPill()
	{
		UIManagement.LearningPill.SetHelp(_lastStates);
	}

	private void OnLanguageChange()
	{
		//if there is an event
		if (GameManagement.CurrentEventCount == _postRacePeople.Length)
		{
			for (var i = 0; i < _postRacePeople.Length; i++)
			{
				_postRacePeople[i].ResetDisplay(GameManagement.CurrentEvent[i]);
			}
		}
		ResetQuestions();
	}

	private void DoBestFit()
	{
		//best fit called even though font size is kept the same as this forces a redraw of the layout groups, which includes the dialogue buttons
		var peopleLayout = _postRacePeople.Select(p => p.GetComponentInChildren<LayoutGroup>()).ToList();
		peopleLayout.ForEach(p => p.BestFit());
		peopleLayout.ForEach(p => p.GetComponentsInChildren<Text>().ToList().ForEach(t => t.fontSize = 15));
	}
}
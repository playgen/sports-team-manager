using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Localization;
using PlayGen.Unity.Utilities.BestFit;
using RAGE.Analytics.Formats;

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
		_closeButton.SetActive(false);
		if (_postRacePeople == null)
		{
			_postRacePeople = GetComponentsInChildren<PostRacePersonUI>(true);
		}
		if (GameManagement.CurrentEvent != null && GameManagement.CurrentEvent.Count == _postRacePeople.Length)
		{
			Localization.LanguageChange += OnLanguageChange;
			BestFit.ResolutionChange += DoBestFit;
			//reorder pop-ups and blockers
			transform.parent.EnableBlocker();
			ResetDisplay();
		}
		else
		{
			Close(string.Empty);
		}
	}

	private void OnDisable()
	{
		if (_closeButton.activeSelf)
		{
			Localization.LanguageChange -= OnLanguageChange;
			BestFit.ResolutionChange -= DoBestFit;
			foreach (var person in _postRacePeople)
			{
				person.EnableQuestions();
				DoBestFit();
			}
			if (transform.parent.GetSiblingIndex() == transform.parent.parent.childCount - 1)
			{
				UIManagement.Blocker.gameObject.SetActive(false);
			}
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Reset and populate the pop-up for a new event
	/// </summary>
	public void ResetDisplay()
	{
		_closeButton.SetActive(false);
		for (var i = 0; i < _postRacePeople.Length; i++)
		{
			_postRacePeople[i].ResetDisplay(GameManagement.CurrentEvent[i]);
		}
		//set current NPC dialogue
		ResetQuestions();
		SUGARManager.GameData.Send("Post Race Event Start", GameManagement.CurrentEvent[0].Dialogue.NextState);
	}

	/// <summary>
	/// Close this UI
	/// </summary>
	public void Close(string source)
	{
		gameObject.SetActive(false);
		if (!string.IsNullOrEmpty(source))
		{
			TrackerEventSender.SendEvent(new TraceEvent("PostRaceEventPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKeys.TriggerUI.ToString(), source },
				{ TrackerContextKeys.EventID.ToString(), GameManagement.GameManager.GetPostRaceEventKeys().First(_lastStates[0].StartsWith) },
			}, AccessibleTracker.Accessible.Screen));
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	private void ResetQuestions()
	{
		if (GameManagement.CurrentEvent != null && GameManagement.CurrentEvent.Count == _postRacePeople.Length)
		{
			var replies = GameManagement.GameManager.EventController.GetEventDialogues(GameManagement.Manager);
			var replyDict = new Dictionary<CrewMember, List<PostRaceEventState>>();
			if (GameManagement.CurrentEvent != null)
			{
				foreach (var ev in GameManagement.CurrentEvent)
				{
					if (!replyDict.ContainsKey(ev.CrewMember))
					{
						replyDict.Add(ev.CrewMember, new List<PostRaceEventState>());
					}
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
			if (GameManagement.CurrentEvent != null)
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
			if (replyDict.Values.Sum(dos => dos.Count) == 0)
			{
				SetBlockerOnClick();
				SUGARManager.GameData.Send("Post Event Crew Average Mood", GameManagement.Team.AverageTeamMood());
				SUGARManager.GameData.Send("Post Event Crew Average Manager Opinion", GameManagement.Team.AverageTeamManagerOpinion());
				SUGARManager.GameData.Send("Post Event Crew Average Opinion", GameManagement.Team.AverageTeamOpinion());
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
					{ TrackerContextKeys.EventID.ToString(), GameManagement.GameManager.GetPostRaceEventKeys().First(res.Dialogue.NextState.StartsWith) },
				}, res.Dialogue.NextState, AlternativeTracker.Alternative.Dialog));
				SUGARManager.GameData.Send("Post Race Event Reply", res.Dialogue.NextState);
			}
			var beforeValues = GameManagement.Team.AverageTeamMood() + GameManagement.Team.AverageTeamManagerOpinion() + GameManagement.Team.AverageTeamOpinion();
			foreach (var res in _selectedResponses)
			{
				ReactionSoundControl.PlaySound(res.Value.Dialogue.Meaning[0], res.Key.Gender == "Male", res.Key.Avatar.Height, res.Key.Avatar.Weight);
			}
			var replies = GameManagement.GameManager.SendPostRaceEvent(_selectedResponses.Values.ToList());
			_selectedResponses = null;
			var afterValues = GameManagement.Team.AverageTeamMood() + GameManagement.Team.AverageTeamManagerOpinion() + GameManagement.Team.AverageTeamOpinion();
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
			_closeButton.SetActive(true);
			transform.parent.EnableBlocker(GetLearningPill, () => Close(TrackerTriggerSources.PopUpBlocker.ToString()), UIManagement.TeamSelection.ResetCrew, SendLearningPill);
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
		if (GameManagement.CurrentEvent != null && GameManagement.CurrentEvent.Count == _postRacePeople.Length)
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
		var peopleLayout = _postRacePeople.Select(p => p.GetComponentInChildren<LayoutGroup>()).ToList();
		peopleLayout.ForEach(p => p.GetComponentsInChildren<Text>().ToList().ForEach(t => t.fontSize = 15));
		peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
		Invoke("BestFitCheckDelay", 0f);
	}

	private void BestFitCheckDelay()
	{
		var peopleLayout = _postRacePeople.Select(p => p.GetComponentInChildren<LayoutGroup>()).ToList();
		peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
		Invoke("BestFitCheck", 0f);
	}

	private void BestFitCheck()
	{
		var peopleLayout = _postRacePeople.Select(p => p.GetComponentInChildren<LayoutGroup>()).ToList();
		if (peopleLayout.Any(p => ((RectTransform)p.transform).sizeDelta.y > 0))
		{
			peopleLayout.ForEach(p => p.GetComponentsInChildren<Text>().ToList().ForEach(t => t.fontSize--));
			peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
			Invoke("BestFitCheckDelay", 0f);
		}
		peopleLayout.ForEach(p => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)p.transform));
	}
}
using UnityEngine;
using System.Collections.Generic;
using IntegratedAuthoringTool.DTOs;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts;
using PlayGen.Unity.Utilities.Localization;

/// <summary>
/// Contains UI logic related to crew members involved in a post-race event
/// </summary>
public class PostRacePersonUI : MonoBehaviour
{
	[SerializeField]
	private AvatarDisplay _avatarDisplay;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private GameObject[] _questions;

	public string LastState { get; private set; }
	public CrewMember CurrentCrewMember { get; private set; }

	/// <summary>
	/// Reset and populate the pop-up for a new event
	/// </summary>
	public void ResetDisplay(PostRaceEventState current)
	{
		if (current.Dialogue != null)
		{
			LastState = current.Dialogue.NextState;
			if (current.Dialogue.NextState == "-")
			{
				LastState = current.Dialogue.CurrentState;
			}
			var subjects = current.Subjects.Select(s => Localization.HasKey(s) ? Localization.Get(s) : Regex.Replace(s, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")).ToArray();
			_dialogueText.text = Localization.GetAndFormat(current.Dialogue.Utterance, false, subjects);
		}
		_avatarDisplay.SetAvatar(current.CrewMember.Avatar, current.CrewMember.GetMood());
		_avatarDisplay.transform.parent.GetComponent<Image>().color = AvatarDisplay.MoodColor(AvatarMoodConfig.GetMood(current.CrewMember.GetMood()));
		CurrentCrewMember = current.CrewMember;
		_nameText.text = current.CrewMember.Name;
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	public void ResetQuestions(PostRaceEventState current, List<PostRaceEventState> replies)
	{
		UpdateSelected(null);
		var eventMember = current.CrewMember;
		//set text and onclick handlers for each question UI object
		for (var i = 0; i < _questions.Length; i++)
		{
			if (replies.Count <= i)
			{
				_questions[i].Active(false);
				continue;
			}
			_questions[i].Active(true);
			var subjects = current.Subjects.Select(s => Localization.HasKey(s) ? Localization.Get(s) : Regex.Replace(s, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")).ToArray();
			_questions[i].GetComponentInChildren<Text>().text = Localization.GetAndFormat(replies[i].Dialogue.Utterance, false, subjects);
			var currentReply = replies[i];
			var postRaceEvent = GetComponentInParent<PostRaceEventUI>();
			var question = _questions[i];
			var button = _questions[i].GetComponent<Button>();
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => UpdateSelected(question));
			button.onClick.AddListener(() => postRaceEvent.SendReply(currentReply));
		}
		//display the button for closing the pop-up and update the displayed character mood if there are no more dialogue options
		if (replies.Count == 0)
		{
			//update displayed avatar moods
			if (eventMember != null)
			{
				_avatarDisplay.UpdateMood(eventMember.Avatar, eventMember.GetMood());
			}
			foreach (var crewMember in UIManagement.CrewMemberUI)
			{
				if (crewMember.Usable || crewMember.transform.parent.parent.name == "Viewport")
				{
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(crewMember.CrewMember.Avatar, crewMember.CrewMember.GetMood());
				}
			}
		}
	}

	/// <summary>
	/// Update the responsse text for the crew member
	/// </summary>
	public void UpdateDialogue(DialogueStateActionDTO response, List<string> subjects)
	{
		if (response != null)
		{
			subjects = subjects.Select(s => Localization.HasKey(s) ? Localization.Get(s) : Regex.Replace(s, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")).ToList();
			_dialogueText.text = Localization.GetAndFormat(response.Utterance, false, subjects.ToArray());
			LastState = response.CurrentState;
			if (response.Style.Length > 0)
			{
				foreach (var style in response.Style.Split('_').Where(sp => !string.IsNullOrEmpty(sp)).ToArray())
				{
					var impactSubjects = new List<string>(subjects);
					impactSubjects.Insert(0, CurrentCrewMember.Name);
					UIManagement.EventImpact.AddImpact(style, impactSubjects);
				}
			}
		}
	}

	/// <summary>
	/// Update the selected response (used with more than one crew member involved in an event)
	/// </summary>
	public void UpdateSelected(GameObject question)
	{
		foreach (var q in _questions)
		{
			q.GetComponent<Image>().color = q == question ? new UnityEngine.Color(0, 244, 214) : UnityEngine.Color.white;
		}
	}

	/// <summary>
	/// Turn qall question buttons on
	/// </summary>
	public void EnableQuestions()
	{
		foreach (var question in _questions)
		{
			question.GetComponentInChildren<Text>().text = string.Empty;
			question.Active(true);
		}
	}

	/// <summary>
	/// Get if the first question button is active, and as such if the event has finished or not for this crew member
	/// </summary>
	public bool ActiveQuestions()
	{
		return _questions[0].activeInHierarchy;
	}
}
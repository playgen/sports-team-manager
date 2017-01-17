using UnityEngine;
using System.Collections.Generic;

using IntegratedAuthoringTool.DTOs;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

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
	private string _lastState;
	public string LastState
	{
		get { return _lastState; }
	}
	private CrewMember _currentCrewMember;
	public CrewMember CurrentCrewMember
	{
		get { return _currentCrewMember; }
	}

	public void ResetDisplay(PostRaceEventState current)
	{
		if (current.Dialogue != null)
		{
			_lastState = current.Dialogue.NextState;
			if (current.Dialogue.NextState == "-")
			{
				_lastState = current.Dialogue.CurrentState;
			}
			var subjects = current.Subjects.Select(s => Localization.HasKey(s) ? Localization.Get(s) : Regex.Replace(s, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")).ToArray();
			_dialogueText.text = Localization.GetAndFormat(current.Dialogue.Utterance, false, subjects);
		}
		_avatarDisplay.SetAvatar(current.CrewMember.Avatar, current.CrewMember.GetMood());
		_currentCrewMember = current.CrewMember;
		_nameText.text = current.CrewMember.Name;
	}

	public void ResetQuestions(PostRaceEventState current, List<PostRaceEventState> replies)
	{
		UpdateSelected(null);
		var eventMember = current.CrewMember;
		//set text and onclick handlers for each question UI object
		for (var i = 0; i < _questions.Length; i++)
		{
			if (replies.Count <= i)
			{
				_questions[i].SetActive(false);
				continue;
			}
			_questions[i].SetActive(true);
			var subjects = current.Subjects.Select(s => Localization.HasKey(s) ? Localization.Get(s) : Regex.Replace(s, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")).ToArray();
			_questions[i].GetComponentInChildren<Text>().text = Localization.GetAndFormat(replies[i].Dialogue.Utterance, false, subjects);
			var currentReply = replies[i];
			var postRaceEvent = GetComponentInParent<PostRaceEventUI>();
			var question = _questions[i];
			_questions[i].GetComponent<Button>().onClick.RemoveAllListeners();
			_questions[i].GetComponent<Button>().onClick.AddListener(delegate { UpdateSelected(question); });
			_questions[i].GetComponent<Button>().onClick.AddListener(delegate { postRaceEvent.SendReply(currentReply); });
		}
		//display the button for closing the pop-up and update the displayed character mood if there are no more dialogue options
		if (replies.Count == 0)
		{
			//update displayed avatar moods
			if (eventMember != null)
			{
				_avatarDisplay.UpdateMood(eventMember.Avatar, eventMember.GetMood());
			}
			foreach (var crewMember in (CrewMemberUI[])FindObjectsOfType(typeof(CrewMemberUI)))
			{
				if (crewMember.Current)
				{
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(crewMember.CrewMember.Avatar, crewMember.CrewMember.GetMood());
				}
			}
		}
	}

	public void UpdateDialogue(DialogueStateActionDTO response, List<string> subjects)
	{
		if (response != null)
		{
			subjects = subjects.Select(s => Localization.HasKey(s) ? Localization.Get(s) : Regex.Replace(s, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")).ToList();
			_dialogueText.text = Localization.GetAndFormat(response.Utterance, false, subjects.ToArray());
			_lastState = response.CurrentState;
		}
	}

	public void UpdateSelected(GameObject question)
	{
		foreach (var q in _questions)
		{
			q.GetComponent<Image>().color = q == question ? new UnityEngine.Color(0, 244, 214) : UnityEngine.Color.white;
		}
	}

	public bool ActiveQuestions()
	{
		return _questions[0].activeInHierarchy;
	}
}
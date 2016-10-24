using UnityEngine;
using System.Collections.Generic;

using IntegratedAuthoringTool.DTOs;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.UI;

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

	public void ResetDisplay(KeyValuePair<CrewMember, DialogueStateActionDTO> current)
	{
		if (current.Value != null)
		{
			_lastState = current.Value.NextState;
			if (current.Value.NextState == "-")
			{
				_lastState = current.Value.CurrentState;
			}
			_dialogueText.text = current.Value.Utterance;
		}
		_avatarDisplay.SetAvatar(current.Key.Avatar, current.Key.GetMood());
		_currentCrewMember = current.Key;
		_nameText.text = current.Key.Name;
	}

	public void ResetQuestions(KeyValuePair<CrewMember, DialogueStateActionDTO> current, List<DialogueStateActionDTO> replies)
	{
		UpdateSelected(null);
		var eventMember = current.Key;
		//set text and onclick handlers for each question UI object
		for (var i = 0; i < _questions.Length; i++)
		{
			if (replies.Count <= i)
			{
				_questions[i].SetActive(false);
				continue;
			}
			_questions[i].SetActive(true);
			_questions[i].GetComponentInChildren<Text>().text = replies[i].Utterance;
			var currentMember = current.Key;
			var currentReply = replies[i];
			var postRaceEvent = GetComponentInParent<PostRaceEventUI>();
			var question = _questions[i];
			_questions[i].GetComponent<Button>().onClick.RemoveAllListeners();
			_questions[i].GetComponent<Button>().onClick.AddListener(delegate { UpdateSelected(question); });
			_questions[i].GetComponent<Button>().onClick.AddListener(delegate { postRaceEvent.SendReply(currentMember, currentReply); });
		}
		//display the button for closing the pop-up and update the displayed character mood if there are no more dialogue options
		if (replies.Count == 0)
		{
			//update displayed avatar moods
			if (eventMember != null)
			{
				_avatarDisplay.UpdateMood(eventMember.Avatar, eventMember.GetMood());
			}
			foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
			{
				if (crewMember.Current)
				{
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(crewMember.CrewMember.Avatar, crewMember.CrewMember.GetMood());
				}
			}
		}
	}

	public void UpdateDialogue(DialogueStateActionDTO response)
	{
		if (response != null)
		{
			_dialogueText.text = response.Utterance;
			_lastState = response.CurrentState;
		}
	}

	public void UpdateSelected(GameObject question)
	{
		foreach (var q in _questions)
		{
			if (q == question)
			{
				q.GetComponent<Image>().color = new UnityEngine.Color(0, 244, 214);
			}
			else
			{
				q.GetComponent<Image>().color = UnityEngine.Color.white;
			}
		}
	}

	public bool ActiveQuestions()
	{
		if (!_questions[0].activeSelf)
		{
			return false;
		}
		return true;
	}
}
﻿using System.Collections.Generic;
using System.Linq;
using IntegratedAuthoringTool.DTOs;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PostRaceEvent))]
/// <summary>
/// Contains all UI logic related to the Post Race pop-up
/// </summary>
public class PostRaceEventUI : MonoBehaviour
{
	private PostRaceEvent _postRaceEvent;
	[SerializeField]
	private AvatarDisplay _avatarDisplay;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private GameObject[] _questions;
	[SerializeField]
	private GameObject _closeButton;
	[SerializeField]
	private Button _popUpBlocker;

	void Awake()
	{
		_postRaceEvent = GetComponent<PostRaceEvent>();
	}

	void OnEnable()
	{
		ResetDisplay();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
	}

	void OnDisable()
	{
		_popUpBlocker.gameObject.SetActive(false);
	}

	/// <summary>
	/// Reset and populate the pop-up for a new event
	/// </summary>
	void ResetDisplay()
	{
		//hide the pop-up in case no event is selected
		GetComponent<CanvasGroup>().alpha = 0;
		_closeButton.SetActive(false);
		gameObject.SetActive(true);
		KeyValuePair<List<CrewMember>, string> current = _postRaceEvent.CurrentEvent;
		_avatarDisplay.SetAvatar(current.Key[0].Avatar, current.Key[0].GetMood());
		_nameText.text = "";
		foreach (CrewMember cm in current.Key)
		{
			if (_nameText.text.Length > 0)
			{
				_nameText.text += " & ";
			}
			_nameText.text += cm.Name;
		}
		if (current.Key != null & current.Value != null)
		{
			GetComponent<CanvasGroup>().alpha = 1;
			_dialogueText.text = current.Value;
			ResetQuestions();
		} else
		{
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	void ResetQuestions()
	{
		DialogueStateActionDTO[] replies = _postRaceEvent.GetEventReplies();
		for (int i = 0; i < _questions.Length; i++)
		{
			if (replies.Length <= i)
			{
				_questions[i].SetActive(false);
				continue;
			}
			_questions[i].SetActive(true);
			_questions[i].GetComponentInChildren<Text>().text = replies[i].Utterance;
			DialogueStateActionDTO currentReply = replies[i];
			_questions[i].GetComponent<Button>().onClick.RemoveAllListeners();
			_questions[i].GetComponent<Button>().onClick.AddListener(delegate { SendReply(currentReply); });
		}
		//display the button for closing the pop-up and update the displayed character mood if there are no more dialogue options
		if (replies.Length == 0)
		{
			_closeButton.SetActive(true);
			_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
			KeyValuePair<List<CrewMember>, string> current = _postRaceEvent.CurrentEvent;
			_avatarDisplay.UpdateMood(current.Key[0].Avatar, current.Key[0].GetMood());
			foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
			{
				if (crewMember.Current)
				{
					crewMember.GetComponentInChildren<AvatarDisplay>().UpdateMood(crewMember.CrewMember.Avatar, crewMember.CrewMember.GetMood());
				}
			}
		}
	}

	/// <summary>
	/// Triggered by button. Send the selected dialogue to the character
	/// </summary>
	public void SendReply(DialogueStateActionDTO reply)
	{
		Tracker.T.alternative.Selected("Post Race Event", reply.NextState, AlternativeTracker.Alternative.Dialog);
		Dictionary<CrewMember, string> response = _postRaceEvent.SendReply(reply);
		if (response != null)
		{
			_dialogueText.text = response.FirstOrDefault().Value;
			ResetQuestions();
		}
	}
}
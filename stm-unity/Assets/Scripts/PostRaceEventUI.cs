using System;
using System.Collections.Generic;
using System.Linq;

using IntegratedAuthoringTool.DTOs;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(PostRaceEvent))]
public class PostRaceEventUI : MonoBehaviour
{
	private PostRaceEvent _postRaceEvent;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private GameObject[] _questions;
	[SerializeField]
	private GameObject _closeButton;

	void Awake()
	{
		_postRaceEvent = GetComponent<PostRaceEvent>();
	}

	void OnEnable()
	{
		ResetDisplay();
	}

	void ResetDisplay()
	{
		GetComponent<CanvasGroup>().alpha = 0;
		_closeButton.SetActive(false);
		gameObject.SetActive(true);
		KeyValuePair<List<CrewMember>, string> current = _postRaceEvent.GetCurrentEvent();
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
		if (replies.Length == 0)
		{
			_closeButton.SetActive(true);
		}
	}

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
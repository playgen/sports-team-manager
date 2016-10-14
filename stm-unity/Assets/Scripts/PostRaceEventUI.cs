using System.Linq;
using IntegratedAuthoringTool.DTOs;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains all UI logic related to the Post Race pop-up
/// </summary>
[RequireComponent(typeof(PostRaceEvent))]
public class PostRaceEventUI : MonoBehaviour
{
	private PostRaceEvent _postRaceEvent;
	[SerializeField]
	private LearningPillUI _learningPill;
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
	private string _lastState;

	private void Awake()
	{
		_postRaceEvent = GetComponent<PostRaceEvent>();
	}

	private void OnEnable()
	{
		//reorder pop-ups and blockers
		ResetDisplay();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
	}

	private void OnDisable()
	{
		if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Reset and populate the pop-up for a new event
	/// </summary>
	public void ResetDisplay()
	{
		//hide the pop-up in case no event is selected
		GetComponent<CanvasGroup>().alpha = 0;
		_closeButton.SetActive(false);
		gameObject.SetActive(true);
		var current = _postRaceEvent.CurrentEvent;
		_nameText.text = "";
		//if there is an event
		if (current.Key != null && current.Value != null)
		{   //display avatar of first CrewMember involved
			_lastState = current.Value.NextState;
			if (current.Value.NextState == "-")
			{
				_lastState = current.Value.CurrentState;
			}
			_avatarDisplay.SetAvatar(current.Key[0].Avatar, current.Key[0].GetMood());
			//display names of all involved
			foreach (var cm in current.Key)
			{
				if (_nameText.text.Length > 0)
				{
					_nameText.text += " & ";
				}
				_nameText.text += cm.Name;
			}
			//set alpha to 1 (fully visible)
			GetComponent<CanvasGroup>().alpha = 1;
			//set current NPC dialogue
			_dialogueText.text = current.Value.Utterance;
			ResetQuestions();
		} else
		{
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Reset the available dialogue options for the player
	/// </summary>
	private void ResetQuestions()
	{
		var current = _postRaceEvent.CurrentEvent;
		var eventMember = current.Key != null ? current.Key[0] : null;
		var replies = _postRaceEvent.GetEventReplies();
		//set text and onclick handlers for each question UI object
		for (var i = 0; i < _questions.Length; i++)
		{
			if (replies.Length <= i)
			{
				_questions[i].SetActive(false);
				continue;
			}
			_questions[i].SetActive(true);
			_questions[i].GetComponentInChildren<Text>().text = replies[i].Utterance;
			var currentReply = replies[i];
			_questions[i].GetComponent<Button>().onClick.RemoveAllListeners();
			_questions[i].GetComponent<Button>().onClick.AddListener(delegate { SendReply(currentReply); });
		}
		//display the button for closing the pop-up and update the displayed character mood if there are no more dialogue options
		if (replies.Length == 0)
		{
			_closeButton.SetActive(true);
			_popUpBlocker.onClick.AddListener(GetLearningPill);
			_popUpBlocker.onClick.AddListener(ResetDisplay);
			var teamSelection = FindObjectOfType(typeof(TeamSelectionUI)) as TeamSelectionUI;
			_popUpBlocker.onClick.AddListener(teamSelection.ResetCrew);
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
		else
		{
			_popUpBlocker.onClick.RemoveAllListeners();
		}
	}

	public void GetLearningPill()
	{
		_learningPill.SetHelp(_lastState);
	}

	/// <summary>
	/// Triggered by button. Send the selected dialogue to the character
	/// </summary>
	public void SendReply(DialogueStateActionDTO reply)
	{
		Tracker.T.alternative.Selected("Post Race Event", reply.NextState, AlternativeTracker.Alternative.Dialog);
		var response = _postRaceEvent.SendReply(reply);
		if (response != null)
		{
			_dialogueText.text = response.First().Value.Utterance;
			_lastState = response.First().Value.CurrentState;
			ResetQuestions();
		}
	}
}
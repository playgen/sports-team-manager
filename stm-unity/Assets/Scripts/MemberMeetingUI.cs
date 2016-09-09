using System;
using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(MemberMeeting))]
public class MemberMeetingUI : MonoBehaviour
{
	private MemberMeeting _memberMeeting;
	private CrewMember _currentMember;
	[SerializeField]
	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private GameObject _meetingPopUp;
	[SerializeField]
	private AvatarDisplay _avatarDisplay;
	[SerializeField]
	private Text[] _crewPopUpText;
	[SerializeField]
	private Image[] _crewPopUpBars;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Text _meetingNameText;
	[SerializeField]
	private Text _statQuestion;
	[SerializeField]
	private Text _roleQuestion;
	[SerializeField]
	private Text _opinionPositiveQuestion;
	[SerializeField]
	private Text _opinionNegativeQuestion;
	[SerializeField]
	private Text _closeText;
	[SerializeField]
	private GameObject _fireWarningPopUp;
	[SerializeField]
	private Button _fireButton;
	[SerializeField]
	private Button _popUpBlocker;

	void Awake()
	{
		_memberMeeting = GetComponent<MemberMeeting>();
	}

	void OnDisable()
	{
		_fireWarningPopUp.SetActive(false);
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			crewMember.transform.Find("Opinion").GetComponent<Image>().enabled = false;
		}
		_teamSelectionUI.ChangeBlockerOrder();
	}

	public void Display(CrewMember crewMember)
	{
		_currentMember = crewMember;
		_meetingPopUp.SetActive(true);
		_fireWarningPopUp.SetActive(false);
		Tracker.T.alternative.Selected("Crew Member", "Meeting", AlternativeTracker.Alternative.Menu);
		ResetDisplay();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
	}

	void ResetDisplay(bool postQuestion = false)
	{
		_fireButton.interactable = true;
		var currentBoat = _memberMeeting.GetBoat();
		var primary = new Color32((byte)currentBoat.TeamColorsPrimary[0], (byte)currentBoat.TeamColorsPrimary[1], (byte)currentBoat.TeamColorsPrimary[2], 255);
		var secondary = new Color32((byte)currentBoat.TeamColorsSecondary[0], (byte)currentBoat.TeamColorsSecondary[1], (byte)currentBoat.TeamColorsSecondary[2], 255);
		_avatarDisplay.SetAvatar(_currentMember.Avatar, _currentMember.GetMood(), primary, secondary);
		_crewPopUpText[0].text = "Name: <color=#ffffffff>" + _currentMember.Name + "</color>";
		_crewPopUpText[1].text = "Age: <color=#ffffffff>" + _currentMember.Age + "</color>";
		_crewPopUpText[2].text = "Role: <color=#ffffffff>" + _memberMeeting.GetCrewMemberPosition(_currentMember) + "</color>";
		_crewPopUpBars[0].fillAmount = _currentMember.RevealedSkills[CrewMemberSkill.Body] * 0.1f;
		_crewPopUpBars[1].fillAmount = _currentMember.RevealedSkills[CrewMemberSkill.Charisma] * 0.1f;
		_crewPopUpBars[2].fillAmount = _currentMember.RevealedSkills[CrewMemberSkill.Perception] * 0.1f;
		_crewPopUpBars[3].fillAmount = _currentMember.RevealedSkills[CrewMemberSkill.Quickness] * 0.1f;
		_crewPopUpBars[4].fillAmount = _currentMember.RevealedSkills[CrewMemberSkill.Willpower] * 0.1f;
		_crewPopUpBars[5].fillAmount = _currentMember.RevealedSkills[CrewMemberSkill.Wisdom] * 0.1f;
		_dialogueText.text = "You wanted to see me?";
		_meetingNameText.text = "What do you want to ask?";
		_statQuestion.text = _memberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		_roleQuestion.text = _memberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		_opinionPositiveQuestion.text = _memberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		_opinionNegativeQuestion.text = _memberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		int allowance = _memberMeeting.QuestionAllowance();
		_statQuestion.GetComponentInParent<Button>().interactable = true;
		_roleQuestion.GetComponentInParent<Button>().interactable = true;
		_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = true;
		_opinionNegativeQuestion.GetComponentInParent<Button>().interactable = true;
		if (allowance < 3)
		{
			_roleQuestion.GetComponentInParent<Button>().interactable = false;
		}
		if (allowance < 2)
		{
			_opinionNegativeQuestion.GetComponentInParent<Button>().interactable = false;
			_fireButton.interactable = false;
		}
		if (allowance < 1)
		{
			_statQuestion.GetComponentInParent<Button>().interactable = false;
			_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = false;
		}
		if (_memberMeeting.CrewEditAllowance() == 0 || !_memberMeeting.CanRemoveCheck())
		{
			_fireButton.interactable = false;
		}
		_closeText.text = "Nevermind, goodbye";
		if (postQuestion)
		{
			_closeText.text = "OK, thank you for telling me that.";
		}
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			var name = crewMember.CrewMember().Name;
			Image opinionImage = crewMember.transform.Find("Opinion").GetComponent<Image>();
			if (name != _currentMember.Name)
			{
				opinionImage.enabled = true;
				foreach (CrewOpinion opinion in _currentMember.RevealedCrewOpinions)
				{
					if (name == opinion.Person.Name)
					{
						if (opinion.Opinion > 1)
						{
							//opinionImage.sprite = ;
							opinionImage.color = Color.green;
						}
						else if (opinion.Opinion < -1)
						{
							//opinionImage.sprite = ;
							opinionImage.color = Color.red;
						}
						else
						{
							//opinionImage.sprite = ;
							opinionImage.color = Color.yellow;
						}
					}
				}
				if (opinionImage.sprite == null)
				{
					//opinionImage.sprite = ;
					//opinionImage.color = Color.yellow;
				}
			}
		}
	}

	public void AskStatQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Stat Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "StatReveal", _currentMember);
		AnswerUpdate(reply);
	}

	public void AskRoleQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Role Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "RoleReveal", _currentMember);
		AnswerUpdate(reply);
	}

	public void AskOpinionPositiveQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Positive Opinion Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "OpinionRevealPositive", _currentMember);
		AnswerUpdate(reply);
	}

	public void AskOpinionNegativeQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Negative Opinion Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "OpinionRevealNegative", _currentMember);
		AnswerUpdate(reply);
	}

	private void AnswerUpdate(string[] reply)
	{
		ResetDisplay(true);
		_dialogueText.text = reply.Length > 0 ? reply[0] : "";
	}

	public void FireCrewWarning()
	{
		Tracker.T.alternative.Selected("Crew Member", "Fire", AlternativeTracker.Alternative.Menu);
		_fireWarningPopUp.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		_fireWarningPopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { CloseFireCrewWarning(); });
	}

	public void CloseFireCrewWarning()
	{
		_fireWarningPopUp.SetActive(false);
		if (gameObject.activeSelf)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			_popUpBlocker.onClick.RemoveAllListeners();
			_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
		}
		else
		{
			_popUpBlocker.gameObject.SetActive(false);
		}
	}

	public void FireCrew()
	{
		Tracker.T.trackedGameObject.Interacted("Fired Crew Member", GameObjectTracker.TrackedGameObject.Npc);
		_memberMeeting.FireCrewMember(_currentMember);
		_teamSelectionUI.ResetCrew();
		_teamSelectionUI.ResetPositionPopUp();
	}
}
﻿using System;
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
	[SerializeField]
	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private GameObject _meetingPopUp;
	[SerializeField]
	private AvatarDisplay _avatarDisplay;
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

	void Awake()
	{
		_memberMeeting = GetComponent<MemberMeeting>();
	}

	void OnEnable()
	{
		Tracker.T.alternative.Selected("Crew Member", "Meeting", AlternativeTracker.Alternative.Menu);
		ResetDisplay();
	}

	void ResetDisplay(bool postQuestion = false)
	{
		_meetingPopUp.SetActive(true);
		var currentBoat = _memberMeeting.GetBoat();
		var primary = new Color32((byte)currentBoat.TeamColorsPrimary[0], (byte)currentBoat.TeamColorsPrimary[1], (byte)currentBoat.TeamColorsPrimary[2], 255);
		var secondary = new Color32((byte)currentBoat.TeamColorsSecondary[0], (byte)currentBoat.TeamColorsSecondary[1], (byte)currentBoat.TeamColorsSecondary[2], 255);
		_avatarDisplay.SetAvatar(_teamSelectionUI.GetCurrentCrewMember().Avatar, _teamSelectionUI.GetCurrentCrewMember().GetMood(), primary, secondary);
		_dialogueText.text = "You wanted to see me?";
		_meetingNameText.text = "What do you want to ask " + _teamSelectionUI.GetCurrentCrewMember().Name + " ?";
		_statQuestion.text = _memberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).FirstOrDefault() + " (1)";
		_roleQuestion.text = _memberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).FirstOrDefault() + " (3)";
		_opinionPositiveQuestion.text = _memberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).FirstOrDefault() + " (1)";
		_opinionNegativeQuestion.text = _memberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).FirstOrDefault() + " (2)";
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
		}
		if (allowance < 1)
		{
			_statQuestion.GetComponentInParent<Button>().interactable = false;
			_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = false;
		}
		_closeText.text = "Nevermind, goodbye";
		if (postQuestion)
		{
			_closeText.text = "OK, thank you for telling me that.";
		}
	}

	public void AskStatQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Stat Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "StatReveal", _teamSelectionUI.GetCurrentCrewMember());
		AnswerUpdate(reply);
	}

	public void AskRoleQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Role Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "RoleReveal", _teamSelectionUI.GetCurrentCrewMember());
		AnswerUpdate(reply);
	}

	public void AskOpinionPositiveQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Positive Opinion Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "OpinionRevealPositive", _teamSelectionUI.GetCurrentCrewMember());
		AnswerUpdate(reply);
	}

	public void AskOpinionNegativeQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Negative Opinion Reveal", AlternativeTracker.Alternative.Question);
		string[] reply = _memberMeeting.AskQuestion("SoloInterview", "OpinionRevealNegative", _teamSelectionUI.GetCurrentCrewMember());
		AnswerUpdate(reply);
	}

	private void AnswerUpdate(string[] reply)
	{
		ResetDisplay(true);
		_dialogueText.text = reply.Length > 0 ? reply[0] : "";
		_teamSelectionUI.DisplayCrewPopUp(_teamSelectionUI.GetCurrentCrewMember());
	}
}
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
		_meetingPopUp.SetActive(true);
		_dialogueText.text = "You wanted to see me?";
		_meetingNameText.text = "What do you want to ask " + _teamSelectionUI.GetCurrentCrewMember().Name + " ?";
		_statQuestion.text = "";
		_roleQuestion.text = "";
		_opinionPositiveQuestion.text = "";
		_opinionNegativeQuestion.text = "";
		_closeText.text = "Nevermind, goodbye";
	}

	public void AskStatQuestion()
	{
		string[] reply = _memberMeeting.AskStatQuestion(_teamSelectionUI.GetCurrentCrewMember());
		_dialogueText.text = reply.Length > 0 ? reply[0] : "";
		_teamSelectionUI.DisplayCrewPopUp(_teamSelectionUI.GetCurrentCrewMember());
	}

	public void AskRoleQuestion()
	{
		string[] reply = _memberMeeting.AskRoleQuestion(_teamSelectionUI.GetCurrentCrewMember());
		_dialogueText.text = reply.Length > 0 ? reply[0] : "";
		_teamSelectionUI.DisplayCrewPopUp(_teamSelectionUI.GetCurrentCrewMember());
	}

	public void AskOpinionPositiveQuestion()
	{
		string[] reply = _memberMeeting.AskOpinionPositiveQuestion(_teamSelectionUI.GetCurrentCrewMember());
		_dialogueText.text = reply.Length > 0 ? reply[0] : "";
		_teamSelectionUI.DisplayCrewPopUp(_teamSelectionUI.GetCurrentCrewMember());
	}

	public void AskOpinionNegativeQuestion()
	{
		string[] reply = _memberMeeting.AskOpinionNegativeQuestion(_teamSelectionUI.GetCurrentCrewMember());
		_dialogueText.text = reply.Length > 0 ? reply[0] : "";
		_teamSelectionUI.DisplayCrewPopUp(_teamSelectionUI.GetCurrentCrewMember());
	}
}
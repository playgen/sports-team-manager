using System;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MemberMeeting))]
/// <summary>
/// Contains all logic related to the CrewMember Meeting pop-up
/// </summary>
public class MemberMeetingUI : MonoBehaviour
{
	private MemberMeeting _memberMeeting;
	private CrewMember _currentMember;
	[SerializeField]
	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private PositionDisplayUI _positionUI;
	[SerializeField]
	private AvatarDisplay _avatarDisplay;
	[SerializeField]
	private Text[] _textList;
	[SerializeField]
	private Button _roleButton;
	[SerializeField]
	private Image[] _barBackgrounds;
	[SerializeField]
	private Sprite _unknownBackBar;
	[SerializeField]
	private Sprite _knownBackBar;
	[SerializeField]
	private Image[] _barForegrounds;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private Text _statQuestion;
	[SerializeField]
	private Text _roleQuestion;
	[SerializeField]
	private Text _opinionPositiveQuestion;
	[SerializeField]
	private Text _opinionNegativeQuestion;
	[SerializeField]
	private Sprite[] _opinionIcons;
	[SerializeField]
	private Text _closeText;
	[SerializeField]
	private GameObject _fireWarningPopUp;
	[SerializeField]
	private Button _fireButton;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private Image _allowanceBar;
	[SerializeField]
	private Text _allowanceText;

	void Awake()
	{
		_memberMeeting = GetComponent<MemberMeeting>();
	}

	/// <summary>
	/// On the GameObject being disabled, hide the fire warning pop-up, all displayed opinions and adjust the order of the pop-ups
	/// </summary>
	void OnDisable()
	{
		_fireWarningPopUp.SetActive(false);
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			if (crewMember.Current)
			{
				crewMember.transform.Find("Opinion").GetComponent<Image>().enabled = false;
			}
		}
		_positionUI.ChangeBlockerOrder();
	}

	/// <summary>
	/// Set-up the pop-up for displaying the given CrewMember
	/// </summary>
	public void SetUpDisplay(CrewMember crewMember)
	{
		_currentMember = crewMember;
		gameObject.SetActive(true);
		_fireWarningPopUp.SetActive(false);
		foreach (var cmui in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			if (cmui.Current)
			{
				cmui.transform.Find("Opinion").GetComponent<Image>().enabled = false;
			}
		}
		Tracker.T.alternative.Selected("Crew Member", "Meeting", AlternativeTracker.Alternative.Menu);
		Display();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	public void Display(bool postQuestion = false)
	{
		_allowanceBar.fillAmount = (float)_memberMeeting.QuestionAllowance() / (float)_memberMeeting.StartingQuestionAllowance();
		_allowanceText.text = _memberMeeting.QuestionAllowance().ToString();
		_fireButton.interactable = true;
		_avatarDisplay.SetAvatar(_currentMember.Avatar, _currentMember.GetMood());
		_textList[0].text = _currentMember.Name;
		_textList[1].text = _currentMember.Age.ToString();
		var currentRole = _memberMeeting.GetCrewMemberPosition(_currentMember);
        _textList[2].text = currentRole == null ? "No Role" : "";
		_roleButton.onClick.RemoveAllListeners();
		if (currentRole != null)
		{
			_roleButton.gameObject.SetActive(true);
			_roleButton.onClick.AddListener(delegate { _positionUI.Display(currentRole); });
			_roleButton.GetComponentInChildren<Text>().text = currentRole.Name.ToUpper();
			_roleButton.transform.Find("Image").GetComponent<Image>().sprite = _teamSelectionUI.RoleLogos.FirstOrDefault(mo => mo.Name == currentRole.Name).Image;
		}
        else
		{
			_roleButton.gameObject.SetActive(false);
		}
        for (int i = 0; i < _barBackgrounds.Length; i++)
        {
            _barForegrounds[i].fillAmount = _currentMember.RevealedSkills[(CrewMemberSkill)i] * 0.1f;
            _barBackgrounds[i].sprite = _currentMember.RevealedSkills[(CrewMemberSkill)i] == 0 ? _unknownBackBar : _knownBackBar;
        }
		_dialogueText.text = "You wanted to see me?";
		_nameText.text = "What do you want to ask?";
		_statQuestion.text = _memberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		_roleQuestion.text = _memberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		_opinionPositiveQuestion.text = _memberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		_opinionNegativeQuestion.text = _memberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).FirstOrDefault();
		int allowance = _memberMeeting.QuestionAllowance();
        _fireButton.interactable = allowance >= 4;
        _statQuestion.GetComponentInParent<Button>().interactable = allowance >= 1;
		_roleQuestion.GetComponentInParent<Button>().interactable = allowance >= 3;
		_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = allowance >= 1;
		_opinionNegativeQuestion.GetComponentInParent<Button>().interactable = allowance >= 2;
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
			if (crewMember.Current)
			{
				var name = crewMember.CrewMember.Name;
				Image opinionImage = crewMember.transform.Find("Opinion").GetComponent<Image>();
				if (name != _currentMember.Name)
				{
					opinionImage.enabled = true;
					opinionImage.sprite = null;
                    var opinion = _currentMember.RevealedCrewOpinions.FirstOrDefault(co => co.Person == crewMember.CrewMember);
                    opinionImage.sprite = _opinionIcons[(opinion.Opinion > 0 ? Mathf.CeilToInt(opinion.Opinion / 3f) : Mathf.FloorToInt(opinion.Opinion / 3f)) + 2];
                }
			}
		}
	}

    /// <summary>
	/// Triggered by button. Sends StatReveal question to Simulation, gets reply from NPC in response.
	/// </summary>
	public void AskStatQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Stat Reveal", AlternativeTracker.Alternative.Question);
		string reply = _memberMeeting.AskQuestion("StatReveal", _currentMember);
		AnswerUpdate(reply);
	}

    /// <summary>
	/// Triggered by button. Sends RoleReveal question to Simulation, gets reply from NPC in response.
	/// </summary>
	public void AskRoleQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Role Reveal", AlternativeTracker.Alternative.Question);
		string reply = _memberMeeting.AskQuestion("RoleReveal", _currentMember);
		AnswerUpdate(reply);
	}

    /// <summary>
	/// Triggered by button. Sends OpinionRevealPositive question to Simulation, gets reply from NPC in response.
	/// </summary>
	public void AskOpinionPositiveQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Positive Opinion Reveal", AlternativeTracker.Alternative.Question);
		string reply = _memberMeeting.AskQuestion("OpinionRevealPositive", _currentMember);
		AnswerUpdate(reply);
	}

    /// <summary>
	/// Triggered by button. Sends OpinionRevealNegative question to Simulation, gets reply from NPC in response.
	/// </summary>
	public void AskOpinionNegativeQuestion()
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", "Negative Opinion Reveal", AlternativeTracker.Alternative.Question);
		string reply = _memberMeeting.AskQuestion("OpinionRevealNegative", _currentMember);
		AnswerUpdate(reply);
	}

    /// <summary>
	/// Reset displayed information and display reply from NPC.
	/// </summary>
	private void AnswerUpdate(string reply)
	{
		Display(true);
		_dialogueText.text = reply.Length > 0 ? reply : "";
	}

    /// <summary>
	/// Triggered by button. Display warning to player that they are about to fire a character.
	/// </summary>
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

    /// <summary>
	/// Triggered by button. Close fire warning pop-up.
	/// </summary>
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

    /// <summary>
	/// Triggered by button. Fires a character from the team.
	/// </summary>
	public void FireCrew()
	{
		Tracker.T.trackedGameObject.Interacted("Fired Crew Member", GameObjectTracker.TrackedGameObject.Npc);
		_memberMeeting.FireCrewMember(_currentMember);
		_teamSelectionUI.ResetCrew();
	}
}
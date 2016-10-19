using System;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains all logic related to the CrewMember Meeting pop-up
/// </summary>
[RequireComponent(typeof(MemberMeeting))]
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

	private void Awake()
	{
		_memberMeeting = GetComponent<MemberMeeting>();
	}

	/// <summary>
	/// On the GameObject being disabled, hide the fire warning pop-up, all displayed opinions and adjust the order of the pop-ups
	/// </summary>
	private void OnDisable()
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
		//make pop-up visible and firing warning not visible
		gameObject.SetActive(true);
		_fireWarningPopUp.SetActive(false);
		//disable opinion images on CrewMember UI objects
		foreach (var cmui in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			if (cmui.Current)
			{
				cmui.transform.Find("Opinion").GetComponent<Image>().enabled = false;
			}
		}
		Tracker.T.alternative.Selected("Crew Member", "Meeting", AlternativeTracker.Alternative.Menu);
		Display();
		//set the order of the pop-ups and pop-up blockers and set-up the click event for the blocker
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
		//ActionAllowance display
		_allowanceBar.fillAmount = _memberMeeting.QuestionAllowance() / (float)_memberMeeting.StartingQuestionAllowance();
		_allowanceText.text = _memberMeeting.QuestionAllowance().ToString();
		//CrewMember avatar
		_avatarDisplay.SetAvatar(_currentMember.Avatar, _currentMember.GetMood());
		//CrewMember information
		_textList[0].text = _currentMember.Name;
		_textList[1].text = _currentMember.Age.ToString();
		var currentRole = _memberMeeting.GetCrewMemberPosition(_currentMember);
		_textList[2].text = currentRole == Position.Null ? "No Role" : "";
		_roleButton.onClick.RemoveAllListeners();
		//set up button onclick if CrewMember is positioned
		if (currentRole != Position.Null)
		{
			_roleButton.gameObject.SetActive(true);
			_roleButton.onClick.AddListener(delegate { _positionUI.SetUpDisplay(currentRole); });
			_roleButton.GetComponentInChildren<Text>().text = currentRole.GetName().ToUpper();
			_roleButton.transform.Find("Image").GetComponent<Image>().sprite = _teamSelectionUI.RoleLogos.First(mo => mo.Name == currentRole.GetName()).Image;
		}
		//hide if not positioned
		else
		{
			_roleButton.gameObject.SetActive(false);
		}
		//set stat bar fill amount (foreground) and sprite (background)
		for (var i = 0; i < _barForegrounds.Length; i++)
		{
			_barForegrounds[i].fillAmount = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] * 0.1f;
			_barForegrounds[i].transform.parent.FindChild("Hidden Image").GetComponent<Image>().enabled = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] == 0;
			_barForegrounds[i].transform.parent.FindChild("Skill Image").GetComponent<Image>().enabled = _currentMember.RevealedSkills[(CrewMemberSkill)Mathf.Pow(2, i)] != 0;
		}
		//set default starting dialogue
		_dialogueText.text = "You wanted to see me?";
		_nameText.text = "What do you want to ask?";
		//set question text for the player
		_statQuestion.text = _memberMeeting.GetEventText("StatReveal").OrderBy(s => Guid.NewGuid()).First();
		_roleQuestion.text = _memberMeeting.GetEventText("RoleReveal").OrderBy(s => Guid.NewGuid()).First();
		_opinionPositiveQuestion.text = _memberMeeting.GetEventText("OpinionRevealPositive").OrderBy(s => Guid.NewGuid()).First();
		_opinionNegativeQuestion.text = _memberMeeting.GetEventText("OpinionRevealNegative").OrderBy(s => Guid.NewGuid()).First();
		//set the cost shown for each question and for firing
		_statQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.StatRevealCost).ToString();
		_roleQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.RoleRevealCost).ToString();
		_opinionPositiveQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealPositiveCost).ToString();
		_opinionNegativeQuestion.transform.parent.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealNegativeCost).ToString();
		_fireButton.transform.FindChild("Image/Text").GetComponent<Text>().text = _memberMeeting.GetConfigValue(ConfigKeys.FiringCost).ToString();
		var allowance = _memberMeeting.QuestionAllowance();
		//set if each button is interactable according to if the player has enough allowance
		_fireButton.interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.FiringCost) && _memberMeeting.CrewEditAllowance() != 0 && _memberMeeting.CanRemoveCheck();
		_statQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.StatRevealCost);
		_roleQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.RoleRevealCost);
		_opinionPositiveQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealPositiveCost);
		_opinionNegativeQuestion.GetComponentInParent<Button>().interactable = allowance >= _memberMeeting.GetConfigValue(ConfigKeys.OpinionRevealNegativeCost);
		//set closing text
		_closeText.text = "Nevermind, goodbye";
		if (postQuestion)
		{
			_closeText.text = "OK, thank you for telling me that.";
		}
		//display revealed opinions for each other active CrewMember
		foreach (var crewMember in FindObjectsOfType(typeof(CrewMemberUI)) as CrewMemberUI[])
		{
			if (crewMember.Current)
			{
				var crewName = crewMember.CrewMember.Name;
				var opinionImage = crewMember.transform.Find("Opinion").GetComponent<Image>();
				if (crewName != _currentMember.Name)
				{
					opinionImage.enabled = true;
					opinionImage.sprite = null;
					var opinion = _currentMember.RevealedCrewOpinions[crewName];
					var opinionColorValue = 0.25f + (0.075f*(10 + _currentMember.RevealedCrewOpinionAges[crewName]));
					opinionImage.color = new UnityEngine.Color(opinionColorValue, opinionColorValue, opinionColorValue);
					opinionImage.sprite = _opinionIcons[(opinion > 0 ? Mathf.CeilToInt(opinion / 3f) : Mathf.FloorToInt(opinion / 3f)) + 2];
				}
			}
		}
		var managerOpinionImage = transform.Find("Manager Opinion").GetComponent<Image>();
		managerOpinionImage.enabled = true;
		managerOpinionImage.sprite = null;
		var managerName = _memberMeeting.GetManagerName();
		var managerOpinion = _currentMember.RevealedCrewOpinions[managerName];
		var managerOpinionColorValue = 0.25f + (0.075f * (10 + _currentMember.RevealedCrewOpinionAges[managerName]));
		managerOpinionImage.color = new UnityEngine.Color(managerOpinionColorValue, managerOpinionColorValue, managerOpinionColorValue);
		managerOpinionImage.sprite = _opinionIcons[(managerOpinion > 0 ? Mathf.CeilToInt(managerOpinion / 3f) : Mathf.FloorToInt(managerOpinion / 3f)) + 2];
	}

	/// <summary>
	/// Triggered by button. Sends provided question to Simulation, gets and displays reply from NPC in response.
	/// </summary>
	public void AskQuestion(string questionType)
	{
		Tracker.T.alternative.Selected("Crew Member Meeting", questionType, AlternativeTracker.Alternative.Question);
		var reply = _memberMeeting.AskQuestion(questionType, _currentMember);
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
		_popUpBlocker.onClick.AddListener(CloseFireCrewWarning);
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
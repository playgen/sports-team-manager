using System;
using System.Linq;
using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.UI;

/// <summary>
/// Contains all UI logic related to the Recruitment pop-up
/// </summary>
[RequireComponent(typeof(RecruitMember))]
public class RecruitMemberUI : MonoBehaviour
{
	private RecruitMember _recruitMember;
	[SerializeField]
	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private GameObject[] _recruitUI;
	[SerializeField]
	private Button[] _questionButtons;
	[SerializeField]
	private Text _dialogueText;
	[SerializeField]
	private Button _popUpBlocker;
	[SerializeField]
	private Text _hireWarningText;
	[SerializeField]
	private GameObject _hireWarningPopUp;
	[SerializeField]
	private Button _hireWarningAccept;
	[SerializeField]
	private GameObject _hireWarningReject;
	[SerializeField]
	private Image _allowanceBar;
	[SerializeField]
	private Text _allowanceText;
	[SerializeField]
	private Icon[] _opinionSprites;

	private void Awake()
	{
		_recruitMember = GetComponent<RecruitMember>();
	}

	private void OnEnable()
	{
		Tracker.T.alternative.Selected("Recruitment", "Recruitment", AlternativeTracker.Alternative.Menu);
		ResetDisplay();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
	}

	private void OnDisable()
	{
		_popUpBlocker.gameObject.SetActive(false);
	}

	/// <summary>
	/// Populate the information required in the pop-up
	/// </summary>
	private void ResetDisplay()
	{
		_allowanceBar.fillAmount = _recruitMember.QuestionAllowance() / (float)_recruitMember.StartingQuestionAllowance();
		_allowanceText.text = _recruitMember.QuestionAllowance().ToString();
		SetDialogueText("Your recruits are here. What do you want to ask them all or who do you want to hire?");
		var recruits = _recruitMember.GetRecruits().OrderBy(r => Guid.NewGuid()).ToList();
		for (var i = 0; i < _recruitUI.Length; i++)
		{
			if (recruits.Count <= i)
			{
				_recruitUI[i].SetActive(false);
				continue;
			}
			var thisRecruit = recruits[i];
			_recruitUI[i].SetActive(true);
			var splitName = thisRecruit.Name.Split(' ');
			var lastName = splitName.Last() + ",\n";
			foreach (var split in splitName)
			{
				if (split != splitName.Last())
				{
					lastName += split + " ";
				}
			}
			lastName = lastName.Remove(lastName.Length - 1, 1);
			_recruitUI[i].transform.Find("Name").GetComponent<Text>().text = lastName;
			_recruitUI[i].transform.Find("Image").GetComponentInChildren<AvatarDisplay>().SetAvatar(thisRecruit.Avatar, 0);
			if (i % 2 != 0)
			{
				_recruitUI[i].transform.Find("Image").localScale = new Vector3(-1, 1, 1);
			}
			_recruitUI[i].transform.Find("Button").GetComponent<Button>().interactable = true;
			_recruitUI[i].transform.Find("Button").GetComponent<Button>().onClick.RemoveAllListeners();
			_recruitUI[i].transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { HireCrewWarning(thisRecruit); });
			var rand = UnityEngine.Random.Range(0, 8);
			switch (rand % 4)
			{
				case 0:
					_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = "Hello";
					break;
				case 1:
					_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = "Hey";
					break;
				case 2:
					_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = "Hi";
					break;
				case 3:
					_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = "Hey there";
					break;
			}
			if (rand / 4 > 0)
			{
				_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text += "!";
			}
			_recruitUI[i].transform.Find("Dialogue Box/Image").GetComponent<Image>().enabled = false;
			_recruitUI[i].transform.Find("Cost Image/Text").GetComponent<Text>().text = _recruitMember.GetConfigValue(ConfigKeys.RecruitmentCost).ToString();
			_recruitUI[i].name = recruits[i].Name;
		}
		var skills = (CrewMemberSkill[])Enum.GetValues(typeof(CrewMemberSkill));
		for (var i = 0; i < _questionButtons.Length; i++)
		{
			if (skills.Length <= i)
			{
				_questionButtons[i].gameObject.SetActive(false);
				continue;
			}
			var selected = skills[i];
			_questionButtons[i].gameObject.SetActive(true);
			_questionButtons[i].interactable = true;
			var questionText = _recruitMember.GetQuestionText("Recruit" + selected).OrderBy(s => Guid.NewGuid()).First();
			_questionButtons[i].transform.Find("Text").GetComponent<Text>().text = questionText;
			_questionButtons[i].transform.Find("Image/Text").GetComponent<Text>().text = _recruitMember.GetConfigValue(ConfigKeys.SendRecruitmentQuestionCost).ToString();
			_questionButtons[i].onClick.RemoveAllListeners();
			_questionButtons[i].onClick.AddListener(delegate { AskQuestion(selected, questionText); });
		}
		CostCheck();
	}

	/// <summary>
	/// Update the allowance bar and disable all questions if they cost too much to ask
	/// </summary>
	private void CostCheck()
	{
		var allowance = _recruitMember.QuestionAllowance();
		_allowanceBar.fillAmount = allowance / (float)_recruitMember.StartingQuestionAllowance();
		_allowanceText.text = allowance.ToString();
		if (allowance < _recruitMember.GetConfigValue(ConfigKeys.SendRecruitmentQuestionCost))
		{
			_questionButtons.ToList().ForEach(qb => qb.interactable = false);
		}
	}

	/// <summary>
	/// Send a question to all recruits and get their replies in response
	/// </summary>
	public void AskQuestion(CrewMemberSkill skill, string questionText)
	{
		Tracker.T.alternative.Selected("Recruitment", skill + " Question", AlternativeTracker.Alternative.Question);
		SetDialogueText(questionText);
		var replies = _recruitMember.AskQuestion(skill);
		foreach (var recruit in _recruitUI)
		{
			var reply = replies.First(r => r.Key.Name == recruit.name);
			recruit.transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = reply.Value;
			recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().enabled = true;
			recruit.transform.Find("Dialogue Box/Image").GetComponent<Image>().sprite = _opinionSprites.First(o => o.Name == reply.Value).Image;
			recruit.transform.Find("Image").GetComponentInChildren<AvatarDisplay>().UpdateMood(reply.Key.Avatar, reply.Value);
		}
		CostCheck();
	}

	/// <summary>
	/// Set the previously asked question to be displayed
	/// </summary>
	private void SetDialogueText(string text)
	{
		_dialogueText.text = text;
	}

	/// <summary>
	/// Display a warning before hiring a recruit
	/// </summary>
	public void HireCrewWarning(CrewMember recruit)
	{
		_hireWarningPopUp.SetActive(true);
		_popUpBlocker.transform.SetAsLastSibling();
		_hireWarningPopUp.transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { CloseHireCrewWarning(); });
		if (_recruitMember.QuestionAllowance() < _recruitMember.GetConfigValue(ConfigKeys.RecruitmentCost))
		{
			_hireWarningText.text = "You don't have enough time left to hire this person.";
			_hireWarningAccept.gameObject.SetActive(false);
			_hireWarningReject.GetComponent<RectTransform>().anchorMin = new Vector2(0.375f, 0.1f);
			_hireWarningReject.GetComponent<RectTransform>().anchorMax = new Vector2(0.625f, 0.35f);
			_hireWarningReject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			_hireWarningReject.GetComponentInChildren<Text>().text = "OK";
		} else
		{
			_hireWarningAccept.onClick.RemoveAllListeners();
			_hireWarningAccept.onClick.AddListener(delegate { Recruit(recruit); });
			_hireWarningAccept.gameObject.SetActive(true);
			_hireWarningText.text = "Are you sure you want to hire " + recruit.Name + "?";
			_hireWarningReject.GetComponent<RectTransform>().anchorMin = new Vector2(0.55f, 0.1f);
			_hireWarningReject.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.35f);
			_hireWarningReject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			_hireWarningReject.GetComponentInChildren<Text>().text = "NO";
		}
	}

	/// <summary>
	/// Close the hire warning
	/// </summary>
	public void CloseHireCrewWarning()
	{
		_hireWarningPopUp.SetActive(false);
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
	/// Hire the selected recruit onto the player's crew
	/// </summary>
	public void Recruit(CrewMember crewMember)
	{
		Tracker.T.trackedGameObject.Interacted("Hired Crew Member", GameObjectTracker.TrackedGameObject.Npc);
		_recruitMember.Recruit(crewMember);
		_teamSelectionUI.ResetCrew();
		gameObject.SetActive(false);
		CloseHireCrewWarning();
	}
}

using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.UI;

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

	void Awake()
	{
		_recruitMember = GetComponent<RecruitMember>();
	}

	void OnEnable()
	{
		Tracker.T.alternative.Selected("Recruitment", "Recruitment", AlternativeTracker.Alternative.Menu);
		ResetDisplay();
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(delegate { gameObject.SetActive(false); });
	}

	void OnDisable()
	{
		_popUpBlocker.gameObject.SetActive(false);
	}

	void ResetDisplay()
	{
		SetDialogueText("");
		List<CrewMember> recruits = _recruitMember.GetRecruits().OrderBy(r => Guid.NewGuid()).ToList();
		for (int i = 0; i < _recruitUI.Length; i++)
		{
			if (recruits.Count <= i)
			{
				_recruitUI[i].SetActive(false);
				continue;
			}
			CrewMember thisRecruit = recruits[i];
			_recruitUI[i].SetActive(true);
			string[] splitName = thisRecruit.Name.Split(' ');
			string name = splitName.Last() + ",\n";
			foreach (string split in splitName)
			{
				if (split != splitName.Last())
				{
					name += split + " ";
				}
			}
			name = name.Remove(name.Length - 1, 1);
			_recruitUI[i].transform.Find("Name").GetComponent<Text>().text = name;
			var currentBoat = _recruitMember.GetBoat();
			var primary = new Color32((byte)currentBoat.TeamColorsPrimary[0], (byte)currentBoat.TeamColorsPrimary[1], (byte)currentBoat.TeamColorsPrimary[2], 255);
			var secondary = new Color32((byte)currentBoat.TeamColorsSecondary[0], (byte)currentBoat.TeamColorsSecondary[1], (byte)currentBoat.TeamColorsSecondary[2], 255);
			_recruitUI[i].transform.Find("Image").GetComponentInChildren<AvatarDisplay>().SetAvatar(thisRecruit.Avatar, thisRecruit.GetMood(), primary, secondary);
			if (i % 2 != 0)
			{
				_recruitUI[i].transform.Find("Image").localScale = new Vector3(-1, 1, 1);
			}
			_recruitUI[i].transform.Find("Name").GetComponent<Button>().interactable = true;
			_recruitUI[i].transform.Find("Name").GetComponent<Button>().onClick.RemoveAllListeners();
			_recruitUI[i].transform.Find("Name").GetComponent<Button>().onClick.AddListener(delegate { Recruit(thisRecruit); });
			_recruitUI[i].transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = "";
			_recruitUI[i].name = recruits[i].Name;
		}
		var skills = (CrewMemberSkill[])Enum.GetValues(typeof(CrewMemberSkill));
		var shuffledSkills = skills.OrderBy(s => Guid.NewGuid()).ToArray();
		for (int i = 0; i < _questionButtons.Length; i++)
		{
			if (skills.Length <= i)
			{
				_questionButtons[i].gameObject.SetActive(false);
				continue;
			}
			CrewMemberSkill selected = shuffledSkills[i];
			_questionButtons[i].gameObject.SetActive(true);
			_questionButtons[i].interactable = true;
			string questionText = _recruitMember.GetQuestionText("Recruit" + selected).OrderBy(s => Guid.NewGuid()).FirstOrDefault();
			_questionButtons[i].GetComponentInChildren<Text>().text = questionText;
			_questionButtons[i].onClick.RemoveAllListeners();
			_questionButtons[i].onClick.AddListener(delegate { AskQuestion(selected, questionText); });
			Button thisButton = _questionButtons[i];
			_questionButtons[i].onClick.AddListener(delegate { thisButton.interactable = false; });
		}
		CostCheck();
	}

	void CostCheck()
	{
		int allowance = _recruitMember.QuestionAllowance();
		if (allowance < 1)
		{
			for (int i = 0; i < _questionButtons.Length; i++)
			{
				_questionButtons[i].interactable = false;
			}
			for (int i = 0; i < _recruitUI.Length; i++)
			{
				_recruitUI[i].transform.Find("Name").GetComponent<Button>().interactable = false;
			}
		}
	}

	public void AskQuestion(CrewMemberSkill skill, string questionText)
	{
		Tracker.T.alternative.Selected("Recruitment", skill + " Question", AlternativeTracker.Alternative.Question);
		SetDialogueText(questionText);
		Dictionary<CrewMember, string> replies = _recruitMember.AskQuestion(skill);
		foreach (GameObject recruit in _recruitUI)
		{
			string reply = replies.Where(r => r.Key.Name == recruit.name).FirstOrDefault().Value;
			recruit.transform.Find("Dialogue Box/Dialogue").GetComponent<Text>().text = reply;
		}
		CostCheck();
	}

	public void SetDialogueText(string text)
	{
		_dialogueText.text = text;
	}

	public void Recruit(CrewMember crewMember)
	{
		Tracker.T.trackedGameObject.Interacted("Hired Crew Member", GameObjectTracker.TrackedGameObject.Npc);
		_recruitMember.Recruit(crewMember);
		_teamSelectionUI.ResetCrew();
		gameObject.SetActive(false);
	}
}

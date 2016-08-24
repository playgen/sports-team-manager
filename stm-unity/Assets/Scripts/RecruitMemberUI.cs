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

	void Awake()
	{
		_recruitMember = GetComponent<RecruitMember>();
	}

	void OnEnable()
	{
		Tracker.T.alternative.Selected("Recruitment", "Recruitment", AlternativeTracker.Alternative.Menu);
		ResetDisplay();
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
			_questionButtons[i].GetComponentInChildren<Text>().text = _recruitMember.GetQuestionText("Recruit" + selected).OrderBy(s => Guid.NewGuid()).FirstOrDefault() + " (2)";
			_questionButtons[i].onClick.RemoveAllListeners();
			_questionButtons[i].onClick.AddListener(delegate { AskQuestion(selected); });
			Button thisButton = _questionButtons[i];
			_questionButtons[i].onClick.AddListener(delegate { thisButton.interactable = false; });
		}
		CostCheck();
	}

	void CostCheck()
	{
		int allowance = _recruitMember.QuestionAllowance();
		if (allowance < 2)
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

	public void AskQuestion(CrewMemberSkill skill)
	{
		Tracker.T.alternative.Selected("Recruitment", skill + " Question", AlternativeTracker.Alternative.Question);
		Dictionary<CrewMember, string> replies = _recruitMember.AskQuestion(skill, 2);
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
		_recruitMember.Recruit(crewMember, 2);
		_teamSelectionUI.ResetCrew();
		gameObject.SetActive(false);
	}
}

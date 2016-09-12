using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PlayGen.RAGE.SportsTeamManager.Simulation;

public class RecruitMember : MonoBehaviour {

	private GameManager _gameManager;

	void Awake()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	public Boat GetBoat()
	{
		return _gameManager.Boat;
	}

	public List<CrewMember> GetRecruits()
	{
		return _gameManager.Boat.Recruits;
	}

	public string[] GetQuestionText(string eventKey)
	{
		return _gameManager.GetEventStrings(eventKey);
	}

	public Dictionary<CrewMember, string> AskQuestion(CrewMemberSkill skill)
	{
		return _gameManager.SendRecruitMembersEvent(skill, _gameManager.Boat.Recruits);
	}

	public void Recruit(CrewMember crewMember)
	{
		_gameManager.AddRecruit(crewMember);
	}

	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
	}

	public int StartingQuestionAllowance()
	{
		return _gameManager.GetStartingActionAllowance();
	}
}

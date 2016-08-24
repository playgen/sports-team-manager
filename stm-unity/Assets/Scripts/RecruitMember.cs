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

	public void CreateNewRecruits()
	{
		_gameManager.CreateRecruits(4);
	}

	public List<CrewMember> GetRecruits()
	{
		return _gameManager.Boat.Recruits;
	}

	public string[] GetQuestionText(string eventKey)
	{
		return _gameManager.GetEventStrings(eventKey);
	}

	public Dictionary<CrewMember, string> AskQuestion(CrewMemberSkill skill, int cost)
	{
		return _gameManager.SendRecruitMembersEvent(skill, _gameManager.Boat.Recruits, cost);
	}

	public void Recruit(CrewMember crewMember, int cost)
	{
		_gameManager.AddRecruit(crewMember, cost);
	}

	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
	}
}

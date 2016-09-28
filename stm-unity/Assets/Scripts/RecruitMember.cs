using UnityEngine;
using System.Collections.Generic;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Contains all logic to communicate between RecruitMemberUI and GameManager
/// </summary>
public class RecruitMember : MonoBehaviour {

	private GameManager _gameManager;

	private void Awake()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	/// <summary>
	/// Gets all current recruits for the boat
	/// </summary>
	public List<CrewMember> GetRecruits()
	{
		return _gameManager.Boat.Recruits;
	}

	/// <summary>
	/// Get the question text for the provided key
	/// </summary>
	public string[] GetQuestionText(string eventKey)
	{
		return _gameManager.GetEventStrings(eventKey);
	}

	/// <summary>
	/// Send a question from the player to all recruits
	/// </summary>
	public Dictionary<CrewMember, string> AskQuestion(CrewMemberSkill skill)
	{
		return _gameManager.SendRecruitMembersEvent(skill, _gameManager.Boat.Recruits);
	}

	/// <summary>
	/// Attempt to hire one of the recruits onto the team
	/// </summary>
	public void Recruit(CrewMember crewMember)
	{
		_gameManager.AddRecruit(crewMember);
	}

	/// <summary>
	/// Get the current ActionAllowance remaining
	/// </summary>
	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
	}

	/// <summary>
	/// Get the amount of ActionAllowance provided at the beginning of the race session
	/// </summary>
	public int StartingQuestionAllowance()
	{
		return _gameManager.GetStartingActionAllowance();
	}
}

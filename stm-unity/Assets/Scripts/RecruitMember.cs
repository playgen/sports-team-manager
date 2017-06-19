﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Contains all logic to communicate between RecruitMemberUI and GameManager
/// </summary>
public class RecruitMember : MonoBehaviour {

	private GameManager _gameManager;

	private void Awake()
	{
		_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
	}

	/// <summary>
	/// Gets all current recruits for the boat
	/// </summary>
	public List<CrewMember> GetRecruits()
	{
		return _gameManager.Team.Recruits.Values.ToList();
	}

	/// <summary>
	/// Get the question text for the provided key
	/// </summary>
	public string[] GetQuestionText(string eventKey)
	{
		return _gameManager.EventController.GetEventStrings(eventKey);
	}

	/// <summary>
	/// Send a question from the player to all recruits
	/// </summary>
	public Dictionary<CrewMember, string> AskQuestion(CrewMemberSkill skill)
	{
		return _gameManager.SendRecruitMembersEvent(skill, GetRecruits());
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
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey)
	{
		return _gameManager.GetConfigValue(eventKey);
	}

	/// <summary>
	/// Get the amount of ActionAllowance provided at the beginning of the race session
	/// </summary>
	public int StartingQuestionAllowance()
	{
		return _gameManager.GetStartingActionAllowance();
	}

	/// <summary>
	/// Get the current session for the current race
	/// </summary>
	public string SessionInRace()
	{
		return (_gameManager.CurrentRaceSession + 1) + "/" + _gameManager.RaceSessionLength;
	}

	/// <summary>
	/// Get the number of Crew Members within the team
	/// </summary>
	public int TeamSize()
	{
		return _gameManager.Team.CrewMembers.Count;
	}

	/// <summary>
	/// Get the amount of sessions since the boat type had changed
	/// </summary>
	public int SessionsSinceLastChange()
	{
		var history = _gameManager.Team.LineUpHistory.AsEnumerable().Reverse().ToList();
		var firstMismatch = history.FirstOrDefault(b => b.Type != _gameManager.Team.Boat.Type);
		return firstMismatch != null ? history.IndexOf(firstMismatch) : 0;
	}
}

using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Contains all logic to communicate between MemberMeetingUI and GameManager
/// </summary>
public class MemberMeeting : MonoBehaviour
{
	private GameManager _gameManager;

	private void Awake()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	/// <summary>
	/// Get the current position (if any) of a CrewMember
	/// </summary>
	public Position GetCrewMemberPosition(CrewMember crewMember)
	{
		var boatPosition = crewMember.GetBoatPosition(_gameManager.Team.Boat.BoatPositionCrew);
		return boatPosition;
	}

	/// <summary>
	/// Get player text for a question
	/// </summary>
	public string[] GetEventText(string eventKey)
	{
		return _gameManager.EventController.GetEventStrings(eventKey);
	}

	/// <summary>
	/// Send question asked by player to CrewMember, get their reply in response
	/// </summary>
	public string AskQuestion(string eventKey, CrewMember crewMember)
	{
		return _gameManager.SendMeetingEvent(eventKey, crewMember);
	}

	/// <summary>
	/// Get the amount of available ActionAllowance remaining for this race
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
	/// Get the amount of available ActionAllowance given at the start of this race
	/// </summary>
	public int StartingQuestionAllowance()
	{
		return _gameManager.GetStartingActionAllowance();
	}

	/// <summary>
	/// Get the amount of hire/fire actions remaining for this race
	/// </summary>
	public int CrewEditAllowance()
	{
		return _gameManager.CrewEditAllowance;
	}

	/// <summary>
	/// Check if it is allowable to fire crew members at this point
	/// </summary>
	public bool CanRemoveCheck()
	{
		return _gameManager.Team.CanRemoveFromCrew();
	}

	/// <summary>
	/// Remove a CrewMember from the team
	/// </summary>
	public void FireCrewMember(CrewMember crewMember)
	{
		_gameManager.RetireCrewMember(crewMember);
	}
}

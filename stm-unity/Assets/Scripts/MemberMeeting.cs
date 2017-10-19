using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Contains all logic to communicate between MemberMeetingUI and GameManager
/// </summary>
public class MemberMeeting
{
	/// <summary>
	/// Get the current position (if any) of a CrewMember
	/// </summary>
	public Position GetCrewMemberPosition(CrewMember crewMember)
	{
		var position = crewMember.GetBoatPosition(GameManagement.GameManager.Team.Boat.PositionCrew);
		return position;
	}

	/// <summary>
	/// Get the name of the team manager
	/// </summary>
	public string GetManagerName()
	{
		return GameManagement.GameManager.Team.Manager.Name;
	}

	/// <summary>
	/// Get player text for a question
	/// </summary>
	public string[] GetEventText(string eventKey)
	{
		return GameManagement.GameManager.EventController.GetEventStrings(eventKey);
	}

	/// <summary>
	/// Send question asked by player to CrewMember, get their reply in response
	/// </summary>
	public List<string> AskQuestion(string eventKey, CrewMember crewMember)
	{
		return GameManagement.GameManager.SendMeetingEvent(eventKey, crewMember);
	}

	/// <summary>
	/// Get the amount of available ActionAllowance remaining for this race
	/// </summary>
	public int QuestionAllowance()
	{
		return GameManagement.GameManager.ActionAllowance;
	}

	/// <summary>
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey, CrewMember member = null)
	{
		return GameManagement.GameManager.GetConfigValue(eventKey, member);
	}

	/// <summary>
	/// Get the amount of available ActionAllowance given at the start of this race
	/// </summary>
	public int StartingQuestionAllowance()
	{
		return GameManagement.GameManager.GetStartingActionAllowance();
	}

	/// <summary>
	/// Get the amount of starting CrewEditAllowance
	/// </summary>
	public int StartingCrewEditAllowance()
	{
		return GameManagement.GameManager.GetStartingCrewEditAllowance();
	}

	/// <summary>
	/// Get the amount of hire/fire actions remaining for this race
	/// </summary>
	public int CrewEditAllowance()
	{
		return GameManagement.GameManager.CrewEditAllowance;
	}

	/// <summary>
	/// Check if it is allowable to fire crew members at this point
	/// </summary>
	public bool CanRemoveCheck()
	{
		return GameManagement.GameManager.Team.CanRemoveFromCrew();
	}

	/// <summary>
	/// Remove a CrewMember from the team
	/// </summary>
	public void FireCrewMember(CrewMember crewMember)
	{
        GameManagement.GameManager.RetireCrewMember(crewMember);
	}

	/// <summary>
	/// Check if the tutorial is currently in progress
	/// </summary>
	public bool TutorialInProgress()
	{
		return GameManagement.GameManager.ShowTutorial;
	}

	/// <summary>
	/// Get the current session in this race
	/// </summary>
	public string SessionInRace()
	{
		return (GameManagement.GameManager.CurrentRaceSession + 1) + "/" + GameManagement.GameManager.RaceSessionLength;
	}

	/// <summary>
	/// Get the current amount of CrewMembers in the team
	/// </summary>
	public int TeamSize()
	{
		return GameManagement.GameManager.Team.CrewMembers.Count;
	}

	/// <summary>
	/// Get the amount of races this CrewMember has been involved in
	/// </summary>
	public int GetTimeInTeam(CrewMember crewMember)
	{
		return GameManagement.GameManager.Team.LineUpHistory.Count(boat => boat.PositionCrew.Values.ToList().Any(c => c.Name == crewMember.Name));
	}
}

using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Contains all logic to communicate between MemberMeetingUI and GameManager
/// </summary>
public class MemberMeeting
{
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
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey, CrewMember member = null)
	{
		return GameManagement.GameManager.GetConfigValue(eventKey, member);
	}

	/// <summary>
	/// Remove a CrewMember from the team
	/// </summary>
	public void FireCrewMember(CrewMember crewMember)
	{
		GameManagement.GameManager.RetireCrewMember(crewMember);
	}

	/// <summary>
	/// Get the amount of races this CrewMember has been involved in
	/// </summary>
	public int GetTimeInTeam(CrewMember crewMember)
	{
		return GameManagement.LineUpHistory.Count(boat => boat.PositionCrew.Values.ToList().Any(c => c.Name == crewMember.Name));
	}
}

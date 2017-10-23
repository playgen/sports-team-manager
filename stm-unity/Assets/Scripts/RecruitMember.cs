using System.Collections.Generic;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Contains all logic to communicate between RecruitMemberUI and GameManager
/// </summary>
public class RecruitMember {
	/// <summary>
	/// Gets all current recruits for the boat
	/// </summary>
	public List<CrewMember> GetRecruits()
	{
		return GameManagement.GameManager.Team.Recruits.Values.ToList();
	}

	/// <summary>
	/// Get the question text for the provided key
	/// </summary>
	public string[] GetQuestionText(string eventKey)
	{
		return GameManagement.GameManager.EventController.GetEventStrings(eventKey);
	}

	/// <summary>
	/// Send a question from the player to all recruits
	/// </summary>
	public Dictionary<CrewMember, string> AskQuestion(CrewMemberSkill skill)
	{
		return GameManagement.GameManager.SendRecruitMembersEvent(skill, GetRecruits());
	}

	/// <summary>
	/// Attempt to hire one of the recruits onto the team
	/// </summary>
	public void Recruit(CrewMember crewMember)
	{
		GameManagement.GameManager.AddRecruit(crewMember);
	}

	/// <summary>
	/// Get the current ActionAllowance remaining
	/// </summary>
	public int QuestionAllowance()
	{
		return GameManagement.GameManager.ActionAllowance;
	}

	/// <summary>
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey)
	{
		return GameManagement.GameManager.GetConfigValue(eventKey);
	}

	/// <summary>
	/// Get the amount of ActionAllowance provided at the beginning of the race session
	/// </summary>
	public int StartingQuestionAllowance()
	{
		return GameManagement.GameManager.GetStartingActionAllowance();
	}

	/// <summary>
	/// Get the current session for the current race
	/// </summary>
	public string SessionInRace()
	{
		return (GameManagement.GameManager.CurrentRaceSession + 1) + "/" + GameManagement.GameManager.RaceSessionLength;
	}

	/// <summary>
	/// Get the number of Crew Members within the team
	/// </summary>
	public int TeamSize()
	{
		return GameManagement.GameManager.Team.CrewMembers.Count;
	}

	/// <summary>
	/// Get the amount of sessions since the boat type had changed
	/// </summary>
	public int SessionsSinceLastChange()
	{
		var history = GameManagement.GameManager.Team.LineUpHistory.AsEnumerable().Reverse().ToList();
		var firstMismatch = history.FirstOrDefault(b => b.Type != GameManagement.GameManager.Team.Boat.Type);
		return firstMismatch != null ? history.IndexOf(firstMismatch) : 0;
	}
}

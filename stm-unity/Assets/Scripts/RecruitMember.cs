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
		return GameManagement.Team.Recruits.Values.ToList();
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
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey)
	{
		return GameManagement.GameManager.GetConfigValue(eventKey);
	}

	/// <summary>
	/// Get the amount of sessions since the boat type had changed
	/// </summary>
	public int SessionsSinceLastChange()
	{
		var history = GameManagement.LineUpHistory.AsEnumerable().Reverse().ToList();
		var firstMismatch = history.FirstOrDefault(b => b.Type != GameManagement.Boat.Type);
		return firstMismatch != null ? history.IndexOf(firstMismatch) : 0;
	}
}

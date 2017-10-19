using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

/// <summary>
/// Contains all logic to communicate between PositionDisplayUI and GameManager
/// </summary>
public class PositionDisplay
{
	/// <summary>
	/// Get the current team
	/// </summary>
	public Team GetTeam()
	{
		return GameManagement.GameManager.Team;
	}

	/// <summary>
	/// Get the history of line-ups
	/// </summary>
	public List<Boat> GetLineUpHistory()
	{
		return GameManagement.GameManager.Team.LineUpHistory;
	}
}
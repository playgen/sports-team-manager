using System.Collections.Generic;

/// <summary>
/// Connecting class between GameManager in logic and the Feedback UI
/// </summary>
public class Feedback {

	/// <summary>
	/// Get a dictionary of management styles and the percentage of their use
	/// </summary>
	public Dictionary<string, float> GatherManagementStyles()
	{
		return GameManagement.GameManager.GatherManagementStyles();
	}

	/// <summary>
	/// Get a dictionary of leaderboard styles and the percentage of their use
	/// </summary>
	public Dictionary<string, float> GatherLeadershipStyles()
	{
		return GameManagement.GameManager.GatherLeadershipStyles();
	}

	/// <summary>
	/// Get an array of the most used leaderboard styles
	/// </summary>
	public string[] GetPrevalentLeadershipStyle()
	{
		return GameManagement.GameManager.GetPrevalentLeadershipStyle();
	}
}

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains all logic to communicate between TeamSelectionUI and GameManager
/// </summary>
public class TeamSelection {
	public void Start()
	{
		GameManagement.PostRaceEvent.GetEvent();
	}

	/// <summary>
	/// Get the history of results, taking and skipping the amount given
	/// </summary>
	public List<KeyValuePair<Boat, KeyValuePair<int, int>>> GetLineUpHistory(int skipAmount, int takeAmount)
	{
		var boats = GameManagement.LineUpHistory.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var offsets = GameManagement.Team.HistoricTimeOffset.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var sessions = GameManagement.Team.HistoricSessionNumber.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var boatOffsets = new List<KeyValuePair<Boat, KeyValuePair<int, int>>>();
		for (var i = 0; i < boats.Count; i++)
		{
			if (i < offsets.Count)
			{
				boatOffsets.Add(new KeyValuePair<Boat, KeyValuePair<int, int>>(boats[i], new KeyValuePair<int, int>(offsets[i], sessions[i])));
			}
		}
		return boatOffsets;
	}

	/// <summary>
	/// Get the history of race results
	/// </summary>
	public List<KeyValuePair<int, int>> GetRaceResults()
	{
		return GameManagement.Team.RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList();
	}

	/// <summary>
	/// Confirm the line-up and get the details for the boat line-up used
	/// </summary>
	public Boat ConfirmLineUp(int offset = 0)
	{
		GameManagement.GameManager.SaveLineUp(offset);
		GameManagement.PostRaceEvent.GetEvent();
		return GameManagement.LineUpHistory.Last();
	}

	/// <summary>
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey)
	{
		return GameManagement.GameManager.GetConfigValue(eventKey);
	}

	/// <summary>
	/// Check if the questionnaire has been completed for this game
	/// </summary>
	public bool QuestionnaireCompleted()
	{
		return GameManagement.GameManager.QuestionnaireCompleted;
	}
}

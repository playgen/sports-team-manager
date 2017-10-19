using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains all logic to communicate between TeamSelectionUI and GameManager
/// </summary>
public class TeamSelection {
	private int _confirmCount;

	public void Start()
	{
		GameManagement.PostRaceEvent.GetEvent();
		_confirmCount = GameManagement.GameManager.Team.LineUpHistory.Count;
	}

	/// <summary>
	/// Get the history of results, taking and skipping the amount given
	/// </summary>
	public List<KeyValuePair<Boat, KeyValuePair<int, int>>> GetLineUpHistory(int skipAmount, int takeAmount)
	{
		var boats = GameManagement.GameManager.Team.LineUpHistory.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var offsets = GameManagement.GameManager.Team.HistoricTimeOffset.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var sessions = GameManagement.GameManager.Team.HistoricSessionNumber.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
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
		return GameManagement.GameManager.Team.RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList();
	}

	/// <summary>
	/// Get the currently available crew
	/// </summary>
	public List<CrewMember> LoadCrew()
	{
		return GameManagement.GameManager.Team.CrewMembers.Values.ToList();
	}

	/// <summary>
	/// Get the current team
	/// </summary>
	public Team GetTeam()
	{
		return GameManagement.GameManager.Team;
	}

	/// <summary>
	/// Assign a CrewMember to a Position
	/// </summary>
	public void AssignCrew(CrewMember crewMember, Position position)
	{
		GameManagement.GameManager.Team.Boat.AssignCrewMember(position, crewMember);
	}

	/// <summary>
	/// Remove a CrewMember from their position
	/// </summary>
	public void RemoveCrew(CrewMember crewMember)
	{
		GameManagement.GameManager.Team.Boat.AssignCrewMember(0, crewMember);
	}

	/// <summary>
	/// Get the amount of sessions raced
	/// </summary>
	public int GetTotalStages()
	{
		return _confirmCount + 1;
	}

	/// <summary>
	/// Get the current session in the race
	/// </summary>
	public int GetStage()
	{
		return GameManagement.GameManager.CurrentRaceSession + 1;
	}

	/// <summary>
	/// Get the amount of sessions in this race
	/// </summary>
	public int GetSessionLength()
	{
		return GameManagement.GameManager.RaceSessionLength;
	}

	/// <summary>
	/// Skips all remaining practice sessions (if any)
	/// </summary>
	public void SkipToRace()
	{
		GameManagement.GameManager.SkipToRace();
	}

	/// <summary>
	/// Confirm the line-up and get the details for the boat line-up used
	/// </summary>
	public Boat ConfirmLineUp(int offset = 0)
	{
		_confirmCount++;
		GameManagement.GameManager.SaveLineUp(offset);
        GameManagement.PostRaceEvent.GetEvent();
		return GameManagement.GameManager.Team.LineUpHistory.Last();
	}

	/// <summary>
	/// Get the amount of ActionAllowance remaining
	/// </summary>
	public int QuestionAllowance()
	{
		return GameManagement.GameManager.ActionAllowance;
	}

	/// <summary>
	/// Get the amount of starting ActionAllowance
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
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey)
	{
		return GameManagement.GameManager.GetConfigValue(eventKey);
	}

	/// <summary>
	/// Get the amount of hire/fire actions remaining for this race
	/// </summary>
	public int CrewEditAllowance()
	{
		return GameManagement.GameManager.CrewEditAllowance;
	}

	/// <summary>
	/// Check if the player is able to hire another character onto the team
	/// </summary>
	public bool CanAddCheck()
	{
		return GameManagement.GameManager.Team.CanAddToCrew();
	}

	/// <summary>
	/// Check the amount of players below the crew limit this boat currently is
	/// </summary>
	public int CanAddAmount()
	{
		return GameManagement.GameManager.Team.CrewLimitLeft();
	}

	/// <summary>
	/// Get the top amount of current mistakes in crew assignment the player is making
	/// </summary>
	public List<string> GetAssignmentMistakes(int amount)
	{
		return GameManagement.GameManager.Team.Boat.GetAssignmentMistakes(amount);
	}

	/// <summary>
	/// Get the average team mood
	/// </summary>
	public float GetTeamAverageMood()
	{
		return GameManagement.GameManager.Team.AverageTeamMood();
	}

	/// <summary>
	/// Get the average team manager opinion
	/// </summary>
	public float GetTeamAverageManagerOpinion()
	{
		return GameManagement.GameManager.Team.AverageTeamManagerOpinion();
	}

	/// <summary>
	/// Get the average team opinion
	/// </summary>
	public float GetTeamAverageOpinion()
	{
		return GameManagement.GameManager.Team.AverageTeamOpinion();
	}

	/// <summary>
	/// Get the average boat mood
	/// </summary>
	public float GetBoatAverageMood()
	{
		return GameManagement.GameManager.Team.Boat.AverageBoatMood();
	}

	/// <summary>
	/// Get the average boat manager opinion
	/// </summary>
	public float GetBoatAverageManagerOpinion()
	{
		return GameManagement.GameManager.Team.Boat.AverageBoatManagerOpinion(GetTeam().Manager.Name);
	}

	/// <summary>
	/// Get the average boat opinion
	/// </summary>
	public float GetBoatAverageOpinion()
	{
		return GameManagement.GameManager.Team.Boat.AverageBoatOpinion();
	}

	/// <summary>
	/// Check if the tutorial is currently in progress
	/// </summary>
	public bool TutorialInProgress()
	{
		return GameManagement.GameManager.ShowTutorial;
	}

	/// <summary>
	/// Check if the questionnaire has been completed for this game
	/// </summary>
	public bool QuestionnaireCompleted()
	{
		return GameManagement.GameManager.QuestionnaireCompleted;
	}
}

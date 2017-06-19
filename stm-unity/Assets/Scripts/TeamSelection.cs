using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains all logic to communicate between TeamSelectionUI and GameManager
/// </summary>
public class TeamSelection : MonoBehaviour {
	private GameManager _gameManager;
	[SerializeField]
	private PostRaceEvent _postRaceEvent;
	private int _confirmCount;

	private void Start()
	{
		_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		_postRaceEvent.GetEvent();
		_confirmCount = _gameManager.Team.LineUpHistory.Count;
	}

	/// <summary>
	/// Get the history of results, taking and skipping the amount given
	/// </summary>
	public List<KeyValuePair<Boat, KeyValuePair<int, int>>> GetLineUpHistory(int skipAmount, int takeAmount)
	{
		var boats = _gameManager.Team.LineUpHistory.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var offsets = _gameManager.Team.HistoricTimeOffset.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
		var sessions = _gameManager.Team.HistoricSessionNumber.AsEnumerable().Reverse().Skip(skipAmount).Take(takeAmount).ToList();
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
		return _gameManager.Team.RaceHistory.Select(r => new KeyValuePair<int, int>(r.Score, r.Positions.Count)).ToList();
	}

	/// <summary>
	/// Get the currently available crew
	/// </summary>
	public List<CrewMember> LoadCrew()
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.Team.CrewMembers.Values.ToList();
	}

	/// <summary>
	/// Get the current team
	/// </summary>
	public Team GetTeam()
	{
		if (_gameManager == null)
		{
			_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
		}
		return _gameManager.Team;
	}

	/// <summary>
	/// Assign a CrewMember to a Position
	/// </summary>
	public void AssignCrew(CrewMember crewMember, Position position)
	{
		_gameManager.Team.Boat.AssignCrewMember(position, crewMember);
	}

	/// <summary>
	/// Remove a CrewMember from their position
	/// </summary>
	public void RemoveCrew(CrewMember crewMember)
	{
		_gameManager.Team.Boat.AssignCrewMember(0, crewMember);
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
		return _gameManager.CurrentRaceSession + 1;
	}

	/// <summary>
	/// Get the amount of sessions in this race
	/// </summary>
	public int GetSessionLength()
	{
		return _gameManager.RaceSessionLength;
	}

	/// <summary>
	/// Skips all remaining practice sessions (if any)
	/// </summary>
	public void SkipToRace()
	{
		_gameManager.SkipToRace();
	}

	/// <summary>
	/// Confirm the line-up and get the details for the boat line-up used
	/// </summary>
	public Boat ConfirmLineUp(int offset = 0)
	{
		_confirmCount++;
		_gameManager.SaveLineUp(offset);
		_postRaceEvent.GetEvent();
		return _gameManager.Team.LineUpHistory.Last();
	}

	/// <summary>
	/// Get the amount of ActionAllowance remaining
	/// </summary>
	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
	}

	/// <summary>
	/// Get the amount of starting ActionAllowance
	/// </summary>
	public int StartingQuestionAllowance()
	{
		return _gameManager.GetStartingActionAllowance();
	}

	/// <summary>
	/// Get the amount of starting CrewEditAllowance
	/// </summary>
	public int StartingCrewEditAllowance()
	{
		return _gameManager.GetStartingCrewEditAllowance();
	}

	/// <summary>
	/// Get the value stored in the config
	/// </summary>
	public float GetConfigValue(ConfigKeys eventKey)
	{
		return _gameManager.GetConfigValue(eventKey);
	}

	/// <summary>
	/// Get the amount of hire/fire actions remaining for this race
	/// </summary>
	public int CrewEditAllowance()
	{
		return _gameManager.CrewEditAllowance;
	}

	/// <summary>
	/// Check if the player is able to hire another character onto the team
	/// </summary>
	public bool CanAddCheck()
	{
		return _gameManager.Team.CanAddToCrew();
	}

	/// <summary>
	/// Check the amount of players below the crew limit this boat currently is
	/// </summary>
	public int CanAddAmount()
	{
		return _gameManager.Team.CrewLimitLeft();
	}

	/// <summary>
	/// Get the top amount of current mistakes in crew assignment the player is making
	/// </summary>
	public List<string> GetAssignmentMistakes(int amount)
	{
		return _gameManager.Team.Boat.GetAssignmentMistakes(amount);
	}

	/// <summary>
	/// Get the average team mood
	/// </summary>
	public float GetTeamAverageMood()
	{
		return _gameManager.Team.AverageTeamMood();
	}

	/// <summary>
	/// Get the average team manager opinion
	/// </summary>
	public float GetTeamAverageManagerOpinion()
	{
		return _gameManager.Team.AverageTeamManagerOpinion();
	}

	/// <summary>
	/// Get the average team opinion
	/// </summary>
	public float GetTeamAverageOpinion()
	{
		return _gameManager.Team.AverageTeamOpinion();
	}

	/// <summary>
	/// Get the average boat mood
	/// </summary>
	public float GetBoatAverageMood()
	{
		return _gameManager.Team.Boat.AverageBoatMood();
	}

	/// <summary>
	/// Get the average boat manager opinion
	/// </summary>
	public float GetBoatAverageManagerOpinion()
	{
		return _gameManager.Team.Boat.AverageBoatManagerOpinion(GetTeam().Manager.Name);
	}

	/// <summary>
	/// Get the average boat opinion
	/// </summary>
	public float GetBoatAverageOpinion()
	{
		return _gameManager.Team.Boat.AverageBoatOpinion();
	}

	/// <summary>
	/// Check if the tutorial is currently in progress
	/// </summary>
	public bool TutorialInProgress()
	{
		return _gameManager.ShowTutorial;
	}

	/// <summary>
	/// Check if the questionnaire has been completed for this game
	/// </summary>
	public bool QuestionnaireCompleted()
	{
		return _gameManager.QuestionnaireCompleted;
	}
}

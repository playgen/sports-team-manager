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
	private int _sessionLength;
	private int _confirmCount;

	private void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		_sessionLength = _gameManager.RaceSessionLength;
	}

	/// <summary>
	/// Get the history of line-ups
	/// </summary>
	public Dictionary<Boat, int> GetLineUpHistory()
	{
		var boats = _gameManager.Team.LineUpHistory;
		var offsets = _gameManager.Team.HistoricTimeOffset;
		var boatOffsets = new Dictionary<Boat, int>();
		for (var i = 0; i < boats.Count; i++)
		{
			if (i < offsets.Count)
			{
				boatOffsets.Add(boats[i], offsets[i]);
			}
		}
		return boatOffsets;
	}

	/// <summary>
	/// Get the currently available crew for the active Boat
	/// </summary>
	public List<CrewMember> LoadCrew()
	{
		if (_gameManager == null)
		{
			_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		}
		return _gameManager.Team.CrewMembers.Values.ToList();
	}

	/// <summary>
	/// Get the history of line-ups
	/// </summary>
	public Team GetTeam()
	{
		return _gameManager.Team;
	}

	/// <summary>
	/// Assign a CrewMember to a Position on the active boat
	/// </summary>
	public void AssignCrew(CrewMember crewMember, Position position)
	{
		_gameManager.Team.Boat.AssignCrew(position, crewMember);
	}

	/// <summary>
	/// Remove a CrewMember from their position on the active boat
	/// </summary>
	public void RemoveCrew(CrewMember crewMember)
	{
		_gameManager.Team.Boat.AssignCrew(0, crewMember);
	}

	/// <summary>
	/// Get the current session in the race
	/// </summary>
	public int GetStage()
	{
		return _confirmCount + 1;
	}

	/// <summary>
	/// Get the amount of sessions in this race
	/// </summary>
	public int GetSessionLength()
	{
		return _sessionLength;
	}

	/// <summary>
	/// Confirm the line-up and get its score
	/// </summary>
	public Boat ConfirmLineUp(int offset = 0, bool historical = false)
	{
		_confirmCount++;
		if (historical)
		{
			if (_confirmCount >= _sessionLength)
			{
				_confirmCount -= _sessionLength;
			}
		}
		else
		{
			_gameManager.SaveLineUp(offset);
			if (_confirmCount >= _sessionLength)
			{
				_gameManager.ConfirmLineUp();
				_confirmCount -= _sessionLength;
			}
			_postRaceEvent.GetEvent();
		}
		return _gameManager.Team.LineUpHistory.Last();
	}

	/// <summary>
	/// If a race has just occured, return true
	/// </summary>
	public bool IsRace()
	{
		if (_confirmCount == 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Get the current ideal score
	/// </summary>
	public float IdealCheck()
	{
		return _gameManager.Team.Boat.IdealMatchScore;
	}

	/// <summary>
	/// Get the amount of ActionAllowance remaining
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
}

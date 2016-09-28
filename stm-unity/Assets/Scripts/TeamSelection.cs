using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

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
		List<Boat> boats = _gameManager.LineUpHistory;
		List<int> offsets = _gameManager.HistoricTimeOffset;
		Dictionary<Boat, int> boatOffsets = new Dictionary<Boat, int>();
		for (int i = 0; i < boats.Count; i++)
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
		_gameManager.RemoveAllCrew();
		return _gameManager.Boat.UnassignedCrew;
	}

	/// <summary>
	/// Get the history of line-ups
	/// </summary>
	public Boat GetBoat()
	{
		return _gameManager.Boat;
	}

	/// <summary>
	/// Assign a CrewMember to a Position on the active boat
	/// </summary>
	public void AssignCrew(CrewMember crewMember, Position position)
	{
		_gameManager.AssignCrew(position, crewMember);
	}

	/// <summary>
	/// Remove a CrewMember from their position on the active boat
	/// </summary>
	public void RemoveCrew(CrewMember crewMember)
	{
		_gameManager.AssignCrew(null, crewMember);
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
	public int ConfirmLineUp(int offset = 0, bool historical = false)
	{
		int score = 0;
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
			score = _gameManager.Boat.BoatScore;
			if (_confirmCount >= _sessionLength)
			{
				_gameManager.ConfirmLineUp();
				_postRaceEvent.GetEvent();
				_confirmCount -= _sessionLength;
			}
		}
		return score;
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
		return _gameManager.Boat.IdealMatchScore;
	}

	/// <summary>
	/// Get the amount of ActionAllowance remaining
	/// </summary>
	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
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
		return _gameManager.CanAddToCrew();
	}

	/// <summary>
	/// Check the amount of players below the crew limit this boat currently is
	/// </summary>
	public int CanAddAmount()
	{
		return _gameManager.CrewLimitLeft();
	}

	/// <summary>
	/// Get the top amount of current mistakes in crew assignment the player is making
	/// </summary>
	public List<string> GetAssignmentMistakes(int amount)
	{
		return _gameManager.GetAssignmentMistakes(amount);
	}
}

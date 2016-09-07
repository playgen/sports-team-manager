using UnityEngine;
using System.Collections;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

public class MemberMeeting : MonoBehaviour
{
	private GameManager _gameManager;

	void Awake()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	public Boat GetBoat()
	{
		return _gameManager.Boat;
	}

	/// <summary>
	/// Get the current position (if any) of a CrewMember
	/// </summary>
	public string GetCrewMemberPosition(CrewMember crewMember)
	{
		return crewMember.GetPosition(_gameManager.Boat);
	}

	public string[] GetEventText(string eventKey)
	{
		return _gameManager.GetEventStrings(eventKey);
	}

	public string[] AskQuestion(string context, string eventKey, CrewMember crewMember)
	{
		return _gameManager.SendMeetingEvent(context, eventKey, new List<CrewMember>() { crewMember });
	}

	public int QuestionAllowance()
	{
		return _gameManager.ActionAllowance;
	}

	public int CrewEditAllowance()
	{
		return _gameManager.CrewEditAllowance;
	}

	public bool CanRemoveCheck()
	{
		return _gameManager.CanRemoveFromCrew();
	}

	public void FireCrewMember(CrewMember crewMember)
	{
		_gameManager.RetireCrewMember(crewMember);
	}
}

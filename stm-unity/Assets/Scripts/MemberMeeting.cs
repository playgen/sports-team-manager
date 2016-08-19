using UnityEngine;
using System.Collections;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using System.Collections.Generic;

public class MemberMeeting : MonoBehaviour
{
	private GameManager _gameManager;

	void Start()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
	}

	public string[] AskStatQuestion(CrewMember crewMember)
	{
		return _gameManager.SendBoatMembersEvent("SoloInterview", "StatReveal", new List<CrewMember>() { crewMember });
	}

	public string[] AskRoleQuestion(CrewMember crewMember)
	{
		return _gameManager.SendBoatMembersEvent("SoloInterview", "RoleReveal", new List<CrewMember>() { crewMember });
	}

	public string[] AskOpinionPositiveQuestion(CrewMember crewMember)
	{
		return new string[0];
	}

	public string[] AskOpinionNegativeQuestion(CrewMember crewMember)
	{
		return new string[0];
	}
}

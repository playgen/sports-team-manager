using System;
using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;

public class PositionUI : MonoBehaviour {

	private TeamSelection _teamSelection;
	[SerializeField]
	private Position _position;
	private CrewMemberUI _crewMemberUI;

	public void SetUp(TeamSelection teamSelection, Position position)
	{
		_teamSelection = teamSelection;
		_position = position;
	}

	public void LinkCrew(CrewMemberUI crewmember)
	{
		if (_crewMemberUI != null)
		{
			_crewMemberUI.Reset();
		}
		_crewMemberUI = crewmember;
		crewmember.ReplacedEvent += new EventHandler(OnReset);
	}

	public string GetName()
	{
		return _position.Name;
	}

	private void OnReset(object sender, EventArgs e)
	{
		_crewMemberUI.ReplacedEvent -= new EventHandler(OnReset);
		_crewMemberUI = null;
	}
}

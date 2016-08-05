using System;
using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;

public class PositionUI : MonoBehaviour {

	private TeamSelectionUI _teamSelectionUI;
	[SerializeField]
	private Position _position;
	private CrewMemberUI _crewMemberUI;

	public void SetUp(TeamSelectionUI teamSelectionUI, Position position)
	{
		_teamSelectionUI = teamSelectionUI;
		_position = position;
	}

	public void ShowPopUp()
	{
		_teamSelectionUI.DisplayPositionPopUp(_position);
	}

	public void LinkCrew(CrewMemberUI crewmember)
	{
		if (_crewMemberUI != null)
		{
			_crewMemberUI.ReplacedEvent -= new EventHandler(OnReset);
			_crewMemberUI.Reset();
			_teamSelectionUI.PositionChange(-1);
		}
		_crewMemberUI = crewmember;
		_teamSelectionUI.PositionChange(1);
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
		_teamSelectionUI.PositionChange(-1);
	}

	public void LockPosition(int score)
	{
		if (_crewMemberUI != null)
		{
			_crewMemberUI.RevealScore(score);
		}
	}
}

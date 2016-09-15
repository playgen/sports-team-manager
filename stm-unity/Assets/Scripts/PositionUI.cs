using System;
using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PositionUI : MonoBehaviour {

	private TeamSelectionUI _teamSelectionUI;
	private PositionDisplayUI _positionUI;
	[SerializeField]
	private Position _position;
	private CrewMemberUI _crewMemberUI;

	public void SetUp(TeamSelectionUI teamSelectionUI, PositionDisplayUI positionUI, Position position)
	{
		_teamSelectionUI = teamSelectionUI;
		_position = position;
		_positionUI = positionUI;
	}

	/// <summary>
	/// Display the information pop-up for Positions
	/// </summary>
	public void ShowPopUp()
	{
		_positionUI.Display(_position);
	}

	/// <summary>
	/// Store a reference to the CrewMemberUI for the CrewMember currently attached to this PositionUI's Position
	/// </summary>
	public void LinkCrew(CrewMemberUI crewmember)
	{
		if (crewmember != _crewMemberUI)
		{
			if (_crewMemberUI != null)
			{
				_crewMemberUI.Reset();
			}
			RemoveCrew();
			_crewMemberUI = crewmember;
			_teamSelectionUI.PositionChange(1);
			crewmember.ReplacedEvent += new EventHandler(OnReset);
		}
	}

	/// <summary>
	/// Return the name of this position
	/// </summary>
	public Position GetPosition()
	{
		return _position;
	}

	public void RemoveCrew()
	{
		if (_crewMemberUI != null)
		{
			_crewMemberUI.ReplacedEvent -= new EventHandler(OnReset);
			_teamSelectionUI.PositionChange(-1);
			_crewMemberUI = null;
		}
	}

	/// <summary>
	/// Triggered by a CrewMember being removed from the position. Removes listener and updates number of currently empty positions
	/// </summary>
	private void OnReset(object sender, EventArgs e)
	{
		RemoveCrew();
	}
}

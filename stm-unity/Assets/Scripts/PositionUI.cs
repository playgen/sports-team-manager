using System;
using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Contains all logic related to Position prefabs
/// </summary>
public class PositionUI : MonoBehaviour {

	private TeamSelectionUI _teamSelectionUI;
	private PositionDisplayUI _positionUI;
	private Position _position;
	private CrewMemberUI _crewMemberUI;
	public Position Position
	{
		get
		{
			return _position;
		}
	}

	/// <summary>
	/// Bring in elements that need to be known to this object
	/// </summary>
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
		_positionUI.SetUpDisplay(_position);
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
			crewmember.ReplacedEvent += OnReset;
		}
	}

	/// <summary>
	/// Remove the reference to the CrewMember previously attached to this Position
	/// </summary>
	public void RemoveCrew()
	{
		if (_crewMemberUI != null)
		{
			_crewMemberUI.ReplacedEvent -= OnReset;
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

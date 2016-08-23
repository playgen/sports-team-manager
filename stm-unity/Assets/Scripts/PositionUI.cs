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

	/// <summary>
	/// Display the information pop-up for Positions
	/// </summary>
	public void ShowPopUp()
	{
		_teamSelectionUI.DisplayPositionPopUp(_position);
	}

	/// <summary>
	/// Store a reference to the CrewMemberUI for the CrewMember currently attached to this PositionUI's Position
	/// </summary>
	public void LinkCrew(CrewMemberUI crewmember)
	{
		if (crewmember != _crewMemberUI)
		{
			Tracker.T.trackedGameObject.Interacted("Positioned Crew Member", GameObjectTracker.TrackedGameObject.Npc);
			if (_crewMemberUI != null)
			{
				Tracker.T.trackedGameObject.Interacted("Unpositioned Crew Member", GameObjectTracker.TrackedGameObject.Npc);
				_crewMemberUI.ReplacedEvent -= new EventHandler(OnReset);
				_crewMemberUI.Reset();
				_teamSelectionUI.PositionChange(-1);
			}
			_crewMemberUI = crewmember;
			_teamSelectionUI.PositionChange(1);
			crewmember.ReplacedEvent += new EventHandler(OnReset);
		}
	}

	/// <summary>
	/// Return the name of this position
	/// </summary>
	public string GetName()
	{
		return _position.Name;
	}

	/// <summary>
	/// Triggered by a CrewMember being removed from the position. Removes listener and updates number of currently empty positions
	/// </summary>
	private void OnReset(object sender, EventArgs e)
	{
		_crewMemberUI.ReplacedEvent -= new EventHandler(OnReset);
		_crewMemberUI = null;
		_teamSelectionUI.PositionChange(-1);
	}

	/// <summary>
	/// Pass the current PositionScore to the CrewMember in this position
	/// </summary>
	public void LockPosition(int score)
	{
		if (_crewMemberUI != null)
		{
			_crewMemberUI.RevealScore(score);
		}
	}
}

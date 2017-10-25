using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;

/// <summary>
/// Contains all logic related to Position prefabs
/// </summary>
public class PositionUI : MonoBehaviour
{
	private TeamSelectionUI _teamSelectionUI;
	private PositionDisplayUI _positionUI;
	private Position _position;
	private CrewMemberUI _crewMemberUI;
	public CrewMemberUI CrewMemberUI
	{
		get
		{
			return _crewMemberUI;
		}
	}
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
		_positionUI.SetUpDisplay(_position, TrackerTriggerSources.TeamManagementScreen.ToString());
	    TutorialController.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _position.ToString());
	}

	/// <summary>
	/// Store a reference to the CrewMemberUI for the CrewMember currently attached to this PositionUI's Position
	/// </summary>
	public void LinkCrew(CrewMemberUI crewMember)
	{
		if (crewMember != _crewMemberUI)
		{
			if (_crewMemberUI != null)
			{
				_crewMemberUI.OnReset();
			}
			RemoveCrew();
			_crewMemberUI = crewMember;
			_teamSelectionUI.PositionChange(1);
		}
	    TutorialController.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _position.ToString(), crewMember);
	}

	/// <summary>
	/// Remove the reference to the CrewMember previously attached to this Position
	/// </summary>
	public void RemoveCrew()
	{
		if (_crewMemberUI != null)
		{
			_teamSelectionUI.PositionChange(-1);
			_crewMemberUI = null;
		}
	}
}
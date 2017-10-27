using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;

/// <summary>
/// Contains all logic related to Position prefabs
/// </summary>
public class PositionUI : MonoBehaviour
{
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
	public void SetUp(Position position)
	{
		_position = position;
	}

	/// <summary>
	/// Display the information pop-up for Positions
	/// </summary>
	public void ShowPopUp()
	{
	    UIManagement.PositionDisplay.SetUpDisplay(_position, TrackerTriggerSources.TeamManagementScreen.ToString());
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _position.ToString());
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
		    UIManagement.TeamSelection.PositionChange(1);
		}
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, _position.ToString(), crewMember);
	}

	/// <summary>
	/// Remove the reference to the CrewMember previously attached to this Position
	/// </summary>
	public void RemoveCrew()
	{
		if (_crewMemberUI != null)
		{
		    UIManagement.TeamSelection.PositionChange(-1);
			_crewMemberUI = null;
		}
	}
}
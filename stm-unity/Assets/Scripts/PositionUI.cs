using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine;

/// <summary>
/// Contains all logic related to Position prefabs
/// </summary>
public class PositionUI : MonoBehaviour
{
	public CrewMemberUI CrewMemberUI { get; private set; }
	public Position Position { get; private set; }

	/// <summary>
	/// Bring in elements that need to be known to this object
	/// </summary>
	public void SetUp(Position position)
	{
		Position = position;
	}

	/// <summary>
	/// Display the information pop-up for Positions
	/// </summary>
	public void ShowPopUp()
	{
	    UIManagement.PositionDisplay.SetUpDisplay(Position, TrackerTriggerSource.TeamManagementScreen.ToString());
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, Position.ToString());
	}

	/// <summary>
	/// Store a reference to the CrewMemberUI for the CrewMember currently attached to this PositionUI's Position
	/// </summary>
	public void LinkCrew(CrewMemberUI crewMember)
	{
		if (crewMember != CrewMemberUI)
		{
			if (CrewMemberUI != null)
			{
				CrewMemberUI.OnReset();
			}
			RemoveCrew();
			CrewMemberUI = crewMember;
		    UIManagement.TeamSelection.PositionChange();
		}
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, Position.ToString(), crewMember);
	}

	/// <summary>
	/// Remove the reference to the CrewMember previously attached to this Position
	/// </summary>
	public void RemoveCrew()
	{
		if (CrewMemberUI != null)
		{
		    UIManagement.TeamSelection.PositionChange();
			CrewMemberUI = null;
		}
	}
}
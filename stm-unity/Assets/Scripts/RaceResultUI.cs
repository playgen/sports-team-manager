using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;
using RAGE.Analytics.Formats;
using UnityEngine;
using UnityEngine.UI;

public class RaceResultUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _postRaceCrewPrefab;
	private Dictionary<Position, CrewMember> _lastRacePositions;
	private int _lastRaceFinishPosition;
	private string _lastRaceFinishPositionText;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		_lastRacePositions = new Dictionary<Position, CrewMember>();
		_lastRaceFinishPosition = 0;
		_lastRaceFinishPositionText = string.Empty;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Display pop-up which shows the race result
	/// </summary>
	public void Display(Dictionary<Position, CrewMember> currentPositions, int finishPosition, string finishPositionText)
	{
		UIManagement.PositionDisplay.ClosePositionPopUp(string.Empty);
		UIManagement.MemberMeeting.CloseCrewMemberPopUp(string.Empty);
		UIManagement.DisableSmallBlocker();
		gameObject.Active(true);
		transform.EnableBlocker(() => Close(TrackerTriggerSources.PopUpBlocker.ToString()));
		_lastRacePositions = new Dictionary<Position, CrewMember>(currentPositions);
		_lastRaceFinishPosition = finishPosition;
		_lastRaceFinishPositionText = finishPositionText;
		foreach (Transform child in transform.Find("Crew"))
		{
			Destroy(child.gameObject);
		}
		var crewCount = 0;
		foreach (var pair in currentPositions)
		{
			var memberObject = Instantiate(_postRaceCrewPrefab);
			memberObject.transform.SetParent(transform.Find("Crew"), false);
			memberObject.name = pair.Value.Name;
			memberObject.transform.FindComponentInChildren<AvatarDisplay>("Avatar").SetAvatar(pair.Value.Avatar, -(finishPosition - 3) * 2);
			memberObject.transform.FindImage("Position").sprite = UIManagement.TeamSelection.RoleLogos.First(mo => mo.Name == pair.Key.ToString()).Image;
			memberObject.transform.FindRect("Position").offsetMin = new Vector2(10, 0);
			if (crewCount % 2 != 0)
			{
				var currentScale = memberObject.transform.Find("Avatar").localScale;
				memberObject.transform.Find("Avatar").localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
				memberObject.transform.FindRect("Position").offsetMin = new Vector2(-10, 0);
			}
			crewCount++;
			memberObject.transform.SetAsLastSibling();
		}
		transform.FindText("Result").text = Localization.GetAndFormat("RACE_RESULT_POSITION", false, GameManagement.TeamName, finishPositionText);
		DoBestFit();
		TrackerEventSender.SendEvent(new TraceEvent("ResultPopUpDisplayed", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.FinishingPosition.ToString(), finishPosition.ToString() }
		}, AccessibleTracker.Accessible.Screen));
	}

	/// <summary>
	/// Close the race result pop-up
	/// </summary>
	public void Close(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			gameObject.Active(false);
			UIManagement.DisableBlocker();
			TrackerEventSender.SendEvent(new TraceEvent("ResultPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKeys.FinishingPosition.ToString(), _lastRaceFinishPosition.ToString() },
				{ TrackerContextKeys.TriggerUI.ToString(), source }
			}, AccessibleTracker.Accessible.Screen));
			if (!GameManagement.SeasonOngoing)
			{
				UIManagement.CupResult.Display();
			}
			else
			{
				UIManagement.Promotion.Display();
			}
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
		}
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		Display(_lastRacePositions, _lastRaceFinishPosition, _lastRaceFinishPositionText);
		DoBestFit();
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Text>().Where(t => t.transform.parent == transform).BestFit();
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Extensions;

using TrackerAssetPackage;

public class RaceResultUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _postRaceCrewPrefab;
	[SerializeField]
	private Transform _crewContainer;
	[SerializeField]
	private Text _resultText;
	private int _lastRaceFinishPosition;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Display pop-up which shows the race result
	/// </summary>
	public void Display(Dictionary<Position, CrewMember> currentPositions, int finishPosition)
	{
		UIManagement.PositionDisplay.ClosePositionPopUp(string.Empty);
		UIManagement.MemberMeeting.CloseCrewMemberPopUp(string.Empty);
		UIManagement.DisableSmallBlocker();
		gameObject.Active(true);
		transform.EnableBlocker(() => Close(TrackerTriggerSource.PopUpBlocker.ToString()));
		_lastRaceFinishPosition = finishPosition;
		foreach (Transform child in _crewContainer)
		{
			Destroy(child.gameObject);
		}
		var crewCount = 0;
		foreach (var pair in currentPositions)
		{
			var memberObject = Instantiate(_postRaceCrewPrefab, _crewContainer, false).transform;
			memberObject.name = pair.Value.Name;
			var avatar = memberObject.Find("Avatar");
			avatar.GetComponent<AvatarDisplay>().SetAvatar(pair.Value.Avatar, -(finishPosition - 3) * 2);
			avatar.localScale = crewCount % 2 == 0 ? avatar.localScale : new Vector3(-avatar.localScale.x, avatar.localScale.y, avatar.localScale.z);
			memberObject.FindImage("Position").sprite = UIManagement.TeamSelection.RoleLogos[pair.Key.ToString()];
			memberObject.FindRect("Position").offsetMin = crewCount % 2 == 0 ? new Vector2(10, 0) : new Vector2(-10, 0);
			crewCount++;
			memberObject.SetAsLastSibling();
		}
		OnLanguageChange();
		TrackerEventSender.SendEvent(new TraceEvent("ResultPopUpDisplayed", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
		{
			{ TrackerContextKey.FinishingPosition, finishPosition }
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
			TrackerEventSender.SendEvent(new TraceEvent("ResultPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.FinishingPosition, _lastRaceFinishPosition },
				{ TrackerContextKey.TriggerUI, source }
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
		_resultText.text = Localization.GetAndFormat("RACE_RESULT_POSITION", false, GameManagement.TeamName, Localization.Get("POSITION_" + _lastRaceFinishPosition));
		DoBestFit();
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Text>().Where(t => t.transform.parent == transform).BestFit();
	}
}

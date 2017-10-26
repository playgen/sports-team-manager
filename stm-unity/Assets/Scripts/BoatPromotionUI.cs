using System.Collections.Generic;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;

using RAGE.Analytics.Formats;

using UnityEngine;
using UnityEngine.UI;

public class BoatPromotionUI : MonoBehaviour
{
	[SerializeField]
	private PostRaceEventUI[] _postRaceEvents;
	[SerializeField]
	private Button _popUpBlocker;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		if (!GameManagement.ShowTutorial && GameManagement.SeasonOngoing && GameManagement.Boat.Type != GameManagement.LineUpHistory.Last().Type)
		{
			Display(GameManagement.LineUpHistory.Last().Positions, GameManagement.Positions);
		}
		else
		{
			Close(string.Empty);
		}
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Display pop-up which shows the boat promotion
	/// </summary>
	public void Display(List<Position> oldPos, List<Position> newPos)
	{
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.gameObject.SetActive(true);
		_popUpBlocker.onClick.RemoveAllListeners();
		_popUpBlocker.onClick.AddListener(() => Close(TrackerTriggerSources.PopUpBlocker.ToString()));

		var addedText = transform.Find("Added List").GetComponent<Text>();
		var removedText = transform.Find("Removed List").GetComponent<Text>();
		var newPositions = newPos.Where(n => !oldPos.Contains(n)).Select(n => Localization.Get(n.ToString())).ToArray();
		var oldPositions = oldPos.Where(o => !newPos.Contains(o)).Select(o => Localization.Get(o.ToString())).ToArray();
		var newList = string.Join("\n", newPositions);
		var oldList = string.Join("\n", oldPositions);
		addedText.text = newList;
		removedText.text = oldList;
		var newString = string.Join(",", newPos.Select(pos => pos.ToString()).ToArray());
		TrackerEventSender.SendEvent(new TraceEvent("PromotionPopUpDisplayed", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.BoatLayout.ToString(), newString },
		}, AccessibleTracker.Accessible.Screen));
	}

	/// <summary>
	/// Close the promotion pop-up
	/// </summary>
	public void Close(string source)
	{
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            _popUpBlocker.gameObject.SetActive(false);
            _postRaceEvents.ToList().ForEach(e => e.gameObject.SetActive(true));
            if (!string.IsNullOrEmpty(source))
            {
                var newString = GameManagement.PositionString;
                TrackerEventSender.SendEvent(new TraceEvent("PromotionPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
            {
                { TrackerContextKeys.BoatLayout.ToString(), newString },
                { TrackerContextKeys.TriggerUI.ToString(), source },
            }, AccessibleTracker.Accessible.Screen));
            }
        }
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		Display(GameManagement.LineUpHistory.Last().Positions, GameManagement.Positions);	
		DoBestFit();
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Button>().Select(b => b.gameObject).BestFit();
	}
}
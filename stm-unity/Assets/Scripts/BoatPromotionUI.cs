using System.Collections.Generic;
using System.Linq;

using PlayGen.Unity.Utilities.Extensions;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Localization;

using TrackerAssetPackage;

using UnityEngine;
using UnityEngine.UI;

public class BoatPromotionUI : MonoBehaviour
{
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
	/// Display pop-up which shows the boat promotion
	/// </summary>
	public void Display()
	{
		if (!GameManagement.ShowTutorial && GameManagement.SeasonOngoing && GameManagement.Boat.Type != GameManagement.PreviousSession.Type)
		{
			var oldPos = GameManagement.PreviousSession.Positions;
			var newPos = GameManagement.Positions;
			gameObject.Active(true);
			transform.EnableBlocker(() => Close(TrackerTriggerSource.PopUpBlocker.ToString()));
			var addedText = transform.FindText("Added List");
			var removedText = transform.FindText("Removed List");
			var newPositions = newPos.Where(n => !oldPos.Contains(n)).Select(n => Localization.Get(n.ToString())).ToArray();
			var oldPositions = oldPos.Where(o => !newPos.Contains(o)).Select(o => Localization.Get(o.ToString())).ToArray();
			var newList = string.Join("\n", newPositions);
			var oldList = string.Join("\n", oldPositions);
			addedText.text = newList;
			removedText.text = oldList;
			var newString = string.Join(",", newPos.Select(pos => pos.ToString()).ToArray());
			TrackerEventSender.SendEvent(new TraceEvent("PromotionPopUpDisplayed", TrackerAsset.Verb.Accessed, new Dictionary<string, string>
			{
				{ TrackerContextKey.BoatLayout.ToString(), newString }
			}, AccessibleTracker.Accessible.Screen));
		}
		else
		{
			Close(string.Empty);
		}
	}

	/// <summary>
	/// Close the promotion pop-up
	/// </summary>
	public void Close(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			gameObject.Active(false);
			UIManagement.DisableBlocker();
			if (!string.IsNullOrEmpty(source))
			{
				var newString = GameManagement.PositionString;
				TrackerEventSender.SendEvent(new TraceEvent("PromotionPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<string, string>
			{
				{ TrackerContextKey.BoatLayout.ToString(), newString },
				{ TrackerContextKey.TriggerUI.ToString(), source }
			}, AccessibleTracker.Accessible.Screen));
			}
		}
		UIManagement.PostRaceEvents.ToList().ForEach(e => e.Display());
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		Display();	
		DoBestFit();
	}

	private void DoBestFit()
	{
		GetComponentsInChildren<Button>().ToList().BestFit();
	}
}
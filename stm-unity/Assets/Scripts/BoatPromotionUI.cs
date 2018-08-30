using System.Collections.Generic;
using System.Linq;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Localization;
using TrackerAssetPackage;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI displayed when the boat layout has been changed
/// </summary>
public class BoatPromotionUI : MonoBehaviour
{
	[SerializeField]
	private Text _addedText;
	[SerializeField]
	private Text _removedText;

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
	/// Display pop-up which shows the changes in layout
	/// </summary>
	public void Display()
	{
		//only display if the tutorial isn't ongoing, the season is ongoing and if the boat type has actually changed
		if (!GameManagement.ShowTutorial && GameManagement.SeasonOngoing && GameManagement.BoatType != GameManagement.PreviousSession.Type)
		{
			var oldPos = GameManagement.PreviousSession.Positions;
			var newPos = GameManagement.Positions;
			var newPositions = newPos.Where(n => !oldPos.Contains(n)).Select(n => Localization.Get(n.ToString())).ToArray();
			var oldPositions = oldPos.Where(o => !newPos.Contains(o)).Select(o => Localization.Get(o.ToString())).ToArray();
			if (newPositions.Length > 0 || oldPositions.Length > 0)
			{
				var newList = string.Join("\n", newPositions);
				var oldList = string.Join("\n", oldPositions);
				_addedText.text = newList;
				_removedText.text = oldList;
				TrackerEventSender.SendEvent(new TraceEvent("PromotionPopUpDisplayed", TrackerAsset.Verb.Accessed, new Dictionary<TrackerContextKey, object>
				{
					{ TrackerContextKey.BoatLayout, GameManagement.PositionString }
				}, AccessibleTracker.Accessible.Screen));
			}
			else
			{
				Close(string.Empty);
			}
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
				TrackerEventSender.SendEvent(new TraceEvent("PromotionPopUpClosed", TrackerAsset.Verb.Skipped, new Dictionary<TrackerContextKey, object>
			{
				{ TrackerContextKey.BoatLayout, GameManagement.PositionString },
				{ TrackerContextKey.TriggerUI, source }
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

	/// <summary>
	/// Resize button text to be the same size
	/// </summary>
	private void DoBestFit()
	{
		GetComponentsInChildren<Button>().ToList().BestFit();
	}
}
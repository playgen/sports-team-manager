using System.Collections.Generic;

using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;

using RAGE.Analytics.Formats;

using UnityEngine;
using UnityEngine.UI;

public class NotesUI : MonoBehaviour {

	[SerializeField]
	private Text _title;
	[SerializeField]
	private InputField _notesField;
	private string _notesSubject;

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
	public void Display(string subject)
	{
		_notesSubject = subject;
		OnLanguageChange();
		_notesField.text = GameManagement.GameManager.EventController.GetNotes(_notesSubject);
		gameObject.Active(true);
		transform.EnableSmallBlocker(() => Close(TrackerTriggerSources.PopUpBlocker.ToString()));
		TrackerEventSender.SendEvent(new TraceEvent("NotesPopUpDisplayed", TrackerVerbs.Accessed, new Dictionary<string, string>
		{
			{ TrackerContextKeys.TriggerUI.ToString(), _notesSubject }
		}, AccessibleTracker.Accessible.Screen));
	}

	/// <summary>
	/// Close the promotion pop-up
	/// </summary>
	public void Close(string source)
	{
		if (gameObject.activeInHierarchy)
		{
			GameManagement.GameManager.EventController.SaveNote(_notesSubject, _notesField.text);
			_notesSubject = null;
			gameObject.Active(false);
			UIManagement.DisableSmallBlocker();
			if (!string.IsNullOrEmpty(source))
			{
				TrackerEventSender.SendEvent(new TraceEvent("NotesPopUpClosed", TrackerVerbs.Skipped, new Dictionary<string, string>
				{
					{ TrackerContextKeys.TriggerUI.ToString(), source }
				}, AccessibleTracker.Accessible.Screen));
			}
		}
	}

	/// <summary>
	/// Redraw UI upon language change
	/// </summary>
	private void OnLanguageChange()
	{
		_title.text = Localization.GetAndFormat("NOTES_HEADER", false, Localization.Get(_notesSubject));
		DoBestFit();
	}

	private void DoBestFit()
	{
		_title.gameObject.BestFit();
	}
}

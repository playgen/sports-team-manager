using System.Collections.Generic;
using System.Reflection;

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
		gameObject.Active(true);
		_notesSubject = subject;
		OnLanguageChange();
		_notesField.text = GameManagement.GameManager.EventController.GetNotes(_notesSubject);
		transform.EnableBlocker(() => Close(TrackerTriggerSources.PopUpBlocker.ToString()));
		UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
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
			gameObject.Active(false);
			UIManagement.DisableBlocker();
			_notesSubject = null;
			UIManagement.Tutorial.ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name);
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
		_notesField.GetComponent<LayoutElement>().preferredHeight = _notesField.GetComponentInParent<ScrollRect>().RectTransform().rect.height;
	}
}

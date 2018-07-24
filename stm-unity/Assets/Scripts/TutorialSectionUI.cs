using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Localization;
using System.Collections.Generic;
using System.Linq;

using PlayGen.Unity.Utilities.Extensions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

/// <summary>
/// A piece of the in-game tutorial UI
/// </summary>
public class TutorialSectionUI : MonoBehaviour
{
	private TutorialObject _tutorialObj;

	private Dictionary<string, List<string>> _sectionText;
	[SerializeField]
	private RectTransform _menuHighlighted;
	[SerializeField]
	private ReverseRaycastTarget _reverseRaycast;
	[SerializeField]
	private SoftMaskScript _softMaskScript;
	private GameObject _tutorialObject;
	private Text _tutorialText;
	private Transform _buttons;
	private int _currentText;
	private static readonly List<object[]> _triggeredObjects = new List<object[]>();
	private Dictionary<string, string> _attributeDict = new Dictionary<string, string>();
	private int _eventTriggerCount;
	private bool _unblocked;

	/// <summary>
	/// Set-up the values required for creating this piece of the tutorial
	/// </summary>
	public void Construct(TutorialObject tutObj)
	{
		_attributeDict.Clear();
		_tutorialObj = tutObj;

		_sectionText = new Dictionary<string, List<string>>();
		tutObj.SectionTextHolder.ForEach(st => _sectionText.Add(st.Key, st.Value));
		_tutorialObject = UIManagement.Tutorial.SectionCount == GameManagement.TutorialStage + 1 ? transform.parent.FindObject("End Close/Tutorial Helper") : transform.FindObject("Tutorial Helper");
		_buttons = _tutorialObject.transform.FindRect("Buttons");
		_tutorialText = _tutorialObject.GetComponentInChildren<Text>();
		_attributeDict = tutObj.CustomAttributes.Select(a => new KeyValuePair<string, string>(a.Split('=')[0], a.Split('=')[1])).ToDictionary(c => c.Key, c => c.Value);
		GameManagement.GameManager.SetCustomTutorialAttributes(GameManagement.TutorialStage, _attributeDict);
		_currentText = 0;
		_eventTriggerCount = 0;
		_triggeredObjects.Clear();
		_unblocked = false;
		SetUp();
	}

	/// <summary>
	/// Set-up connections to UI elements for this piece of the tutorial
	/// </summary>
	protected void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += SetUp;
	}

	protected void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= SetUp;
	}

	public void ChangePage(int change)
	{
		_currentText += change;
		SetUp();
	}

	/// <summary>
	/// Draw this piece of the UI
	/// </summary>
	private void SetUp()
	{
		if (_attributeDict.ContainsKey("part" + _currentText))
		{
			if (_attributeDict["part" + _currentText].Contains("mood"))
			{
				UIManagement.CrewMemberUI.ToList().ForEach(c => c.ForcedMoodChange(_attributeDict["part" + _currentText].Replace("mood-", string.Empty)));
			}
			if (_attributeDict["part" + _currentText].Contains("opinion"))
			{
				UIManagement.MemberMeeting.DisplayOpinions(_attributeDict["part" + _currentText].Replace("opinion-", string.Empty) == "accurate");
			}
		}
		_reverseRaycast.MaskRect.Clear();
		_reverseRaycast.MaskRect.Add(_menuHighlighted);

		if (_tutorialObj.HighlightedObject[_currentText].Length > 0)
		{
			_softMaskScript.FlipAlphaMask = false;
			var anchorObject = UIManagement.Canvas.transform.Find(_tutorialObj.HighlightedObject[_currentText]).RectTransform() ?? UIManagement.Canvas.RectTransform();
			_softMaskScript.maskScalingRect = anchorObject;
			_reverseRaycast.MaskRect.Add(anchorObject);
		}
		else
		{
			_softMaskScript.FlipAlphaMask = true;
			_softMaskScript.maskScalingRect = null;
		}

		if (_tutorialObj.BlacklistButtons[_currentText] != null)
		{
			var blacklistButtons = _tutorialObj.BlacklistButtons[_currentText].List.Select(blb => _reverseRaycast.MaskRect[1].FindAll(blb)).SelectMany(x => x).Where(x => x.GetComponent<Selectable>()).Select(x => x.RectTransform()).ToList();
			_reverseRaycast.BlacklistRect.Clear();
			_reverseRaycast.BlacklistRect.AddRange(blacklistButtons);
			var whiteList = new List<RectTransform>(_reverseRaycast.MaskRect);
			whiteList.RemoveAt(0);
			foreach (var trans in whiteList)
			{
				if (!trans.GetComponent<Canvas>())
				{
					_reverseRaycast.BlacklistRect.Add(trans);
				}
			}
		}

		if (UIManagement.Tutorial.SectionCount == GameManagement.TutorialStage + 1)
		{
			transform.FindObject("Tutorial Helper").Active(false);
			_tutorialText.text = _sectionText[Localization.SelectedLanguage.Name][_currentText];
		}
		else
		{
			//draw UI differently according to if the side displaying the helper is reversed
			transform.localScale = _tutorialObj.ShowOnLeft ? new Vector2(-1, 1) : Vector2.one;
			_tutorialText.transform.localScale = _tutorialObj.ShowOnLeft ? new Vector2(-1, 1) : Vector2.one;
			_buttons.transform.localScale = _tutorialObj.ShowOnLeft ? new Vector2(-1, 1) : Vector2.one;

			var mhMin = _menuHighlighted.anchorMin.x;
			if ((_tutorialObj.ShowOnLeft && mhMin < 0) || (!_tutorialObj.ShowOnLeft && mhMin > 0))
			{
				var mhMax = _menuHighlighted.anchorMax.x;
				_menuHighlighted.anchorMin = new Vector2(1 - mhMax, _menuHighlighted.anchorMin.y);
				_menuHighlighted.anchorMax = new Vector2(1 - mhMin, _menuHighlighted.anchorMax.y);
			}
			GetComponentInChildren<LayoutGroup>().childAlignment = _tutorialObj.ShowOnLeft ? TextAnchor.UpperRight : TextAnchor.UpperLeft;

			var pageNumber = _buttons.FindText("Page Number");
			//if text is provided, display the tutorial helper
			if (_sectionText[Localization.SelectedLanguage.Name].Count == 0)
			{
				_tutorialObject.Active(false);
				_reverseRaycast.UnblockWhitelisted();
				pageNumber.text = string.Empty;
			}
			else
			{
				//display different buttons and sections according to what oart should be displayed
				_tutorialObject.Active(true);
				_tutorialText.text = _sectionText[Localization.SelectedLanguage.Name][_currentText];
				_buttons.FindObject("Back").Active(_currentText != 0);
				var lastPage = _currentText == _sectionText[Localization.SelectedLanguage.Name].Count - 1;
				_buttons.FindObject("Forward").Active(!lastPage);
				if (lastPage)
				{
					_reverseRaycast.UnblockWhitelisted();
					_unblocked = true;
				}
				pageNumber.text = _sectionText[Localization.SelectedLanguage.Name].Count == 1 ? string.Empty : _currentText + 1 + "/" + _sectionText[Localization.SelectedLanguage.Name].Count;
			}
			var showProgress = _tutorialObj.EventTriggerCountRequired > 1 && _unblocked;
			_buttons.FindText("Progress Count").text = showProgress ? (_tutorialObj.EventTriggerCountRequired - _eventTriggerCount).ToString() : string.Empty;
			pageNumber.RectTransform().anchorMin = showProgress ? new Vector2(0.375f, 0) : new Vector2(0.45f, 0);
			pageNumber.RectTransform().anchorMax = showProgress ? new Vector2(0.575f, 1) : new Vector2(0.65f, 1);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(_tutorialObject.transform.FindRect("Image"));
	}

	/// <summary>
	/// Upon an event related to this part of the tutorial being triggered, add to count of events and advance to next section if required
	/// </summary>
	public void EventReceived(string typeName, string methodName, params object[] additional)
	{
		foreach (var trigger in _tutorialObj.Triggers)
		{
			if (typeName == trigger.Key && methodName == trigger.Value)
			{
				if (_tutorialObj.UniqueEvents)
				{
					foreach (var to in _triggeredObjects)
					{
						if (to.Length == additional.Length)
						{
							var match = true;
							for (var i = 0; i < to.Length; i++)
							{
								if (match && !to[i].Equals(additional[i]))
								{
									match = false;
								}
							}
							if (match)
							{
								return;
							}
						}
					}
				}
				_triggeredObjects.Add(additional);
				_eventTriggerCount++;
				if (_eventTriggerCount >= _tutorialObj.EventTriggerCountRequired)
				{
					UIManagement.Tutorial.AdvanceStage();

				}
			}
		}
		if (_buttons && _tutorialObj.EventTriggerCountRequired > 1)
		{
			_buttons.FindText("Progress Count").text = (_tutorialObj.EventTriggerCountRequired - _eventTriggerCount).ToString();
		}
	}

	private void OnLanguageChange()
	{
		_currentText = 0;
		SetUp();
	}
}
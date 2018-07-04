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
	private RectTransform _menuHighlighted;
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
		_menuHighlighted = transform.FindRect("Menu Highlighted");
		_tutorialObject = UIManagement.Tutorial.SectionCount == GameManagement.TutorialStage + 1 ? transform.parent.FindObject("End Close/Tutorial Helper") : transform.FindObject("Tutorial Helper");
		_buttons = _tutorialObject.transform.FindRect("Buttons");
		_tutorialText = _tutorialObject.GetComponentInChildren<Text>();
		_attributeDict = tutObj.CustomAttributes.Select(a => new KeyValuePair<string, string>(a.Split('=')[0], a.Split('=')[1])).ToDictionary(c => c.Key, c => c.Value);
		GameManagement.GameManager.SetCustomTutorialAttributes(GameManagement.TutorialStage, _attributeDict);
		_currentText = 0;
		_eventTriggerCount = 0;
		_triggeredObjects.Clear();
		_unblocked = false;
		Invoke("SetUp", 0f);
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

	public void Back()
	{
		_currentText--;
		SetUp();
	}

	public void Forward()
	{
		_currentText++;
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
				UIManagement.MemberMeeting.ForcedOpinionChange(_attributeDict["part" + _currentText].Replace("opinion-", string.Empty));
			}
		}
		var reverseRaycast = GetComponentInChildren<ReverseRaycastTarget>();
		reverseRaycast.MaskRect.Clear();
		reverseRaycast.MaskRect.Add(_menuHighlighted);
		var anchorObject = transform.root.RectTransform();
		var softMaskScript = GetComponentInChildren <SoftMaskScript>();

		if (_tutorialObj.HighlightedObject[_currentText].Length > 0)
		{
			softMaskScript.FlipAlphaMask = false;
			anchorObject = anchorObject.Find(_tutorialObj.HighlightedObject[_currentText]).RectTransform() ?? anchorObject;
			softMaskScript.maskScalingRect = anchorObject;
			reverseRaycast.MaskRect.Add(anchorObject);
		}
		else
		{
			softMaskScript.FlipAlphaMask = true;
			softMaskScript.maskScalingRect = null;
		}

		if (_tutorialObj.BlacklistButtons[_currentText] != null)
		{
			var blacklistButtons = _tutorialObj.BlacklistButtons[_currentText].List.Select(blb => reverseRaycast.MaskRect[1].FindAll(blb)).SelectMany(x => x).Select(x => x.RectTransform()).ToList();
			reverseRaycast.BlacklistRect.Clear();
			reverseRaycast.BlacklistRect.AddRange(blacklistButtons);
			var whiteList = new List<RectTransform>(reverseRaycast.MaskRect);
			whiteList.RemoveAt(0);
			foreach (var trans in whiteList)
			{
				if (!trans.GetComponent<Canvas>())
				{
					reverseRaycast.BlacklistRect.Add(trans);
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
			if (_tutorialObj.ShowOnLeft)
			{
				transform.localScale = new Vector2(-1, 1);
				_tutorialText.transform.localScale = new Vector2(-1, 1);
				_buttons.transform.localScale = new Vector2(-1, 1);

				var mhMin = _menuHighlighted.anchorMin.x;
				if (mhMin < 0)
				{
					var mhMax = _menuHighlighted.anchorMax.x;
					_menuHighlighted.anchorMin = new Vector2(1 - mhMax, _menuHighlighted.anchorMin.y);
					_menuHighlighted.anchorMax = new Vector2(1 - mhMin, _menuHighlighted.anchorMax.y);
					GetComponentInChildren<LayoutGroup>().childAlignment = TextAnchor.UpperRight;
				}
			}
			else
			{
				transform.localScale = Vector2.one;
				_tutorialText.transform.localScale = Vector2.one;
				_buttons.transform.localScale = Vector2.one;

				var mhMin = _menuHighlighted.anchorMin.x;
				if (mhMin > 0)
				{
					var mhMax = _menuHighlighted.anchorMax.x;
					_menuHighlighted.anchorMin = new Vector2(1 - mhMax, _menuHighlighted.anchorMin.y);
					_menuHighlighted.anchorMax = new Vector2(1 - mhMin, _menuHighlighted.anchorMax.y);
					GetComponentInChildren<LayoutGroup>().childAlignment = TextAnchor.UpperRight;
				}
				GetComponentInChildren<LayoutGroup>().childAlignment = TextAnchor.UpperLeft;
			}

			var back = _buttons.FindObject("Back");
			var forward = _buttons.FindObject("Forward");
			var pageNumber = _buttons.FindText("Page Number");
			//if text is provided, display the tutorial helper
			if (_sectionText[Localization.SelectedLanguage.Name].Count == 0)
			{
				_tutorialObject.Active(false);
				GetComponentInChildren<ReverseRaycastTarget>().UnblockWhitelisted();
				pageNumber.text = string.Empty;
			}
			else
			{
				//display different buttons and sections according to what oart should be displayed
				_tutorialObject.Active(true);
				_tutorialText.text = _sectionText[Localization.SelectedLanguage.Name][_currentText];
				back.Active(true);
				forward.Active(true);
				if (_currentText == 0)
				{
					back.Active(false);
				}
				if (_currentText == _sectionText[Localization.SelectedLanguage.Name].Count - 1)
				{
					forward.Active(false);
					GetComponentInChildren<ReverseRaycastTarget>().UnblockWhitelisted();
					_unblocked = true;
				}
				if (_sectionText[Localization.SelectedLanguage.Name].Count == 1)
				{
					pageNumber.text = string.Empty;
				}
				else
				{
					pageNumber.text = _currentText + 1 + "/" + _sectionText[Localization.SelectedLanguage.Name].Count;
				}
			}
			if (_tutorialObj.EventTriggerCountRequired > 1 && _unblocked)
			{
				_buttons.FindText("Progress Count").text = (_tutorialObj.EventTriggerCountRequired - _eventTriggerCount).ToString();
				pageNumber.RectTransform().anchorMin = new Vector2(0.375f, 0);
				pageNumber.RectTransform().anchorMax = new Vector2(0.575f, 1);
			}
			else
			{
				_buttons.FindText("Progress Count").text = string.Empty;
				pageNumber.RectTransform().anchorMin = new Vector2(0.45f, 0);
				pageNumber.RectTransform().anchorMax = new Vector2(0.65f, 1);
			}
		}
		var speechBubble = _tutorialObject.transform.Find("Image");
		LayoutRebuilder.ForceRebuildLayoutImmediate(speechBubble.RectTransform());
		Invoke("PaddingSetUp", 0f);
	}

	/// <summary>
	/// Set up padding for text
	/// </summary>
	private void PaddingSetUp()
	{
		var speechBubble = _tutorialObject.transform.Find("Image");
		LayoutRebuilder.ForceRebuildLayoutImmediate(speechBubble.RectTransform());
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
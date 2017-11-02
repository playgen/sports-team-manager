using System;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

/// <summary>
/// A piece of the in-game tutorial UI
/// </summary>
public class TutorialSectionUI : MonoBehaviour
{
    private TutorialObject _tutorialObj;

	private Dictionary<string, string[]> _sectionText;
	private RectTransform _menuHighlighted;
	private GameObject _tutorialObject;
	private Text _tutorialText;
	private Transform _buttons;
	private int _currentText;
	private static readonly List<object[]> _triggeredObjects = new List<object[]>();
	private int _eventTriggerCount;
	private bool _unblocked;

	/// <summary>
	/// Set-up the values required for creating this piece of the tutorial
	/// </summary>
	public void Construct(TutorialObject tutObj)
	{
        _tutorialObj = tutObj;

        _sectionText = new Dictionary<string, string[]>();
        tutObj.SectionTextHolder.ForEach(st => _sectionText.Add(st.Key, st.Value));
        _menuHighlighted = (RectTransform)transform.Find("Menu Highlighted");
        _tutorialText = GetComponentInChildren<Text>();
        _tutorialObject = transform.Find("Tutorial Helper").gameObject;
        _buttons = (RectTransform)transform.Find("Tutorial Helper/Buttons");
        var anchorObject = (RectTransform)transform.root;
        foreach (var obj in tutObj.HighlightedObjects)
        {
            anchorObject = (RectTransform)anchorObject.FindInactive(obj) ?? anchorObject;
        }
        GetComponentInChildren<SoftMaskScript>().maskScalingRect = anchorObject;
        GetComponentInChildren<ReverseRaycastTarget>().MaskRect.Add(anchorObject);
        GetComponentInChildren<SoftMaskScript>().FlipAlphaMask = true;
        var reverseRaycast = GetComponentInChildren<ReverseRaycastTarget>();
        if (tutObj.BlacklistButtons != null)
        {
            var blacklistButtons = tutObj.BlacklistButtons.Select(blb => reverseRaycast.MaskRect[1].FindAll(blb)).SelectMany(x => x).Select(x => (RectTransform)x).ToList();
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
        var attributeDict = tutObj.CustomAttributes.Select(a => new KeyValuePair<string, string>(a.Split('=')[0], a.Split('=')[1])).ToDictionary(c => c.Key, c => c.Value);
        UIManagement.Tutorial.CustomAttributes(attributeDict);
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
		//draw UI differently according to if the side displaying the helper is reversed
		if (_tutorialObj.Reversed)
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
		var back = _buttons.Find("Back").gameObject;
		var forward = _buttons.Find("Forward").gameObject;
		var pageNumber = _buttons.Find("Page Number").GetComponent<Text>();
		//if text is provided, display the tutorial helper
		if (_sectionText[Localization.SelectedLanguage.Name].Length == 0)
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
			if (_currentText == _sectionText[Localization.SelectedLanguage.Name].Length - 1)
			{
				forward.Active(false);
				GetComponentInChildren<ReverseRaycastTarget>().UnblockWhitelisted();
				_unblocked = true;
			}
			if (_sectionText[Localization.SelectedLanguage.Name].Length == 1)
			{
				pageNumber.text = string.Empty;
			}
			else
			{
				pageNumber.text = _currentText + 1 + "/" + _sectionText[Localization.SelectedLanguage.Name].Length;
			}
		}
		if (_tutorialObj.EventTriggerCountRequired > 1 && _unblocked)
		{
			_buttons.Find("Progress Count").GetComponent<Text>().text = (_tutorialObj.EventTriggerCountRequired - _eventTriggerCount).ToString();
		}
		else
		{
			_buttons.Find("Progress Count").GetComponent<Text>().text = string.Empty;
			((RectTransform)pageNumber.transform).anchorMin = new Vector2(0.45f, 0);
			((RectTransform)pageNumber.transform).anchorMax = new Vector2(0.65f, 1);
		}
		if (_currentText >= _tutorialObj.HighlightTrigger)
		{
			GetComponentInChildren<SoftMaskScript>().FlipAlphaMask = false;
		}
		if (UIManagement.Tutorial.SectionCount == transform.GetSiblingIndex())
		{
			_buttons.Find("End Text").gameObject.Active(true);
		}
		var speechBubble = transform.Find("Tutorial Helper/Image");
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)speechBubble);
		Invoke("PaddingSetUp", 0f);
	}

	/// <summary>
	/// Set up padding for text
	/// </summary>
	private void PaddingSetUp()
	{
		var speechBubble = transform.Find("Tutorial Helper/Image");
		speechBubble.GetComponent<LayoutGroup>().padding.bottom = (int)(((RectTransform)speechBubble).sizeDelta.y * 0.25f) + 16;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)speechBubble);
		Invoke("ButtonSetUp", 0f);
	}

	/// <summary>
	/// Set up button positioning
	/// </summary>
	private void ButtonSetUp()
	{
		var speechBubble = transform.Find("Tutorial Helper/Image");
		var buttons = transform.Find("Tutorial Helper/Buttons");
		speechBubble.GetComponent<LayoutGroup>().padding.bottom = (int)(((RectTransform)speechBubble).sizeDelta.y * 0.25f) + 16;
		((RectTransform)buttons).anchoredPosition = new Vector2(0, (int)(((RectTransform)speechBubble).sizeDelta.y * 0.25f) + 16);
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
			_buttons.Find("Progress Count").GetComponent<Text>().text = (_tutorialObj.EventTriggerCountRequired - _eventTriggerCount).ToString();
		}
	}

	private void OnLanguageChange()
	{
		_currentText = 0;
		SetUp();
	}
}
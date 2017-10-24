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
	[Serializable]
	class LanguageKeyValuePair
	{
		public string Key;
		[TextArea]
		public string[] Value;

		public LanguageKeyValuePair(string k, string[] v)
		{
			Key = k;
			Value = v;
		}
	}
	[Serializable]
	class TriggerKeyValuePair
	{
		public string Key;
		public string Value;

		public TriggerKeyValuePair(string k, string v)
		{
			Key = k;
			Value = v;
		}
	}
	private TutorialController _tutorial;
	[Header("UI")]
	[SerializeField]
	private List<LanguageKeyValuePair> _sectionTextHolder;
	private Dictionary<string, string[]> _sectionText;
	[SerializeField]
	private bool _reversed;
	private RectTransform _menuHighlighted;
	private GameObject _tutorialObject;
	private Text _tutorialText;
	private Transform _buttons;
	[SerializeField]
	private int _highlightTrigger;
	private int _currentText;

	[Header("Tutorial Trigger")]
	[SerializeField]
	private List<TriggerKeyValuePair> _triggers;
	[SerializeField]
	private bool _uniqueEvents;
	private static readonly List<object[]> _triggeredObjects = new List<object[]>();
	[SerializeField]
	private int _eventTriggerCountRequired;
	private int _eventTriggerCount;
	[SerializeField]
	private int _saveNextSection;
	[SerializeField]
	private List<string> _blacklistButtons;
	[SerializeField]
	private List<string> _customAttributes;
	public int SaveNextSection
	{
		get { return _saveNextSection; }
	}

	private bool _unblocked;

	/// <summary>
	/// Set-up the values required for creating this piece of the tutorial
	/// </summary>
	public void Construct(Dictionary<string, string[]> text, int highlightTrigger, bool reversed, KeyValuePair<string, string>[] triggers, int triggerCount, bool uniqueTriggers, int saveSection, List<string> blacklist, List<string> attributes)
	{
		_sectionTextHolder = new List<LanguageKeyValuePair>();
		foreach (var kvp in text)
		{
			_sectionTextHolder.Add(new LanguageKeyValuePair(kvp.Key, kvp.Value));
		}
		_triggers = new List<TriggerKeyValuePair>();
		foreach (var kvp in triggers)
		{
			_triggers.Add(new TriggerKeyValuePair(kvp.Key, kvp.Value));
		}
		_highlightTrigger = highlightTrigger;
		_reversed = reversed;
		_eventTriggerCountRequired = triggerCount;
		_uniqueEvents = uniqueTriggers;
		_saveNextSection = saveSection;
		_blacklistButtons = blacklist;
		_customAttributes = attributes;
	}

	/// <summary>
	/// Set-up connections to UI elements for this piece of the tutorial
	/// </summary>
	protected void OnEnable()
	{
		_tutorial = GetComponentInParent<TutorialController>();
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += SetUp;
		_sectionText = new Dictionary<string, string[]>();
		_sectionTextHolder.ForEach(st => _sectionText.Add(st.Key, st.Value));
		_menuHighlighted = (RectTransform)transform.Find("Menu Highlighted");
		_tutorialText = GetComponentInChildren<Text>();
		_tutorialObject = transform.Find("Tutorial Helper").gameObject;
		_buttons = (RectTransform)transform.Find("Tutorial Helper/Buttons");
		GetComponentInChildren<SoftMaskScript>().FlipAlphaMask = true;
		var reverseRaycast = GetComponentInChildren<ReverseRaycastTarget>();
		if (_blacklistButtons != null)
		{
			var blacklistButtons = _blacklistButtons.Select(blb => reverseRaycast.MaskRect[1].FindAll(blb)).SelectMany(x => x).Select(x => (RectTransform)x).ToList();
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
		var attributeDict = _customAttributes.Select(a => new KeyValuePair<string, string>(a.Split('=')[0], a.Split('=')[1])).ToDictionary(c => c.Key, c => c.Value);
		_tutorial.CustomAttributes(attributeDict);
		Invoke("SetUp", 0f);
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
		if (_reversed)
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
			_tutorialObject.SetActive(false);
			GetComponentInChildren<ReverseRaycastTarget>().UnblockWhitelisted();
			pageNumber.text = string.Empty;
		}
		else
		{
			//display different buttons and sections according to what oart should be displayed
			_tutorialObject.SetActive(true);
			_tutorialText.text = _sectionText[Localization.SelectedLanguage.Name][_currentText];
			back.SetActive(true);
			forward.SetActive(true);
			if (_currentText == 0)
			{
				back.SetActive(false);
			}
			if (_currentText == _sectionText[Localization.SelectedLanguage.Name].Length - 1)
			{
				forward.SetActive(false);
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
		if (_eventTriggerCountRequired > 1 && _unblocked)
		{
			_buttons.Find("Progress Count").GetComponent<Text>().text = (_eventTriggerCountRequired - _eventTriggerCount).ToString();
		}
		else
		{
			_buttons.Find("Progress Count").GetComponent<Text>().text = string.Empty;
			((RectTransform)pageNumber.transform).anchorMin = new Vector2(0.45f, 0);
			((RectTransform)pageNumber.transform).anchorMax = new Vector2(0.65f, 1);
		}
		if (_currentText >= _highlightTrigger)
		{
			GetComponentInChildren<SoftMaskScript>().FlipAlphaMask = false;
		}
		if (transform.parent.childCount - 1 == transform.GetSiblingIndex())
		{
			_buttons.Find("End Text").gameObject.SetActive(true);
		}
		var speechBubble = transform.Find("Tutorial Helper/Image");
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)speechBubble);
		Invoke("PaddingSetUp", 0f);
		_triggeredObjects.Clear();
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
		foreach (var trigger in _triggers)
		{
			if (typeName == trigger.Key && methodName == trigger.Value)
			{
				if (_uniqueEvents)
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
				if (_eventTriggerCount >= _eventTriggerCountRequired)
				{
					_tutorial.AdvanceStage();

				}
			}
		}
		if (_buttons && _eventTriggerCountRequired > 1)
		{
			_buttons.Find("Progress Count").GetComponent<Text>().text = (_eventTriggerCountRequired - _eventTriggerCount).ToString();
		}
	}

	private void OnLanguageChange()
	{
		_currentText = 0;
		SetUp();
	}
}

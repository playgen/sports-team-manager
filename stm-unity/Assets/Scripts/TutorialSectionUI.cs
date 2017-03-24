using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TutorialSectionUI : ObserverMonoBehaviour
{
	[System.Serializable]
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

	public void Construct(Dictionary<string, string[]> text, int highlightTrigger, bool reversed, KeyValueMessage[] triggers, int triggerCount, bool uniqueTriggers, int saveSection, List<string> blacklist, List<string> attributes)
	{
		_sectionTextHolder = new List<LanguageKeyValuePair>();
		foreach (var kvp in text)
		{
			_sectionTextHolder.Add(new LanguageKeyValuePair(kvp.Key, kvp.Value));
		}
		_highlightTrigger = highlightTrigger;
		_reversed = reversed;
		_triggers = triggers;
		_eventTriggerCountRequired = triggerCount;
		_uniqueEvents = uniqueTriggers;
		_saveNextSection = saveSection;
		_blacklistButtons = blacklist;
		_customAttributes = attributes;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
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

	protected override void OnDisable()
	{
		base.OnDisable();
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

	private void SetUp()
	{
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
		if (_sectionText[Localization.SelectedLanguage.Name].Length == 0)
		{
			_tutorialObject.SetActive(false);
			GetComponentInChildren<ReverseRaycastTarget>().UnblockWhitelisted();
			pageNumber.text = string.Empty;
		}
		else
		{
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
		if (_eventTriggerCountRequired > 1)
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
		var speechBubble = transform.Find("Tutorial Helper/Image");
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)speechBubble);
		Invoke("PaddingSetUp", 0f);
		_triggeredObjects.Clear();
	}

	private void PaddingSetUp()
	{
		var speechBubble = transform.Find("Tutorial Helper/Image");
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)speechBubble);
		speechBubble.GetComponent<LayoutGroup>().padding.bottom = (int)(((RectTransform)speechBubble).sizeDelta.y * 0.25f) + 16;
		Invoke("ButtonSetUp", 0f);
	}

	private void ButtonSetUp()
	{
		var speechBubble = transform.Find("Tutorial Helper/Image");
		var buttons = transform.Find("Tutorial Helper/Buttons");
		speechBubble.GetComponent<LayoutGroup>().padding.bottom = (int)(((RectTransform)speechBubble).sizeDelta.y * 0.25f) + 16;
		((RectTransform)buttons).anchoredPosition = new Vector2(0, (int)(((RectTransform)speechBubble).sizeDelta.y * 0.25f) + 16);
	}

	public override void OnNext(KeyValueMessage message)
	{
		foreach (var trigger in _triggers)
		{
			if (message.TypeName == trigger.TypeName && message.MethodName == trigger.MethodName)
			{
				if (_uniqueEvents)
				{
					foreach (var to in _triggeredObjects)
					{
						if (to.Length == message.Additional.Length)
						{
							bool match = true;
							for (int i = 0; i < to.Length; i++)
							{
								if (match && !to[i].Equals(message.Additional[i]))
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
				_triggeredObjects.Add(message.Additional);
				_eventTriggerCount++;
				if (_eventTriggerCount >= _eventTriggerCountRequired)
				{
					_tutorial.AdvanceStage();

				}
			}
		}
		if (_eventTriggerCountRequired > 1)
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

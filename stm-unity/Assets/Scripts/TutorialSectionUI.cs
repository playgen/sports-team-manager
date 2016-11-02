using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSectionUI : ObserverMonoBehaviour
{
	[System.Serializable]
	class LanguageKeyValuePair
	{
		public Language Key;
		[TextArea]
		public string[] Value;

		public LanguageKeyValuePair(Language k, string[] v)
		{
			Key = k;
			Value = v;
		}
	}
	private TutorialController _tutorial;
	[Header("UI")]
	[SerializeField]
	private List<LanguageKeyValuePair> _sectionTextHolder;
	private Dictionary<Language, string[]> _sectionText;
	[SerializeField]
	private bool _reversed;
	private RectTransform _menuHighlighted;
	private GameObject _tutorialObject;
	private Text _tutorialText;
	private Transform _buttons;
	private DynamicPadding _dynamicPadding;
	private int _currentText;

	[Header("Tutorial Trigger")]
	[SerializeField]
	private bool _uniqueEvents;
	private static readonly List<object[]> _triggeredObjects = new List<object[]>();
	[SerializeField]
	private int _eventTriggerCountRequired;
	private int _eventTriggerCount;
	[SerializeField]
	private bool _wipeTriggered;
	[SerializeField]
	private int _saveNextSection;
	[SerializeField]
	private List<string> _blacklistButtons;
	public int SaveNextSection
	{
		get { return _saveNextSection; }
	}

	public void Construct(Dictionary<Language, string[]> text, bool reversed, KeyValueMessage[] triggers, int triggerCount, bool uniqueTriggers, bool wipeTriggered, int saveSection, List<string> blacklist)
	{
		_sectionTextHolder = new List<LanguageKeyValuePair>();
		foreach (var kvp in text)
		{
			_sectionTextHolder.Add(new LanguageKeyValuePair(kvp.Key, kvp.Value));
		}
		_reversed = reversed;
		_triggers = triggers;
		_eventTriggerCountRequired = triggerCount;
		_uniqueEvents = uniqueTriggers;
		_wipeTriggered = wipeTriggered;
		_saveNextSection = saveSection;
		_blacklistButtons = blacklist;
	}

	private void Start()
	{
		_tutorial = GetComponentInParent<TutorialController>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_sectionText = new Dictionary<Language, string[]>();
		_sectionTextHolder.ForEach(st => _sectionText.Add(st.Key, st.Value));
		_menuHighlighted = (RectTransform)transform.Find("Menu Highlighted");
		_tutorialText = GetComponentInChildren<Text>();
		_tutorialObject = transform.Find("Tutorial Helper").gameObject;
		_buttons = (RectTransform)transform.Find("Tutorial Helper/Buttons");
		_dynamicPadding = GetComponentInChildren<DynamicPadding>();
		var reverseRaycast = GetComponentInChildren<ReverseRaycastTarget>();
		if (_blacklistButtons != null)
		{
			var blacklistButtons = _blacklistButtons.Select(blb => reverseRaycast.MaskRect[1].FindAll(blb)).SelectMany(x => x).Select(x => (RectTransform)x).ToList();
			reverseRaycast.BlacklistRect.AddRange(blacklistButtons);
		}
		SetUp();
		Localization.LanguageChange += OnLanguageChange;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Localization.LanguageChange -= OnLanguageChange;
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
				GetComponentInChildren<LayoutGroup>().childAlignment = TextAnchor.MiddleRight;
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
				GetComponentInChildren<LayoutGroup>().childAlignment = TextAnchor.MiddleRight;
			}
			GetComponentInChildren<LayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
		}
		var back = _buttons.Find("Back").gameObject;
		var forward = _buttons.Find("Forward").gameObject;
		if (_sectionText[Localization.SelectedLanguage].Length == 0)
		{
			_tutorialObject.SetActive(false);
		}
		else
		{
			_tutorialObject.SetActive(true);
			_tutorialText.text = _sectionText[Localization.SelectedLanguage][_currentText];
			back.SetActive(true);
			forward.SetActive(true);
			if (_currentText == 0)
			{
				back.SetActive(false);
			}
			if (_currentText == _sectionText[Localization.SelectedLanguage].Length - 1)
			{
				forward.SetActive(false);
			}
		}
		_dynamicPadding.Adjust();
		if (_wipeTriggered)
		{
			_triggeredObjects.Clear();
		}
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
	}

	private void OnLanguageChange()
	{
		_currentText = 0;
		SetUp();
	}
}

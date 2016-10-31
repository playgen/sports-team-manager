using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TutorialSectionUI : ObserverMonoBehaviour
{
	private TutorialController _tutorial;
	[Header("UI")]
	[SerializeField]
	private Vector2 _highlightedAreaMin;
	[SerializeField]
	private Vector2 _highlightedAreaMax;
	[SerializeField]
	[TextArea]
	private string[] _sectionText;
	[SerializeField]
	private bool _reversed;
	private RectTransform _highlighted;
	private RectTransform _menuHighlighted;
	private GameObject _tutorialObject;
	private Text _tutorialText;
	private Transform _buttons;
	private DynamicPadding _dynamicPadding;
	private int _currentText;

	[Header("Tutorial Trigger")]
	[SerializeField]
	private bool _uniqueEvents;
	private static List<GameObject> _triggeredObjects = new List<GameObject>();
	[SerializeField]
	private int _eventTriggerCountRequired;
	private int _eventTriggerCount;

	public void Construct(string[] text, Vector2 min, Vector2 max, bool reversed, KeyValueMessage[] triggers, int triggerCount, bool uniqueTriggers, bool wipeTriggered)
	{
		_sectionText = text;
		_highlightedAreaMin = min;
		_highlightedAreaMax = max;
		_reversed = reversed;
		_triggers = triggers;
		_eventTriggerCountRequired = triggerCount;
		_uniqueEvents = uniqueTriggers;
		if (wipeTriggered)
		{
			_triggeredObjects.Clear();
		}
	}

	private void Start()
	{
		_tutorial = GetComponentInParent<TutorialController>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_highlighted = (RectTransform)transform.Find("Highlighted");
		_menuHighlighted = (RectTransform)transform.Find("Menu Highlighted");
		_tutorialText = GetComponentInChildren<Text>();
		_tutorialObject = transform.Find("Tutorial Helper").gameObject;
		_buttons = (RectTransform)transform.Find("Tutorial Helper/Buttons");
		_dynamicPadding = GetComponentInChildren<DynamicPadding>();
		SetUp();
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
			_highlighted.transform.localScale = new Vector2(-1, 1);
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
			_highlighted.transform.localScale = Vector2.one;
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
		_highlighted.anchorMin = _highlightedAreaMin;
		_highlighted.anchorMax = _highlightedAreaMax;
		if (_sectionText.Length == 0)
		{
			_tutorialObject.SetActive(false);
		}
		else
		{
			_tutorialObject.SetActive(true);
			_tutorialText.text = _sectionText[_currentText];
		}
		_dynamicPadding.Adjust();
		if (_currentText == 0)
		{
			back.SetActive(false);
			forward.SetActive(true);
		}
		else if (_currentText == _sectionText.Length - 1)
		{
			back.SetActive(true);
			forward.SetActive(false);

		}
		else
		{
			back.SetActive(true);
			forward.SetActive(true);
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
					if (_triggeredObjects.Contains(message.SourceObject))
					{
						return;
					}
				}
				_triggeredObjects.Add(message.SourceObject);
				_eventTriggerCount++;
				if (_eventTriggerCount >= _eventTriggerCountRequired)
				{
					_tutorial.AdvanceStage();

				}
			}
		}
	}
}

using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class TutorialSectionUI : MonoBehaviour {

	[SerializeField]
	private Vector2 _highlightedAreaMin;
	[SerializeField]
	private Vector2 _highlightedAreaMax;
	[SerializeField]
	private string[] _sectionText;
	[SerializeField]
	private bool _reversed;
	private RectTransform _highlighted;
	private RectTransform _menuHighlighted;
	private Text _tutorialText;
	private Transform _buttons;
	private DynamicPadding _dynamicPadding;
	private int _currentText;

	private void OnEnable()
	{
		_highlighted = (RectTransform)transform.Find("Highlighted");
		_menuHighlighted = (RectTransform)transform.Find("Menu Highlighted");
		_tutorialText = GetComponentInChildren<Text>();
		_buttons = (RectTransform)transform.Find("Tutorial Helper/Buttons");
		_dynamicPadding = GetComponentInChildren<DynamicPadding>();
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
		} else
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
		var back = _buttons.Find("Back").gameObject;
		var forward = _buttons.Find("Forward").gameObject;
		_highlighted.anchorMin = _highlightedAreaMin;
		_highlighted.anchorMax = _highlightedAreaMax;
		_tutorialText.text = _sectionText[_currentText];
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
}

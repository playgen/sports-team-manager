using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LearningPill))]
public class LearningPillUI : ObservableMonoBehaviour {

	private LearningPill _learningPill;
	[SerializeField]
	private PostRaceEventUI[] _postRaceEvents;
	[SerializeField]
	private Text _helpText;
	[SerializeField]
	private Animation _popUpAnim;
	[SerializeField]
	private Button _popUpBlocker;
	private string _currentHelp;
	private List<string> _furtherHelp;

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
	}

	public void SetHelp(List<string> keys, bool further = false)
	{
		if (_learningPill == null)
		{
			_learningPill = GetComponent<LearningPill>();
		}
		_currentHelp = keys[0];
		var tip = _learningPill.GetHelpText(keys[0]);
		keys.RemoveAt(0);
		_furtherHelp = keys;
		_helpText.text = "";
		if (tip != null)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			_popUpBlocker.gameObject.SetActive(true);
			_popUpBlocker.onClick.RemoveAllListeners();
			StartCoroutine(Animate(true, further, Localization.Get(tip)));
		}
	}

	public void SetFurtherHelp()
	{
		SetHelp(_furtherHelp, true);
	}

	public void ClosePill()
	{
		_popUpBlocker.onClick.RemoveAllListeners();
		if (_furtherHelp.Count == 0)
		{
			StartCoroutine(Animate());
			_popUpBlocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			_popUpBlocker.gameObject.SetActive(false);
		}
		if (_furtherHelp.Count > 0)
		{
			StartCoroutine(Animate(false, true));
			Invoke("SetFurtherHelp", 1.1f);
		}
		else
		{
			foreach (var pre in _postRaceEvents)
			{
				if (pre.gameObject.activeInHierarchy && !Mathf.Approximately(pre.GetComponent<CanvasGroup>().alpha, 0))
				{
					_popUpBlocker.gameObject.SetActive(true);
					pre.transform.parent.SetAsLastSibling();
					pre.SetBlockerOnClick();
					return;
				}
			}
		}
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "LearningPill", "LearningPillClosed", AlternativeTracker.Alternative.Dialog));
	}

	private IEnumerator Animate(bool upward = false, bool keep = false, string tip = "")
	{
		WaitForEndOfFrame endFrame = new WaitForEndOfFrame();
		WaitForSecondsRealtime endReal = new WaitForSecondsRealtime(0.2f);
		int start = upward ? keep ? 1 : 0 : 2;
		int limit = keep ? 1 : 2;
		_popUpAnim["LearningPill"].speed = 1;
		_popUpAnim["LearningPill"].time = start;
		_popUpAnim.Play();
		while (_popUpAnim["LearningPill"].time <= start + limit)
		{
			yield return endFrame;
		}
		_popUpAnim["LearningPill"].speed = 0;
		_popUpAnim["LearningPill"].time = start + limit;
		ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, upward, keep, tip);
		if (upward)
		{
			foreach (char t in tip)
			{
				if (tip == Localization.Get(_learningPill.GetHelpText(_currentHelp))) {
					_helpText.text += t;
					yield return endReal;
				}
			}
			ShareEvent(GetType().Name, MethodBase.GetCurrentMethod().Name, new KeyValueMessage(typeof(AlternativeTracker).Name, "Selected", "LearningPill", "LearningPillDisplayed", AlternativeTracker.Alternative.Dialog));
			_popUpBlocker.onClick.AddListener(ClosePill);
		}
	}

	private void OnLanguageChange()
	{
		if (_currentHelp != null) {
			var tip = _learningPill.GetHelpText(_currentHelp);
			_helpText.text = Localization.Get(tip);
		}
	}
}

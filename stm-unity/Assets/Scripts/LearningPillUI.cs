using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LearningPill))]
public class LearningPillUI : MonoBehaviour {

	private LearningPill _learningPill;
	[SerializeField]
	private PostRaceEventUI[] _postRaceEvents;
	[SerializeField]
	private Text _helpText;
	[SerializeField]
	private Animation _popUpAnim;
	[SerializeField]
	private Button _popUpBlocker;
	private List<string> _furtherHelp;

	public void SetHelp(List<string> keys)
	{
		if (_learningPill == null)
		{
			_learningPill = GetComponent<LearningPill>();
		}
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
			StartCoroutine(Animate(true, tip));
		}
	}

	public void SetFurtherHelp()
	{
		SetHelp(_furtherHelp);
	}

	public void ClosePill()
	{
		StartCoroutine(Animate());
		_popUpBlocker.onClick.RemoveAllListeners();
		if (_furtherHelp.Count == 0)
		{
			_popUpBlocker.transform.SetAsLastSibling();
			transform.SetAsLastSibling();
			_popUpBlocker.gameObject.SetActive(false);
		}
		if (_furtherHelp.Count > 0)
		{
			Invoke("SetFurtherHelp", 1);
		}
		else
		{
			foreach (var pre in _postRaceEvents)
			{
				if (pre.gameObject.activeSelf && !Mathf.Approximately(pre.GetComponent<CanvasGroup>().alpha, 0))
				{
					_popUpBlocker.gameObject.SetActive(true);
					pre.transform.parent.SetAsLastSibling();
					pre.SetBlockerOnClick();
					return;
				}
			}
		}
	}

	private IEnumerator Animate(bool upward = false, string tip = "")
	{
		WaitForEndOfFrame endFrame = new WaitForEndOfFrame();
		WaitForSecondsRealtime endReal = new WaitForSecondsRealtime(0.04f);
		int start = upward ? 0 : 1;
		_popUpAnim["LearningPill"].speed = 1;
		_popUpAnim["LearningPill"].time = start;
		_popUpAnim.Play();
		while (_popUpAnim["LearningPill"].time < start + 1)
		{
			yield return endFrame;
		}
		_popUpAnim["LearningPill"].speed = 0;
		_popUpAnim["LearningPill"].time = start + 1;
		if (upward)
		{
			foreach (char t in tip)
			{
				_helpText.text += t;
				yield return endReal;
			}
			_popUpBlocker.onClick.AddListener(ClosePill);
		}
	}
}

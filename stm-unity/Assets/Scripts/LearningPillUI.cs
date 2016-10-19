using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LearningPill))]
public class LearningPillUI : MonoBehaviour {

	private LearningPill _learningPill;
	[SerializeField]
	private PostRaceEventUI _postRaceEvent;
	[SerializeField]
	private Text _helpText;
	[SerializeField]
	private Animation _popUpAnim;
	[SerializeField]
	private Button _popUpBlocker;

	public void SetHelp(string key)
	{
		if (_learningPill == null)
		{
			_learningPill = GetComponent<LearningPill>();
		}
		var tip = _learningPill.GetHelpText(key);
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

	public void ClosePill()
	{
		StartCoroutine(Animate());
		_popUpBlocker.transform.SetAsLastSibling();
		transform.SetAsLastSibling();
		_popUpBlocker.onClick.RemoveAllListeners();
		if (_postRaceEvent.gameObject.activeSelf)
		{
			_postRaceEvent.transform.SetAsLastSibling();
			_postRaceEvent.SetBlockerOnClick();
		}
		else
		{
			_popUpBlocker.gameObject.SetActive(false);
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

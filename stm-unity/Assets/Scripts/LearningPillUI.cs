using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LearningPill))]
public class LearningPillUI : MonoBehaviour {

	private LearningPill _learningPill;
	[SerializeField]
	private Text _titleText;
	[SerializeField]
	private Text _helpText;

	public void SetHelp(string key)
	{
		if (_learningPill == null)
		{
			_learningPill = GetComponent<LearningPill>();
		}
		var tip = _learningPill.GetHelpText(key);
		_titleText.text = "TIP:";
		_helpText.text = tip;
	}
}

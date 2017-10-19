using System.Collections.Generic;

using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;

using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Contains all logic relating to displaying post-game questionnaire
/// </summary>
public class QuestionnaireUI : MonoBehaviour
{
    [SerializeField]
    private TextAsset _questionAsset;
    [SerializeField]
    private TextAsset _answerStyleAsset;
    [SerializeField]
	private GameObject _questionnairePanel;
	[SerializeField]
	private GameObject _questionPrefab;
	[SerializeField]
	private GameObject _submitButton;
	private readonly List<GameObject> _questionObjs = new List<GameObject>();

	/// <summary>
	/// On Enable, clear questionnaire and recreate
	/// </summary>
	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		foreach (var obj in _questionObjs)
		{
			Destroy(obj);
		}
		_questionObjs.Clear();
		GameManagement.Questionnaire.GetQuestionniare(_questionAsset, _answerStyleAsset);
		foreach (var question in GameManagement.Questionnaire.Questions)
		{
			var questionObj = Instantiate(_questionPrefab, transform, false);
			questionObj.name = _questionPrefab.name;
			questionObj.transform.Find("Question").GetComponent<Text>().text = Localization.Get("QUESTION") + " " + (_questionObjs.Count + 1);
			questionObj.transform.Find("Answer A").GetComponentInChildren<Text>().text = "A. " + question.AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.Find("Answer B").GetComponentInChildren<Text>().text = "B. " + question.AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.Find("Answer A").GetComponentInChildren<Toggle>().onValueChanged.AddListener(delegate { CheckAllToggled(); });
			questionObj.transform.Find("Answer B").GetComponentInChildren<Toggle>().onValueChanged.AddListener(delegate { CheckAllToggled(); });
			_questionObjs.Add(questionObj);
		}
		CheckAllToggled();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
		Invoke("DoBestFit", 0);
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Check that all questions have been answered and enable the submit button if so
	/// </summary>
	private void CheckAllToggled()
	{
		_submitButton.GetComponent<Button>().interactable = (_questionObjs.All(q => q.GetComponent<ToggleGroup>().AnyTogglesOn()));
	}

	/// <summary>
	/// Collect the amount of each style answered and sne dthis information on to be saved
	/// </summary>
	public void SubmitAnswers()
	{
		var results = new Dictionary<string, int>();
		for (int i = 0; i < _questionObjs.Count; i++)
		{
			var style = string.Empty;
			if (_questionObjs[i].transform.Find("Answer A").GetComponentInChildren<Toggle>().isOn)
			{
				style = GameManagement.Questionnaire.Questions[i].AnswerA.Style;
			}
			else if (_questionObjs[i].transform.Find("Answer B").GetComponentInChildren<Toggle>().isOn)
			{
				style = GameManagement.Questionnaire.Questions[i].AnswerB.Style;
			}
			if (results.ContainsKey(style))
			{
				results[style]++;
			}
			else
			{
				results.Add(style, 1);
			}
		}
		GameManagement.Questionnaire.SubmitAnswers(results);
        UIStateManager.StaticGoToFeedback();
	}

	private void OnLanguageChange()
	{
		for (int i = 0; i < _questionObjs.Count; i++)
		{
			_questionObjs[i].transform.Find("Question").GetComponent<Text>().text = Localization.Get("QUESTION") + " " + (i + 1);
			_questionObjs[i].transform.Find("Answer A").GetComponentInChildren<Text>().text = "A. " + GameManagement.Questionnaire.Questions[i].AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			_questionObjs[i].transform.Find("Answer B").GetComponentInChildren<Text>().text = "B. " + GameManagement.Questionnaire.Questions[i].AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
		GetComponentsInChildren<Text>().BestFit();
		_questionObjs.ForEach(q => q.transform.Find("Question").GetComponent<Text>().fontSize = (int)(q.transform.Find("Question").GetComponent<Text>().fontSize * 1.5f));
	}
}
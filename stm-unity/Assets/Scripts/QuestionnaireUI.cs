using System;
using System.Collections.Generic;

using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;

using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Questionnaire))]
public class QuestionnaireUI : MonoBehaviour
{
	private Questionnaire _questionnaire;
	[SerializeField]
	private GameObject _questionnairePanel;
	[SerializeField]
	private GameObject _questionPrefab;
	[SerializeField]
	private GameObject _submitButton;
	private List<GameObject> _questionObjs = new List<GameObject>();

	private void Awake()
	{
		_questionnaire = GetComponent<Questionnaire>();
	}

	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		foreach (var obj in _questionObjs)
		{
			Destroy(obj);
		}
		_questionObjs.Clear();
		_questionnaire.GetQuestionniare();
		foreach (var question in _questionnaire.Questions)
		{
			var questionObj = Instantiate(_questionPrefab, _questionnairePanel.transform, false);
			questionObj.name = _questionPrefab.name;
			questionObj.transform.Find("Question").GetComponent<Text>().text = Localization.Get("QUESTION") + " " + (_questionObjs.Count + 1);
			questionObj.transform.Find("Answer A").GetComponentInChildren<Text>().text = question.AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.Find("Answer B").GetComponentInChildren<Text>().text = question.AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.Find("Answer A").GetComponentInChildren<Toggle>().onValueChanged.AddListener(delegate { CheckAllToggled(); });
			questionObj.transform.Find("Answer B").GetComponentInChildren<Toggle>().onValueChanged.AddListener(delegate { CheckAllToggled(); });
			_questionObjs.Add(questionObj);
		}
		CheckAllToggled();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_questionnairePanel.transform);
		Invoke("DoBestFit", 0);
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	private void CheckAllToggled()
	{
		_submitButton.GetComponent<Button>().interactable = (_questionObjs.All(q => q.GetComponent<ToggleGroup>().AnyTogglesOn()));
	}

	public void SubmitAnswers()
	{
		var results = new Dictionary<string, int>();
		for (int i = 0; i < _questionObjs.Count; i++)
		{
			var style = string.Empty;
			if (_questionObjs[i].transform.Find("Answer A").GetComponentInChildren<Toggle>().isOn)
			{
				style = _questionnaire.Questions[i].AnswerA.Style;
			}
			else if (_questionObjs[i].transform.Find("Answer B").GetComponentInChildren<Toggle>().isOn)
			{
				style = _questionnaire.Questions[i].AnswerB.Style;
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
		_questionnaire.SubmitAnswers(results);
		((UIStateManager)FindObjectOfType(typeof(UIStateManager))).GoToFeedback();
	}

	private void OnLanguageChange()
	{
		for (int i = 0; i < _questionObjs.Count; i++)
		{
			_questionObjs[i].transform.Find("Question").GetComponent<Text>().text = Localization.Get("QUESTION") + " " + (i + 1);
			_questionObjs[i].transform.Find("Answer A").GetComponentInChildren<Text>().text = _questionnaire.Questions[i].AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			_questionObjs[i].transform.Find("Answer B").GetComponentInChildren<Text>().text = _questionnaire.Questions[i].AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_questionnairePanel.transform);
		_questionnairePanel.GetComponentsInChildren<Text>().BestFit();
		_questionObjs.ForEach(q => q.transform.Find("Question").GetComponent<Text>().fontSize = (int)(q.transform.Find("Question").GetComponent<Text>().fontSize * 1.5f));
	}
}
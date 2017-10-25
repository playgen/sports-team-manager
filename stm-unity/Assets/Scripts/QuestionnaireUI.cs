using System.Collections.Generic;
using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SimpleJSON;

/// <summary>
/// Contains all logic relating to displaying post-game questionnaire
/// </summary>
public class QuestionnaireUI : MonoBehaviour
{
	class Answer
	{
		public Dictionary<string, string> Text = new Dictionary<string, string>();
		public string Style;
	}

	class Question
	{
		public Answer AnswerA;
		public Answer AnswerB;
	}

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
	private List<Question> _questions;

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
		GetQuestionniare(_questionAsset, _answerStyleAsset);
		foreach (var question in _questions)
		{
			var questionObj = Instantiate(_questionPrefab, _questionnairePanel.transform, false);
			questionObj.name = _questionPrefab.name;
			questionObj.transform.Find("Question").GetComponent<Text>().text = Localization.Get("QUESTION") + " " + (_questionObjs.Count + 1);
			questionObj.transform.Find("Answer A").GetComponentInChildren<Text>().text = "A. " + question.AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.Find("Answer B").GetComponentInChildren<Text>().text = "B. " + question.AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.Find("Answer A").GetComponentInChildren<Toggle>().onValueChanged.AddListener(CheckAllToggled);
			questionObj.transform.Find("Answer B").GetComponentInChildren<Toggle>().onValueChanged.AddListener(CheckAllToggled);
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

	/// <summary>
	/// Get questions for questionnaire for currently selected language
	/// </summary>
	public void GetQuestionniare(TextAsset question, TextAsset answer)
	{
		var questions = new List<Question>();

		var parsedQuestionAsset = JSON.Parse(question.text);
		var questionDict = new Dictionary<string, Dictionary<string, string>>();
		for (var i = 0; i < parsedQuestionAsset.Count; i++)
		{
			var questionLangDict = new Dictionary<string, string>();
			foreach (var lang in Localization.Languages)
			{
				var langName = lang.Name.ToLower();
				if (parsedQuestionAsset[i][langName] != null)
				{
					questionLangDict.Add(langName, parsedQuestionAsset[i][langName].Value.Replace("\"", ""));
				}
				else
				{
					questionLangDict.Add(langName, parsedQuestionAsset[i][0].Value.Replace("\"", ""));
				}
			}
			questionDict.Add(parsedQuestionAsset[i][0], questionLangDict);
		}

		var parsedStyleAsset = JSON.Parse(answer.text);
		var styleDict = new Dictionary<string, Dictionary<string, string>>();
		for (var i = 0; i < parsedStyleAsset.Count; i++)
		{
			var questionStyleDict = new Dictionary<string, string>
			{
				{ "A", parsedStyleAsset[i]["A"].Value.Replace("\"", "") },
				{ "B", parsedStyleAsset[i]["B"].Value.Replace("\"", "") }
			};
			styleDict.Add(parsedStyleAsset[i][0], questionStyleDict);
		}

		var questionFound = true;
		while (questionFound)
		{
			var currentQuestion = string.Format("QUESTION_{0}", questions.Count + 1);
			if (questionDict.ContainsKey(currentQuestion + "_A") && questionDict.ContainsKey(currentQuestion + "_B") && styleDict.ContainsKey(currentQuestion))
			{
				var q = new Question
				{
					AnswerA = new Answer(),
					AnswerB = new Answer()
				};
				foreach (var lang in Localization.Languages)
				{
					var langName = lang.Name.ToLower();
					if (questionDict[currentQuestion + "_A"][langName] != null)
					{
						q.AnswerA.Text.Add(langName, questionDict[currentQuestion + "_A"][langName].RemoveJSONNodeChars());
					}
					else
					{
						q.AnswerA.Text.Add(langName, questionDict[currentQuestion + "_A"][questionDict[currentQuestion + "_A"].Keys.ToList()[0]].RemoveJSONNodeChars());
					}
					if (questionDict[currentQuestion + "_B"][langName] != null)
					{
						q.AnswerB.Text.Add(langName, questionDict[currentQuestion + "_B"][langName].RemoveJSONNodeChars());
					}
					else
					{
						q.AnswerB.Text.Add(lang.Name, questionDict[currentQuestion + "_B"][questionDict[currentQuestion + "_B"].Keys.ToList()[0]].RemoveJSONNodeChars());
					}
				}
				q.AnswerA.Style = styleDict[currentQuestion]["A"].RemoveJSONNodeChars();
				q.AnswerB.Style = styleDict[currentQuestion]["B"].RemoveJSONNodeChars();
				questions.Add(q);
			}
			else
			{
				questionFound = false;
			}
		}
		_questions = questions;
	}

	/// <summary>
	/// Check that all questions have been answered and enable the submit button if so
	/// </summary>
	private void CheckAllToggled(bool toggle = false)
	{
		_submitButton.GetComponent<Button>().interactable = (_questionObjs.All(q => q.GetComponent<ToggleGroup>().AnyTogglesOn()));
	}

	/// <summary>
	/// Collect the amount of each style answered and sne dthis information on to be saved
	/// </summary>
	public void SubmitAnswers()
	{
		var results = new Dictionary<string, int>();
		for (var i = 0; i < _questionObjs.Count; i++)
		{
			var style = string.Empty;
			if (_questionObjs[i].transform.Find("Answer A").GetComponentInChildren<Toggle>().isOn)
			{
				style = _questions[i].AnswerA.Style;
			}
			else if (_questionObjs[i].transform.Find("Answer B").GetComponentInChildren<Toggle>().isOn)
			{
				style = _questions[i].AnswerB.Style;
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
		GameManagement.GameManager.SaveQuestionnaireResults(results);
		UIStateManager.StaticGoToFeedback();
	}

	private void OnLanguageChange()
	{
		for (var i = 0; i < _questionObjs.Count; i++)
		{
			_questionObjs[i].transform.Find("Question").GetComponent<Text>().text = Localization.Get("QUESTION") + " " + (i + 1);
			_questionObjs[i].transform.Find("Answer A").GetComponentInChildren<Text>().text = "A. " + _questions[i].AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			_questionObjs[i].transform.Find("Answer B").GetComponentInChildren<Text>().text = "B. " + _questions[i].AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
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
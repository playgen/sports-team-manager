using System.Collections.Generic;
using PlayGen.Unity.Utilities.Text;
using PlayGen.Unity.Utilities.Localization;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SimpleJSON;
using PlayGen.Unity.Utilities.Extensions;

/// <summary>
/// Contains all logic relating to displaying post-game questionnaire
/// </summary>
public class QuestionnaireUI : MonoBehaviour
{
	private class Answer
	{
		public readonly Dictionary<string, string> Text = new Dictionary<string, string>();
		public string Style;
	}

	private class Question
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
		BestFit.ResolutionChange += DoBestFitDelay;
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
			questionObj.transform.FindText("Question").text = Localization.Get("QUESTION") + " " + (_questionObjs.Count + 1);
			questionObj.transform.FindText("Answer A/Label").text = "A. " + question.AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.FindText("Answer B/Label").text = "B. " + question.AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
			questionObj.transform.FindComponentInChildren<Toggle>("Answer A").onValueChanged.AddListener(CheckAllToggled);
			questionObj.transform.FindComponentInChildren<Toggle>("Answer B").onValueChanged.AddListener(CheckAllToggled);
			_questionObjs.Add(questionObj);
		}
		CheckAllToggled();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_questionnairePanel.RectTransform());
		Invoke("DoBestFitDelay", 0);
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFitDelay;
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
			    questionLangDict.Add(langName, (parsedQuestionAsset[i][langName] != null ? parsedQuestionAsset[i][langName] : parsedQuestionAsset[i][0]).Value.Replace("\"", ""));
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
				    q.AnswerA.Text.Add(langName, (questionDict[currentQuestion + "_A"][langName] != null ? questionDict[currentQuestion + "_A"][langName] : questionDict[currentQuestion + "_A"][questionDict[currentQuestion + "_A"].Keys.ToList()[0]]).RemoveJSONNodeChars());
				    q.AnswerB.Text.Add(langName, (questionDict[currentQuestion + "_B"][langName] != null ? questionDict[currentQuestion + "_B"][langName] : questionDict[currentQuestion + "_B"][questionDict[currentQuestion + "_B"].Keys.ToList()[0]]).RemoveJSONNodeChars());
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
			if (_questionObjs[i].transform.FindComponentInChildren<Toggle>("Answer A").isOn)
			{
				style = _questions[i].AnswerA.Style;
			}
			else if (_questionObjs[i].transform.FindComponentInChildren<Toggle>("Answer B").isOn)
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
			_questionObjs[i].transform.FindText("Question").text = Localization.Get("QUESTION") + " " + (i + 1);
			_questionObjs[i].transform.FindText("Answer A/Label").text = "A. " + _questions[i].AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()];
			_questionObjs[i].transform.FindText("Answer B/Label").text = "B. " + _questions[i].AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()];
		}
		DoBestFit();
	}

	private void DoBestFitDelay()
	{
		Invoke("DoBestFitSecondDelay", 0);
	}

	private void DoBestFitSecondDelay()
	{
		Invoke("DoBestFit", 0);
	}

	private void DoBestFit()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform.RectTransform());
		_questionObjs.SelectMany(s => s.GetComponentsInChildren<Text>(true).Where(t => !t.name.Contains("Checkmark"))).BestFit();
		_questionObjs.ForEach(q => q.transform.FindText("Question").fontSize = (int)(q.transform.FindText("Question").fontSize * 1.5f));
	}
}
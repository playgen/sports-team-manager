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
		public GameObject GameObject;
		public ToggleGroup ToggleGroup;
		public Text QuestionText;
		public Text AnswerAText;
		public Text AnswerBText;
		public Toggle AnswerAToggle;
		public Toggle AnswerBToggle;
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
	private Button _submitButton;
	private List<Question> _questions = new List<Question>();

	/// <summary>
	/// On Enable, clear questionnaire and recreate
	/// </summary>
	private void OnEnable()
	{
		Localization.LanguageChange += OnLanguageChange;
		BestFit.ResolutionChange += DoBestFit;
		CreateQuestionnaire();
	}

	private void OnDisable()
	{
		Localization.LanguageChange -= OnLanguageChange;
		BestFit.ResolutionChange -= DoBestFit;
	}

	private void CreateQuestionnaire()
	{
		foreach (var obj in _questions)
		{
			Destroy(obj.GameObject);
		}
		_questions.Clear();
		GetQuestionniare(_questionAsset, _answerStyleAsset);
		CheckAllToggled();
		OnLanguageChange();
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
			var questionLangDict = Localization.Languages.ToDictionary(langName => langName.Name.ToLower(), langName => (parsedQuestionAsset[i][langName.Name.ToLower()] ?? parsedQuestionAsset[i][0]).Value.Replace("\"", string.Empty));
			questionDict.Add(parsedQuestionAsset[i][0], questionLangDict);
		}

		var parsedStyleAsset = JSON.Parse(answer.text);
		var styleDict = new Dictionary<string, Dictionary<string, string>>();
		for (var i = 0; i < parsedStyleAsset.Count; i++)
		{
			var questionStyleDict = new Dictionary<string, string>
			{
				{ "A", parsedStyleAsset[i]["A"].Value.Replace("\"", string.Empty) },
				{ "B", parsedStyleAsset[i]["B"].Value.Replace("\"", string.Empty) }
			};
			styleDict.Add(parsedStyleAsset[i][0], questionStyleDict);
		}

		var questionFound = true;
		while (questionFound)
		{
			var currentQuestion = $"QUESTION_{questions.Count + 1}";
			if (questionDict.ContainsKey(currentQuestion + "_A") && questionDict.ContainsKey(currentQuestion + "_B") && styleDict.ContainsKey(currentQuestion))
			{
				var newQuestion = new Question
				{
					AnswerA = new Answer(),
					AnswerB = new Answer()
				};
				foreach (var lang in Localization.Languages)
				{
					var langName = lang.Name.ToLower();
					newQuestion.AnswerA.Text.Add(langName, (questionDict[currentQuestion + "_A"][langName] ?? questionDict[currentQuestion + "_A"][questionDict[currentQuestion + "_A"].Keys.ToList()[0]]).RemoveJSONNodeChars());
					newQuestion.AnswerB.Text.Add(langName, (questionDict[currentQuestion + "_B"][langName] ?? questionDict[currentQuestion + "_B"][questionDict[currentQuestion + "_B"].Keys.ToList()[0]]).RemoveJSONNodeChars());
				}
				newQuestion.AnswerA.Style = styleDict[currentQuestion]["A"].RemoveJSONNodeChars();
				newQuestion.AnswerB.Style = styleDict[currentQuestion]["B"].RemoveJSONNodeChars();
				var questionObj = Instantiate(_questionPrefab, _questionnairePanel.transform, false);
				questionObj.name = _questionPrefab.name;
				newQuestion.GameObject = questionObj;
				newQuestion.ToggleGroup = questionObj.GetComponent<ToggleGroup>();
				newQuestion.QuestionText = newQuestion.GameObject.transform.FindText("Question");
				newQuestion.AnswerAText = newQuestion.GameObject.transform.FindText("Answer A/Label");
				newQuestion.AnswerBText = newQuestion.GameObject.transform.FindText("Answer B/Label");
				newQuestion.AnswerAToggle = newQuestion.GameObject.transform.FindComponentInChildren<Toggle>("Answer A");
				newQuestion.AnswerBToggle = newQuestion.GameObject.transform.FindComponentInChildren<Toggle>("Answer B");
				newQuestion.AnswerAToggle.onValueChanged.AddListener(CheckAllToggled);
				newQuestion.AnswerBToggle.onValueChanged.AddListener(CheckAllToggled);
				questions.Add(newQuestion);
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
		_submitButton.interactable = _questions.All(q => q.ToggleGroup.AnyTogglesOn());
	}

	/// <summary>
	/// Collect the amount of each style answered and send this information on to be saved
	/// </summary>
	public void SubmitAnswers()
	{
		var results = new Dictionary<string, int>();
		foreach (Question question in _questions)
		{
			var style = question.AnswerAToggle.isOn ? question.AnswerA.Style : question.AnswerBToggle.isOn ? question.AnswerB.Style : string.Empty;
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
		var questionCount = 0;
		foreach (Question question in _questions)
		{
			questionCount++;
			question.QuestionText.text = $"{Localization.Get("QUESTION")} {questionCount}";
			question.AnswerAText.text = $"A. {question.AnswerA.Text[Localization.SelectedLanguage.Name.ToLower()]}";
			question.AnswerBText.text = $"B. {question.AnswerB.Text[Localization.SelectedLanguage.Name.ToLower()]}";
		}
		DoBestFit();
	}

	private void DoBestFit()
	{
		var fontSize = _questions.SelectMany(s => new Component[] { s.AnswerAText, s.AnswerBText, s.QuestionText }).BestFit();
		_questions.ForEach(q => q.QuestionText.fontSize = (int)(q.QuestionText.fontSize * 1.5f));
		if (fontSize == 0)
		{
			Invoke(nameof(DoBestFit), 0f);
		}
	}
}
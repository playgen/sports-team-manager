using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using PlayGen.Unity.Utilities.Localization;

using SimpleJSON;
using UnityEngine;

public class Answer
{
	public Dictionary<string, string> Text = new Dictionary<string, string>();
	public string Style;
}

public class Question
{
	public Answer AnswerA;
	public Answer AnswerB;
}

public class Questionnaire : MonoBehaviour
{
	private GameManager _gameManager;
	[SerializeField]
	private TextAsset _questionAsset;
	[SerializeField]
	private TextAsset _answerStyleAsset;
	private List<Question> _questions;
	public List<Question> Questions
	{
		get { return _questions; }
	}

	private void Start()
	{
		_gameManager = ((GameManagerObject)FindObjectOfType(typeof(GameManagerObject))).GameManager;
	}

	public void GetQuestionniare()
	{
		var questions = new List<Question>();

		var parsedQuestionAsset = JSON.Parse(_questionAsset.text);
		var questionDict = new Dictionary<string, Dictionary<string, string>>();
		for (int i = 0; i < parsedQuestionAsset.Count; i++)
		{
			var questionLangDict = new Dictionary<string, string>();
			foreach (CultureInfo lang in Localization.Languages)
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

		var parsedStyleAsset = JSON.Parse(_answerStyleAsset.text);
		var styleDict = new Dictionary<string, Dictionary<string, string>>();
		for (int i = 0; i < parsedStyleAsset.Count; i++)
		{
			var questionStyleDict = new Dictionary<string, string>();
			questionStyleDict.Add("A", parsedStyleAsset[i]["A"].Value.Replace("\"", ""));
			questionStyleDict.Add("B", parsedStyleAsset[i]["B"].Value.Replace("\"", ""));
			styleDict.Add(parsedStyleAsset[i][0], questionStyleDict);
		}

		var questionFound = true;
		while (questionFound)
		{
			var currentQuestion = string.Format("QUESTION_{0}", questions.Count + 1);
			if (questionDict.ContainsKey(currentQuestion + "_A") && questionDict.ContainsKey(currentQuestion + "_B") && styleDict.ContainsKey(currentQuestion))
			{
				var question = new Question
				{
					AnswerA = new Answer(),
					AnswerB = new Answer()
				};
				foreach (CultureInfo lang in Localization.Languages)
				{
					var langName = lang.Name.ToLower();
					if (questionDict[currentQuestion + "_A"][langName] != null)
					{
						question.AnswerA.Text.Add(langName, questionDict[currentQuestion + "_A"][langName].RemoveJSONNodeChars());
					}
					else
					{
						question.AnswerA.Text.Add(langName, questionDict[currentQuestion + "_A"][questionDict[currentQuestion + "_A"].Keys.ToList()[0]].RemoveJSONNodeChars());
					}
					if (questionDict[currentQuestion + "_B"][langName] != null)
					{
						question.AnswerB.Text.Add(langName, questionDict[currentQuestion + "_B"][langName].RemoveJSONNodeChars());
					}
					else
					{
						question.AnswerB.Text.Add(lang.Name, questionDict[currentQuestion + "_B"][questionDict[currentQuestion + "_B"].Keys.ToList()[0]].RemoveJSONNodeChars());
					}
				}
				question.AnswerA.Style = styleDict[currentQuestion]["A"].RemoveJSONNodeChars();
				question.AnswerB.Style = styleDict[currentQuestion]["B"].RemoveJSONNodeChars();
				questions.Add(question);
			}
			else
			{
				questionFound = false;
			}
		}
		_questions = questions;
	}

	public void SubmitAnswers(Dictionary<string, int> answers)
	{
		_gameManager.SaveQuestionnaireResults(answers);
	}
}

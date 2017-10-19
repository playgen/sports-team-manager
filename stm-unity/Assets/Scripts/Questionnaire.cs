using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

/// <summary>
/// Connecting class between GameManager in logic and the Questionnaire UI
/// </summary>
public class Questionnaire
{
	private List<Question> _questions;
	public List<Question> Questions
	{
		get { return _questions; }
	}

	/// <summary>
	/// Get questions for questionnaire for currently selected language
	/// </summary>
	public void GetQuestionniare(TextAsset question, TextAsset answer)
	{
		var questions = new List<Question>();

		var parsedQuestionAsset = JSON.Parse(question.text);
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

		var parsedStyleAsset = JSON.Parse(answer.text);
		var styleDict = new Dictionary<string, Dictionary<string, string>>();
		for (int i = 0; i < parsedStyleAsset.Count; i++)
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
				foreach (CultureInfo lang in Localization.Languages)
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
	/// Send amount of answers given for each type
	/// </summary>
	public void SubmitAnswers(Dictionary<string, int> answers)
	{
		GameManagement.GameManager.SaveQuestionnaireResults(answers);
	}
}

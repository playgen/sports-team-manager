using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
using System;
using System.Linq;

public enum Language
{
	English = 1, Italian
};

public class Localization : MonoBehaviour {
	private static readonly Dictionary<Language, Dictionary<string, string>> LocalizationDict = new Dictionary<Language, Dictionary<string, string>>();

	public string Key;
	public bool ToUpper;

	public static string filePath = "StringLocalizations";
	public static Language SelectedLanguage { get; set; }
	public static event Action LanguageChange = delegate {};

	void Awake()
	{
		if (SelectedLanguage != 0)
		{
			return;
		}
		TextAsset jsonTextAsset = Resources.Load(filePath) as TextAsset;

		var N = JSON.Parse(jsonTextAsset.text);
		foreach (Language l in Enum.GetValues(typeof(Language)))
		{
			Dictionary<string, string> languageStrings = new Dictionary<string, string>();
			for (int i = 0; i < N.Count; i++)
			{
				//go through the list and add the strings to the dictionary
				string _key = N[i]["Key"].ToString();
				_key = _key.Replace("\"", "");
				if (N[i][l.ToString()] != null)
				{
					string _value = N[i][l.ToString()].ToString();
					_value = _value.Replace("\"", "");
					languageStrings[_key] = _value;
				}
			}
			LocalizationDict[l] = languageStrings;
		}
		if (PlayerPrefs.HasKey("Language"))
		{
			SelectedLanguage = (Language)PlayerPrefs.GetInt("Language");
		}
		else
		{
			SelectedLanguage = Language.English;
			PlayerPrefs.SetInt("Language", (int)SelectedLanguage);
		}
	}

	void OnEnable()
	{
		Set();
	}

	public static string Get(string key, bool toUpper = false)
	{
		string txt;
		key = key.ToUpper();
		key = key.Replace('-', '_');

		LocalizationDict[SelectedLanguage].TryGetValue(key, out txt);
		if (txt == null)
		{
			LocalizationDict[Language.English].TryGetValue(key, out txt);
			if (txt == null)
			{
				txt = key;
			}
		}
		//new line character in spreadsheet is *n*
		txt = txt.Replace("\\n", "\n");
		if (toUpper)
		{
			txt = txt.ToUpper();
		}
		return txt;
	}

	public static string GetAndFormat(string key, bool toUpper, params object[] args)
	{
		return string.Format(Get(key, toUpper), args);
	}

	public void Set() {
		Text _text = GetComponent<Text>();
		if (_text == null)
			Debug.LogError("Localization script could not find Text component attached to this gameObject: " + gameObject.name);
		_text.text = Get(Key);
		if (_text.text == "")
		{
			Debug.LogError("Could not find string with key: " + Key);
		}
		if (ToUpper)
			_text.text = _text.text.ToUpper();
	}

	public static void UpdateLanguage(Language language)
	{
		SelectedLanguage = language;
		PlayerPrefs.SetInt("Language", (int)SelectedLanguage);
		((Localization[])FindObjectsOfType(typeof(Localization))).ToList().ForEach(l => l.Set());
		LanguageChange();
	}
}

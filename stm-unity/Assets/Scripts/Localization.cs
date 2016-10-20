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
	public bool toUpper;

	public static string filePath = "StringLocalizations";

	public static Language SelectedLanguage { get; set; }

	void Awake()
	{
		ConvertJsonToDict();
		if (PlayerPrefs.HasKey("Language"))
		{
			SelectedLanguage = (Language)Enum.Parse(typeof(Language), PlayerPrefs.GetString("Language"));
		}
		else
		{
			SelectedLanguage = Language.Italian;
		}
	}

	void OnEnable()
	{
		Set();
	}

	static void ConvertJsonToDict()
	{
		TextAsset jsonTextAsset = Resources.Load(filePath) as TextAsset;

		var N = JSON.Parse(jsonTextAsset.text);
		for (int l = 1; l <= Enum.GetNames(typeof(Language)).Length; l++)
		{
			Dictionary<string, string> languageStrings = new Dictionary<string, string>();
			for (int i = 0; N[i] != null; i++)
			{
				//go through the list and add the strings to the dictionary
				string _key = N[i][0].ToString();
				_key = _key.Replace("\"", "");
				string _value = N[i][l].ToString();
				_value = _value.Replace("\"", "");
				languageStrings[_key] = _value;
			}
			LocalizationDict[(Language)l] = languageStrings;
		}
	}
	
	public static string Get(string key, bool toUpper = false)
	{
		string txt;
		key = key.ToUpper();
		key = key.Replace('-', '_');

		LocalizationDict[SelectedLanguage].TryGetValue(key, out txt);

		//new line character in spreadsheet is *n*
		txt = txt.Replace("\\n", "\n");
		if (toUpper)
		{
			txt = txt.ToUpper();
		}
		return txt;
	}

	public static string GetByLanguage(string key, Language language)
	{
		if (language == SelectedLanguage)
		{
			return key;
		}
		var singleOrDefault = LocalizationDict[language].SingleOrDefault(k => k.Value.ToLower() == key.ToLower()).Key;
		if (singleOrDefault == null)
		{
			Debug.LogError("Multiple values in " + language + " equal " + key);
			return string.Empty;
		}
		return Get(singleOrDefault);
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
		if (toUpper)
			_text.text = _text.text.ToUpper();
	}
}

public static class LocalizationExtensions
{
	public static string Localize (this string key)
	{
		return Localization.Get(key);
	}

	public static string LocalizeToUpper(this string key)
	{
		return Localization.Get(key, true);
	}

	public static string LocalizeFromEnglish(this string text)
	{
		return Localization.GetByLanguage(text, Language.English);
	}
}

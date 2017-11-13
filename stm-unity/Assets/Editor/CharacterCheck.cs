using PlayGen.Unity.Utilities.Localization;
using SimpleJSON;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCheck : EditorWindow
{
    private Text _text;

    [MenuItem("Tools/Localization Check")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CharacterCheck), true, "Localization Check", true);
    }

    void OnGUI()
    {
        _text = (Text)EditorGUILayout.ObjectField(_text, typeof(Text), true);

        if (_text)
        {
            if (GUILayout.Button("Set", GUILayout.ExpandWidth(false)))
            {
                Localization.UpdateLanguage(Localization.SelectedLanguage != null ? Localization.SelectedLanguage.Name : "en");
                var resourceList = new[] { "Tutorial", "Questionnaire", "Localization" };
                var languageString = string.Empty;
                foreach (var resource in resourceList)
                {
                    var jsonTextAssets = Resources.LoadAll(resource, typeof(TextAsset)).Cast<TextAsset>().ToArray();
                    foreach (var textAsset in jsonTextAssets)
                    {
                        foreach (var l in Localization.Languages)
                        {
                            var n = JSON.Parse(textAsset.text);
                            for (var i = 0; i < n.Count; i++)
                            {
                                //go through the list and add the strings to the dictionary
                                if (n[i][l.Name.ToLower()] != null)
                                {
                                    var value = n[i][l.Name.ToLower()].ToString();
                                    value = value.Replace("\"", "");
                                    foreach (var c in value)
                                    {
                                        if (!languageString.Contains(c))
                                        {
                                            languageString += c;
                                        }
                                    }
                                }
                                else if (n[i]["Section Text " + l.EnglishName] != null)
                                {
                                    var value = n[i]["Section Text " + l.EnglishName].ToString();
                                    value = value.Replace("\"", "");
                                    foreach (var c in value)
                                    {
                                        if (!languageString.Contains(c))
                                        {
                                            languageString += c;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _text.text = languageString;
                Debug.Log(languageString);
            }
        }
    }
}

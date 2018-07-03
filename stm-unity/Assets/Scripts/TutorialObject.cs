using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LanguageKeyValuePair
{
    public string Key;
    [TextArea]
    public List<string> Value;

    public LanguageKeyValuePair(string k, List<string> v)
    {
        Key = k;
        Value = v;
    }
}
[Serializable]
public class TriggerKeyValuePair
{
    public string Key;
    public string Value;

    public TriggerKeyValuePair(string k, string v)
    {
        Key = k;
        Value = v;
    }
}

[Serializable]
public class StringList
{
    public List<string> List;

    public StringList(List<string> l)
    {
        List = l;
    }

    public StringList(string[] l)
    {
        List = l.ToList();
    }
}

[Serializable]
public class TutorialObject
{
	/// <summary>
	/// Section Name is only used in inspector to identify each step easily
	/// </summary>
	public string SectionName; 
    public List<LanguageKeyValuePair> SectionTextHolder;
    public bool Reversed;
    public List<StringList> HighlightedObjects;
    public List<TriggerKeyValuePair> Triggers;
    public bool UniqueEvents;
    public int EventTriggerCountRequired;
    public int SaveNextSection;
    public List<StringList> BlacklistButtons;
    public List<string> CustomAttributes;

    public TutorialObject(Dictionary<string, List<string>> text, List<string[]> highlightObjs, bool reversed, KeyValuePair<string, string>[] triggers, int triggerCount, bool uniqueTriggers, int saveSection, List<List<string>> blacklist, List<string> attributes, string sectionName)
    {
	    SectionName = sectionName;
        SectionTextHolder = new List<LanguageKeyValuePair>();
        foreach (var kvp in text)
        {
            SectionTextHolder.Add(new LanguageKeyValuePair(kvp.Key, kvp.Value));
        }
        Triggers = new List<TriggerKeyValuePair>();
        foreach (var kvp in triggers)
        {
            Triggers.Add(new TriggerKeyValuePair(kvp.Key, kvp.Value));
        }
        HighlightedObjects = new List<StringList>();
        foreach (var obj in highlightObjs)
        {
            HighlightedObjects.Add(new StringList(obj));
        }
        Reversed = reversed;
        EventTriggerCountRequired = triggerCount;
        UniqueEvents = uniqueTriggers;
        SaveNextSection = saveSection;
        BlacklistButtons = new List<StringList>();
        foreach (var obj in blacklist)
        {
            BlacklistButtons.Add(new StringList(obj));
        }
        CustomAttributes = attributes;
    }
}

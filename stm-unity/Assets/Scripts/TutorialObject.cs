using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LanguageKeyValuePair
{
    public string Key;
    [TextArea]
    public string[] Value;

    public LanguageKeyValuePair(string k, string[] v)
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
public class TutorialObject
{
    public List<LanguageKeyValuePair> SectionTextHolder;
    public bool Reversed;
    public string[] HighlightedObjects;
    public int HighlightTrigger;
    public List<TriggerKeyValuePair> Triggers;
    public bool UniqueEvents;
    public int EventTriggerCountRequired;
    public int SaveNextSection;
    public List<string> BlacklistButtons;
    public List<string> CustomAttributes;

    public TutorialObject(Dictionary<string, string[]> text, string[] highlightObjs, int highlightTrigger, bool reversed, KeyValuePair<string, string>[] triggers, int triggerCount, bool uniqueTriggers, int saveSection, List<string> blacklist, List<string> attributes)
    {
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
        HighlightedObjects = highlightObjs;
        HighlightTrigger = highlightTrigger;
        Reversed = reversed;
        EventTriggerCountRequired = triggerCount;
        UniqueEvents = uniqueTriggers;
        SaveNextSection = saveSection;
        BlacklistButtons = blacklist;
        CustomAttributes = attributes;
    }
}

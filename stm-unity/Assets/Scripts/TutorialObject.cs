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
	[TextArea]
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
	/// <summary>
	/// Text to be displayed to the user
	/// </summary>
    public List<LanguageKeyValuePair> SectionTextHolder;
	/// <summary>
	/// If the popup should be shown on the left side of the screen
	/// </summary>
	public bool ShowOnLeft;
	/// <summary>
	/// Path to the object that will be highlighted to guide the player (from Canvas level)
	/// </summary>
	[TextArea]
	public List<string> HighlightedObject;
	/// <summary>
	/// The events that must happen for the player to complete this step of the tutorial
	/// </summary>
	public List<TriggerKeyValuePair> Triggers;
	/// <summary>
	/// Whether the events that happen before must be unique and in the correct order
	/// </summary>
	public bool UniqueEvents;
	/// <summary>
	/// The number of times players must complete activate the trigger event to continue to the next step
	/// </summary>
	public int EventTriggerCountRequired;
	/// <summary>
	/// If the current step in the tutorial can be saved, sometimes UI must be open for the current step to activate/finish properly
	/// </summary>
	public bool SafeToSave;
	/// <summary>
	/// Buttons which cannot be interacted with at this stage in the tutorial
	/// </summary>
	public List<StringList> BlacklistButtons;
	/// <summary>
	/// Additional attributes to help set up the stage in the tutorial
	/// </summary>
	public List<string> CustomAttributes;

    public TutorialObject(Dictionary<string, List<string>> text, string highlightObjs, bool showOnLeft, KeyValuePair<string, string>[] triggers, int triggerCount, bool uniqueTriggers, bool safeToSave, List<List<string>> blacklist, List<string> attributes, string sectionName)
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
	    HighlightedObject = new List<string> {highlightObjs};
        ShowOnLeft = showOnLeft;
        EventTriggerCountRequired = triggerCount;
        UniqueEvents = uniqueTriggers;
        SafeToSave = safeToSave;
        BlacklistButtons = new List<StringList>();
        foreach (var obj in blacklist)
        {
            BlacklistButtons.Add(new StringList(obj));
        }
        CustomAttributes = attributes;
    }
}

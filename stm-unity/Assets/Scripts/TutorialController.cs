using System;
using System.Collections.Generic;

using PlayGen.RAGE.SportsTeamManager.Simulation;
using SimpleJSON;
using UnityEngine;
using System.Linq;

using UnityEngine.UI.Extensions;

public class TutorialController : MonoBehaviour
{
    private GameManager _gameManager;
    private int _stage;
    [SerializeField]
    private GameObject _tutorialSectionPrefab;

    [ContextMenu("Create Tutorial")]
    public void Createtutorial()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        TextAsset textAsset = Resources.Load("Tutorial") as TextAsset;
        var parsedAsset = JSON.Parse(textAsset.text);
        for (int i = 0; i < parsedAsset.Count; i++)
        {
            var tutorialSection = Instantiate(_tutorialSectionPrefab, transform, false) as GameObject;
            var section = tutorialSection.GetComponent<TutorialSectionUI>();
            var textList = parsedAsset[i]["Section Text"].Value.Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).ToList();
            var text = textList.Where(tl => tl.Length > 0).ToArray();
            var objectNames = parsedAsset[i]["Highlighted Object"].RemoveJSONNodeChars().Split('/');
            var anchorObject = (RectTransform)transform.root;
            foreach (var obj in objectNames)
            {
                anchorObject = (RectTransform)anchorObject.FindInactive(obj) ?? anchorObject;
            }
            tutorialSection.GetComponentInChildren<SoftMaskScript>().maskScalingRect = anchorObject;
            tutorialSection.GetComponentInChildren<ReverseRaycastTarget>().MaskRect.Add(anchorObject);
            var reversed = bool.Parse(parsedAsset[i]["Reversed UI"].RemoveJSONNodeChars());
            var triggerSplit = parsedAsset[i]["Triggers"].RemoveJSONNodeChars().Split('\n').ToList().Select(te => te.RemoveJSONNodeChars()).ToList();
            var triggers = triggerSplit.Select(ts => ts.NoSpaces().Split(',')).Select(ts => new KeyValueMessage(ts[0], ts[1])).ToArray();
            var triggerCount = int.Parse(parsedAsset[i]["Trigger Count Required"].RemoveJSONNodeChars());
            var uniqueTriggers = bool.Parse(parsedAsset[i]["Unique Triggers"].RemoveJSONNodeChars());
            var wipeTriggers = bool.Parse(parsedAsset[i]["Wipe Triggered Objects"].RemoveJSONNodeChars());
            section.Construct(text, reversed, triggers, triggerCount, uniqueTriggers, wipeTriggers);
            tutorialSection.name = _tutorialSectionPrefab.name;
        }
        foreach (Transform child in transform)
        {
            if (child.GetSiblingIndex() == _stage)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void AdvanceStage()
    {
        transform.GetChild(_stage).gameObject.SetActive(false);
        _stage++;
        if (_stage < transform.childCount)
        {
            transform.GetChild(_stage).gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

public static class JSONStringParse
{
    public static string RemoveJSONNodeChars(this string text)
    {
        text = text.Replace("\"", "");
        text = text.Replace("\\\\n", "\n");
        return text;
    }

    public static string RemoveJSONNodeChars(this JSONNode node)
    {
        var text = node.Value.Replace("\"", "");
        text = text.Replace("\\\\n", "\n");
        return text;
    }
}
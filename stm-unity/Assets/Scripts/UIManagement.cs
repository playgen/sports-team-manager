using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UIManagement
{
    private static List<GameObject> _rootObjects = new List<GameObject>();
    private static PostRaceEventUI[] _postRaceEvents;
    private static MemberMeetingUI _memberMeeting;
    private static PositionDisplayUI _positionDisplay;
    private static TutorialController _tutorial;
    private static SettingsUI _settings;
    private static RecruitMemberUI _recruitment;
    private static TeamSelectionUI _teamSelection;
    private static RaceResultUI _raceResult;
    private static CupResultUI _cupResult;
    private static BoatPromotionUI _promotion;
    private static PreRaceConfirmUI _preRace;
    private static LearningPillUI _learningPill;
    private static HoverPopUpUI _hover;
    private static PostRaceEventImpactUI _eventImpact;

    private static Button _smallBlocker;
    private static Button _blocker;

    public static PostRaceEventUI[] PostRaceEvents
    {
        get { return _postRaceEvents; }
    }
    public static MemberMeetingUI MemberMeeting
    {
        get { return _memberMeeting; }
    }
    public static PositionDisplayUI PositionDisplay
    {
        get { return _positionDisplay; }
    }
    public static TutorialController Tutorial
    {
        get { return _tutorial; }
    }
    public static SettingsUI Settings
    {
        get { return _settings; }
    }
    public static RecruitMemberUI Recruitment
    {
        get { return _recruitment; }
    }
    public static TeamSelectionUI TeamSelection
    {
        get { return _teamSelection; }
    }
    public static RaceResultUI RaceResult
    {
        get { return _raceResult; }
    }
    public static CupResultUI CupResult
    {
        get { return _cupResult; }
    }
    public static BoatPromotionUI Promotion
    {
        get { return _promotion; }
    }
    public static PreRaceConfirmUI PreRace
    {
        get { return _preRace; }
    }
    public static LearningPillUI LearningPill
    {
        get { return _learningPill; }
    }
    public static HoverPopUpUI Hover
    {
        get { return _hover; }
    }
    public static PostRaceEventImpactUI EventImpact
    {
        get { return _eventImpact; }
    }

    public static Button SmallBlocker
    {
        get { return _smallBlocker; }
    }
    public static Button Blocker
    {
        get { return _blocker; }
    }

    public static void Initialize()
    {
        _rootObjects.Clear();
        _rootObjects = SceneManager.GetActiveScene().GetRootGameObjects().Where(r => r.gameObject != null).ToList();
        _postRaceEvents = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PostRaceEventUI>(true)).ToArray();
        _memberMeeting = _rootObjects.SelectMany(g => g.GetComponentsInChildren<MemberMeetingUI>(true)).First();
        _positionDisplay = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PositionDisplayUI>(true)).First();
        _tutorial = _rootObjects.SelectMany(g => g.GetComponentsInChildren<TutorialController>(true)).First();
        _settings = _rootObjects.SelectMany(g => g.GetComponentsInChildren<SettingsUI>(true)).Last();
        _recruitment = _rootObjects.SelectMany(g => g.GetComponentsInChildren<RecruitMemberUI>(true)).First();
        _teamSelection = _rootObjects.SelectMany(g => g.GetComponentsInChildren<TeamSelectionUI>(true)).First();
        _raceResult = _rootObjects.SelectMany(g => g.GetComponentsInChildren<RaceResultUI>(true)).First();
        _cupResult = _rootObjects.SelectMany(g => g.GetComponentsInChildren<CupResultUI>(true)).First();
        _promotion = _rootObjects.SelectMany(g => g.GetComponentsInChildren<BoatPromotionUI>(true)).First();
        _preRace = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PreRaceConfirmUI>(true)).First();
        _learningPill = _rootObjects.SelectMany(g => g.GetComponentsInChildren<LearningPillUI>(true)).First();
        _hover = _rootObjects.SelectMany(g => g.GetComponentsInChildren<HoverPopUpUI>(true)).First();
        _eventImpact = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PostRaceEventImpactUI>(true)).First();

        _smallBlocker = _rootObjects.Single(g => g.name == "Canvas").transform.Find("Team Management/Pop-up Bounds/Blocker").GetComponent<Button>();
        _blocker = _rootObjects.Single(g => g.name == "Canvas").transform.Find("Team Management/Pop-up Bounds/Bigger Blocker").GetComponent<Button>();
    }

    public static CrewMemberUI[] CrewMemberUI
    {
        get { return _rootObjects.Where(r => r.gameObject != null).SelectMany(g => g.GetComponentsInChildren<CrewMemberUI>()).ToArray(); }
    }

    public static void EnableSmallBlocker(this Transform obj, params UnityAction[] actions)
    {
        _smallBlocker.transform.SetAsLastSibling();
        obj.SetAsLastSibling();
        _smallBlocker.gameObject.SetActive(true);
        _smallBlocker.onClick.RemoveAllListeners();
        foreach (var action in actions)
        {
            _smallBlocker.onClick.AddListener(action);
        }
    }

    public static void EnableBlocker(this Transform obj, params UnityAction[] actions)
    {
        _blocker.transform.SetAsLastSibling();
        obj.SetAsLastSibling();
        _blocker.gameObject.SetActive(true);
        _blocker.onClick.RemoveAllListeners();
        foreach (var action in actions)
        {
            _blocker.onClick.AddListener(action);
        }
    }
}
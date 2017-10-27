using System.Linq;

using UnityEngine.SceneManagement;

public static class UIManagement
{
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

    public static void Initialize()
    {
        _postRaceEvents = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PostRaceEventUI>(true)).ToArray();
        _memberMeeting = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MemberMeetingUI>(true)).First();
        _positionDisplay = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PositionDisplayUI>(true)).First();
        _tutorial = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TutorialController>(true)).First();
        _settings = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<SettingsUI>(true)).Last();
        _recruitment = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RecruitMemberUI>(true)).First();
        _teamSelection = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TeamSelectionUI>(true)).First();
        _raceResult = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RaceResultUI>(true)).First();
        _cupResult = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<CupResultUI>(true)).First();
        _promotion = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoatPromotionUI>(true)).First();
        _preRace = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PreRaceConfirmUI>(true)).First();
        _learningPill = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LearningPillUI>(true)).First();
        _hover = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<HoverPopUpUI>(true)).First();
    }

    public static CrewMemberUI[] CrewMemberUI
    {
        get { return SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<CrewMemberUI>()).ToArray(); }
    }
}
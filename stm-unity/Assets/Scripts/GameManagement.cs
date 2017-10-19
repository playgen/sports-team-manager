using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Container for the GameManager in the Simulation
/// </summary>
public static class GameManagement
{
    private static GameManager _gameManager = new GameManager(Application.platform == RuntimePlatform.Android);
    private static Feedback _feedback = new Feedback();
    private static LearningPill _learningPill = new LearningPill();
    private static LoadGame _loadGame = new LoadGame();
    private static MemberMeeting _memberMeeting = new MemberMeeting();
    private static NewGame _newGame = new NewGame();
    private static PositionDisplay _positionDisplay = new PositionDisplay();
    private static PostRaceEvent _postRaceEvent = new PostRaceEvent();
    private static Questionnaire _questionnaire = new Questionnaire();
    private static RecruitMember _recruitMember = new RecruitMember();
    private static TeamSelection _teamSelection = new TeamSelection();

    public static GameManager GameManager
    {
        get { return _gameManager; }
    }
    public static Feedback Feedback
    {
        get { return _feedback; }
    }
    public static LearningPill LearningPill
    {
        get { return _learningPill; }
    }
    public static LoadGame LoadGame
    {
        get { return _loadGame; }
    }
    public static MemberMeeting MemberMeeting
    {
        get { return _memberMeeting; }
    }
    public static NewGame NewGame
    {
        get { return _newGame; }
    }
    public static PositionDisplay PositionDisplay
    {
        get { return _positionDisplay; }
    }
    public static PostRaceEvent PostRaceEvent
    {
        get { return _postRaceEvent; }
    }
    public static Questionnaire Questionnaire
    {
        get { return _questionnaire; }
    }
    public static RecruitMember RecruitMember
    {
        get { return _recruitMember; }
    }
    public static TeamSelection TeamSelection
    {
        get { return _teamSelection; }
    }
}

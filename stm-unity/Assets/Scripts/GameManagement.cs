using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Container for the GameManager in the Simulation
/// </summary>
public static class GameManagement
{
    private static readonly GameManager _gameManager = new GameManager(Application.platform == RuntimePlatform.Android);
    private static readonly Feedback _feedback = new Feedback();
    private static readonly LearningPill _learningPill = new LearningPill();
    private static readonly LoadGame _loadGame = new LoadGame();
    private static readonly MemberMeeting _memberMeeting = new MemberMeeting();
    private static readonly NewGame _newGame = new NewGame();
    private static readonly PositionDisplay _positionDisplay = new PositionDisplay();
    private static readonly PostRaceEvent _postRaceEvent = new PostRaceEvent();
    private static readonly Questionnaire _questionnaire = new Questionnaire();
    private static readonly RecruitMember _recruitMember = new RecruitMember();
    private static readonly TeamSelection _teamSelection = new TeamSelection();

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

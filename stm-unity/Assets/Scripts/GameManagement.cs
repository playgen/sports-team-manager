using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Container for the GameManager in the Simulation
/// </summary>
public static class GameManagement
{
    private static readonly GameManager _gameManager = new GameManager(Application.platform == RuntimePlatform.Android);
    private static readonly LoadGame _loadGame = new LoadGame();
    private static readonly MemberMeeting _memberMeeting = new MemberMeeting();
    private static readonly NewGame _newGame = new NewGame();
    private static readonly PostRaceEvent _postRaceEvent = new PostRaceEvent();
    private static readonly Questionnaire _questionnaire = new Questionnaire();
    private static readonly RecruitMember _recruitMember = new RecruitMember();
    private static readonly TeamSelection _teamSelection = new TeamSelection();

    public static GameManager GameManager
    {
        get { return _gameManager; }
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

    public static Team Team
    {
        get { return _gameManager.Team; }
    }
    public static string TeamName
    {
        get { return _gameManager.Team.Name; }
    }
    public static Dictionary<string, CrewMember> CrewMembers
    {
        get { return _gameManager.Team.CrewMembers; }
    }
    public static List<Boat> LineUpHistory
    {
        get { return _gameManager.Team.LineUpHistory; }
    }
    public static Person Manager
    {
        get { return _gameManager.Team.Manager; }
    }
    public static Boat Boat
    {
        get { return _gameManager.Team.Boat; }
    }
    public static Dictionary<Position, CrewMember> PositionCrew
    {
        get { return _gameManager.Team.Boat.PositionCrew; }
    }
    public static List<Position> Positions
    {
        get { return _gameManager.Team.Boat.Positions; }
    }
    public static int PositionCount
    {
        get { return _gameManager.Team.Boat.Positions.Count; }
    }
    public static List<PostRaceEventState> CurrentEvent
    {
        get { return _gameManager.EventController.PostRaceEvents.First(); }
    }
    public static bool ShowTutorial
    {
        get { return _gameManager.ShowTutorial; }
    }
    public static int TutorialStage
    {
        get { return _gameManager.TutorialStage; }
    }
    public static int CurrentRaceSession
    {
        get { return _gameManager.CurrentRaceSession + 1; }
    }
    public static int RaceSessionLength
    {
        get { return _gameManager.RaceSessionLength; }
    }
    public static string CurrentSessionString
    {
        get { return CurrentRaceSession + "/" + RaceSessionLength; }
    }
    public static int ActionAllowance
    {
        get { return _gameManager.ActionAllowance; }
    }
    public static int CrewEditAllowance
    {
        get { return _gameManager.CrewEditAllowance; }
    }
    public static int StartingActionAllowance
    {
        get { return _gameManager.GetStartingActionAllowance(); }
    }
    public static int StartingCrewEditAllowance
    {
        get { return _gameManager.GetStartingCrewEditAllowance(); }
    }
}

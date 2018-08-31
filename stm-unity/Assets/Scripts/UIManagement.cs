using PlayGen.Unity.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIManagement
{
	public static UIStateManager StateManager { get; private set; }
	public static PostRaceEventUI[] PostRaceEvents { get; private set; }
	public static MemberMeetingUI MemberMeeting { get; private set; }
	public static PositionDisplayUI PositionDisplay { get; private set; }
	public static NotesUI Notes { get; private set; }
	public static TutorialController Tutorial { get; private set; }
	public static SettingsUI[] Settings { get; private set; }
	public static RecruitMemberUI Recruitment { get; private set; }
	public static TeamSelectionUI TeamSelection { get; private set; }
	public static RaceResultUI RaceResult { get; private set; }
	public static CupResultUI CupResult { get; private set; }
	public static BoatPromotionUI Promotion { get; private set; }
	public static PreRaceConfirmUI PreRace { get; private set; }
	public static LearningPillUI LearningPill { get; private set; }
	public static HoverPopUpUI Hover { get; private set; }
	public static PostRaceEventImpactUI EventImpact { get; private set; }
	public static GameObject Canvas { get; private set; }
	public static Transform DragCanvas { get; private set; }
	public static Button SmallBlocker { get; private set; }
	public static Button Blocker { get; private set; }

	/// <summary>
	/// Set all of the object references
	/// </summary>
	public static void Initialize()
	{
		Canvas = GameObject.Find("Canvas");
		StateManager = Canvas.GetComponentInChildren<UIStateManager>(true);
		PostRaceEvents = Canvas.GetComponentsInChildren<PostRaceEventUI>(true);
		MemberMeeting = Canvas.GetComponentInChildren<MemberMeetingUI>(true);
		Notes = Canvas.GetComponentInChildren<NotesUI>(true);
		PositionDisplay = Canvas.GetComponentInChildren<PositionDisplayUI>(true);
		Tutorial = Canvas.GetComponentInChildren<TutorialController>(true);
		Settings = Canvas.GetComponentsInChildren<SettingsUI>(true);
		Recruitment = Canvas.GetComponentInChildren<RecruitMemberUI>(true);
		TeamSelection = Canvas.GetComponentInChildren<TeamSelectionUI>(true);
		RaceResult = Canvas.GetComponentInChildren<RaceResultUI>(true);
		CupResult = Canvas.GetComponentInChildren<CupResultUI>(true);
		Promotion = Canvas.GetComponentInChildren<BoatPromotionUI>(true);
		PreRace = Canvas.GetComponentInChildren<PreRaceConfirmUI>(true);
		LearningPill = GameObject.Find("Learning Pill Canvas").GetComponentInChildren<LearningPillUI>(true);
		Hover = Canvas.GetComponentInChildren<HoverPopUpUI>(true);
		EventImpact = Canvas.GetComponentInChildren<PostRaceEventImpactUI>(true);

		DragCanvas = GameObject.Find("Drag Canvas").transform;
		SmallBlocker = Canvas.transform.FindButton("Team Management/Pop-up Bounds/Blocker");
		Blocker = Canvas.transform.FindButton("Team Management/Pop-up Bounds/Bigger Blocker");
	}

	/// <summary>
	/// Array of all CrewMemberUI objects in the scene
	/// </summary>
	public static CrewMemberUI[] CrewMemberUI => Canvas.GetComponentsInChildren<CrewMemberUI>();

	/// <summary>
	/// Enable the small blocker, rearrange transform children so that all other objects are blocked and set the blocker to trigger the provided actions
	/// </summary>
	public static void EnableSmallBlocker(this Transform obj, params UnityAction[] actions)
	{
		SmallBlocker.transform.SetAsLastSibling();
		obj.SetAsLastSibling();
		SmallBlocker.gameObject.Active(true);
		SmallBlocker.onClick.RemoveAllListeners();
		foreach (var action in actions)
		{
			SmallBlocker.onClick.AddListener(action);
		}
	}

	/// <summary>
	/// Enable the larger blocker, rearrange transform children so that all other objects are blocked and set the blocker to trigger the provided actions
	/// </summary>
	public static void EnableBlocker(this Transform obj, params UnityAction[] actions)
	{
		Blocker.transform.SetAsLastSibling();
		obj.SetAsLastSibling();
		Blocker.gameObject.Active(true);
		Blocker.gameObject.Active(true);
		Blocker.onClick.RemoveAllListeners();
		foreach (var action in actions)
		{
			Blocker.onClick.AddListener(action);
		}
	}

	/// <summary>
	/// Disable the small blocker
	/// </summary>
	public static void DisableSmallBlocker()
	{
		SmallBlocker.gameObject.Active(false);
	}

	/// <summary>
	/// Disable the larger blocker
	/// </summary>
	public static void DisableBlocker()
	{
		Blocker.gameObject.Active(false);
	}

	/// <summary>
	/// Set this object to be active if it isn't already active in the hierarchy
	/// </summary>
	public static void Active(this GameObject go, bool active)
	{
		if (go.activeInHierarchy != active)
		{
			go.SetActive(active);
		}
	}
}
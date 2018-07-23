using PlayGen.Unity.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UIManagement
{
	private static List<GameObject> _rootObjects = new List<GameObject>();

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

	public static void Initialize()
	{
		_rootObjects.Clear();
		_rootObjects = SceneManager.GetActiveScene().GetRootGameObjects().Where(r => r.gameObject != null).ToList();
		Canvas = _rootObjects.Single(g => g.name == "Canvas");
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
		LearningPill = Canvas.GetComponentInChildren<LearningPillUI>(true);
		Hover = Canvas.GetComponentInChildren<HoverPopUpUI>(true);
		EventImpact = Canvas.GetComponentInChildren<PostRaceEventImpactUI>(true);

		DragCanvas = GameObject.Find("Drag Canvas").transform;
		SmallBlocker = _rootObjects.Single(g => g.name == "Canvas").transform.FindButton("Team Management/Pop-up Bounds/Blocker");
		Blocker = _rootObjects.Single(g => g.name == "Canvas").transform.FindButton("Team Management/Pop-up Bounds/Bigger Blocker");
	}

	public static CrewMemberUI[] CrewMemberUI => Canvas.GetComponentsInChildren<CrewMemberUI>();

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

	public static void DisableSmallBlocker()
	{
		SmallBlocker.gameObject.Active(false);
	}

	public static void DisableBlocker()
	{
		Blocker.gameObject.Active(false);
	}

	public static void Active(this GameObject go, bool active)
	{
		if (go.activeInHierarchy != active)
		{
			go.SetActive(active);
		}
	}
}
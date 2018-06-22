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

	public static PostRaceEventUI[] PostRaceEvents { get; private set; }
	public static MemberMeetingUI MemberMeeting { get; private set; }
	public static PositionDisplayUI PositionDisplay { get; private set; }
	public static NotesUI Notes { get; private set; }
	public static TutorialController Tutorial { get; private set; }
	public static SettingsUI Settings { get; private set; }
	public static RecruitMemberUI Recruitment { get; private set; }
	public static TeamSelectionUI TeamSelection { get; private set; }
	public static RaceResultUI RaceResult { get; private set; }
	public static CupResultUI CupResult { get; private set; }
	public static BoatPromotionUI Promotion { get; private set; }
	public static PreRaceConfirmUI PreRace { get; private set; }
	public static LearningPillUI LearningPill { get; private set; }
	public static HoverPopUpUI Hover { get; private set; }
	public static PostRaceEventImpactUI EventImpact { get; private set; }
	public static Button SmallBlocker { get; private set; }
	public static Button Blocker { get; private set; }

	public static void Initialize()
	{
		_rootObjects.Clear();
		_rootObjects = SceneManager.GetActiveScene().GetRootGameObjects().Where(r => r.gameObject != null).ToList();
		PostRaceEvents = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PostRaceEventUI>(true)).ToArray();
		MemberMeeting = _rootObjects.SelectMany(g => g.GetComponentsInChildren<MemberMeetingUI>(true)).First();
		Notes = _rootObjects.SelectMany(g => g.GetComponentsInChildren<NotesUI>(true)).First();
		PositionDisplay = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PositionDisplayUI>(true)).First();
		Tutorial = _rootObjects.SelectMany(g => g.GetComponentsInChildren<TutorialController>(true)).First();
		Settings = _rootObjects.SelectMany(g => g.GetComponentsInChildren<SettingsUI>(true)).Last();
		Recruitment = _rootObjects.SelectMany(g => g.GetComponentsInChildren<RecruitMemberUI>(true)).First();
		TeamSelection = _rootObjects.SelectMany(g => g.GetComponentsInChildren<TeamSelectionUI>(true)).First();
		RaceResult = _rootObjects.SelectMany(g => g.GetComponentsInChildren<RaceResultUI>(true)).First();
		CupResult = _rootObjects.SelectMany(g => g.GetComponentsInChildren<CupResultUI>(true)).First();
		Promotion = _rootObjects.SelectMany(g => g.GetComponentsInChildren<BoatPromotionUI>(true)).First();
		PreRace = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PreRaceConfirmUI>(true)).First();
		LearningPill = _rootObjects.SelectMany(g => g.GetComponentsInChildren<LearningPillUI>(true)).First();
		Hover = _rootObjects.SelectMany(g => g.GetComponentsInChildren<HoverPopUpUI>(true)).First();
		EventImpact = _rootObjects.SelectMany(g => g.GetComponentsInChildren<PostRaceEventImpactUI>(true)).First();

		SmallBlocker = _rootObjects.Single(g => g.name == "Canvas").transform.FindButton("Team Management/Pop-up Bounds/Blocker");
		Blocker = _rootObjects.Single(g => g.name == "Canvas").transform.FindButton("Team Management/Pop-up Bounds/Bigger Blocker");
	}

	public static CrewMemberUI[] CrewMemberUI
	{
		get { return _rootObjects.Where(r => r.gameObject != null).SelectMany(g => g.GetComponentsInChildren<CrewMemberUI>()).ToArray(); }
	}

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
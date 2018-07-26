using PlayGen.SUGAR.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.Loading;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controls switching between different game state panels
/// </summary>
public class UIStateManager : MonoBehaviour
{
	[Serializable]
	public class StatePair
	{
		public State Name;
		public GameObject GameObject;
	}

	[SerializeField]
	private List<StatePair> _states;
	private Dictionary<State, GameObject> _stateDict;
	private static bool _reload;

	/// <summary>
	/// Trigger SUGAR sign-in on first load
	/// </summary>
	private void Awake()
	{
		UIManagement.Initialize();
		AvatarDisplay.LoadSprites();
		_stateDict = _states.ToDictionary(s => s.Name, s => s.GameObject);
		GoToState(State.MainMenu);
		if (GameManagement.RageMode)
		{
			foreach (var obj in GameManagement.PlatformSettings.RageObjects)
			{
				var newObj = Instantiate(obj);
				newObj.name = obj.name;
			}
		}
		if (_reload)
		{
			Loading.Start();
			GameManagement.GameManager.LoadGameTask(GameManagement.GameSavePath, GameManagement.TeamName, success =>
			{
				if (success)
				{
					GoToState(State.TeamManagement);
					_reload = false;
				}
				Loading.Stop();
			});
		}
	}

	private void Update()
	{
		if (EventSystem.current.currentSelectedGameObject && !EventSystem.current.currentSelectedGameObject.GetComponent<InputField>())
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
#if UNITY_EDITOR
		//takes a screenshot whenever down arrow is pressed
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			ScreenCapture.CaptureScreenshot(DateTime.UtcNow.ToFileTimeUtc() + ".png");
		}
#endif
	}

	public void GoToState(State newState)
	{
		foreach (var state in _stateDict.Values)
		{
			state.Active(false);
		}
		_stateDict[newState].Active(true);
	}

	/// <summary>
	/// Reload the scene
	/// </summary>
	public void ReloadScene()
	{
		_reload = true;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// Reload the scene
	/// </summary>
	public void ResetScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// Trigger showing SUGAR achievements
	/// </summary>
	public void ShowAchievements()
	{
		SUGARManager.Achievement.DisplayList();
	}

	/// <summary>
	/// Trigger showing SUGAR leaderboards
	/// </summary>
	public void ShowLeaderboards()
	{
		SUGARManager.GameLeaderboard.DisplayList();
	}
}
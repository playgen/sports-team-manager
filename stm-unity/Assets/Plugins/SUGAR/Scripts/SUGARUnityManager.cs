using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using PlayGen.SUGAR.Client;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PlayGen.SUGAR.Unity
{
	[RequireComponent(typeof(AccountUnityClient))]
	[RequireComponent(typeof(AchievementUnityClient))]
	[RequireComponent(typeof(LeaderboardUnityClient))]
	[RequireComponent(typeof(LeaderboardListUnityClient))]
	[RequireComponent(typeof(ResponseHandler))]

	public class SUGARUnityManager : MonoBehaviour
	{
		[SerializeField]
		private string _baseAddress;
		[SerializeField]
		private string _gameToken;
		[SerializeField]
		private int _gameId;

		internal string baseAddress
		{
			get { return _baseAddress; }
		}
		internal string gameToken
		{
			get { return _gameToken; }
		}
		internal int gameId
		{
			set { _gameId = value; }
		}
		private Canvas _canvas;
		[SerializeField]
		private bool _useAchievements = true;
		[SerializeField]
		private bool _useLeaderboards = true;
		[SerializeField]
		private GameObject _uiBlocker;
		private readonly List<GameObject> _blockQueue = new List<GameObject>();

		public bool AnyActiveUI
		{
			get
			{
				return (SUGARManager.account && SUGARManager.account.IsActive) ||
						(SUGARManager.achievement && SUGARManager.achievement.IsActive) ||
						(SUGARManager.gameLeaderboard && SUGARManager.gameLeaderboard.IsActive) ||
						(SUGARManager.leaderboard && SUGARManager.leaderboard.IsActive);
			}
		}

		private void Awake()
		{
			if (SUGARManager.Register(this))
			{
				DontDestroyOnLoad(this);
			}
			else
			{
				Destroy(gameObject);
				return;
			}

			SUGARManager.unity = this;
			SUGARManager.GameId = _gameId;
			SUGARManager.account = GetComponent<AccountUnityClient>();
			SUGARManager.achievement = _useAchievements ? GetComponent<AchievementUnityClient>() : null;
			SUGARManager.leaderboard = _useLeaderboards ? GetComponent<LeaderboardUnityClient>() : null;
			SUGARManager.gameLeaderboard = _useLeaderboards ? GetComponent<LeaderboardListUnityClient>() : null;
			_canvas = GetComponentInChildren<Canvas>();
			GetComponent<AccountUnityClient>().CreateInterface(_canvas);

			if (!LoadConfig())
			{
				SetUpClient();
			}
		}

		private bool LoadConfig()
		{
			var path = ConfigPath;
			if (File.Exists(ConfigPath))
			{
				StartCoroutine(LoadOnlineConfig(path));
				return true;
			}
			return false;
		}

		private IEnumerator LoadOnlineConfig(string path)
		{
			path = "file:///" + path;
			var www = new WWW(path);
			yield return www;

			SUGARManager.config = JsonConvert.DeserializeObject<Config>(www.text);
			Debug.Log(SUGARManager.config.BaseUri);

			_baseAddress = SUGARManager.config.BaseUri;
			SetUpClient();
		}

		private void SetUpClient()
		{
			SUGARManager.Client = new SUGARClient(_baseAddress);
			if (_useLeaderboards)
			{
				GetComponent<LeaderboardListUnityClient>().CreateInterface(_canvas);
				GetComponent<LeaderboardUnityClient>().CreateInterface(_canvas);
			}
			if (_useAchievements)
			{
				GetComponent<AchievementUnityClient>().CreateInterface(_canvas);
			}
			if (_uiBlocker)
			{
				bool blockerInScene = _uiBlocker.scene == SceneManager.GetActiveScene();
				if (!blockerInScene)
				{
					var newBlocker = Instantiate(_uiBlocker, _canvas.transform, false);
					newBlocker.name = _uiBlocker.name;
					_uiBlocker = newBlocker;
				}
				_uiBlocker.gameObject.SetActive(false);
			}
		}

		private string ConfigPath
		{
			get
			{
				string path = Application.streamingAssetsPath + "/SUGAR.config.json";
				#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				//path = "file:///" + path;
				#endif
				return path;
			}
		}

		internal void EnableObject(GameObject activeObject)
		{
			if (_uiBlocker)
			{
				_uiBlocker.GetComponent<Button>().onClick.RemoveAllListeners();
				var objectToDisable = activeObject;
				_uiBlocker.GetComponent<Button>().onClick.AddListener(delegate { DisableObject(objectToDisable); });
				foreach (Transform child in _uiBlocker.transform)
				{
					if (activeObject != child.gameObject)
					{
						child.SetParent(_canvas.transform, false);
						_blockQueue.Add(child.gameObject);
					}
				}
				activeObject.transform.SetParent(_uiBlocker.transform, false);
				_uiBlocker.transform.SetAsLastSibling();
				_uiBlocker.SetActive(true);
			}
			activeObject.SetActive(true);
			activeObject.transform.SetAsLastSibling();
		}

		internal void DisableObject(GameObject activeObject)
		{
			if (_uiBlocker)
			{
				foreach (Transform child in _uiBlocker.transform)
				{
					child.SetParent(_canvas.transform, false);
				}
				activeObject.SetActive(false);
				if (_blockQueue.Count > 0)
				{
					EnableObject(_blockQueue[_blockQueue.Count - 1]);
					_blockQueue.RemoveAt(_blockQueue.Count - 1);
					return;
				}
				_uiBlocker.SetActive(false);
				return;
			}
			activeObject.SetActive(false);
		}

		internal void ButtonBestFit(GameObject interfaceObj)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)interfaceObj.transform);

			var buttonObj = interfaceObj.GetComponentsInChildren<Button>();
			int smallestFontSize = 0;
			foreach (var button in buttonObj)
			{
				var textObj = button.GetComponentsInChildren<Text>();
				foreach (var text in textObj)
				{
					text.resizeTextForBestFit = true;
					text.resizeTextMinSize = 1;
					text.resizeTextMaxSize = 100;
					text.cachedTextGenerator.Invalidate();
					text.cachedTextGenerator.Populate(text.text, text.GetGenerationSettings(text.rectTransform.rect.size));
					text.resizeTextForBestFit = false;
					var newSize = text.cachedTextGenerator.fontSizeUsedForBestFit;
					var newSizeRescale = text.rectTransform.rect.size.x / text.cachedTextGenerator.rectExtents.size.x;
					if (text.rectTransform.rect.size.y / text.cachedTextGenerator.rectExtents.size.y < newSizeRescale)
					{
						newSizeRescale = text.rectTransform.rect.size.y / text.cachedTextGenerator.rectExtents.size.y;
					}
					newSize = Mathf.FloorToInt(newSize * newSizeRescale);
					if (newSize < smallestFontSize || smallestFontSize == 0)
					{
						smallestFontSize = newSize;
					}
				}
			}
			foreach (var button in buttonObj)
			{
				var textObj = button.GetComponentsInChildren<Text>();
				foreach (var text in textObj)
				{
					text.fontSize = smallestFontSize;
				}
			}
		}
	}
}
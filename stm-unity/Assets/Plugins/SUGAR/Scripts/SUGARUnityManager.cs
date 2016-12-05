using System.Collections;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using PlayGen.SUGAR.Client;

namespace SUGAR.Unity
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

            SUGARManager.GameId = _gameId;
            SUGARManager.account = GetComponent<AccountUnityClient>();
            SUGARManager.achievement = _useAchievements ? GetComponent<AchievementUnityClient>() : null;
            SUGARManager.leaderboard = _useLeaderboards ? GetComponent<LeaderboardUnityClient>() : null;
            SUGARManager.gameLeaderboard = _useLeaderboards ? GetComponent<LeaderboardListUnityClient>() : null;
            _canvas = GetComponentInChildren<Canvas>();
            GetComponent<AccountUnityClient>().CreateInterface(_canvas);
            if (_useLeaderboards)
            {
                GetComponent<LeaderboardListUnityClient>().CreateInterface(_canvas);
                GetComponent<LeaderboardUnityClient>().CreateInterface(_canvas);
            }

            if (!LoadConfig())
		    {
                SetUpClient();
		    }
		}

		public bool LoadConfig()
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

        public void SetUpClient()
	    {
            SUGARManager.Client = new SUGARClient(_baseAddress);
            if (_useAchievements)
            {
                GetComponent<AchievementUnityClient>().CreateInterface(_canvas);
            }
        }

        private string ConfigPath
        {
            get
            {
                string path = Application.streamingAssetsPath + "/config.json";
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                //path = "file:///" + path;
                #endif
                return path;
            }
        }
    }
}
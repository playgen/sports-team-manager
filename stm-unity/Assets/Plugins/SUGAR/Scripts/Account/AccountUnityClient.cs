using System;
using PlayGen.SUGAR.Contracts.Shared;
using PlayGen.SUGAR.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SUGAR.Unity
{
	[DisallowMultipleComponent]
	public class AccountUnityClient : MonoBehaviour
	{
		[SerializeField] private LoginUserInterface _loginUserInterface;

		[SerializeField] private bool _allowAutoLogin;

		[SerializeField] private string _defaultSourceToken = "SUGAR";

		private CommandLineOptions _options;

		private Action<bool> _signInCallback;

		internal void CreateInterface(Canvas canvas)
		{
			bool inScene = _loginUserInterface.gameObject.scene == SceneManager.GetActiveScene();
			if (!inScene)
			{
				var newInterface = Instantiate(_loginUserInterface.gameObject, canvas.transform, false) as GameObject;
				newInterface.name = _loginUserInterface.name;
				_loginUserInterface = newInterface.GetComponent<LoginUserInterface>();
			}
			_loginUserInterface.gameObject.SetActive(false);
		}

		public void SignIn(Action<bool> success)
		{
			_signInCallback = success;
		#if UNITY_EDITOR
			_options = CommandLineUtility.ParseArgs(new [] { "-ujim" , "-sSPL", "-a"});
			Debug.Log(_options.UserId + " : " + _options.AuthenticationSource);
		#else
			_options = CommandLineUtility.ParseArgs(System.Environment.GetCommandLineArgs());
		#endif

			if (_options.AuthenticationSource == null)
			{
				_options.AuthenticationSource = _defaultSourceToken;
			}

			if (_allowAutoLogin && _options.AutoLogin)
			{
				LoginUser(_options.UserId, _options.AuthenticationSource, string.Empty);
			}
			else
			{
				if (_loginUserInterface != null)
				{
					_loginUserInterface.Login += LoginUserInterfaceOnLogin;
					_loginUserInterface.Show();
				}
			}
		}

		private void LoginUser(string user, string sourceToken, string pass)
		{
			var accountRequest = CreateAccountRequest(user, pass, sourceToken);
			SUGARManager.Client.Session.LoginAsync(SUGARManager.GameId, accountRequest,
			response =>
			{
				Debug.Log("SUCCESS");
				if (_loginUserInterface != null)
				{
					_loginUserInterface.SetStatus("Success! " + response.User.Id + ": " + response.User.Name);
					_loginUserInterface.Hide();
				}
				SUGARManager.CurrentUser = response.User;
				_signInCallback(true);
			},
			exception =>
			{
				Debug.LogError(exception);
				if (_loginUserInterface != null)
				{
					_loginUserInterface.SetStatus("Login Error: " + exception.Message);
				}
				_signInCallback(false);
			});
		}

		private AccountRequest CreateAccountRequest(string user, string pass, string source)
		{
			return new AccountRequest
			{
				Name = user,
				Password = pass,
				SourceToken = source,
			};
		}


		private void LoginUserInterfaceOnLogin(object sender, LoginEventArgs loginEventArgs)
		{
			LoginUser(loginEventArgs.Username, _defaultSourceToken, loginEventArgs.Password);
		}
	}
}

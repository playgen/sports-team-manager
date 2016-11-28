using System;
using PlayGen.SUGAR.Client;
using PlayGen.SUGAR.Contracts.Shared;
using PlayGen.SUGAR.Unity;
using UnityEngine;

namespace SUGAR.Unity
{
    public class AccountUnityClient : MonoBehaviour
    {
        [SerializeField] private LoginUserInterface _loginUserInterface;

        [SerializeField] private bool _allowAutoLogin;

        [SerializeField] private string _defaultSourceToken = "SUGAR";

        private CommandLineOptions _options;
        private AccountClient _accountClient;


        void Start()
        {
            _accountClient = SUGARManager.Client.Account;

        #if UNITY_EDITOR
            _options = CommandLineUtility.ParseArgs(new string[] { "-ujim" , "-sSPL", "-a"});
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
                LoginUser(_options.UserId, _options.AuthenticationSource);
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

        private void LoginUser(string user, string sourceToken, string pass = "")
        {
            var accountResponse = GetLoginAccountResponse(user, pass, sourceToken);
            if (accountResponse != null)
            {
                Debug.Log("SUCCESS");
                if (_loginUserInterface != null)
                {
                    _loginUserInterface.SetStatus("Success! " + accountResponse.User.Id + ": " + accountResponse.User.Name);
                    _loginUserInterface.Hide();
                }
                SUGARManager.CurrentUser = accountResponse.User;
            }
        }


        private AccountResponse GetLoginAccountResponse(string username, string password, string source)
        {

            var accountRequest = CreateAccountRequest(username, password, source);
            Debug.Log(accountRequest.Name + " : " + accountRequest.SourceToken);
            try
            {
                return _accountClient.Login(accountRequest);
            }
            catch (Exception ex)
            {
                if (_loginUserInterface != null)
                {
                    _loginUserInterface.SetStatus("Login Error: " + ex.Message);
                }
                Debug.Log(ex);
                return null;
            }
        }

        private AccountRequest CreateAccountRequest(string user, string pass, string source, bool autoLogin = false)
        {
            return new AccountRequest()
            {
                Name = user,
                Password = pass,
                SourceToken = source,
                AutoLogin = autoLogin
            };
        }


        private void LoginUserInterfaceOnLogin(object sender, LoginEventArgs loginEventArgs)
        {
            LoginUser(loginEventArgs.Username, _defaultSourceToken, loginEventArgs.Password);
        }
    }
}

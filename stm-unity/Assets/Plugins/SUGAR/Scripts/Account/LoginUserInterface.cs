using System;
using UnityEngine;
using UnityEngine.UI;

namespace SUGAR.Unity
{
	public class LoginUserInterface : MonoBehaviour
	{
		[SerializeField]
		private InputField _name;
		[SerializeField]
		private InputField _password;
		[SerializeField]
		private Button _loginButton;
		[SerializeField]
		private Button _registerButton;
		[SerializeField]
		private Button _closeButton;
		[SerializeField]
		private Text _statusText;

		public event EventHandler<LoginEventArgs> Login;

		public event EventHandler<LoginEventArgs> Register;

		private void Awake()
		{
			_loginButton.onClick.AddListener(InvokeLogin);
			_registerButton.onClick.AddListener(InvokeRegister);
			_closeButton.onClick.AddListener(Hide);
		}

		internal void Show()
		{
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
		}

		internal void Hide()
		{
			gameObject.SetActive(false);
		}

		internal void RegisterButtonDisplay(bool display)
		{
			_registerButton.gameObject.SetActive(display);
		}

		internal void SetStatus(string text)
		{
			_statusText.text = text;
		}

		private void InvokeLogin()
		{
			var args = new LoginEventArgs(_name.text, _password.text);
			Login(this, args);
		}

		private void InvokeRegister()
		{
			var args = new LoginEventArgs(_name.text, _password.text);
			Register(this, args);
		}
	}
}

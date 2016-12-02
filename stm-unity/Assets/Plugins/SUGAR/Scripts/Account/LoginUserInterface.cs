using System;
using UnityEngine;
using UnityEngine.UI;

namespace SUGAR.Unity
{
	public class LoginUserInterface : MonoBehaviour, ILoginUserInterface
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
		private Text _statusText;

		public event EventHandler<LoginEventArgs> Login;

		void Awake()
		{
			_loginButton.onClick.AddListener(InvokeLogin);
		}

		public virtual void Show()
		{
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
		}

		public virtual void Hide()
		{
			gameObject.SetActive(false);
		}

		public virtual void SetStatus(string text)
		{
			_statusText.text = text;
		}

		protected virtual void InvokeLogin()
		{
			var args = new LoginEventArgs(_name.text, _password.text);
			var handler = Login;
			if (handler != null) handler(this, args);
		}
	}
}

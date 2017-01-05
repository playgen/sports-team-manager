using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlayGen.SUGAR.Unity
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
			if (_registerButton)
			{
				_registerButton.onClick.AddListener(InvokeRegister);
			}
			if (_closeButton)
			{
				_closeButton.onClick.AddListener(Hide);
			}
		}

		private void OnEnable()
		{
			SUGARManager.Unity.ButtonBestFit(gameObject);
		}

		internal void Show()
		{
			SUGARManager.Unity.EnableObject(gameObject);
		}

		internal void Hide()
		{
			SUGARManager.Unity.DisableObject(gameObject);
			_name.text = "";
			_password.text = "";
			_statusText.text = "";
		}

		internal void RegisterButtonDisplay(bool display)
		{
			if (_registerButton)
			{
				_registerButton.gameObject.SetActive(display);
			}
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

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift) && EventSystem.current.currentSelectedGameObject != null)
			{
				var next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();

				if (next == null)
					return;

				var inputfield = next.GetComponent<InputField>();
				if (inputfield != null)
				{
					inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
				}
				EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
			}
			else if (Input.GetKeyDown(KeyCode.Tab) && EventSystem.current.currentSelectedGameObject != null)
			{
				var next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

				if (next == null)
					return;

				var inputfield = next.GetComponent<InputField>();
				if (inputfield != null)
				{
					inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
				}
				EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				InvokeLogin();
			}
		}
	}
}

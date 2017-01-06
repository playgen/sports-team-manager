using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {
	[SerializeField]
	private Image _musicToggle;
	[SerializeField]
	private Image _soundToggle;
	[SerializeField]
	private Sprite _onSprite;
	[SerializeField]
	private Sprite _offSprite;
	[SerializeField]
	private Dropdown _languageDropdown;

	private void OnEnable()
	{
		Setup();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			transform.parent.gameObject.SetActive(false);
		}
	}

	private void Setup()
	{
		_musicToggle.sprite = UIStateManager.MusicOn ? _onSprite : _offSprite;
		_soundToggle.sprite = UIStateManager.SoundOn ? _onSprite : _offSprite;
		_languageDropdown.ClearOptions();
		var languageNames = Localization.AvailableLanguages();
		_languageDropdown.AddOptions(languageNames);
		var selectedIndex = languageNames.IndexOf(Localization.SelectedLanguage.ToString());
		if (selectedIndex == -1)
		{
			var nullList = new List<string> { string.Empty };
			_languageDropdown.AddOptions(nullList);
			_languageDropdown.value = languageNames.Count;
			_languageDropdown.options.RemoveAt(languageNames.Count);
		}
		else
		{
			_languageDropdown.value = selectedIndex;
		}
	}

	public void ToggleMusic()
	{
		UIStateManager.MusicOn = !UIStateManager.MusicOn;
		PlayerPrefs.SetInt("Music", UIStateManager.MusicOn ? 1 : 0);
		Setup();
	}

	public void ToggleSound()
	{
		UIStateManager.SoundOn = !UIStateManager.SoundOn;
		PlayerPrefs.SetInt("Sound", UIStateManager.SoundOn ? 1 : 0);
		Setup();
	}

	public void ChangeLanguage()
	{
		Localization.UpdateLanguage(_languageDropdown.value);
		gameObject.SetActive(false);
		gameObject.SetActive(true);
	}
}

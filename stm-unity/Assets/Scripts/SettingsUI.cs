using System;
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

	private void Setup()
	{
		_musicToggle.sprite = UIStateManager.MusicOn ? _onSprite : _offSprite;
		_soundToggle.sprite = UIStateManager.SoundOn ? _onSprite : _offSprite;
		_languageDropdown.ClearOptions();
		var languageNames = Enum.GetNames(typeof(Language)).ToList();
		var translatedLanguageNames = languageNames.Select(lang => Localization.Get(lang)).ToList();
		_languageDropdown.AddOptions(translatedLanguageNames);
		_languageDropdown.value = (int)Localization.SelectedLanguage - 1;
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
		Localization.UpdateLanguage((Language)(_languageDropdown.value + 1));
		gameObject.SetActive(false);
		gameObject.SetActive(true);
	}
}

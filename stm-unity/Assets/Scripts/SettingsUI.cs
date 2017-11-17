using PlayGen.Unity.Utilities.BestFit;
using PlayGen.Unity.Utilities.Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  Contains all logic relating to displaying the settings pop-up
/// </summary>
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
		BestFit.ResolutionChange += DoBestFit;
	}

	private void OnDisable()
	{
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Set up to match the player's current settings
	/// </summary>
	private void Setup()
	{
		_musicToggle.sprite = UIStateManager.MusicOn ? _onSprite : _offSprite;
		_soundToggle.sprite = UIStateManager.SoundOn ? _onSprite : _offSprite;
		_languageDropdown.ClearOptions();
		var languages = Localization.Languages.Select(l => string.IsNullOrEmpty(l.Parent.Name) ? l.EnglishName : l.Parent.EnglishName).ToList();
		_languageDropdown.GetComponent<DropdownLocalization>().SetOptions(languages);
		var selectedIndex = Localization.Languages.IndexOf(Localization.SelectedLanguage);
		if (selectedIndex == -1)
		{
			var nullList = new List<string> { string.Empty };
			_languageDropdown.AddOptions(nullList);
			_languageDropdown.value = languages.Count;
			_languageDropdown.options.RemoveAt(languages.Count);
		}
		else
		{
			_languageDropdown.value = selectedIndex;
		}
		DoBestFit();
	}

	/// <summary>
	/// Toggle music on/off
	/// </summary>
	public void ToggleMusic()
	{
		UIStateManager.MusicOn = !UIStateManager.MusicOn;
		PlayerPrefs.SetInt("Music", UIStateManager.MusicOn ? 1 : 0);
		Setup();
	}

	/// <summary>
	/// Toggle sound on/off
	/// </summary>
	public void ToggleSound()
	{
		UIStateManager.SoundOn = !UIStateManager.SoundOn;
		PlayerPrefs.SetInt("Sound", UIStateManager.SoundOn ? 1 : 0);
		Setup();
	}

	/// <summary>
	/// Change the selected language
	/// </summary>
	public void ChangeLanguage()
	{
		Localization.UpdateLanguage(Localization.Languages[_languageDropdown.value]);
		gameObject.Active(false);
		gameObject.Active(true);
	}

	private void DoBestFit()
	{
		gameObject.GetComponentsInChildren<Text>().Where(t => t.transform.parent == transform).BestFit();
		_languageDropdown.GetComponentsInChildren<Text>().Where(t => !t.name.Contains("Checkmark")).BestFit();
	}
}
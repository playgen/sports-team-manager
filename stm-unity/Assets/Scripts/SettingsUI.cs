using PlayGen.Unity.Utilities.Text;
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
		_languageDropdown.BestFit();
	}
}
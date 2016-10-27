﻿using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.UI;

/// <summary>
/// Controls the UI displayed at the top of the screen
/// </summary>
public class ScreenSideUI : MonoBehaviour {
	private GameManager _gameManager;
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private GameObject _selected;

	/// <summary>
	/// Set the information displayed at the top of the screen
	/// </summary>
	private void OnEnable()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		_nameText.text = _gameManager.Team.Name.ToUpper();
	}

	/// <summary>
	/// Change the position of the selected object to match the current UI screen
	/// </summary>
	public void ChangeSelected(int position)
	{
		((RectTransform)_selected.transform).anchorMax = new Vector2(0.25f + (0.15f * position), 1);
		((RectTransform)_selected.transform).anchorMin = new Vector2(0.1f + (0.15f * position), 0);
		((RectTransform)_selected.transform).anchoredPosition = Vector2.zero;
	}
}

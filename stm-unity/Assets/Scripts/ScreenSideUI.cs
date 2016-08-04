using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using UnityEngine.UI;

public class ScreenSideUI : MonoBehaviour {

	private GameManager _gameManager;
	[SerializeField]
	private Text _nameText;
	[SerializeField]
	private Text _scoreText;
	[SerializeField]
	private GameObject _selected;

	void OnEnable()
	{
		_gameManager = (FindObjectOfType(typeof(GameManagerObject)) as GameManagerObject).GameManager;
		_nameText.text = _gameManager.Boat.Name;
		_scoreText.text = "00";
	}

	public void ChangeSelected(int position)
	{
		_selected.GetComponent<RectTransform>().anchorMax = new Vector2(0.4f + (0.15f * position), 0.15f);
		_selected.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f + (0.15f * position), 0);
		_selected.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
	}
}

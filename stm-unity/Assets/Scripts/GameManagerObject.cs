using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;

public class GameManagerObject : MonoBehaviour
{
	public GameManager GameManager;

	void Awake()
	{
		GameManager = new GameManager();
		DontDestroyOnLoad(gameObject);
	}
}

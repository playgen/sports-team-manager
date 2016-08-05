using UnityEngine;
using System.Collections;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Container for the GameManager in the Simulation
/// </summary>
public class GameManagerObject : MonoBehaviour
{
	public GameManager GameManager;

	void Awake()
	{
		GameManager = new GameManager();
		DontDestroyOnLoad(gameObject);
	}
}

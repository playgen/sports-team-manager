using UnityEngine;
using PlayGen.RAGE.SportsTeamManager.Simulation;

/// <summary>
/// Container for the GameManager in the Simulation
/// </summary>
public class GameManagerObject : MonoBehaviour
{
	public GameManager GameManager;

	private void Awake()
	{
		if (FindObjectsOfType<GameManagerObject>().Length > 1)
		{
			Destroy(gameObject);
			return;
		}
		GameManager = new GameManager();
		DontDestroyOnLoad(gameObject);
	}
#if UNITY_EDITOR
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			Application.CaptureScreenshot(System.DateTime.UtcNow.ToFileTimeUtc() + ".png");
		}
	}
#endif
}

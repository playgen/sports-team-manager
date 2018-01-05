using System.Linq;
using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SetPlatformPositioning : MonoBehaviour {

	[MenuItem("Tools/Set Positioning/Standalone")]
	public static void SetStandalone()
	{
		SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PlatformPositioning>(true)).ToList().ForEach(p => p.SetPosition(true));
	}

	[MenuItem("Tools/Set Positioning/Mobile")]
	public static void SetMobile()
	{
		SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PlatformPositioning>(true)).ToList().ForEach(p => p.SetPosition(true, true));
	}
}

using UnityEngine;
using UnityEditor;

public class BuildExe : MonoBehaviour
{

	[MenuItem("Tools/PC Build")]
	static void Build()
	{
		string[] scenes = { "Assets/Scenes/Menu.unity" };
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
		PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
		BuildPipeline.BuildPlayer(scenes, @"Builds/Sports Team Manager/Sports Team Manager.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
	}
}
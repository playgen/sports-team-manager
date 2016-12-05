using UnityEngine;
using UnityEditor;

public class BuildExe : MonoBehaviour
{

	[MenuItem("Tools/PC Build")]
	static void Build()
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, @"Build/Sports Team Manager/SportsTeamManager.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
	}
}
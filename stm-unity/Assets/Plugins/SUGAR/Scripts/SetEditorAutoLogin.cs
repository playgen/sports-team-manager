#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace PlayGen.SUGAR.Unity
{
	public static class SetEditorAutoLogin
	{
		[MenuItem("Tools/SUGAR/Set Auto Log-in Values")]
		public static void SeedAchivements()
		{
			AutoLogIn window = ScriptableObject.CreateInstance<AutoLogIn>();
			window.titleContent.text = "Set Auto Log-in Values";
			window.Show();
		}
	}

	public class AutoLogIn : EditorWindow
	{
		string username;
		string password;
		string source;
		bool auto;
		bool passRequired;

		void OnEnable()
		{
			passRequired = !EditorPrefs.HasKey("AutoLoginSourcePassRequired") || EditorPrefs.GetBool("AutoLoginSourcePassRequired");
			auto = !EditorPrefs.HasKey("AutoLoginAuto") || EditorPrefs.GetBool("AutoLoginAuto");
			username = EditorPrefs.HasKey("AutoLoginUsername") ? EditorPrefs.GetString("AutoLoginUsername") : string.Empty;
			password = EditorPrefs.HasKey("AutoLoginUsername") ? EditorPrefs.GetString("AutoLoginPassword") : string.Empty;
			source = EditorPrefs.HasKey("AutoLoginUsername") ? EditorPrefs.GetString("AutoLoginSourceToken") : string.Empty;
		}

		void OnGUI()
		{
			username = EditorGUILayout.TextField("Username", username, EditorStyles.textField);
			passRequired = EditorGUILayout.Toggle("Password Required", passRequired, EditorStyles.toggle);
			if (passRequired) {
				password = EditorGUILayout.PasswordField("Password", password, EditorStyles.textField);
			}
			source = EditorGUILayout.TextField("Account Source", source, EditorStyles.textField);
			auto = EditorGUILayout.Toggle("Auto Log-in", auto, EditorStyles.toggle);
			if (GUILayout.Button("Save"))
			{
				EditorPrefs.SetBool("AutoLoginSourcePassRequired", passRequired);
				EditorPrefs.SetBool("AutoLoginAuto", auto);
				EditorPrefs.SetString("AutoLoginUsername", username);
				EditorPrefs.SetString("AutoLoginPassword", password);
				EditorPrefs.SetString("AutoLoginSourceToken", source);
				Close();
			}
		}
	}
}
#endif

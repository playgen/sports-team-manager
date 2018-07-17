using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using AssetManagerPackage;
using EmotionalAppraisal;
using EmotionalDecisionMaking;

using IntegratedAuthoringTool;

using Newtonsoft.Json;
using RolePlayCharacter;
using SocialImportance;

[assembly: InternalsVisibleTo("PlayGen.RAGE.SportsTeamManager.UnitTest")]
namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to access values related to functionality
	/// </summary>
	internal class ConfigStore
	{
		internal static Dictionary<ConfigKey, float> ConfigValues { get; set; }
		internal static Dictionary<string, List<Position>> BoatTypes { get; set; }
		internal static GameConfig GameConfig { get; set; }
		internal static NameConfig NameConfig { get; set; }
		internal static IntegratedAuthoringToolAsset IntegratedAuthoringTool { get; set; }
		internal static IntegratedAuthoringToolAsset HelpIntegratedAuthoringTool { get; set; }
		internal static RolePlayCharacterAsset RolePlayCharacter { get; set; }
		internal static EmotionalAppraisalAsset EmotionalAppraisal { get; set; }
		internal static EmotionalDecisionMakingAsset EmotionalDecisionMaking { get; set; }
		internal static SocialImportanceAsset SocialImportance { get; set; }
		internal static Platform Platform { get; set; }

		internal ConfigStore(Platform platform = Platform.Windows)
		{
			Platform = platform;
			ConfigValues = new Dictionary<ConfigKey, float>();
			var configText = Templates.ResourceManager.GetString("config");
			ConfigValues = JsonConvert.DeserializeObject<Dictionary<ConfigKey, float>>(configText);
			foreach (var key in (ConfigKey[])Enum.GetValues(typeof(ConfigKey)))
			{
				if (!ConfigValues.ContainsKey(key))
				{
					throw new Exception("Config key " + key + " not included in config!");
				}
			}
			BoatTypes = new Dictionary<string, List<Position>>();
			var boatText = Templates.ResourceManager.GetString("boat_config");
			BoatTypes = JsonConvert.DeserializeObject<Dictionary<string, List<Position>>>(boatText);
			GameConfig = new GameConfig().GetConfig();
			NameConfig = new NameConfig().GetConfig();
			Avatar.Config = new AvatarGeneratorConfig().GetConfig();

			AssetManager.Instance.Bridge = new TemplateBridge();
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile("template_rpc");
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile("template_ea");
			EmotionalDecisionMaking = EmotionalDecisionMakingAsset.LoadFromFile("template_edm");
			SocialImportance = SocialImportanceAsset.LoadFromFile("template_si");
			IntegratedAuthoringTool = IntegratedAuthoringToolAsset.LoadFromFile("template_iat");

			switch (Platform)
			{
				case Platform.Android:
					AssetManager.Instance.Bridge = new AndroidBaseBridge();
					break;
				case Platform.iOS:
					AssetManager.Instance.Bridge = new IOSBaseBridge();
					break;
				case Platform.Windows:
					AssetManager.Instance.Bridge = new BaseBridge();
					break;
			}
		}
	}
}

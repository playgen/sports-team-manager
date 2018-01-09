using System;
using System.Collections.Generic;
using AssetManagerPackage;
using EmotionalAppraisal;
using EmotionalDecisionMaking;

using IntegratedAuthoringTool;

using Newtonsoft.Json;
using RolePlayCharacter;
using SocialImportance;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to access values related to functionality
	/// </summary>
	public class ConfigStore
	{
		internal Dictionary<ConfigKeys, float> ConfigValues { get; set; }
		internal Dictionary<string, List<Position>> BoatTypes { get; set; }
		internal GameConfig GameConfig { get; set; }
		internal NameConfig NameConfig { get; set; }
		internal static IntegratedAuthoringToolAsset IntegratedAuthoringTool { get; set; }
		internal static IntegratedAuthoringToolAsset HelpIntegratedAuthoringTool { get; set; }
		internal static RolePlayCharacterAsset RolePlayCharacter { get; set; }
		internal static EmotionalAppraisalAsset EmotionalAppraisal { get; set; }
		internal static EmotionalDecisionMakingAsset EmotionalDecisionMaking { get; set; }
		internal static SocialImportanceAsset SocialImportance { get; set; }
		internal static Platform Platform { get; set; }

		public ConfigStore(Platform platform = Platform.Null)
		{
			if (platform != Platform.Null)
			{
				Platform = platform;
			}
			if (Platform == Platform.Null)
			{
				Platform = Platform.Windows;
			}
			ConfigValues = new Dictionary<ConfigKeys, float>();
			var configText = Templates.ResourceManager.GetString("config");
			ConfigValues = JsonConvert.DeserializeObject<Dictionary<ConfigKeys, float>>(configText);
			foreach (var key in (ConfigKeys[])Enum.GetValues(typeof(ConfigKeys)))
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
			LoadAssets();
		}

		internal void LoadAssets()
		{
			AssetManager.Instance.Bridge = new TemplateBridge();
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile("template_rpc");
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(RolePlayCharacter.EmotionalAppraisalAssetSource);
			EmotionalDecisionMaking = EmotionalDecisionMakingAsset.LoadFromFile(RolePlayCharacter.EmotionalDecisionMakingSource);
			SocialImportance = SocialImportanceAsset.LoadFromFile(RolePlayCharacter.SocialImportanceAssetSource);
			IntegratedAuthoringTool = IntegratedAuthoringToolAsset.LoadFromFile("template_iat");
			HelpIntegratedAuthoringTool = IntegratedAuthoringToolAsset.LoadFromFile("help_dialogue");
			switch (Platform)
			{
				case Platform.Android:
					AssetManager.Instance.Bridge = new AndroidBaseBridge();
					break;
				case Platform.iOS:
					AssetManager.Instance.Bridge = new AndroidBaseBridge();
					break;
				case Platform.Windows:
					AssetManager.Instance.Bridge = new BaseBridge();
					break;
			}
		}
	}
}

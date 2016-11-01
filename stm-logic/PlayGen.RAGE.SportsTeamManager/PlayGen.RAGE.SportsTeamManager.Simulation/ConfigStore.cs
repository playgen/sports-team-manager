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
		public Dictionary<ConfigKeys, float> ConfigValues { get; set; }
		public Dictionary<string, List<Position>> BoatTypes { get; set; }
		public GameConfig GameConfig { get; set; }
		public NameConfig NameConfig { get; set; }
		public static IntegratedAuthoringToolAsset IntegratedAuthoringTool { get; set; }
		public static IntegratedAuthoringToolAsset HelpIntegratedAuthoringTool { get; set; }
		public static RolePlayCharacterAsset RolePlayCharacter { get; set; }
		public static EmotionalAppraisalAsset EmotionalAppraisal { get; set; }
		public static EmotionalDecisionMakingAsset EmotionalDecisionMaking { get; set; }
		public static SocialImportanceAsset SocialImportance { get; set; }

		public ConfigStore()
		{
			ConfigValues = new Dictionary<ConfigKeys, float>();
			var configText = Templates.ResourceManager.GetString("config");
			ConfigValues = JsonConvert.DeserializeObject<Dictionary<ConfigKeys, float>>(configText);
			foreach (var key in Enum.GetValues(typeof(ConfigKeys)) as ConfigKeys[])
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
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(RolePlayCharacter.EmotionalAppraisalAssetSource);
			EmotionalDecisionMaking = EmotionalDecisionMakingAsset.LoadFromFile(RolePlayCharacter.EmotionalDecisionMakingSource);
			SocialImportance = SocialImportanceAsset.LoadFromFile(RolePlayCharacter.SocialImportanceAssetSource);
			IntegratedAuthoringTool = IntegratedAuthoringToolAsset.LoadFromFile("template_iat");
			HelpIntegratedAuthoringTool = IntegratedAuthoringToolAsset.LoadFromFile("help_dialogue");
			AssetManager.Instance.Bridge = new BaseBridge();
		}

		public void ReloadAssets()
		{
			AssetManager.Instance.Bridge = new TemplateBridge();
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile("template_rpc");
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(RolePlayCharacter.EmotionalAppraisalAssetSource);
			EmotionalDecisionMaking = EmotionalDecisionMakingAsset.LoadFromFile(RolePlayCharacter.EmotionalDecisionMakingSource);
			SocialImportance = SocialImportanceAsset.LoadFromFile(RolePlayCharacter.SocialImportanceAssetSource);
			IntegratedAuthoringTool = IntegratedAuthoringToolAsset.LoadFromFile("template_iat");
			HelpIntegratedAuthoringTool = IntegratedAuthoringToolAsset.LoadFromFile("help_dialogue");
			AssetManager.Instance.Bridge = new BaseBridge();
		}
	}
}

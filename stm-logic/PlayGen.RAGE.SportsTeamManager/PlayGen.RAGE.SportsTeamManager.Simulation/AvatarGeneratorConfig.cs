using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Container for shared Avatar values
	/// </summary>
	internal class AvatarGeneratorConfig
	{
		internal bool RandomHairColor { get; set; }
		internal int HairTypesCount { get; set; }
		internal int OutfitTypesCount { get; set; }

		internal Color LightSkinColor { get; set; }
		internal Color MediumSkinColor { get; set; }
		internal Color DarkSkinColor { get; set; }
		internal Color BlondeHairColor { get; set; }
		internal Color BrownHairColor { get; set; }
		internal Color BlackHairColor { get; set; }
		internal Color GingerHairColor { get; set; }
		internal Color BlueEyeColor { get; set; }
		internal Color BrownEyeColor { get; set; }
		internal Color GreenEyeColor { get; set; }

		//get and return values for avatar configs
		internal AvatarGeneratorConfig GetConfig()
		{
			var configText = Templates.ResourceManager.GetString("avatar_config");
			var contractResolver = new PrivatePropertyResolver();
			var settings = new JsonSerializerSettings
			{
				ContractResolver = contractResolver
			};
			var config = JsonConvert.DeserializeObject<AvatarGeneratorConfig>(configText, settings);
			return config;
		}
	}
}

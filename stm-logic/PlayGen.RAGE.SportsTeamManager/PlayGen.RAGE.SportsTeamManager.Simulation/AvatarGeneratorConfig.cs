using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Container for shared Avatar values
	/// </summary>
	internal class AvatarGeneratorConfig
	{
		public bool RandomHairColor { get; set; }
		public int HairTypesCount { get; set; }
		public int OutfitTypesCount { get; set; }

		public Color LightSkinColor { get; private set; }
		public Color MediumSkinColor { get; private set; }
		public Color DarkSkinColor { get; private set; }
		public Color BlondeHairColor { get; private set; }
		public Color BrownHairColor { get; private set; }
		public Color BlackHairColor { get; private set; }
		public Color GingerHairColor { get; private set; }
		public Color BlueEyeColor { get; private set; }
		public Color BrownEyeColor { get; private set; }
		public Color GreenEyeColor { get; private set; }

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

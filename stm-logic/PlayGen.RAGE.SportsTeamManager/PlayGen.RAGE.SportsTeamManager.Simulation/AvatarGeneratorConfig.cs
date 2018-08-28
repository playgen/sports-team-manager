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

		public Color LightSkinColor { get; set; }
		public Color MediumSkinColor { get; set; }
		public Color DarkSkinColor { get; set; }
		public Color BlondeHairColor { get; set; }
		public Color BrownHairColor { get; set; }
		public Color BlackHairColor { get; set; }
		public Color GingerHairColor { get; set; }
		public Color BlueEyeColor { get; set; }
		public Color BrownEyeColor { get; set; }
		public Color GreenEyeColor { get; set; }

		//
		/// <summary>
		/// Get and return values for avatar configs
		/// </summary>
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

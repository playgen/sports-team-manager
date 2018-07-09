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
		public byte[] LightSkinColorValues { get; internal set; }
		public byte[] MediumSkinColorValues { get; internal set; }
		public byte[] DarkSkinColorValues { get; internal set; }
		public byte[] BlondeHairColorValues { get; internal set; }
		public byte[] BrownHairColorValues { get; internal set; }
		public byte[] BlackHairColorValues { get; internal set; }
		public byte[] GingerHairColorValues { get; internal set; }
		public byte[] BlueEyeColorValues { get; internal set; }
		public byte[] BrownEyeColorValues { get; internal set; }
		public byte[] GreenEyeColorValues { get; internal set; }

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
			config.LightSkinColor = new Color(config.LightSkinColorValues[0], config.LightSkinColorValues[1], config.LightSkinColorValues[2]);
			config.MediumSkinColor = new Color(config.MediumSkinColorValues[0], config.MediumSkinColorValues[1], config.MediumSkinColorValues[2]);
			config.DarkSkinColor = new Color(config.DarkSkinColorValues[0], config.DarkSkinColorValues[1], config.DarkSkinColorValues[2]);
			config.BlondeHairColor = new Color(config.BlondeHairColorValues[0], config.BlondeHairColorValues[1], config.BlondeHairColorValues[2]);
			config.BrownHairColor = new Color(config.BrownHairColorValues[0], config.BrownHairColorValues[1], config.BrownHairColorValues[2]);
			config.BlackHairColor = new Color(config.BlackHairColorValues[0], config.BlackHairColorValues[1], config.BlackHairColorValues[2]);
			config.GingerHairColor = new Color(config.GingerHairColorValues[0], config.GingerHairColorValues[1], config.GingerHairColorValues[2]);
			config.BlueEyeColor = new Color(config.BlueEyeColorValues[0], config.BlueEyeColorValues[1], config.BlueEyeColorValues[2]);
			config.BrownEyeColor = new Color(config.BrownEyeColorValues[0], config.BrownEyeColorValues[1], config.BrownEyeColorValues[2]);
			config.GreenEyeColor = new Color(config.GreenEyeColorValues[0], config.GreenEyeColorValues[1], config.GreenEyeColorValues[2]);
			return config;
		}
	}
}

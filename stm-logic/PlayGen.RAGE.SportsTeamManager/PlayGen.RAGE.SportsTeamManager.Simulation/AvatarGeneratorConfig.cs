using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Container for shared Avatar values
	/// </summary>
	public class AvatarGeneratorConfig
	{
		public bool RandomHairColor { get; set; }
		public int HairTypesCount { get; set; }
		public int OutfitTypesCount { get; set; }
		public byte[] LightSkinColorValues { get; set; }
		public byte[] MediumSkinColorValues { get; set; }
		public byte[] DarkSkinColorValues { get; set; }
		public byte[] BlondeHairColorValues { get; set; }
		public byte[] BrownHairColorValues { get; set; }
		public byte[] BlackHairColorValues { get; set; }
		public byte[] GingerHairColorValues { get; set; }

		public Color LightSkinColor { get; private set; }
		public Color MediumSkinColor { get; private set; }
		public Color DarkSkinColor { get; private set; }
		public Color BlondeHairColor { get; private set; }
		public Color BrownHairColor { get; private set; }
		public Color BlackHairColor { get; private set; }
		public Color GingerHairColor { get; private set; }

		//get and return values for avatar configs
		public AvatarGeneratorConfig GetConfig()
		{
			var configText = Templates.ResourceManager.GetString("avatar_config");
			var config = JsonConvert.DeserializeObject<AvatarGeneratorConfig>(configText);
			config.LightSkinColor = new Color(config.LightSkinColorValues[0], config.LightSkinColorValues[1], config.LightSkinColorValues[2], 255);
			config.MediumSkinColor = new Color(config.MediumSkinColorValues[0], config.MediumSkinColorValues[1], config.MediumSkinColorValues[2], 255);
			config.DarkSkinColor = new Color(config.DarkSkinColorValues[0], config.DarkSkinColorValues[1], config.DarkSkinColorValues[2], 255);
			config.BlondeHairColor = new Color(config.BlondeHairColorValues[0], config.BlondeHairColorValues[1], config.BlondeHairColorValues[2], 255);
			config.BrownHairColor = new Color(config.BrownHairColorValues[0], config.BrownHairColorValues[1], config.BrownHairColorValues[2], 255);
			config.BlackHairColor = new Color(config.BlackHairColorValues[0], config.BlackHairColorValues[1], config.BlackHairColorValues[2], 255);
			config.GingerHairColor = new Color(config.GingerHairColorValues[0], config.GingerHairColorValues[1], config.GingerHairColorValues[2], 255);
			return config;
		}
	}
}

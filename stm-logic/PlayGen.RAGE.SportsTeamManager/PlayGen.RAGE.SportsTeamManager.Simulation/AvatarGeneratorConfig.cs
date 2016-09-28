using System.Drawing;
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
		public int[] LightSkinColorValues { get; set; }
		public int[] MediumSkinColorValues { get; set; }
		public int[] DarkSkinColorValues { get; set; }
		public int[] BlondeHairColorValues { get; set; }
		public int[] BrownHairColorValues { get; set; }
		public int[] BlackHairColorValues { get; set; }
		public int[] GingerHairColorValues { get; set; }

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
			string configText = Templates.ResourceManager.GetString("avatar_config");
			var config = JsonConvert.DeserializeObject<AvatarGeneratorConfig>(configText);
			config.LightSkinColor = Color.FromArgb(255, config.LightSkinColorValues[0], config.LightSkinColorValues[1], config.LightSkinColorValues[2]);
			config.MediumSkinColor = Color.FromArgb(255, config.MediumSkinColorValues[0], config.MediumSkinColorValues[1], config.MediumSkinColorValues[2]);
			config.DarkSkinColor = Color.FromArgb(255, config.DarkSkinColorValues[0], config.DarkSkinColorValues[1], config.DarkSkinColorValues[2]);
			config.BlondeHairColor = Color.FromArgb(255, config.BlondeHairColorValues[0], config.BlondeHairColorValues[1], config.BlondeHairColorValues[2]);
			config.BrownHairColor = Color.FromArgb(255, config.BrownHairColorValues[0], config.BrownHairColorValues[1], config.BrownHairColorValues[2]);
			config.BlackHairColor = Color.FromArgb(255, config.BlackHairColorValues[0], config.BlackHairColorValues[1], config.BlackHairColorValues[2]);
			config.GingerHairColor = Color.FromArgb(255, config.GingerHairColorValues[0], config.GingerHairColorValues[1], config.GingerHairColorValues[2]);
			return config;
		}
	}
}

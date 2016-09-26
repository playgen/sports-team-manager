using System.Drawing;
using Newtonsoft.Json;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
    /// <summary>
    /// Container for shared Avatar values
    /// </summary>
    public class AvatarGeneratorConfig
	{
		public bool RandomHairColor;
		public int HairTypesCount;
		public int OutfitTypesCount;

		public Color LightSkinColor;
		public Color MediumSkinColor;
		public Color DarkSkinColor;
		public Color BlondeHairColor;
		public Color BrownHairColor;
		public Color BlackHairColor;
		public Color GingerHairColor;

		public int[] LightSkinColorValues;
		public int[] MediumSkinColorValues;
		public int[] DarkSkinColorValues;
		public int[] BlondeHairColorValues;
		public int[] BrownHairColorValues;
		public int[] BlackHairColorValues;
		public int[] GingerHairColorValues;

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

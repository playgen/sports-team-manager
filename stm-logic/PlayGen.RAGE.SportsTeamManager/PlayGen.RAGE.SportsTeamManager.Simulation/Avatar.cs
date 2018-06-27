using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Avatar class containing names of sprites that make up the avatar
	/// </summary>
	public class Avatar
	{
		internal static AvatarGeneratorConfig Config;

		public string BodyType { get; private set; }
		public string OutfitBaseType { get; }
		public string OutfitHighlightType { get; }
		public string OutfitShadowType { get; }
		public string HairType { get; private set; }
		public string EyeType { get; private set; }
		public string EyebrowType { get; private set; }
		public string NoseType { get; private set; }
		public string MouthType { get; private set; }
		public string TeethType { get; private set; }
		public float Weight { get; private set; }
		public float Height { get; private set; }

		public bool IsMale => _gender == "M";

		public CrewMemberSkill BestSkill { get; private set; }

		public bool CustomOutfitColor { get; }

		public Color SkinColor { get; private set; }
		public string MouthColor { get; private set; }
		public Color HairColor { get; private set; }
		public Color EyeColor { get; private set; }
		public Color PrimaryOutfitColor { get; set; }
		public Color SecondaryOutfitColor { get; set; }

		private readonly string _gender;

		internal Avatar (CrewMember crewMember, bool isActive = true, bool canLoad = false)
		{
			//set outfit type
			var outfit = !isActive ? "01" : "0" + ((StaticRandom.Int(0, 100) % 2) + 2);
			_gender = crewMember.Gender;
			var bodyType = GetBodyType(BestSkill);
			CustomOutfitColor = outfit != "01";
			//recreate pre-existing avatar if one already exists
			if (canLoad)
			{
				LoadAvatar(crewMember);
			}
			//otherwise, create new avatar
			else
			{
				CreateAvatar(crewMember, _gender);
			}
			//set outfit according to type, best skill and gender
			OutfitBaseType = $"Outfit{_gender}_Base_{bodyType}_{outfit}";
			OutfitHighlightType = $"Outfit{_gender}_Highlight_{bodyType}_{outfit}";
			OutfitShadowType = $"Outfit{_gender}_Shadow_{bodyType}_{outfit}";
		}

		private void CreateAvatar(CrewMember crewMember, string gender)
		{
			//Get Best Skill
			BestSkill = GetBestSkill(crewMember);

			//Set Skin Color
			SkinColor = GetRandomSkinColor();

			//Set Hair Color
			HairColor = Config.RandomHairColor ? GetRandomHairColor() : GetHairColorForSkin(SkinColor);

			//Set Primary Color
			PrimaryOutfitColor = CustomOutfitColor ? StaticRandom.Color() : new Color(255, 255, 255, 255);

			//Set Secondary Color
			SecondaryOutfitColor = CustomOutfitColor ? StaticRandom.Color() : new Color(0, 0, 0, 0);

			//Set Body Type
			BodyType = $"Body{gender}_{GetBodyType(BestSkill)}";

			//Set Hair Type
			HairType = $"Hair{StaticRandom.Int(1, Config.HairTypesCount + 1):00}{gender}";

			//Set Eye Type
			EyeType = $"Eye{gender}_{BestSkill}";

			//Set Eye Color
			EyeColor = GetRandomEyeColor();

			//Set Face type
			EyebrowType = $"Face{gender}_{BestSkill}_Eyebrows";
			NoseType = $"Face{gender}_{BestSkill}_Nose";

			//Specify the teeth for male avatars
			if (gender == "M")
			{
				TeethType = $"Face{gender}_{BestSkill}_Teeth";
			}

			//Set Mouth Type
			MouthType = $"Face{gender}_{BestSkill}_Mouth";

			// Set Height and Width
			Height = 1 + StaticRandom.Float(-0.075f, 0.075f);
			Weight = 1 + StaticRandom.Float(-0.075f, 0.075f);

			//Save attributes
			UpdateAvatarBeliefs(crewMember);
		}


		/// <summary>
		/// Method to help limit an integer between 2 values
		/// </summary>
		/// <param name="value"></param>
		/// <param name="inclusiveMinimum">Assuming default 0, for colour usage</param>
		/// <param name="inclusiveMaximum">Assuming default 255, for colour usage</param>
		/// <returns></returns>
		public int LimitToRange(int value, int inclusiveMinimum = 0, int inclusiveMaximum = 255) 
		{
			if (value < inclusiveMinimum) { return inclusiveMinimum; }
			if (value > inclusiveMaximum) { return inclusiveMaximum; }
			return value;
		}

		/// <summary>
		/// Get the highest rated skill for this CrewMember
		/// </summary>
		private CrewMemberSkill GetBestSkill(CrewMember crewMember)
		{
			var bestSkill = CrewMemberSkill.Charisma;
			var bestSkillLevel = 0;
			//for each available skill
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				//no Body avatar parts at the moment, so this skill is skipped
				if (skill == CrewMemberSkill.Body)
				{
					continue;
				}
				//if the skill rating is equal to the current highest, randomly select if it should become the new bestSkill
				if (crewMember.Skills[skill] == bestSkillLevel)
				{
					if (StaticRandom.Int(0, 100) % 2 == 0)
					{
						bestSkillLevel = crewMember.Skills[skill];
						bestSkill = skill;
					}
				}
				//if the skill rating is higher, make this skill the new bestSkill
				if (crewMember.Skills[skill] > bestSkillLevel)
				{
					bestSkillLevel = crewMember.Skills[skill];
					bestSkill = skill;
				}
			}
			return bestSkill;
		}

		/// <summary>
		/// Get a random skil colour, from config values
		/// </summary>
		/// <returns></returns>
		private Color GetRandomSkinColor()
		{
			switch (StaticRandom.Int(0, 3))
			{
				case 0:
					MouthColor = "Light";
					return Config.LightSkinColor;
				case 1:
					MouthColor = "Dark";
					return Config.DarkSkinColor;
				default:
					MouthColor = "Medium";
					return Config.MediumSkinColor;
			}
		}

		/// <summary>
		/// Get a suitable hair colour for the skin colour, prevents returning hair which is unusual for certain skin colours
		/// </summary>
		/// <param name="skin"></param>
		/// <returns></returns>
		private Color GetHairColorForSkin(Color skin)
		{
			// We want to limit the hair colors that are available, so dark skin does not give bright colored hair
			if (skin == Config.LightSkinColor || skin == Config.MediumSkinColor)
			{
				//lighter color skin tones have all hair colors
				return GetRandomHairColor();
			}
			switch (StaticRandom.Int(0, 2))
			{
				case 0:
					return RandomizeColor(Config.BlackHairColor);
				default:
					return RandomizeColor(Config.BrownHairColor);
			}
		}

		/// <summary>
		/// Get a random hair colour from all the available colours specified in config
		/// </summary>
		/// <returns></returns>
		private Color GetRandomHairColor()
		{
			switch (StaticRandom.Int(0, 4))
			{
				case 0:
					return RandomizeColor(Config.BlondeHairColor);
				case 1:
					return RandomizeColor(Config.BlackHairColor);
				case 2:
					return RandomizeColor(Config.GingerHairColor);
				default:
					return RandomizeColor(Config.BrownHairColor);
			}
		}

		/// <summary>
		/// Randomize a color to add more variation to avatars
		/// </summary>
		/// <param name="original"></param>
		/// <returns></returns>
		private Color RandomizeColor(Color original)
		{
			var change = StaticRandom.Int(-50, 50);
			var colorRed = original.R + change;
			var colorGreen = original.G + change;
			var colorBlue = original.B + change;
			return new Color((byte)LimitToRange(colorRed), (byte)LimitToRange(colorGreen), (byte)LimitToRange(colorBlue), 255);
		}

		/// <summary>
		/// Get a body type by skill
		/// </summary>
		/// <param name="skill">The most prominent skill for the avatar</param>
		/// <returns></returns>
		private string GetBodyType(CrewMemberSkill skill)
		{
			switch (skill)
			{
				case CrewMemberSkill.Quickness:
					return "Skinny";
				case CrewMemberSkill.Willpower:
					return "Plump";
				default:
					return "Normal";
			}
		}

		/// <summary>
		/// Gets a random eye color from those specified in the config
		/// </summary>
		/// <returns></returns>
		private Color GetRandomEyeColor()
		{
			switch (StaticRandom.Int(0, 3))
			{
				case 0:
					return RandomizeColor(Config.BlueEyeColor);
				case 1:
					return RandomizeColor(Config.GreenEyeColor);
				default:
					return RandomizeColor(Config.BrownEyeColor);
			}
		} 

		/// <summary>
		/// Get a saved eye color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private Color GetEyeColorFromText(string color)
		{
			switch (color)
			{
				case "Blue":
					return Config.BlueEyeColor;
				case "Green":
					return Config.GreenEyeColor;
				default:
					return Config.BrownEyeColor;
			}
		}

		/// <summary>
		/// recover the avatar attributes from beliefs stored in the EmotionalAppraisal Asset
		/// </summary>
		private void LoadAvatar(CrewMember crewMember)
		{
			BestSkill = (CrewMemberSkill)Enum.Parse(typeof(CrewMemberSkill), crewMember.LoadBelief(NPCBeliefs.AvatarBestSkill.GetDescription()));
			BodyType = crewMember.LoadBelief(NPCBeliefs.AvatarBodyType.GetDescription());
			EyebrowType = crewMember.LoadBelief(NPCBeliefs.AvatarEyebrowType.GetDescription());
			EyeType = crewMember.LoadBelief(NPCBeliefs.AvatarEyeType.GetDescription());
			if (crewMember.LoadBelief(NPCBeliefs.AvatarEyeColor.GetDescription()) != null)
			{
				EyeColor = GetEyeColorFromText(crewMember.LoadBelief(NPCBeliefs.AvatarEyeColor.GetDescription()));
			}
			else
			{
				var eyeColorRed = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarEyeColorRed.GetDescription()));
				var eyeColorGreen = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarEyeColorGreen.GetDescription()));
				var eyeColorBlue = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarEyeColorBlue.GetDescription()));
				EyeColor = new Color(eyeColorRed, eyeColorGreen, eyeColorBlue, 255);
			}
			var hairColorRed = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarHairColorRed.GetDescription()));
			var hairColorGreen = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarHairColorGreen.GetDescription()));
			var hairColorBlue = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarHairColorBlue.GetDescription()));
			HairColor = new Color(hairColorRed, hairColorGreen, hairColorBlue, 255);
			HairType = crewMember.LoadBelief(NPCBeliefs.AvatarHairType.GetDescription());
			Height = float.Parse(crewMember.LoadBelief(NPCBeliefs.AvatarHeight.GetDescription()));
			MouthType = crewMember.LoadBelief(NPCBeliefs.AvatarMouthType.GetDescription());
			MouthColor = crewMember.LoadBelief(NPCBeliefs.AvatarMouthColor.GetDescription());
			TeethType = crewMember.LoadBelief(NPCBeliefs.AvatarTeethType.GetDescription());
			NoseType = crewMember.LoadBelief(NPCBeliefs.AvatarNoseType.GetDescription());
			var skinColorRed = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarSkinColorRed.GetDescription()));
			var skinColorGreen = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarSkinColorGreen.GetDescription()));
			var skinColorBlue = Convert.ToByte(crewMember.LoadBelief(NPCBeliefs.AvatarSkinColorBlue.GetDescription()));
			SkinColor = new Color(skinColorRed, skinColorGreen, skinColorBlue, 255);
			Weight = float.Parse(crewMember.LoadBelief(NPCBeliefs.AvatarWeight.GetDescription()));
		}

		/// <summary>
		/// save the current attributes of the CrewMember to their EmotionalAppraisal Asset
		/// </summary>
		internal void UpdateAvatarBeliefs(CrewMember crewMember)
		{
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarBestSkill.GetDescription(), BestSkill.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarBodyType.GetDescription(), BodyType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyebrowType.GetDescription(), EyebrowType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeType.GetDescription(), EyeType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeColorRed.GetDescription(), EyeColor.R.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeColorGreen.GetDescription(), EyeColor.G.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeColorBlue.GetDescription(), EyeColor.B.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorRed.GetDescription(), HairColor.R.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorGreen.GetDescription(), HairColor.G.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorBlue.GetDescription(), HairColor.B.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairType.GetDescription(), HairType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHeight.GetDescription(), Height.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarMouthType.GetDescription(), MouthType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarMouthColor.GetDescription(), MouthColor);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarTeethType.GetDescription(), TeethType ?? WellFormedNames.Name.NIL_STRING);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarNoseType.GetDescription(), NoseType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorRed.GetDescription(), SkinColor.R.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorGreen.GetDescription(), SkinColor.G.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorBlue.GetDescription(), SkinColor.B.ToString());
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarWeight.GetDescription(), Weight.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
		}
	}
}
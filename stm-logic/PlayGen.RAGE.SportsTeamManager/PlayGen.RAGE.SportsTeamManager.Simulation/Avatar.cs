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
		public bool IsMale { get; }
		public CrewMemberSkill BestSkill { get; private set; }

		public bool CustomOutfitColor { get; }

		public Color SkinColor { get; private set; }
		public string MouthColor { get; private set; }
		public Color HairColor { get; private set; }
		public Color EyeColor { get; private set; }
		public Color PrimaryOutfitColor { get; set; }
		public Color SecondaryOutfitColor { get; set; }

		internal Avatar (CrewMember crewMember, bool isActive = true, bool canLoad = false)
		{
			//set outfit type
			var outfit = !isActive ? "01" : "0" + ((StaticRandom.Int(0, 100) % 2) + 2);
			var gender = crewMember.Gender == "Male" ? "M" : "F";
			IsMale = crewMember.Gender == "Male";
			CustomOutfitColor = outfit != "01";
			//recreate pre-existing avatar if one already exists
			if (canLoad)
			{
				LoadAvatar(crewMember);
			}
			//otherwise, create new avatar
			else
			{
				CreateAvatar(crewMember, gender);
			}
			//set outfit according to type, best skill and gender
			OutfitBaseType = string.Format("Outfit{0}_Base_{1}_{2}", gender, GetBodyType(BestSkill), outfit);
			OutfitHighlightType = string.Format("Outfit{0}_Highlight_{1}_{2}", gender, GetBodyType(BestSkill), outfit);
			OutfitShadowType = string.Format("Outfit{0}_Shadow_{1}_{2}", gender, GetBodyType(BestSkill), outfit);
		}

		private void CreateAvatar(CrewMember crewMember, string gender)
		{
			//Get Best Skill
			BestSkill = GetBestSkill(crewMember);

			//Set Skin Color
			SkinColor = GetRandomSkinColor();

			//Set Hair Color
			HairColor = Config.RandomHairColor ? GetRandomHairColor() : GetHairColorForSkin(SkinColor);
			var hairChange = StaticRandom.Int(-50, 50);
			var hairColorRed = HairColor.R + hairChange;
			var hairColorGreen = HairColor.G + hairChange;
			var hairColorBlue = HairColor.B + hairChange;
			HairColor = new Color((byte)(hairColorRed < 0 ? 0 : hairColorRed > 255 ? 255 : hairColorRed), (byte)(hairColorGreen < 0 ? 0 : hairColorGreen > 255 ? 255 : hairColorGreen), (byte)(hairColorBlue < 0 ? 0 : hairColorBlue > 255 ? 255 : hairColorBlue), 255);

			//Set Primary Color
			PrimaryOutfitColor = CustomOutfitColor ? StaticRandom.Color() : new Color(255, 255, 255, 255);

			//Set Secondary Color
			SecondaryOutfitColor = CustomOutfitColor ? StaticRandom.Color() : new Color(0, 0, 0, 0);

			//Set Body Type
			BodyType = string.Format("Body{0}_{1}", gender, GetBodyType(BestSkill));

			//Set Hair Type
			HairType = string.Format("Hair{0:00}{1}", StaticRandom.Int(1, Config.HairTypesCount + 1), gender);

			//Set Eye Type
			EyeType = string.Format("Eye{0}_{1}", gender, BestSkill);

			//Set Eye Color
			EyeColor = GetRandomEyeColor();
			var eyeChange = StaticRandom.Int(-50, 50);
			var eyeColorRed = EyeColor.R + eyeChange;
			var eyeColorGreen = EyeColor.G + eyeChange;
			var eyeColorBlue = EyeColor.B + eyeChange;
			EyeColor = new Color((byte)(eyeColorRed < 0 ? 0 : eyeColorRed > 255 ? 255 : eyeColorRed), (byte)(eyeColorGreen < 0 ? 0 : eyeColorGreen > 255 ? 255 : eyeColorGreen), (byte)(eyeColorBlue < 0 ? 0 : eyeColorBlue > 255 ? 255 : eyeColorBlue), 255);

			//Set Face type
			EyebrowType = string.Format("Face{0}_{1}_Eyebrows", gender, BestSkill);
			NoseType = string.Format("Face{0}_{1}_Nose", gender, BestSkill);

			//Specify the teeth for male avatars
			if (gender == "M")
			{
				TeethType = string.Format("Face{0}_{1}_Teeth", gender, BestSkill);
			}

			//Set Mouth Type
			MouthType = string.Format("Face{0}_{1}_Mouth", gender, BestSkill);

			// Set Height and Width
			Height = 1 + StaticRandom.Float(-0.075f, 0.075f);
			Weight = 1 + StaticRandom.Float(-0.075f, 0.075f);

			//Save attributes
			UpdateAvatarBeliefs(crewMember);
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
					return Config.BlackHairColor;
				default:
					return Config.BrownHairColor;
			}
		}

		private Color GetRandomHairColor()
		{
			switch (StaticRandom.Int(0, 4))
			{
				case 0:
					return Config.BlondeHairColor;
				case 1:
					return Config.BlackHairColor;
				case 2:
					return Config.GingerHairColor;
				default:
					return Config.BrownHairColor;
			}
		}

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

		private Color GetRandomEyeColor()
		{
			switch (StaticRandom.Int(0, 3))
			{
				case 0:
					return Config.BlueEyeColor;
				case 1:
					return Config.GreenEyeColor;
				default:
					return Config.BrownEyeColor;
			}
		}

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
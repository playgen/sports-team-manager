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

		internal Avatar (CrewMember crewMember, bool isActive = true)
		{
			//set outfit type
			var outfit = isActive ? "0" + ((StaticRandom.Int(0, 100) % 2) + 2) : "01";
			_gender = crewMember.Gender;
			CustomOutfitColor = isActive;
			//attempt to recreate pre-existing avatar if one already exists, create new avatar otherwise
			CreateAvatar(crewMember);
			//set outfit according to type, best skill and gender
			OutfitBaseType = $"Outfit{_gender}_Base_{GetBodyType(BestSkill)}_{outfit}";
			OutfitHighlightType = $"Outfit{_gender}_Highlight_{GetBodyType(BestSkill)}_{outfit}";
			OutfitShadowType = $"Outfit{_gender}_Shadow_{GetBodyType(BestSkill)}_{outfit}";
		}

		private void CreateAvatar(CrewMember crewMember)
		{
			//Get Best Skill
			var currentBestSkill = crewMember.LoadBelief(NPCBeliefs.AvatarBestSkill);
			BestSkill = Enum.TryParse<CrewMemberSkill>(currentBestSkill, out var loadedBestSkill) ? loadedBestSkill : GetBestSkill(crewMember);

			//Set Skin Color
			var loadedMouthColor = crewMember.LoadBelief(NPCBeliefs.AvatarMouthColor);
			if (loadedMouthColor != null &&
				byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarSkinColorRed), out var skinColorRed) &&
				byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarSkinColorGreen), out var skinColorGreen) &&
				byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarSkinColorBlue), out var skinColorBlue))
			{
				SkinColor = new Color(skinColorRed, skinColorGreen, skinColorBlue);
				MouthColor = loadedMouthColor;
			}
			else
			{
				SkinColor = GetRandomSkinColor();
			}

			//Set Hair Color
			if (byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarHairColorRed), out var hairColorRed) &&
				byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarHairColorGreen), out var hairColorGreen) &&
				byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarHairColorBlue), out var hairColorBlue))
			{
				HairColor = new Color(hairColorRed, hairColorGreen, hairColorBlue);
			}
			else
			{
				HairColor = Config.RandomHairColor ? GetRandomHairColor() : GetHairColorForSkin(SkinColor);
			}
			
			//Set Body Type
			BodyType = crewMember.LoadBelief(NPCBeliefs.AvatarBodyType) ?? $"Body{_gender}_{GetBodyType(BestSkill)}";

			//Set Hair Type
			HairType = crewMember.LoadBelief(NPCBeliefs.AvatarHairType) ?? $"Hair{StaticRandom.Int(1, Config.HairTypesCount + 1):00}{_gender}";

			//Set Eye Type
			EyeType = crewMember.LoadBelief(NPCBeliefs.AvatarEyeType) ?? $"Eye{_gender}_{BestSkill}";

			//Set Eye Color
			var textEyeColor = crewMember.LoadBelief(NPCBeliefs.AvatarEyeColor);
			if (textEyeColor != null)
			{
				EyeColor = GetEyeColorFromText(textEyeColor);
			}
			else
			{
				if (byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarEyeColorRed), out var eyeColorRed) &&
					byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarEyeColorGreen), out var eyeColorGreen) &&
					byte.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarEyeColorBlue), out var eyeColorBlue))
				{
					EyeColor = new Color(eyeColorRed, eyeColorGreen, eyeColorBlue);
				}
				else
				{
					EyeColor = GetRandomEyeColor();
				}
			}

			//Set Face type
			EyebrowType = crewMember.LoadBelief(NPCBeliefs.AvatarEyebrowType) ?? $"Face{_gender}_{BestSkill}_Eyebrows";
			NoseType = crewMember.LoadBelief(NPCBeliefs.AvatarNoseType) ?? $"Face{_gender}_{BestSkill}_Nose";

			//Specify the teeth for male avatars
			if (IsMale)
			{
				TeethType = crewMember.LoadBelief(NPCBeliefs.AvatarTeethType) ?? $"Face{_gender}_{BestSkill}_Teeth";
			}

			//Set Mouth Type
			MouthType = crewMember.LoadBelief(NPCBeliefs.AvatarMouthType) ?? $"Face{_gender}_{BestSkill}_Mouth";

			// Set Height and Width
			if (float.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarHeight), out var loadedHeight))
			{
				Height = loadedHeight;
			}
			else
			{
				Height = 1 + StaticRandom.Float(-0.075f, 0.075f);
			}

			if (float.TryParse(crewMember.LoadBelief(NPCBeliefs.AvatarWeight), out var loadedWeight))
			{
				Weight = loadedWeight;
			}
			else
			{
				Weight = 1 + StaticRandom.Float(-0.075f, 0.075f);
			}

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
					return Config.BlackHairColor.RandomVariation(-50, 50);
				default:
					return Config.BrownHairColor.RandomVariation(-50, 50);
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
					return Config.BlondeHairColor.RandomVariation(-50, 50);
				case 1:
					return Config.BlackHairColor.RandomVariation(-50, 50);
				case 2:
					return Config.GingerHairColor.RandomVariation(-50, 50);
				default:
					return Config.BrownHairColor.RandomVariation(-50, 50);
			}
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
					return Config.BlueEyeColor.RandomVariation(-50, 50);
				case 1:
					return Config.GreenEyeColor.RandomVariation(-50, 50);
				default:
					return Config.BrownEyeColor.RandomVariation(-50, 50);
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
		/// Set the avatar outfit colors to match the team colors
		/// </summary>
		internal void SetCrewColors(Color primary, Color secondary)
		{
			PrimaryOutfitColor = primary;
			SecondaryOutfitColor = secondary;
		}

		/// <summary>
		/// save the current attributes of the CrewMember to their EmotionalAppraisal Asset
		/// </summary>
		internal void UpdateAvatarBeliefs(CrewMember crewMember)
		{
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarBestSkill, BestSkill);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarBodyType, BodyType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyebrowType, EyebrowType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeType, EyeType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeColorRed, EyeColor.R);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeColorGreen, EyeColor.G);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeColorBlue, EyeColor.B);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorRed, HairColor.R);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorGreen, HairColor.G);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorBlue, HairColor.B);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairType, HairType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHeight, Height);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarMouthType, MouthType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarMouthColor, MouthColor);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarTeethType, TeethType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarNoseType, NoseType);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorRed, SkinColor.R);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorGreen, SkinColor.G);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorBlue, SkinColor.B);
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarWeight, Weight);
		}
	}
}
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

		public bool CustomOutfitColor { get; }

		public Color SkinColor { get; private set; }
		public string MouthColor { get; private set; }
		public Color HairColor { get; private set; }
		public Color EyeColor { get; private set; }
		public Color PrimaryOutfitColor { get; set; }
		public Color SecondaryOutfitColor { get; set; }

		private readonly string _gender;
		private Skill _bestSkill;
		private string _bodyType;

		internal Avatar (CrewMember crewMember, bool isActive = true)
		{
			//set outfit type
			var outfit = isActive ? "0" + ((StaticRandom.Int(0, 100) % 2) + 2) : "01";
			_gender = crewMember.Gender;
			CustomOutfitColor = isActive;
			//attempt to recreate pre-existing avatar if one already exists, create new avatar otherwise
			CreateAvatar(crewMember);
			//set outfit according to type, best skill and gender
			OutfitBaseType = $"Outfit{_gender}_Base_{_bodyType}_{outfit}";
			OutfitHighlightType = $"Outfit{_gender}_Highlight_{_bodyType}_{outfit}";
			OutfitShadowType = $"Outfit{_gender}_Shadow_{_bodyType}_{outfit}";
		}

		/// <summary>
		/// Create a new Avatar based on randomness and this Crew Member's best skill or load the existing Avatar for this Crew Member
		/// </summary>
		private void CreateAvatar(CrewMember crewMember)
		{
			//Get Best Skill
			var currentBestSkill = crewMember.LoadBelief(NPCBelief.AvatarBestSkill);
			_bestSkill = Enum.TryParse<Skill>(currentBestSkill, out var loadedBestSkill) ? loadedBestSkill : GetBestSkill(crewMember);
			_bodyType = GetBodyType();

			//Set Skin Color
			var loadedMouthColor = crewMember.LoadBelief(NPCBelief.AvatarMouthColor);
			if (loadedMouthColor != null &&
				byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarSkinColorRed), out var skinColorRed) &&
				byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarSkinColorGreen), out var skinColorGreen) &&
				byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarSkinColorBlue), out var skinColorBlue))
			{
				SkinColor = new Color(skinColorRed, skinColorGreen, skinColorBlue);
				MouthColor = loadedMouthColor;
			}
			else
			{
				SkinColor = GetRandomSkinColor();
			}

			//Set Hair Color
			if (byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarHairColorRed), out var hairColorRed) &&
				byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarHairColorGreen), out var hairColorGreen) &&
				byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarHairColorBlue), out var hairColorBlue))
			{
				HairColor = new Color(hairColorRed, hairColorGreen, hairColorBlue);
			}
			else
			{
				HairColor = Config.RandomHairColor ? GetRandomHairColor() : GetHairColorForSkin(SkinColor);
			}
			
			//Set Body Type
			BodyType = crewMember.LoadBelief(NPCBelief.AvatarBodyType) ?? $"Body{_gender}_{_bodyType}";

			//Set Hair Type
			HairType = crewMember.LoadBelief(NPCBelief.AvatarHairType) ?? $"Hair{StaticRandom.Int(1, Config.HairTypesCount + 1):00}{_gender}";

			//Set Eye Type
			EyeType = crewMember.LoadBelief(NPCBelief.AvatarEyeType) ?? $"Eye{_gender}_{_bestSkill}";

			//Set Eye Color
			var textEyeColor = crewMember.LoadBelief(NPCBelief.AvatarEyeColor);
			if (textEyeColor != null)
			{
				EyeColor = GetEyeColorFromText(textEyeColor);
			}
			else
			{
				if (byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarEyeColorRed), out var eyeColorRed) &&
					byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarEyeColorGreen), out var eyeColorGreen) &&
					byte.TryParse(crewMember.LoadBelief(NPCBelief.AvatarEyeColorBlue), out var eyeColorBlue))
				{
					EyeColor = new Color(eyeColorRed, eyeColorGreen, eyeColorBlue);
				}
				else
				{
					EyeColor = GetRandomEyeColor();
				}
			}

			//Set Face type
			EyebrowType = crewMember.LoadBelief(NPCBelief.AvatarEyebrowType) ?? $"Face{_gender}_{_bestSkill}_Eyebrows";
			NoseType = crewMember.LoadBelief(NPCBelief.AvatarNoseType) ?? $"Face{_gender}_{_bestSkill}_Nose";

			//Specify the teeth for male avatars
			if (IsMale)
			{
				TeethType = crewMember.LoadBelief(NPCBelief.AvatarTeethType) ?? $"Face{_gender}_{_bestSkill}_Teeth";
			}

			//Set Mouth Type
			MouthType = crewMember.LoadBelief(NPCBelief.AvatarMouthType) ?? $"Face{_gender}_{_bestSkill}_Mouth";

			// Set Height and Width (Weight)
			if (float.TryParse(crewMember.LoadBelief(NPCBelief.AvatarHeight), out var loadedHeight))
			{
				Height = loadedHeight;
			}
			else
			{
				Height = 1 + StaticRandom.Float(-0.075f, 0.075f);
			}

			if (float.TryParse(crewMember.LoadBelief(NPCBelief.AvatarWeight), out var loadedWeight))
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
		private Skill GetBestSkill(CrewMember crewMember)
		{
			var bestSkill = Skill.Charisma;
			var bestSkillLevel = 0;
			//for each available skill
			foreach (Skill skill in Enum.GetValues(typeof(Skill)))
			{
				//no Body avatar parts at the moment, so this skill is skipped
				if (skill == Skill.Body)
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
		private string GetBodyType()
		{
			switch (_bestSkill)
			{
				case Skill.Quickness:
					return "Skinny";
				case Skill.Willpower:
					return "Plump";
				default:
					return "Normal";
			}
		}

		/// <summary>
		/// Gets a random eye color from those specified in the config
		/// </summary>
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
		/// Save the current attributes of the CrewMember to their EmotionalAppraisal Asset
		/// </summary>
		internal void UpdateAvatarBeliefs(CrewMember crewMember)
		{
			crewMember.UpdateSingleBelief(NPCBelief.AvatarBestSkill, _bestSkill);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarBodyType, BodyType);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarEyebrowType, EyebrowType);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarEyeType, EyeType);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarEyeColorRed, EyeColor.R);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarEyeColorGreen, EyeColor.G);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarEyeColorBlue, EyeColor.B);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarHairColorRed, HairColor.R);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarHairColorGreen, HairColor.G);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarHairColorBlue, HairColor.B);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarHairType, HairType);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarHeight, Height);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarMouthType, MouthType);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarMouthColor, MouthColor);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarTeethType, TeethType);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarNoseType, NoseType);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarSkinColorRed, SkinColor.R);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarSkinColorGreen, SkinColor.G);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarSkinColorBlue, SkinColor.B);
			crewMember.UpdateSingleBelief(NPCBelief.AvatarWeight, Weight);
		}
	}
}
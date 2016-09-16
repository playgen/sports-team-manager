using System;
using System.Collections;
using System.Drawing;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Avatar
	{
		/// <summary>
		/// Avatar class containing names of sprites that make up the avatar
		/// </summary>
		public string BodyType { get; set; }
		public string OutfitBaseType { get; set; }
		public string OutfitHiglightType { get; set; }
		public string OutfitShadowType { get; set; }
		public string HairType { get; set; }
		public string EyeType { get; set; }
        public string EyeColor { get; set; }
        public string EyebrowType { get; set; }
		public string NoseType { get; set; }
		public string MouthType { get; set; }
        public string TeethType { get; set; }
        public float Weight { get; set; }
		public float Height { get; set; }
		public bool IsMale { get; set; }
		public CrewMemberSkill BestSkill { get; set; }

		public bool CustomOutfitColor { get; set; }

		public Color SkinColor { get; set; }
		public string MouthColor { get; set; }
		public Color HairColor { get; set; }
		public Color PrimaryOutfitColor { get; set; }
		public Color SecondaryOutfitColor { get; set; }

		private AvatarGeneratorConfig _config { get; set; }

		public Avatar (CrewMember crewMember, bool isActive = true, bool canLoad = false)
		{
			GetConfig();
			Random random = new Random();
			var outfit = !isActive ? "01" : ("0" + ((random.Next(0, 100) % 2) + 2));
			var gender = crewMember.Gender == "Male" ? "M" : "F";
			IsMale = crewMember.Gender == "Male";
			CustomOutfitColor = outfit != "01";
			if (canLoad)
			{
				LoadAvatar(crewMember);
			}
			else
			{
				CreateAvatar(crewMember, gender, random);
			}
			OutfitBaseType = string.Format("Outfit{0}_Base_{1}_{2}", gender, GetBody(BestSkill), outfit);
			OutfitHiglightType = string.Format("Outfit{0}_Highlight_{1}_{2}", gender, GetBody(BestSkill), outfit);
			OutfitShadowType = string.Format("Outfit{0}_Shadow_{1}_{2}", gender, GetBody(BestSkill), outfit);
		}

		private void CreateAvatar(CrewMember crewMember, string gender, Random random)
		{
			BestSkill = GetBestSkill(crewMember, random);
			// Set Skin Color
			SkinColor = GetSkin(random);

			// Set Hair Color
			HairColor = GetHairColor(SkinColor, random);

			// Set Prinary Color
			PrimaryOutfitColor = CustomOutfitColor ? GetRandomColor(random) : Color.White;

			// Set Secondary Color
			SecondaryOutfitColor = CustomOutfitColor ? GetRandomColor(random) : Color.Black;

			// Set Body Type
			BodyType = string.Format("Body{0}_{1}", gender, GetBody(BestSkill));

			// Set Hair Type
			HairType = string.Format("Hair{0:00}{1}", random.Next(1, _config.HairTypesCount + 1), gender);

			// Set Eye Type
			EyeType = string.Format("Eye{0}_{1}", gender, BestSkill);

            //Set Eye Color
            EyeColor = GetEyes(random);

			// Set Face type
			EyebrowType = string.Format("Face{0}_{1}_Eyebrows", gender, BestSkill);
			NoseType = string.Format("Face{0}_{1}_Nose", gender, BestSkill);

			//our charisma and quickness faces have open mouths so specify the mouth
			if (gender == "M")
			{
                TeethType = string.Format("Face{0}_{1}_Teeth", gender, BestSkill);
            }
            MouthType = string.Format("Face{0}_{1}_Mouth", gender, BestSkill);

            // Set Height and Width
            Height = 1 + (((float)random.NextDouble() * 0.15f) - 0.075f);
			Weight = 1 + (((float)random.NextDouble() * 0.15f) - 0.075f);
			UpdateAvatarBeliefs(crewMember);
		}

		private void LoadAvatar(CrewMember crewMember)
		{
			BestSkill = (CrewMemberSkill)Enum.Parse((typeof(CrewMemberSkill)), crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarBestSkill.GetDescription()));
			BodyType = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarBodyType.GetDescription());
			EyebrowType = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarEyebrowType.GetDescription());
			EyeType = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarEyeType.GetDescription());
            EyeColor = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarEyeColor.GetDescription());
            int hairColorRed = int.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarHairColorRed.GetDescription()));
			int hairColorGreen = int.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarHairColorGreen.GetDescription()));
			int hairColorBlue = int.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarHairColorBlue.GetDescription()));
			HairColor = Color.FromArgb(255, hairColorRed, hairColorGreen, hairColorBlue);
			HairType = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarHairType.GetDescription());
			Height = float.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarHeight.GetDescription()));
			MouthType = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarMouthType.GetDescription());
			MouthColor = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarMouthColor.GetDescription());
            TeethType = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarTeethType.GetDescription());
            NoseType = crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarNoseType.GetDescription());
			int skinColorRed = int.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarSkinColorRed.GetDescription()));
			int skinColorGreen = int.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarSkinColorGreen.GetDescription()));
			int skinColorBlue = int.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarSkinColorBlue.GetDescription()));
			SkinColor = Color.FromArgb(255, skinColorRed, skinColorGreen, skinColorBlue);
			Weight = float.Parse(crewMember.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.AvatarWeight.GetDescription()));
		}

		public void UpdateAvatarBeliefs(CrewMember crewMember)
		{
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarBestSkill.GetDescription(), BestSkill.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarBodyType.GetDescription(), BodyType, "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyebrowType.GetDescription(), EyebrowType, "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeType.GetDescription(), EyeType, "SELF");
            crewMember.UpdateSingleBelief(NPCBeliefs.AvatarEyeColor.GetDescription(), EyeColor, "SELF");
            crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorRed.GetDescription(), HairColor.R.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorGreen.GetDescription(), HairColor.G.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairColorBlue.GetDescription(), HairColor.B.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHairType.GetDescription(), HairType, "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarHeight.GetDescription(), Height.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarMouthType.GetDescription(), MouthType, "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarMouthColor.GetDescription(), MouthColor, "SELF");
            crewMember.UpdateSingleBelief(NPCBeliefs.AvatarTeethType.GetDescription(), TeethType, "SELF");
            crewMember.UpdateSingleBelief(NPCBeliefs.AvatarNoseType.GetDescription(), NoseType, "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorRed.GetDescription(), SkinColor.R.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorGreen.GetDescription(), SkinColor.G.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarSkinColorBlue.GetDescription(), SkinColor.B.ToString(), "SELF");
			crewMember.UpdateSingleBelief(NPCBeliefs.AvatarWeight.GetDescription(), Weight.ToString(), "SELF");
			crewMember.SaveStatus();
		}

		private string GetBody(CrewMemberSkill bestSkill)
		{
			return GetBodyType(bestSkill);
		}
		private Color GetSkin(Random random)
		{
			return GetRandomSkinColor(random);
		}
		private Color GetHairColor(Color skin, Random random)
		{
			// we can check the skin colour here to determine which hair colours to return
			return _config.RandomHairColor ? GetRandomHairColor(random) : GetHairColorForSkin(skin, random);
		}

		private string GetEyes(Random random)
		{
			return GetRandomEyeColor(random);
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

		private Color GetRandomHairColor(Random rand)
		{
			switch (rand.Next(0, 4))
			{
				case 0:
					return _config.BlondeHairColor;
				case 1:
					return _config.BlackHairColor;
				case 2:
					return _config.GingerHairColor;
				default:
					return _config.BrownHairColor;
			}
		}

		private Color GetHairColorForSkin(Color skin, Random rand)
		{
			// We want to limit the hair colours that are available, so dark skin does not give bright coloured hair
			if (skin == _config.LightSkinColor || skin == _config.MediumSkinColor)
			{
				//lighter coiour skin tones have all hair colours
				return GetRandomHairColor(rand);
			}
			switch (rand.Next(0, 2))
			{
				case 0:
					return _config.BlackHairColor;
				default:
					return _config.BrownHairColor;
			}
		}
		private Color GetRandomSkinColor(Random rand)
		{
			switch (rand.Next(0, 3))
			{
				case 0:
					MouthColor = "Light";
					return _config.LightSkinColor;
				case 1:
					MouthColor = "Dark";
					return _config.DarkSkinColor;
				default:
					MouthColor = "Medium";
					return _config.MediumSkinColor;
			}
		}

		private string GetRandomEyeColor(Random rand)
		{
			switch (rand.Next(0, 3))
			{
				case 0:
					return "Blue";
				case 1:
					return "Green";
				default:
					return "Brown";
			}
		}

		private Color GetRandomColor(Random rand)
		{
			return Color.FromArgb(255, rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
		}

		private void GetConfig()
		{
			var config = new AvatarGeneratorConfig().GetConfig();
			_config = config;
		}

		private CrewMemberSkill GetBestSkill(CrewMember crewMember, Random rand)
		{
			CrewMemberSkill bestSkill = CrewMemberSkill.Charisma;
			int bestSkillLevel = 0;
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (skill == CrewMemberSkill.Body)
				{
					continue;
				}
				if (crewMember.Skills[skill] == bestSkillLevel)
				{
					if (rand.Next(0, 100) % 2 == 0)
					{
						bestSkillLevel = crewMember.Skills[skill];
						bestSkill = skill;
					}
				}
				if (crewMember.Skills[skill] > bestSkillLevel)
				{
					bestSkillLevel = crewMember.Skills[skill];
					bestSkill = skill;
				}
			}
			return bestSkill;
		}
	}
}
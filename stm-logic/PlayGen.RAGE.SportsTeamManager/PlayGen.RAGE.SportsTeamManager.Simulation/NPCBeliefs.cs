﻿using System.ComponentModel;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Enum of Beliefs stored for NPCs
	/// </summary>
	internal enum NPCBeliefs
	{
		[Description("Value({0})")]
		Skill,
		[Description("Opinion({0})")]
		Opinion,
		[Description("RevealedValue({0})")]
		RevealedSkill,
		[Description("RevealedOpinion({0})")]
		RevealedOpinion,
		[Description("RevealedOpinionAge({0})")]
		RevealedOpinionAge,
		[Description("Value(Age)")]
		Age,
		[Description("Value(Gender)")]
		Gender,
		[Description("Value(Position)")]
		Position,
		[Description("Race(Rest)")]
		Rest,
		[Description("Value(BoatType)")]
		BoatType,
		[Description("Value(ShowTutorial)")]
		ShowTutorial,
		[Description("Value(QuestionnaireCompleted)")]
		QuestionnaireCompleted,
		[Description("Value(TutorialStage)")]
		TutorialStage,
		[Description("Value(Nationality)")]
		Nationality,
		[Description("CrewEdit(Allowance)")]
		CrewEditAllowance,
		[Description("Action(Allowance)")]
		ActionAllowance,
		[Description("Expected(Position)")]
		ExpectedPosition,
		[Description("ExpectedAfter(Position)")]
		ExpectedPositionAfter,
		[Description("Expected(Selection)")]
		ExpectedSelection,
		[Description("ColorPrimary(Red)")]
		TeamColorRedPrimary,
		[Description("ColorPrimary(Green)")]
		TeamColorGreenPrimary,
		[Description("ColorPrimary(Blue)")]
		TeamColorBluePrimary,
		[Description("ColorSecondary(Red)")]
		TeamColorRedSecondary,
		[Description("ColorSecondary(Green)")]
		TeamColorGreenSecondary,
		[Description("ColorSecondary(Blue)")]
		TeamColorBlueSecondary,
		[Description("Avatar(SkinColorRed)")]
		AvatarSkinColorRed,
		[Description("Avatar(HairColorRed)")]
		AvatarHairColorRed,
		[Description("Avatar(EyeColorRed)")]
		AvatarEyeColorRed,
		[Description("Avatar(SkinColorGreen)")]
		AvatarSkinColorGreen,
		[Description("Avatar(HairColorGreen)")]
		AvatarHairColorGreen,
		[Description("Avatar(EyeColorGreen)")]
		AvatarEyeColorGreen,
		[Description("Avatar(SkinColorBlue)")]
		AvatarSkinColorBlue,
		[Description("Avatar(HairColorBlue)")]
		AvatarHairColorBlue,
		[Description("Avatar(EyeColorBlue)")]
		AvatarEyeColorBlue,
		[Description("Avatar(BodyType)")]
		AvatarBodyType,
		[Description("Avatar(HairType)")]
		AvatarHairType,
		[Description("Avatar(EyeType)")]
		AvatarEyeType,
		[Description("Avatar(EyeColor)")]
		AvatarEyeColor,
		[Description("Avatar(EyebrowType)")]
		AvatarEyebrowType,
		[Description("Avatar(NoseType)")]
		AvatarNoseType,
		[Description("Avatar(MouthType)")]
		AvatarMouthType,
		[Description("Avatar(MouthColor)")]
		AvatarMouthColor,
		[Description("Avatar(TeethType)")]
		AvatarTeethType,
		[Description("Avatar(Height)")]
		AvatarHeight,
		[Description("Avatar(Weight)")]
		AvatarWeight,
		[Description("Avatar(BestSkill)")]
		AvatarBestSkill,
		[Description("Note({0})")]
		Note,
	}
}
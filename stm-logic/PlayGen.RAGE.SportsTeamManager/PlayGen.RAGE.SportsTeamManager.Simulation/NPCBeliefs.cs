using System.ComponentModel;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public enum NPCBeliefs
	{
		[Description("Value({0})")]
		Skill,
		[Description("Opinion({0})")]
		Opinion,
		[Description("RevealedValue({0})")]
		RevealedSkill,
		[Description("RevealedOpinion({0})")]
		RevealedOpinion,
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
		[Description("CrewEdit(Allowance)")]
		CrewEditAllowance,
		[Description("Action(Allowance)")]
		ActionAllowance,
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
	}
}
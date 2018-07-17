namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal static class ColorExtensions
	{
		internal static Color RandomVariation(this Color original, int min, int max)
		{
			var change = StaticRandom.Int(-50, 50);
			var colorRed = original.R + change;
			var colorGreen = original.G + change;
			var colorBlue = original.B + change;
			return new Color(colorRed, colorGreen, colorBlue, 255);
		}
	}
}
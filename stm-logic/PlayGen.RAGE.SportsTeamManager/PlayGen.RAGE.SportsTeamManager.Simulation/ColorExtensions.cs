namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal static class ColorExtensions
	{
		/// <summary>
		/// Generate a random variation of the color provided using the min and max values provided
		/// </summary>
		internal static Color RandomVariation(this Color original, int min, int max)
		{
			var change = StaticRandom.Int(min, max);
			var colorRed = original.R + change;
			var colorGreen = original.G + change;
			var colorBlue = original.B + change;
			return new Color(colorRed, colorGreen, colorBlue, 255);
		}
	}
}
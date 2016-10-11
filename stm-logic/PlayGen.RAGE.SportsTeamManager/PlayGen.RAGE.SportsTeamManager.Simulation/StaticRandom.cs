using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class StaticRandom
	{
		private static readonly Random random = new Random();

		public static int Int(int min, int max)
		{
			return random.Next(min, max);
		}

		public static float Float(float min, float max)
		{
			return (float)((random.NextDouble() * (max - min)) + min);
		}

		public static Color Color()
		{
			var bytes = new byte[3];
			random.NextBytes(bytes);
			return new Color(bytes[0], bytes[1], bytes[2], 255);
		}
	}
}

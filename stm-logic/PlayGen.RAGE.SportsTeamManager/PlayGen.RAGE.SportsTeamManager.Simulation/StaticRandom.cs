using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class StaticRandom
	{
		/// <summary>
		/// Static random instance
		/// </summary>
		private static readonly Random Random = new Random();

		/// <summary>
		/// Select a random int
		/// </summary>
		public static int Int(int min, int max)
		{
			return Random.Next(min, max);
		}

		/// <summary>
		/// Select a random double which then takes into account set min and max values and is converted to a float
		/// </summary>
		public static float Float(float min, float max)
		{
			return (float)((Random.NextDouble() * (max - min)) + min);
		}

		/// <summary>
		/// Select a random array of bytes, which is converted into a Color object
		/// </summary>
		public static Color Color()
		{
			var bytes = new byte[3];
			Random.NextBytes(bytes);
			return new Color(bytes[0], bytes[1], bytes[2], 255);
		}
	}
}

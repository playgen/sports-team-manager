using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class StaticRandom
	{
		/// <summary>
		/// Static random instance
		/// </summary>
		private static readonly Random random = new Random();

		/// <summary>
		/// Select a random int
		/// </summary>
		public static int Int(int min, int max)
		{
			return random.Next(min, max);
		}

		/// <summary>
		//static a random double which then takes into account set min and max values and is converted to a float
		/// </summary>
		public static float Float(float min, float max)
		{
			return (float)((random.NextDouble() * (max - min)) + min);
		}

		/// <summary>
		//select a random array of bytes, which is converted into a Color object
		/// </summary>
		public static Color Color()
		{
			var bytes = new byte[3];
			random.NextBytes(bytes);
			return new Color(bytes[0], bytes[1], bytes[2], 255);
		}
	}
}

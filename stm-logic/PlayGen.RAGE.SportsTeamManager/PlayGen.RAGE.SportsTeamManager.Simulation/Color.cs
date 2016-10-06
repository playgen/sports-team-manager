using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public struct Color : IEquatable<Color>
	{
		public byte R, G, B, A;

		public Color(byte red, byte green, byte blue, byte alpha)
		{
			R = red;
			G = green;
			B = blue;
			A = alpha;
		}

		public bool Equals(Color other)
		{
			return Equals(other, this);
		}

		public static bool operator ==(Color c1, Color c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(Color c1, Color c2)
		{
			return !c1.Equals(c2);
		}
	}
}

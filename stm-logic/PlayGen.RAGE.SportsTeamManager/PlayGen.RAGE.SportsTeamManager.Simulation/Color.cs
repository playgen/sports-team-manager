using System;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Struct used to store team colours
	/// </summary>
	public struct Color : IEquatable<Color>
	{
		public byte R, G, B, A;

		internal Color(byte red, byte green, byte blue)
		{
			R = red;
			G = green;
			B = blue;
			A = byte.MaxValue;
		}

		internal Color(byte red, byte green, byte blue, byte alpha)
		{
			R = red;
			G = green;
			B = blue;
			A = alpha;
		}

		internal Color(int red, int green, int blue)
		{
			R = LimitToByteRange(red);
			G = LimitToByteRange(green);
			B = LimitToByteRange(blue);
			A = byte.MaxValue;
		}

		internal Color(int red, int green, int blue, int alpha)
		{
			R = LimitToByteRange(red);
			G = LimitToByteRange(green);
			B = LimitToByteRange(blue);
			A = LimitToByteRange(alpha);
		}

		public bool Equals(Color other)
		{
			return Equals(other, this);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Color color && Equals(color);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = R.GetHashCode();
				hashCode = (hashCode * 397) ^ G.GetHashCode();
				hashCode = (hashCode * 397) ^ B.GetHashCode();
				hashCode = (hashCode * 397) ^ A.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Color c1, Color c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(Color c1, Color c2)
		{
			return !c1.Equals(c2);
		}

		/// <summary>
		/// Method to help limit an integer between 2 values
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static byte LimitToByteRange(int value)
		{
			if (value < byte.MinValue)
			{
				return byte.MinValue;
			}
			if (value > byte.MaxValue)
			{
				return byte.MaxValue;
			}
			return (byte)value;
		}
	}

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

using System;
using System.ComponentModel;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal static class EnumExtensions
	{
		/// <summary>
		/// Get the description attribute of an enum.
		/// </summary>
		internal static string GetDescription(this Enum value)
		{
			var fieldInfo = value.GetType().GetField(value.ToString());

			var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return attributes.Any() ? attributes.First().Description : value.ToString();
		}
	}
}

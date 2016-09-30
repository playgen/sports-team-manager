using System;
using System.ComponentModel;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class EnumExtensions
	{
		public static string GetDescription(this Enum value)
		{
			var fieldInfo = value.GetType().GetField(value.ToString());

			var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return attributes.FirstOrDefault()?.Description ?? value.ToString();
		}
	}
}

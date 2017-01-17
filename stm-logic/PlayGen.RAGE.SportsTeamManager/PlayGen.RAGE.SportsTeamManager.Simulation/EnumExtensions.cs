using System;
using System.ComponentModel;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class EnumExtensions
	{
		public static string GetName(this Enum value)
		{
			var fieldInfo = value.GetType().GetField(value.ToString());

			var attributes = (NameAttribute[])fieldInfo.GetCustomAttributes(typeof(NameAttribute), false);

			return attributes.Any() ? attributes.First().Name : value.ToString();
		}

		public static string GetDescription(this Enum value)
		{
			var fieldInfo = value.GetType().GetField(value.ToString());

			var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return attributes.Any() ? attributes.First().Description : value.ToString();
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class NameAttribute : Attribute
	{
		public string Name { get; }

		public NameAttribute(string name)
		{
			Name = name;
		}
	}
}

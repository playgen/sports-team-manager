using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	internal static class NPCBeliefExtensions
	{
		private static readonly Dictionary<NPCBelief, string> BeliefDescriptionCache = new Dictionary<NPCBelief, string>();

		static NPCBeliefExtensions()
		{
			foreach (var belief in (NPCBelief[])Enum.GetValues(typeof(NPCBelief)))
			{
				BeliefDescriptionCache.Add(belief, belief.GetDescription());
			}
		}

		private static string GetDescription(this NPCBelief value)
		{
			var fieldInfo = value.GetType().GetField(value.ToString());
			var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
			return attributes.Any() ? attributes.First().Description : value.ToString();
		}

		/// <summary>
		/// Get the description attribute of an enum.
		/// </summary>
		internal static string Description(this NPCBelief belief)
		{
			return BeliefDescriptionCache[belief];
		}
	}
}
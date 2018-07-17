using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class PositionExtensions
	{
		private static readonly Dictionary<Position, Skill> RequiredSkillsCache = new Dictionary<Position, Skill>();
		private static readonly Dictionary<Position, Skill[]> EnumerableRequiredSkillsCache = new Dictionary<Position, Skill[]>();

		static PositionExtensions()
		{
			foreach (var position in (Position[])Enum.GetValues(typeof(Position)))
			{
				RequiredSkillsCache.Add(position, position.GetRequiredSkills());
				EnumerableRequiredSkillsCache.Add(position, ((Skill[])Enum.GetValues(typeof(Skill))).Where(skill => RequiredSkillsCache[position].ContainsSkill(skill)).ToArray());
			}
		}

		private static Skill GetRequiredSkills(this Position position)
		{
			var fieldInfo = position.GetType().GetField(position.ToString());
			var attributes = (RequiredSkillsAttribute[])fieldInfo.GetCustomAttributes(typeof(RequiredSkillsAttribute), false);
			return attributes.Any() ? attributes.First().RequiredSkills : 0;
		}

		/// <summary>
		/// Get if a Skill is required for the given Position
		/// </summary>
		internal static bool RequiresSkill(this Position position, Skill skill)
		{
			return RequiredSkillsCache[position].ContainsSkill(skill);
		}

		/// <summary>
		/// Get a list of RequiredSkills for the given Position
		/// </summary>
		public static IEnumerable<Skill> RequiredSkills(this Position position)
		{
			return EnumerableRequiredSkillsCache[position];
		}

		/// <summary>
		/// Get the average skill rating for this CrewMember in this Position
		/// </summary>
		internal static int GetPositionRating(this Position position, CrewMember crewMember)
		{
			var positionCount = 0;
			var crewScore = 0;
			foreach (var skill in position.RequiredSkills())
			{
				crewScore += crewMember.Skills[skill];
				positionCount++;
			}

			crewScore = (int)Math.Round((float)crewScore / positionCount);

			return crewScore;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class RequiredSkillsAttribute : Attribute
	{
		public Skill RequiredSkills { get; }

		public RequiredSkillsAttribute(Skill requiredSkills)
		{
			RequiredSkills = requiredSkills;
		}
	}
}

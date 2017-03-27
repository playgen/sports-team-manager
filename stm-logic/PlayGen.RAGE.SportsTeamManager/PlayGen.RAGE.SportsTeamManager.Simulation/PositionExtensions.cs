using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public static class PositionExtensions
	{
		private static readonly Dictionary<Position, CrewMemberSkill> RequiredSkillsCache = new Dictionary<Position, CrewMemberSkill>();
		private static readonly Dictionary<Position, CrewMemberSkill[]> EnumerableRequiredSkillsCache = new Dictionary<Position, CrewMemberSkill[]>();

		static PositionExtensions()
		{
			foreach (var position in (Position[])Enum.GetValues(typeof(Position)))
			{
				RequiredSkillsCache.Add(position, position.GetRequiredSkills());
				EnumerableRequiredSkillsCache.Add(position, ((CrewMemberSkill[])Enum.GetValues(typeof(CrewMemberSkill)))
					.Where(skill => RequiredSkillsCache[position].RequiresSkills(skill)).ToArray());
			}
		}

		private static CrewMemberSkill GetRequiredSkills(this Position position)
		{
			var fieldInfo = position.GetType().GetField(position.ToString());

			var attributes = (RequiredSkillsAttribute[])fieldInfo.GetCustomAttributes(typeof(RequiredSkillsAttribute), false);

			return attributes.Any() ? attributes.First().RequiredSkills : 0;

		}

		/// <summary>
		/// Get if a Skill is required for the given Position
		/// </summary>
		public static bool RequiresSkill(this Position position, CrewMemberSkill skill)
		{
			return RequiredSkillsCache[position].RequiresSkills(skill);
		}

		/// <summary>
		/// Get a list of RequiredSkills for the given Position
		/// </summary>
		public static IEnumerable<CrewMemberSkill> RequiredSkills(this Position position)
		{
			return EnumerableRequiredSkillsCache[position];
		}

		/// <summary>
		/// Get the average skill rating for this CrewMember in this Position
		/// </summary>
		public static int GetPositionRating(this Position position, CrewMember crewMember)
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
		public CrewMemberSkill RequiredSkills { get; }

		public RequiredSkillsAttribute(CrewMemberSkill requiredSkills)
		{
			RequiredSkills = requiredSkills;
		}
	}
}

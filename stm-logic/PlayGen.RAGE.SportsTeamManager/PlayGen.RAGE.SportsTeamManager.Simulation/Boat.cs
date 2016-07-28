﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Boat
	{
		public string Name { get; set; }
		public List<BoatPosition> BoatPositions { get; set; } = new List<BoatPosition>();
		public List<CrewMember> UnassignedCrew { get; set; } = new List<CrewMember>();
		public int BoatScore { get; set; }
		public Person Manager { get; set; }

		public void AddCrew(CrewMember crewMember)
		{
			var current = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (current != null)
			{
				return;
			}
			UnassignedCrew.Add(crewMember);
			UpdateBoatScore();
		}

		public void AssignCrew(BoatPosition boatPosition, CrewMember crewMember)
		{
			var current = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (current != null)
			{
				RemoveCrew(current);
			}
			if (boatPosition != null)
			{
				if (UnassignedCrew.Contains(crewMember))
				{
					UnassignedCrew.Remove(crewMember);
				}
				if (boatPosition.CrewMember != null)
				{
					RemoveCrew(boatPosition);
				}
				boatPosition.CrewMember = crewMember;
				crewMember.OpinionChange += new EventHandler(OnOpinionChange);
			}
			DataLoader.UpdateCrew(crewMember, boatPosition.Position.Name);
			UpdateBoatScore();
		}

		public void RemoveCrew(BoatPosition boatPosition)
		{
			boatPosition.CrewMember.OpinionChange -= new EventHandler(OnOpinionChange);
			UnassignedCrew.Add(boatPosition.CrewMember);
			DataLoader.UpdateCrew(boatPosition.CrewMember, "null");
			boatPosition.CrewMember = null;
		}

		void OnOpinionChange(object sender, EventArgs e)
		{
			DataLoader.UpdateCrew(sender as CrewMember);
			UpdateBoatScore();
		}

		public void UpdateBoatScore()
		{
			foreach (BoatPosition bp in BoatPositions)
			{
				UpdateCrewMemberScore(bp);
			}
			BoatScore = BoatPositions.Sum(bp => bp.PositionScore);
		}

		public void UpdateCrewMemberScore(BoatPosition boatPosition)
		{
			if (boatPosition.CrewMember == null || boatPosition.Position == null)
			{
				boatPosition.PositionScore = 0;
				return;
			}
			int positionCount = 0;
			int crewScore = 0;
			if (boatPosition.Position.RequiresBody)
			{
				crewScore += boatPosition.CrewMember.Body;
				positionCount++;
			}
			if (boatPosition.Position.RequiresCharisma)
			{
				crewScore += boatPosition.CrewMember.Charisma;
				positionCount++;
			}
			if (boatPosition.Position.RequiresPerception)
			{
				crewScore += boatPosition.CrewMember.Perception;
				positionCount++;
			}
			if (boatPosition.Position.RequiresQuickness)
			{
				crewScore += boatPosition.CrewMember.Quickness;
				positionCount++;
			}
			if (boatPosition.Position.RequiresWillpower)
			{
				crewScore += boatPosition.CrewMember.Willpower;
				positionCount++;
			}
			if (boatPosition.Position.RequiresWisdom)
			{
				crewScore += boatPosition.CrewMember.Wisdom;
				positionCount++;
			}

			crewScore = crewScore / positionCount;

			int opinion = 0;
			int opinionCount = 0;
			int managerOpinion = 0;
			if (boatPosition.CrewMember.CrewOpinions != null && boatPosition.CrewMember.CrewOpinions.Count > 0)
			{
				foreach (BoatPosition bp in BoatPositions)
				{
					if (bp != boatPosition && bp.CrewMember != null)
					{
						var crewMember = boatPosition.CrewMember.CrewOpinions.SingleOrDefault(op => op.Person == bp.CrewMember);
						if (crewMember != null)
						{
							opinion += crewMember.Opinion;
						}
						opinionCount++;
					}
				}
				var manager = boatPosition.CrewMember.CrewOpinions.SingleOrDefault(op => op.Person == Manager);
				if (manager != null)
				{
					managerOpinion += manager.Opinion;
				}
			}

			if (opinionCount > 0)
			{
				opinion = opinion / opinionCount;
			}
			crewScore += opinion;

			crewScore += managerOpinion;

			crewScore += DataLoader.GetMood(boatPosition.CrewMember);

			boatPosition.PositionScore = crewScore;
		}
	}
}
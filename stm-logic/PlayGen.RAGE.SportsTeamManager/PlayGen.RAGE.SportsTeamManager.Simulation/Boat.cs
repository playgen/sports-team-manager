﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GAIPS.Rage;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Boat
	{
		public string Name { get; set; }
		public List<BoatPosition> BoatPositions { get; set; } = new List<BoatPosition>();
		public List<CrewMember> UnassignedCrew { get; set; } = new List<CrewMember>();
		public int BoatScore { get; set; }
		public Person Manager { get; set; }

		public List<CrewMember> GetAllCrewMembers()
		{
			List<CrewMember> crew = new List<CrewMember>();
			foreach (CrewMember crewMember in UnassignedCrew)
			{
				crew.Add(crewMember);
			}
			foreach (BoatPosition boatPosition in BoatPositions)
			{
				if (boatPosition.CrewMember != null)
				{
					crew.Add(boatPosition.CrewMember);
				}
			}
			crew = crew.OrderBy(c => c.Name).ToList();
			return crew;
		}

		public void AddCrew(CrewMember crewMember)
		{
			var currentPosition = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			var current = currentPosition == null ? null : currentPosition.CrewMember;
			if (current != null)
			{
				return;
			}
			current = UnassignedCrew.SingleOrDefault(c => c == crewMember);
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
			crewMember.UpdateBeliefs(boatPosition.Position.Name);
			UpdateBoatScore();
		}

		public void RemoveCrew(BoatPosition boatPosition)
		{
			boatPosition.CrewMember.OpinionChange -= new EventHandler(OnOpinionChange);
			UnassignedCrew.Add(boatPosition.CrewMember);
			boatPosition.CrewMember.UpdateBeliefs("null");
			boatPosition.CrewMember = null;
		}

		void OnOpinionChange(object sender, EventArgs e)
		{
			UpdateBoatScore();
		}

		public void UpdateBoatScore()
		{
			foreach (BoatPosition bp in BoatPositions)
			{
				bp.UpdateCrewMemberScore(this);
			}
			BoatScore = BoatPositions.Sum(bp => bp.PositionScore);
		}

		public void ConfirmChanges()
		{
			List<CrewMember> crew = GetAllCrewMembers();
			crew = crew.OrderBy(p => p.Name).ToList();
			crew.ForEach(p => p.DecisionFeedback(this));
			Manager.SaveStatus();
			UpdateBoatScore();
		}
	}
}
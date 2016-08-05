using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Boat
	{
		public string Name { get; set; }
		public List<BoatPosition> BoatPositions { get; set; }
		public List<CrewMember> UnassignedCrew { get; set; }
		public List<CrewMember> RetiredCrew { get; set; }
		public int BoatScore { get; set; }
		public Person Manager { get; set; }

		public Boat()
		{
			BoatPositions = new List<BoatPosition>();
			UnassignedCrew = new List<CrewMember>();
			RetiredCrew = new List<CrewMember>();
		}

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

		public List<CrewMember> GetAllCrewMembersIncludingRetired()
		{
			List<CrewMember> crew = GetAllCrewMembers();
			foreach (CrewMember crewMember in RetiredCrew)
			{
				crew.Add(crewMember);
			}
			crew = crew.OrderBy(c => c.Name).ToList();
			return crew;
		}

		public void AddCrew(CrewMember crewMember)
		{
			var currentPosition = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (currentPosition != null)
			{
				return;
			}
			var current = UnassignedCrew.SingleOrDefault(c => c == crewMember);
			if (current != null)
			{
				return;
			}
			UnassignedCrew.Add(crewMember);
			UpdateBoatScore();
		}

		public void AssignCrew(BoatPosition boatPosition, CrewMember crewMember)
		{
			if (crewMember != null)
			{
				var current = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
				if (current != null)
				{
					RemoveCrew(current);
				}
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
				if (boatPosition.CrewMember != null)
				{
					crewMember.OpinionChange += new EventHandler(OnOpinionChange);
				}
			}
			if (crewMember != null && boatPosition != null)
			{
				crewMember.UpdateBeliefs(boatPosition.Position.Name);
			}
			UpdateBoatScore();
		}

		public void RemoveCrew(BoatPosition boatPosition)
		{
			boatPosition.CrewMember.OpinionChange -= new EventHandler(OnOpinionChange);
			UnassignedCrew.Add(boatPosition.CrewMember);
			boatPosition.CrewMember.UpdateBeliefs("null");
			boatPosition.CrewMember = null;
		}

		public void RemoveAllCrew()
		{
			foreach (BoatPosition boatPosition in BoatPositions)
			{
				if (boatPosition.CrewMember != null)
				{
					RemoveCrew(boatPosition);
				}
			}
		}

		public void RetireCrew(CrewMember crewMember)
		{
			var currentPosition = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (currentPosition != null)
			{
				RemoveCrew(currentPosition);
			}
			var current = UnassignedCrew.SingleOrDefault(c => c == crewMember);
			if (current != null)
			{
				UnassignedCrew.Remove(crewMember);
			}
			RetiredCrew.Add(crewMember);
			crewMember.Retire();
			UpdateBoatScore();
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
﻿using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlayGen.RAGE.SportsTeamManager.UnitTest
{
	[TestClass]
	public class UnitTest
	{
		[TestMethod]
		public void PerfectBoat()
		{
			List<CrewMember> crew = CreateCrew();
			Boat boat = SetUpBoat();
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(20, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(30, boat.BoatScore);
		}

		[TestMethod]
		public void BadBoat()
		{
			List<CrewMember> crew = CreateCrew();
			Boat boat = SetUpBoat();
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Nick Pony"));
			Assert.AreEqual(4, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Rav Age"));
			Assert.AreEqual(9, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(13, boat.BoatScore);
		}

		[TestMethod]
		public void OnePersonBoat()
		{
			List<CrewMember> crew = CreateCrew();
			Boat boat = SetUpBoat();
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Rav Age"));
			Assert.AreEqual(5, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Rav Age"));
			Assert.AreEqual(5, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Rav Age"));
			Assert.AreEqual(5, boat.BoatScore);
		}

		public List<CrewMember> CreateCrew()
		{
			List<CrewMember> crew = new List<CrewMember>();
			CrewMember member1 = new CrewMember
			{
				Name = "Skippy Skip",
				Body = 2,
				Charisma = 10,
				Perception = 2,
				Quickness = 2,
				Wisdom = 10,
				Willpower = 10
			};
			crew.Add(member1);
			CrewMember member2 = new CrewMember
			{
				Name = "Wise Nav",
				Body = 2,
				Charisma = 2,
				Perception = 10,
				Quickness = 2,
				Wisdom = 10,
				Willpower = 2
			};
			crew.Add(member2);
			CrewMember member3 = new CrewMember
			{
				Name = "Dim Wobnam",
				Body = 10,
				Charisma = 2,
				Perception = 2,
				Quickness = 10,
				Wisdom = 2,
				Willpower = 10
			};
			crew.Add(member3);
			CrewMember member4 = new CrewMember
			{
				Name = "Rav Age",
				Body = 5,
				Charisma = 5,
				Perception = 5,
				Quickness = 5,
				Wisdom = 5,
				Willpower = 5
			};
			crew.Add(member4);
			CrewMember member5 = new CrewMember
			{
				Name = "Nick Pony",
				Body = 7,
				Charisma = 7,
				Perception = 7,
				Quickness = 3,
				Wisdom = 3,
				Willpower = 3
			};
			crew.Add(member5);
			return crew;
		}

		public Boat SetUpBoat()
		{
			Position skipper = new Position
			{
				Name = "Skipper",
				RequiresBody = false,
				RequiresCharisma = true,
				RequiresPerception = false,
				RequiresQuickness = false,
				RequiresWillpower = true,
				RequiresWisdom = true
			};

			Position navigator = new Position
			{
				Name = "Navigator",
				RequiresBody = false,
				RequiresCharisma = false,
				RequiresPerception = true,
				RequiresQuickness = false,
				RequiresWillpower = false,
				RequiresWisdom = true
			};

			Position midbow = new Position
			{
				Name = "Mid-Bowman",
				RequiresBody = true,
				RequiresCharisma = false,
				RequiresPerception = false,
				RequiresQuickness = true,
				RequiresWillpower = true,
				RequiresWisdom = false
			};

			Boat boat = new Boat
			{
				Name = "Testy McTestFace",
				BoatPositions = new List<BoatPosition>()
				{
					new BoatPosition
					{
						Position = skipper,
					},
					new BoatPosition
					{
						Position = navigator,
					},
					new BoatPosition
					{
						Position = midbow,
					}
				}
			};

			return boat;
		}
	}
}

using System.Collections.Generic;
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

		[TestMethod]
		public void PerfectBoatWithFriendlyCrew()
		{
			List<CrewMember> crew = CreateCrew();
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 5
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = 5
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = 5
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = 5
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 5
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = 5
			});
			Boat boat = SetUpBoat();
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(30, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(45, boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithUnfriendlyCrew()
		{
			List<CrewMember> crew = CreateCrew();
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = -5
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = -5
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = -5
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = -5
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = -5
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = -5
			});
			Boat boat = SetUpBoat();
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(15, boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithMixedOpinionCrew()
		{
			List<CrewMember> crew = CreateCrew();
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 3
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = 2
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = -2
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = -4
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 1
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = 5
			});
			Boat boat = SetUpBoat();
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(21, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(32, boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithUnfriendlySkip()
		{
			List<CrewMember> crew = CreateCrew();
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = crew.Single(c => c.Name == "Wise Nav"),
				Opinion = -5
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = crew.Single(c => c.Name == "Dim Wobnam"),
				Opinion = -5
			});
			Boat boat = SetUpBoat();
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(15, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(25, boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithManagerOpinions()
		{
			List<CrewMember> crew = CreateCrew();
			Person manager = new CrewMember
			{
				Name = "Player Manager"
			};
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = 3
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = -5
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = -4
			});
			Boat boat = SetUpBoat();
			boat.Manager = manager;
			Assert.AreEqual(0, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(13, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(18, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(24, boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithManagerAndCrewOpinions()
		{
			List<CrewMember> crew = CreateCrew();
			Person manager = new CrewMember
			{
				Name = "Player Manager"
			};
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = 3
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 4
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = 1
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = -5
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = -3
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = -1
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = -2
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 3
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = 5
			});
			Boat boat = SetUpBoat();
			boat.Manager = manager;
			Assert.AreEqual(0, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(13, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(19, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(30, boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithOpinionsOnUnused()
		{
			List<CrewMember> crew = CreateCrew();
			Person manager = new CrewMember
			{
				Name = "Player Manager"
			};
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			var unused = crew.Single(c => c.Name == "Nick Pony");
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = 3
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 4
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = 1
			});
			skip.CrewOpinions.Add(new CrewOpinion
			{
				Person = unused,
				Opinion = 3
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = -5
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = -3
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = bow,
				Opinion = -1
			});
			nav.CrewOpinions.Add(new CrewOpinion
			{
				Person = unused,
				Opinion = 5
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = manager,
				Opinion = -2
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = nav,
				Opinion = 3
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = skip,
				Opinion = 5
			});
			bow.CrewOpinions.Add(new CrewOpinion
			{
				Person = unused,
				Opinion = -4
			});
			Boat boat = SetUpBoat();
			boat.Manager = manager;
			Assert.AreEqual(0, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(13, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(19, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(30, boat.BoatScore);
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
					},
				},
			};

			return boat;
		}
	}
}

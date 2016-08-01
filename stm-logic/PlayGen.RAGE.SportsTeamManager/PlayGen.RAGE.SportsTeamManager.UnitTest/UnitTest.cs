using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GAIPS.Rage;
using System.IO;

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
			crew.ForEach(c => boat.AddCrew(c));
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
			crew.ForEach(c => boat.AddCrew(c));
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
			crew.ForEach(c => boat.AddCrew(c));
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
			skip.AddOrUpdateOpinion(nav, 5);
			skip.AddOrUpdateOpinion(bow, 5);
			nav.AddOrUpdateOpinion(skip, 5);
			nav.AddOrUpdateOpinion(bow, 5);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 5);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
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
			skip.AddOrUpdateOpinion(nav, -5);
			skip.AddOrUpdateOpinion(bow, -5);
			nav.AddOrUpdateOpinion(skip, -5);
			nav.AddOrUpdateOpinion(bow, -5);
			bow.AddOrUpdateOpinion(skip, -5);
			bow.AddOrUpdateOpinion(nav, -5);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
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
			skip.AddOrUpdateOpinion(nav, 3);
			skip.AddOrUpdateOpinion(bow, 2);
			nav.AddOrUpdateOpinion(skip, -2);
			nav.AddOrUpdateOpinion(bow, -4);
			bow.AddOrUpdateOpinion(skip, 1);
			bow.AddOrUpdateOpinion(nav, 5);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
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
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(nav, -5);
			skip.AddOrUpdateOpinion(bow, -5);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
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
			Person manager = new Person
			{
				Name = "Player Manager"
			};
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(manager, 3);
			nav.AddOrUpdateOpinion(manager, -5);
			bow.AddOrUpdateOpinion(manager, -4);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
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
			Person manager = new Person
			{
				Name = "Player Manager"
			};
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(manager, 3);
			skip.AddOrUpdateOpinion(nav, 4);
			skip.AddOrUpdateOpinion(bow, 1);
			nav.AddOrUpdateOpinion(manager, -5);
			nav.AddOrUpdateOpinion(skip, -3);
			nav.AddOrUpdateOpinion(bow, -1);
			bow.AddOrUpdateOpinion(manager, -2);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 3);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
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
			Person manager = new Person
			{
				Name = "Player Manager"
			};
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			var unused = crew.Single(c => c.Name == "Nick Pony");
			skip.AddOrUpdateOpinion(manager, 3);
			skip.AddOrUpdateOpinion(nav, 4);
			skip.AddOrUpdateOpinion(bow, 1);
			skip.AddOrUpdateOpinion(unused, 3);
			nav.AddOrUpdateOpinion(manager, -5);
			nav.AddOrUpdateOpinion(skip, -3);
			nav.AddOrUpdateOpinion(bow, -1);
			nav.AddOrUpdateOpinion(unused, -5);
			bow.AddOrUpdateOpinion(manager, -2);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 3);
			bow.AddOrUpdateOpinion(unused, 4);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
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
		public void PerfectBoatWithOpinionUpdates()
		{
			List<CrewMember> crew = CreateCrew();
			Person manager = new Person
			{
				Name = "Player Manager"
			};
			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(manager, 3);
			skip.AddOrUpdateOpinion(nav, 4);
			skip.AddOrUpdateOpinion(bow, 1);
			nav.AddOrUpdateOpinion(manager, -5);
			nav.AddOrUpdateOpinion(skip, -3);
			nav.AddOrUpdateOpinion(bow, -1);
			bow.AddOrUpdateOpinion(manager, -2);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 3);
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
			boat.Manager = manager;
			Assert.AreEqual(0, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(13, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(19, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(30, boat.BoatScore);
			skip.AddOrUpdateOpinion(manager, 2);
			skip.AddOrUpdateOpinion(nav, 2);
			skip.AddOrUpdateOpinion(bow, 2);
			Assert.AreEqual(34, boat.BoatScore);
			nav.AddOrUpdateOpinion(manager, -1);
			nav.AddOrUpdateOpinion(skip, 2);
			nav.AddOrUpdateOpinion(bow, -3);
			Assert.AreEqual(34, boat.BoatScore);
			bow.AddOrUpdateOpinion(manager, 1);
			bow.AddOrUpdateOpinion(skip, 1);
			bow.AddOrUpdateOpinion(nav, -2);
			Assert.AreEqual(34, boat.BoatScore);
		}

		[TestMethod]
		public void CreateAndSaveNewBoat()
		{
			GameManager gameManager = new GameManager();
			List<CrewMember> crew = CreateCrew();
			Person manager = new Person
			{
				Name = "Player Manager",
				Age = 18,
				Gender = "Male"
			};
			Boat boat = SetUpBoat();
			crew.ForEach(c => boat.AddCrew(c));
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat, crew, manager);
		}

		[TestMethod]
		public void CreateAndLoadNewBoat()
		{
			GameManager gameManager = new GameManager();
			List<CrewMember> crew = CreateCrew();
			Person manager = new Person
			{
				Name = "Player Manager",
				Age = 18,
				Gender = "Male"
			};
			Boat boat = SetUpBoat();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat, crew, manager);

			Boat loadedBoat = gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat.Name);

			Assert.AreEqual(boat.Name, loadedBoat.Name);
			Assert.AreEqual(manager.Name, loadedBoat.Manager.Name);
			Assert.AreEqual(crew.Count, loadedBoat.UnassignedCrew.Count);
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoat()
		{
			GameManager gameManager = new GameManager();
			List<CrewMember> crew = CreateCrew();
			Person manager = new Person
			{
				Name = "Player Manager",
				Age = 18,
				Gender = "Male"
			};
			Boat boat = SetUpBoat();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat, crew, manager);

			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(20, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(30, boat.BoatScore);
			boat.ConfirmChanges();
			Assert.AreEqual(33, boat.BoatScore);
			//39 with mood

			Boat loadedBoat = gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat.Name);

			Assert.AreEqual(boat.Name, loadedBoat.Name);
			Assert.AreEqual(manager.Name, loadedBoat.Manager.Name);
			Assert.AreEqual(crew.Count - loadedBoat.BoatPositions.Count, loadedBoat.UnassignedCrew.Count);
			Assert.AreEqual(33, loadedBoat.BoatScore);
			//39 with mood
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithOpinions()
		{
			GameManager gameManager = new GameManager();
			List<CrewMember> crew = CreateCrew();
			Person manager = new Person
			{
				Name = "Player Manager",
				Age = 18,
				Gender = "Male"
			};

			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(manager, 5);
			skip.AddOrUpdateOpinion(nav, 1);
			skip.AddOrUpdateOpinion(bow, -3);
			nav.AddOrUpdateOpinion(manager, -3);
			nav.AddOrUpdateOpinion(skip, 1);
			nav.AddOrUpdateOpinion(bow, -1);
			bow.AddOrUpdateOpinion(manager, -5);
			bow.AddOrUpdateOpinion(skip, -3);
			bow.AddOrUpdateOpinion(nav, -5);

			Boat boat = SetUpBoat();

			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat, crew, manager);

			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(15, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(24, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(22, boat.BoatScore);
			boat.ConfirmChanges();
			Assert.AreEqual(24, boat.BoatScore);
			//30 with mood

			Boat loadedBoat = gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat.Name);

			Assert.AreEqual(boat.Name, loadedBoat.Name);
			Assert.AreEqual(manager.Name, loadedBoat.Manager.Name);
			Assert.AreEqual(crew.Count - loadedBoat.BoatPositions.Count, loadedBoat.UnassignedCrew.Count);
			Assert.AreEqual(24, loadedBoat.BoatScore);
			//30 with mood
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithUpdatingOpinions()
		{
			GameManager gameManager = new GameManager();
			List<CrewMember> crew = CreateCrew();
			Person manager = new Person
			{
				Name = "Player Manager",
				Age = 18,
				Gender = "Male"
			};

			var skip = crew.Single(c => c.Name == "Skippy Skip");
			var nav = crew.Single(c => c.Name == "Wise Nav");
			var bow = crew.Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(manager, 5);
			skip.AddOrUpdateOpinion(nav, 1);
			skip.AddOrUpdateOpinion(bow, -3);
			nav.AddOrUpdateOpinion(manager, -3);
			nav.AddOrUpdateOpinion(skip, 1);
			nav.AddOrUpdateOpinion(bow, -1);
			bow.AddOrUpdateOpinion(manager, -5);
			bow.AddOrUpdateOpinion(skip, -3);
			bow.AddOrUpdateOpinion(nav, -5);

			Boat boat = SetUpBoat();

			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat, crew, manager);

			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(15, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Wise Nav"));
			Assert.AreEqual(24, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Dim Wobnam"));
			Assert.AreEqual(22, boat.BoatScore);
			boat.ConfirmChanges();
			Assert.AreEqual(24, boat.BoatScore);
			//30 with mood

			Boat loadedBoat = gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat.Name);

			Assert.AreEqual(boat.Name, loadedBoat.Name);
			Assert.AreEqual(manager.Name, loadedBoat.Manager.Name);
			Assert.AreEqual(crew.Count - loadedBoat.BoatPositions.Count, loadedBoat.UnassignedCrew.Count);
			Assert.AreEqual(24, loadedBoat.BoatScore);
			//30 with mood

			skip = loadedBoat.BoatPositions.Single(c => c.CrewMember.Name == "Skippy Skip").CrewMember;
			nav = loadedBoat.BoatPositions.Single(c => c.CrewMember.Name == "Wise Nav").CrewMember;
			bow = loadedBoat.BoatPositions.Single(c => c.CrewMember.Name == "Dim Wobnam").CrewMember;

			skip.AddOrUpdateOpinion(loadedBoat.Manager, 2);
			skip.AddOrUpdateOpinion(nav, 2);
			skip.AddOrUpdateOpinion(bow, 2);
			Assert.AreEqual(26, loadedBoat.BoatScore);
			//32 with mood
			nav.AddOrUpdateOpinion(loadedBoat.Manager, -1);
			nav.AddOrUpdateOpinion(skip, 2);
			nav.AddOrUpdateOpinion(bow, -3);
			Assert.AreEqual(25, loadedBoat.BoatScore);
			//31 with mood
			bow.AddOrUpdateOpinion(loadedBoat.Manager, 1);
			bow.AddOrUpdateOpinion(skip, 1);
			bow.AddOrUpdateOpinion(nav, -2);
			Assert.AreEqual(27, loadedBoat.BoatScore);
			//33 with mood
			loadedBoat.ConfirmChanges();
			Assert.AreEqual(29, loadedBoat.BoatScore);
			//41 with mood

			Boat updatedBoat = gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat.Name);

			Assert.AreEqual(29, updatedBoat.BoatScore);
			//41 with mood
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithIncorrectPositions()
		{
			GameManager gameManager = new GameManager();
			List<CrewMember> crew = CreateCrew();
			Person manager = new Person
			{
				Name = "Player Manager",
				Age = 18,
				Gender = "Male"
			};

			var skip = crew.Single(c => c.Name == "Nick Pony");
			var nav = crew.Single(c => c.Name == "Rav Age");
			var bow = crew.Single(c => c.Name == "Skippy Skip");
			bow.AddOrUpdateOpinion(skip, -3);
			bow.AddOrUpdateOpinion(nav, -3);

			Boat boat = SetUpBoat();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat, crew, manager);

			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Skipper"), crew.Single(c => c.Name == "Nick Pony"));
			Assert.AreEqual(4, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Navigator"), crew.Single(c => c.Name == "Rav Age"));
			Assert.AreEqual(9, boat.BoatScore);
			boat.AssignCrew(boat.BoatPositions.Single(bp => bp.Position.Name == "Mid-Bowman"), crew.Single(c => c.Name == "Skippy Skip"));
			Assert.AreEqual(10, boat.BoatScore);
			boat.ConfirmChanges();
			Assert.AreEqual(7, boat.BoatScore);
			//5 with mood


			Boat loadedBoat = gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), boat.Name);

			Assert.AreEqual(boat.Name, loadedBoat.Name);
			Assert.AreEqual(manager.Name, loadedBoat.Manager.Name);
			Assert.AreEqual(crew.Count - loadedBoat.BoatPositions.Count, loadedBoat.UnassignedCrew.Count);
			Assert.AreEqual(7, loadedBoat.BoatScore);
			//5 with mood
		}

		public List<CrewMember> CreateCrew()
		{
			List<CrewMember> crew = new List<CrewMember>();
			CrewMember member1 = new CrewMember
			{
				Name = "Skippy Skip",
				Age = 35,
				Gender = "Male",
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
				Age = 28,
				Gender = "Male",
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
				Age = 19,
				Gender = "Male",
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
				Age = 25,
				Gender = "Male",
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
				Age = 32,
				Gender = "Male",
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
			Dinghy boat = new Dinghy()
			{
				Name = "Testy McTestFace",
			};

			return boat;
		}
	}
}

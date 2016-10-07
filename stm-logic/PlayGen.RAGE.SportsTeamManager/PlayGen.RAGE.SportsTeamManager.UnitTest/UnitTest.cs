using System.Collections.Generic;
using System.Linq;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace PlayGen.RAGE.SportsTeamManager.UnitTest
{
	[TestClass]
	public class UnitTest
	{
		private int _testCount = 25;

		[TestMethod]
		public void PerfectBoat()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(20, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void BadBoat()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Nick Pony"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(4, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Rav Age"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(9, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void OnePersonBoat()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Rav Age"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(5, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Rav Age"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(5, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Rav Age"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(5, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithFriendlyCrew()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			skip.AddOrUpdateOpinion(nav.Name, 5);
			skip.AddOrUpdateOpinion(bow.Name, 5);
			nav.AddOrUpdateOpinion(skip.Name, 5);
			nav.AddOrUpdateOpinion(bow.Name, 5);
			bow.AddOrUpdateOpinion(skip.Name, 5);
			bow.AddOrUpdateOpinion(nav.Name, 5);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(45, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithUnfriendlyCrew()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			skip.AddOrUpdateOpinion(nav.Name, -5);
			skip.AddOrUpdateOpinion(bow.Name, -5);
			nav.AddOrUpdateOpinion(skip.Name, -5);
			nav.AddOrUpdateOpinion(bow.Name, -5);
			bow.AddOrUpdateOpinion(skip.Name, -5);
			bow.AddOrUpdateOpinion(nav.Name, -5);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(15, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithMixedOpinionCrew()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			skip.AddOrUpdateOpinion(nav.Name, 3);
			skip.AddOrUpdateOpinion(bow.Name, 2);
			nav.AddOrUpdateOpinion(skip.Name, -2);
			nav.AddOrUpdateOpinion(bow.Name, -4);
			bow.AddOrUpdateOpinion(skip.Name, 1);
			bow.AddOrUpdateOpinion(nav.Name, 5);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(21, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(32, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithUnfriendlySkip()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			skip.AddOrUpdateOpinion(nav.Name, -5);
			skip.AddOrUpdateOpinion(bow.Name, -5);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(15, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(25, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithManagerOpinions()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 3);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -5);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -4);
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(18, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(24, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithManagerAndCrewOpinions()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 3);
			skip.AddOrUpdateOpinion(nav.Name, 4);
			skip.AddOrUpdateOpinion(bow.Name, 1);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -5);
			nav.AddOrUpdateOpinion(skip.Name, -3);
			nav.AddOrUpdateOpinion(bow.Name, -1);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -2);
			bow.AddOrUpdateOpinion(skip.Name, 5);
			bow.AddOrUpdateOpinion(nav.Name, 3);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(19, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithOpinionsOnUnused()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			var unused = gameManager.Boat.GetAllCrewMembers()["Nick Pony"];
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 3);
			skip.AddOrUpdateOpinion(nav.Name, 4);
			skip.AddOrUpdateOpinion(bow.Name, 1);
			skip.AddOrUpdateOpinion(unused.Name, 3);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -5);
			nav.AddOrUpdateOpinion(skip.Name, -3);
			nav.AddOrUpdateOpinion(bow.Name, -1);
			nav.AddOrUpdateOpinion(unused.Name, -5);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -2);
			bow.AddOrUpdateOpinion(skip.Name, 5);
			bow.AddOrUpdateOpinion(nav.Name, 3);
			bow.AddOrUpdateOpinion(unused.Name, 4);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(19, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithOpinionUpdates()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
			var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
			var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 3);
			skip.AddOrUpdateOpinion(nav.Name, 4);
			skip.AddOrUpdateOpinion(bow.Name, 1);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -5);
			nav.AddOrUpdateOpinion(skip.Name, -3);
			nav.AddOrUpdateOpinion(bow.Name, -1);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -2);
			bow.AddOrUpdateOpinion(skip.Name, 5);
			bow.AddOrUpdateOpinion(nav.Name, 3);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(19, gameManager.Boat.BoatScore);
			gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 2);
			skip.AddOrUpdateOpinion(nav.Name, 2);
			skip.AddOrUpdateOpinion(bow.Name, 2);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(34, gameManager.Boat.BoatScore);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -1);
			nav.AddOrUpdateOpinion(skip.Name, 2);
			nav.AddOrUpdateOpinion(bow.Name, -3);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(34, gameManager.Boat.BoatScore);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 1);
			bow.AddOrUpdateOpinion(skip.Name, 1);
			bow.AddOrUpdateOpinion(nav.Name, -2);
			gameManager.Boat.UpdateBoatScore();
			Assert.AreEqual(34, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void CreateAndSaveNewBoat()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
		}

		[TestMethod]
		public void CreateAndLoadNewBoat()
		{
			var config = new ConfigStore();
			var crew = CreateInitialCrew(config);
			var gameManager = new GameManager();
			gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);
			gameManager.LoadGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

			Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
			Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
			Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count, gameManager.Boat.UnassignedCrew.Count);
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoat()
		{
			for (var i = 0; i < _testCount; i++)
			{
				var config = new ConfigStore();
				var crew = CreateInitialCrew(config);
				var gameManager = new GameManager();
				gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);

				gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(10, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(20, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(30, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(30, gameManager.Boat.BoatScore);
				//Assert.AreEqual(33, gameManager.Boat.BoatScore); opinion changes

				gameManager.LoadGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				//Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(30, gameManager.Boat.BoatScore);
				//Assert.AreEqual(33, gameManager.Boat.BoatScore); opinion changes
			}
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithOpinions()
		{
			for (var i = 0; i < _testCount; i++)
			{
				var config = new ConfigStore();
				var crew = CreateInitialCrew(config);
				var gameManager = new GameManager();
				gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);

				var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
				var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
				var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
				skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 5);
				skip.AddOrUpdateOpinion(nav.Name, 1);
				skip.AddOrUpdateOpinion(bow.Name, -3);
				nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -3);
				nav.AddOrUpdateOpinion(skip.Name, 1);
				nav.AddOrUpdateOpinion(bow.Name, -1);
				bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -5);
				bow.AddOrUpdateOpinion(skip.Name, -3);
				bow.AddOrUpdateOpinion(nav.Name, -5);

				gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(15, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(24, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				//Assert.AreEqual(24, gameManager.Boat.BoatScore); opinion changes

				gameManager.LoadGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				//Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				//Assert.AreEqual(24, gameManager.Boat.BoatScore); opinion changes
			}
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithUpdatingOpinions()
		{
			for (var i = 0; i < _testCount; i++)
			{
				var config = new ConfigStore();
				var crew = CreateInitialCrew(config);
				var gameManager = new GameManager();
				gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);

				var skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
				var nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
				var bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];
				skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 5);
				skip.AddOrUpdateOpinion(nav.Name, 1);
				skip.AddOrUpdateOpinion(bow.Name, -3);
				nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -3);
				nav.AddOrUpdateOpinion(skip.Name, 1);
				nav.AddOrUpdateOpinion(bow.Name, -1);
				bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -5);
				bow.AddOrUpdateOpinion(skip.Name, -3);
				bow.AddOrUpdateOpinion(nav.Name, -5);

				gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(15, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Wise Nav"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(24, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				//Assert.AreEqual(24, gameManager.Boat.BoatScore); opinion changes

				gameManager.LoadGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				//Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				//Assert.AreEqual(24, gameManager.Boat.BoatScore); opinion changes

				skip = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
				nav = gameManager.Boat.GetAllCrewMembers()["Wise Nav"];
				bow = gameManager.Boat.GetAllCrewMembers()["Dim Wobnam"];

				skip.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 2);
				skip.AddOrUpdateOpinion(nav.Name, 2);
				skip.AddOrUpdateOpinion(bow.Name, 2);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(24, gameManager.Boat.BoatScore);
				//Assert.AreEqual(26, gameManager.Boat.BoatScore); opinion changes
				nav.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, -1);
				nav.AddOrUpdateOpinion(skip.Name, 2);
				nav.AddOrUpdateOpinion(bow.Name, -3);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(23, gameManager.Boat.BoatScore);
				//Assert.AreEqual(25, gameManager.Boat.BoatScore); opinion changes
				bow.AddOrUpdateOpinion(gameManager.Boat.Manager.Name, 1);
				bow.AddOrUpdateOpinion(skip.Name, 1);
				bow.AddOrUpdateOpinion(nav.Name, -2);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(25, gameManager.Boat.BoatScore);
				//Assert.AreEqual(27, gameManager.Boat.BoatScore); opinion changes
				gameManager.ConfirmLineUp();
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(25, gameManager.Boat.BoatScore);
				//Assert.AreEqual(25, gameManager.Boat.BoatScore); promotion
				//Assert.AreEqual(29, gameManager.Boat.BoatScore); opinion changes

				gameManager.LoadGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(25, gameManager.Boat.BoatScore);
				//Assert.AreEqual(25, gameManager.Boat.BoatScore); promotion
				//Assert.AreEqual(29, gameManager.Boat.BoatScore); opinion changes
			}
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithIncorrectPositions()
		{
			for (var i = 0; i < _testCount; i++)
			{
				var config = new ConfigStore();
				var crew = CreateInitialCrew(config);
				var gameManager = new GameManager();
				gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", new byte[] {0, 0, 0}, new byte[] {0, 0, 0}, "Player Manager", "18", "Male", crew);

				var skip = gameManager.Boat.GetAllCrewMembers()["Nick Pony"];
				var nav = gameManager.Boat.GetAllCrewMembers()["Rav Age"];
				var bow = gameManager.Boat.GetAllCrewMembers()["Skippy Skip"];
				bow.AddOrUpdateOpinion(skip.Name, -3);
				bow.AddOrUpdateOpinion(nav.Name, -3);

				gameManager.AssignCrew(Position.Skipper, gameManager.Boat.GetAllCrewMembers()["Nick Pony"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(4, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.Navigator, gameManager.Boat.GetAllCrewMembers()["Rav Age"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(9, gameManager.Boat.BoatScore);
				gameManager.AssignCrew(Position.MidBowman, gameManager.Boat.GetAllCrewMembers()["Skippy Skip"]);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(10, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(10, gameManager.Boat.BoatScore);
				//Assert.AreEqual(4, gameManager.Boat.BoatScore); opinion changes

				gameManager.LoadGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				//Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				gameManager.Boat.UpdateBoatScore();
				Assert.AreEqual(10, gameManager.Boat.BoatScore);
				//Assert.AreEqual(4, gameManager.Boat.BoatScore); opinion changes
			}
		}

		[TestMethod]
		public void IdealSpeedTest()
		{
			for (var i = 0; i < _testCount; i++)
			{
				var gameManager = new GameManager();
				gameManager.NewGame(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Ideal Test", new byte[] { 0, 0, 0 }, new byte[] { 0, 0, 0 }, "Player Manager", "18", "Male");
				gameManager.PromoteBoat();
				gameManager.PromoteBoat();
				gameManager.PromoteBoat();
				gameManager.AddRecruit(gameManager.Boat.Recruits.First().Value);
				gameManager.AddRecruit(gameManager.Boat.Recruits.First().Value);
				gameManager.SaveLineUp(0);
			}
		}

		public List<CrewMember> CreateInitialCrew(ConfigStore config)
		{
			CrewMember[] crew = {
			new CrewMember (config)
			{
				Name = "Skippy Skip",
				Age = 35,
				Gender = "Male",
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, 2},
					{CrewMemberSkill.Charisma, 10},
					{CrewMemberSkill.Perception, 2},
					{CrewMemberSkill.Quickness, 2},
					{CrewMemberSkill.Wisdom, 10},
					{CrewMemberSkill.Willpower, 10}
				}
			},
			new CrewMember (config)
			{
				Name = "Wise Nav",
				Age = 28,
				Gender = "Male",
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, 2},
					{CrewMemberSkill.Charisma, 2},
					{CrewMemberSkill.Perception, 10},
					{CrewMemberSkill.Quickness, 2},
					{CrewMemberSkill.Wisdom, 10},
					{CrewMemberSkill.Willpower, 2}
				}
			},
			new CrewMember (config)
			{
				Name = "Dim Wobnam",
				Age = 19,
				Gender = "Male",
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, 10},
					{CrewMemberSkill.Charisma, 2},
					{CrewMemberSkill.Perception, 2},
					{CrewMemberSkill.Quickness, 10},
					{CrewMemberSkill.Wisdom, 2},
					{CrewMemberSkill.Willpower, 10}
				}
			},
			new CrewMember (config)
			{
				Name = "Rav Age",
				Age = 25,
				Gender = "Male",
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, 5},
					{CrewMemberSkill.Charisma, 5},
					{CrewMemberSkill.Perception, 5},
					{CrewMemberSkill.Quickness, 5},
					{CrewMemberSkill.Wisdom, 5},
					{CrewMemberSkill.Willpower, 5}
				}
			},
			new CrewMember (config)
			{
				Name = "Nick Pony",
				Age = 32,
				Gender = "Male",
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, 7},
					{CrewMemberSkill.Charisma, 7},
					{CrewMemberSkill.Perception, 7},
					{CrewMemberSkill.Quickness, 3},
					{CrewMemberSkill.Wisdom, 3},
					{CrewMemberSkill.Willpower, 3}
				}
			}
			};
			return crew.ToList();
		}
	}
}

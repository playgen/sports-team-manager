﻿using System.Collections.Generic;
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
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(20, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void BadBoat()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			gameManager.AssignCrew("Skipper", "Nick Pony");
			Assert.AreEqual(4, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Rav Age");
			Assert.AreEqual(9, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Skippy Skip");
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void OnePersonBoat()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			gameManager.AssignCrew("Skipper", "Rav Age");
			Assert.AreEqual(5, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Rav Age");
			Assert.AreEqual(5, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Rav Age");
			Assert.AreEqual(5, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithFriendlyCrew()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(nav, 5);
			skip.AddOrUpdateOpinion(bow, 5);
			nav.AddOrUpdateOpinion(skip, 5);
			nav.AddOrUpdateOpinion(bow, 5);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 5);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(45, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithUnfriendlyCrew()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(nav, -5);
			skip.AddOrUpdateOpinion(bow, -5);
			nav.AddOrUpdateOpinion(skip, -5);
			nav.AddOrUpdateOpinion(bow, -5);
			bow.AddOrUpdateOpinion(skip, -5);
			bow.AddOrUpdateOpinion(nav, -5);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(15, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithMixedOpinionCrew()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(nav, 3);
			skip.AddOrUpdateOpinion(bow, 2);
			nav.AddOrUpdateOpinion(skip, -2);
			nav.AddOrUpdateOpinion(bow, -4);
			bow.AddOrUpdateOpinion(skip, 1);
			bow.AddOrUpdateOpinion(nav, 5);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(21, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(32, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithUnfriendlySkip()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(nav, -5);
			skip.AddOrUpdateOpinion(bow, -5);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(10, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(15, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(25, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithManagerOpinions()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 3);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -5);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager, -4);
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(18, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(24, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithManagerAndCrewOpinions()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 3);
			skip.AddOrUpdateOpinion(nav, 4);
			skip.AddOrUpdateOpinion(bow, 1);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -5);
			nav.AddOrUpdateOpinion(skip, -3);
			nav.AddOrUpdateOpinion(bow, -1);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager, -2);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 3);
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(19, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithOpinionsOnUnused()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			var unused = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Nick Pony");
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 3);
			skip.AddOrUpdateOpinion(nav, 4);
			skip.AddOrUpdateOpinion(bow, 1);
			skip.AddOrUpdateOpinion(unused, 3);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -5);
			nav.AddOrUpdateOpinion(skip, -3);
			nav.AddOrUpdateOpinion(bow, -1);
			nav.AddOrUpdateOpinion(unused, -5);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager, -2);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 3);
			bow.AddOrUpdateOpinion(unused, 4);
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(19, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void PerfectBoatWithOpinionUpdates()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
			var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
			var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 3);
			skip.AddOrUpdateOpinion(nav, 4);
			skip.AddOrUpdateOpinion(bow, 1);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -5);
			nav.AddOrUpdateOpinion(skip, -3);
			nav.AddOrUpdateOpinion(bow, -1);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager, -2);
			bow.AddOrUpdateOpinion(skip, 5);
			bow.AddOrUpdateOpinion(nav, 3);
			Assert.AreEqual(0, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Skipper", "Skippy Skip");
			Assert.AreEqual(13, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Navigator", "Wise Nav");
			Assert.AreEqual(19, gameManager.Boat.BoatScore);
			gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
			Assert.AreEqual(30, gameManager.Boat.BoatScore);
			skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 2);
			skip.AddOrUpdateOpinion(nav, 2);
			skip.AddOrUpdateOpinion(bow, 2);
			Assert.AreEqual(34, gameManager.Boat.BoatScore);
			nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -1);
			nav.AddOrUpdateOpinion(skip, 2);
			nav.AddOrUpdateOpinion(bow, -3);
			Assert.AreEqual(34, gameManager.Boat.BoatScore);
			bow.AddOrUpdateOpinion(gameManager.Boat.Manager, 1);
			bow.AddOrUpdateOpinion(skip, 1);
			bow.AddOrUpdateOpinion(nav, -2);
			Assert.AreEqual(34, gameManager.Boat.BoatScore);
		}

		[TestMethod]
		public void CreateAndSaveNewBoat()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
		}

		[TestMethod]
		public void CreateAndLoadNewBoat()
		{
			GameManager gameManager = new GameManager();
			gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");
			gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

			Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
			Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
			Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count, gameManager.Boat.UnassignedCrew.Count);
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoat()
		{
			for (int i = 0; i < 25; i++)
			{
				GameManager gameManager = new GameManager();
				gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");

				gameManager.AssignCrew("Skipper", "Skippy Skip");
				Assert.AreEqual(10, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Navigator", "Wise Nav");
				Assert.AreEqual(20, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
				Assert.AreEqual(30, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				Assert.AreEqual(39, gameManager.Boat.BoatScore);

				gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				Assert.AreEqual(39, gameManager.Boat.BoatScore);
			}
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithOpinions()
		{
			for (int i = 0; i < 25; i++)
			{
				GameManager gameManager = new GameManager();
				gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");

				var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
				var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
				var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
				skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 5);
				skip.AddOrUpdateOpinion(nav, 1);
				skip.AddOrUpdateOpinion(bow, -3);
				nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -3);
				nav.AddOrUpdateOpinion(skip, 1);
				nav.AddOrUpdateOpinion(bow, -1);
				bow.AddOrUpdateOpinion(gameManager.Boat.Manager, -5);
				bow.AddOrUpdateOpinion(skip, -3);
				bow.AddOrUpdateOpinion(nav, -5);

				gameManager.AssignCrew("Skipper", "Skippy Skip");
				Assert.AreEqual(15, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Navigator", "Wise Nav");
				Assert.AreEqual(24, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				Assert.AreEqual(33, gameManager.Boat.BoatScore);

				gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				Assert.AreEqual(33, gameManager.Boat.BoatScore);
			}
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithUpdatingOpinions()
		{
			for (int i = 0; i < 25; i++)
			{
				GameManager gameManager = new GameManager();
				gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");

				var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
				var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Wise Nav");
				var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Dim Wobnam");
				skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 5);
				skip.AddOrUpdateOpinion(nav, 1);
				skip.AddOrUpdateOpinion(bow, -3);
				nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -3);
				nav.AddOrUpdateOpinion(skip, 1);
				nav.AddOrUpdateOpinion(bow, -1);
				bow.AddOrUpdateOpinion(gameManager.Boat.Manager, -5);
				bow.AddOrUpdateOpinion(skip, -3);
				bow.AddOrUpdateOpinion(nav, -5);

				gameManager.AssignCrew("Skipper", "Skippy Skip");
				Assert.AreEqual(15, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Navigator", "Wise Nav");
				Assert.AreEqual(24, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
				Assert.AreEqual(22, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				Assert.AreEqual(33, gameManager.Boat.BoatScore);

				gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				Assert.AreEqual(33, gameManager.Boat.BoatScore);

				skip = gameManager.Boat.BoatPositions.Single(c => c.CrewMember.Name == "Skippy Skip").CrewMember;
				nav = gameManager.Boat.BoatPositions.Single(c => c.CrewMember.Name == "Wise Nav").CrewMember;
				bow = gameManager.Boat.BoatPositions.Single(c => c.CrewMember.Name == "Dim Wobnam").CrewMember;

				skip.AddOrUpdateOpinion(gameManager.Boat.Manager, 2);
				skip.AddOrUpdateOpinion(nav, 2);
				skip.AddOrUpdateOpinion(bow, 2);
				Assert.AreEqual(35, gameManager.Boat.BoatScore);
				nav.AddOrUpdateOpinion(gameManager.Boat.Manager, -1);
				nav.AddOrUpdateOpinion(skip, 2);
				nav.AddOrUpdateOpinion(bow, -3);
				Assert.AreEqual(34, gameManager.Boat.BoatScore);
				bow.AddOrUpdateOpinion(gameManager.Boat.Manager, 1);
				bow.AddOrUpdateOpinion(skip, 1);
				bow.AddOrUpdateOpinion(nav, -2);
				Assert.AreEqual(36, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				Assert.AreEqual(55, gameManager.Boat.BoatScore);

				gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual(55, gameManager.Boat.BoatScore);
			}
		}

		[TestMethod]
		public void CreateUpdateAndLoadBoatWithIncorrectPositions()
		{
			for (int i = 0; i < 25; i++)
			{
				GameManager gameManager = new GameManager();
				gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");

				var skip = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Nick Pony");
				var nav = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Rav Age");
				var bow = gameManager.Boat.GetAllCrewMembers().Single(c => c.Name == "Skippy Skip");
				bow.AddOrUpdateOpinion(skip, -3);
				bow.AddOrUpdateOpinion(nav, -3);

				gameManager.AssignCrew("Skipper", "Nick Pony");
				Assert.AreEqual(4, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Navigator", "Rav Age");
				Assert.AreEqual(9, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Mid-Bowman", "Skippy Skip");
				Assert.AreEqual(10, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				Assert.AreEqual(5, gameManager.Boat.BoatScore);

				gameManager.LoadGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace");

				Assert.AreEqual("Testy McTestFace", gameManager.Boat.Name);
				Assert.AreEqual("Player Manager", gameManager.Boat.Manager.Name);
				Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count - gameManager.Boat.BoatPositions.Count, gameManager.Boat.UnassignedCrew.Count);
				Assert.AreEqual(5, gameManager.Boat.BoatScore);
			}
		}

		[TestMethod]
		public void SendPreRaceEncouragement()
		{
			for (int i = 0; i < 25; i++)
			{
				GameManager gameManager = new GameManager();
				gameManager.NewGame(LocalStorageProvider.Instance, Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "Testing"), "Testy McTestFace", "Player Manager", "18", "Male");

				gameManager.AssignCrew("Skipper", "Skippy Skip");
				//Assert.AreEqual(10, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Navigator", "Wise Nav");
				//Assert.AreEqual(20, gameManager.Boat.BoatScore);
				gameManager.AssignCrew("Mid-Bowman", "Dim Wobnam");
				//Assert.AreEqual(30, gameManager.Boat.BoatScore);
				gameManager.ConfirmLineUp();
				//Assert.AreEqual(39, gameManager.Boat.BoatScore);

				var replies = gameManager.SendEvent(gameManager.EventController.PreRaceEvents.First());
				Assert.AreEqual(gameManager.Boat.GetAllCrewMembers().Count, replies.Count());
			}
		}
	}
}

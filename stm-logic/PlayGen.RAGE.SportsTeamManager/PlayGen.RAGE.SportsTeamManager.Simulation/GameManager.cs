﻿using EmotionalAppraisal;
using EmotionalAppraisal.DTOs;
using EmotionalDecisionMaking;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class GameManager
	{
		public Boat Boat { get; set; }
		public EventController EventController { get; set; }

		public void NewGame(IStorageProvider storagePorvider, string storageLocation, string boatName, string managerName, string managerAge, string managerGender)
		{
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(templateStorage, "template_iat");
			Boat = new Dinghy();
			Boat.Name = boatName;
			iat.ScenarioName = Boat.Name;
			List<CrewMember> crew = CreateInitialCrew();

			foreach (CrewMember member in crew)
			{
				member.CreateFile(iat, templateStorage, storagePorvider, storageLocation);
				Boat.AddCrew(member);
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}

			Person manager = new Person
			{
				Name = managerName,
				Age = int.Parse(managerAge),
				Gender = managerGender
			};

			manager.CreateFile(iat, templateStorage, storagePorvider, storageLocation);
			manager.UpdateBeliefs("Manager");
			manager.UpdateSingleBelief("Value(BoatType)", Boat.GetType().Name, "SELF");
			Boat.Manager = manager;
			manager.SaveStatus();

			var noSpaceBoatName = Boat.Name.Replace(" ", "");
			iat.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceBoatName + ".iat"));
			EventController = new EventController(iat);
		}

		public List<CrewMember> CreateInitialCrew()
		{
			CrewMember[] crew = {
			new CrewMember
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
			},
			new CrewMember
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
			},
			new CrewMember
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
			},
			new CrewMember
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
			},
			new CrewMember
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
			}
			};
			return crew.ToList();
		}

		public void LoadGame(IStorageProvider storagePorvider, string storageLocation, string boatName)
		{
			UnloadGame();
			Boat = new Boat();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, boatName.Replace(" ", "") + ".iat"));
			var rpcList = iat.GetAllCharacters();

			List<CrewMember> crewList = new List<CrewMember>();

			foreach (RolePlayCharacterAsset rpc in rpcList)
			{
				var tempea = EmotionalAppraisalAsset.LoadFromFile(storagePorvider, rpc.EmotionalAppraisalAssetSource);
				string position = tempea.GetBeliefValue("Value(Position)");
				if (position == "Manager")
				{
					Person person = new Person(storagePorvider, rpc);
					Boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + person.EmotionalAppraisal.GetBeliefValue("Value(BoatType)")));
					Boat.Name = iat.ScenarioName;
					Boat.Manager = person;
					continue;
				}
				CrewMember crewMember = new CrewMember(storagePorvider, rpc);
				crewList.Add(crewMember);
			}

			crewList.ForEach(cm => Boat.AddCrew(cm));
			crewList.ForEach(cm => cm.LoadBeliefs(Boat));
			EventController = new EventController(iat);
		}

		public void UnloadGame()
		{
			Boat = null;
		}

		public void AssignCrew(string positionName, string crewName)
		{
			BoatPosition position = Boat.BoatPositions.SingleOrDefault(p => p.Position.Name == positionName);
			CrewMember crewMember = Boat.GetAllCrewMembers().SingleOrDefault(c => c.Name == crewName);
			if (position != null && crewMember != null)
			{
				Boat.AssignCrew(position, crewMember);
			}
		}

		public void ConfirmLineUp()
		{
			Boat.ConfirmChanges();
		}

		public string[] SendEvent(DialogueStateActionDTO selected)
		{
			var replies = EventController.SelectEvent(selected, Boat);
			Boat.UpdateBoatScore();
			return replies.ToArray();
		}
	}
}

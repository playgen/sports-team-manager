using EmotionalAppraisal;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class GameManager
	{
		public Boat Boat { get; set; }
		public EventController EventController { get; set; }

		public List<Boat> LineUpHistory { get; set; }

		/// <summary>
		/// Create a new game
		/// </summary>
		public void NewGame(IStorageProvider storagePorvider, string storageLocation, string boatName, string managerName, string managerAge, string managerGender, List<CrewMember> crew = null)
		{
			var noSpaceBoatName = boatName.Replace(" ", "");
			storageLocation = Path.Combine(storageLocation, noSpaceBoatName);
			Directory.CreateDirectory(storageLocation);
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(templateStorage, "template_iat");
			Boat = new Dinghy();
			Boat.Name = boatName;
			iat.ScenarioName = Boat.Name;
			if (crew == null)
			{
				crew = CreateInitialCrew(managerName);
			}

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

			iat.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceBoatName + ".iat"));
			EventController = new EventController(iat);
			LineUpHistory = new List<Boat>();
		}

		/// <summary>
		/// Create the CrewMember for the start of every game
		/// </summary>
		public List<CrewMember> CreateInitialCrew(string managerName)
		{
			Random random = new Random();
			CrewMember[] crew = {
			new CrewMember (random)
			{
				Body = 2,
				Charisma = 10,
				Perception = 2,
				Quickness = 2,
				Wisdom = 10,
				Willpower = 10
			},
			new CrewMember (random)
			{
				Body = 2,
				Charisma = 2,
				Perception = 10,
				Quickness = 2,
				Wisdom = 10,
				Willpower = 2
			},
			new CrewMember (random)
			{
				Body = 10,
				Charisma = 2,
				Perception = 2,
				Quickness = 10,
				Wisdom = 2,
				Willpower = 10
			},
			new CrewMember (random)
			{
				Body = 5,
				Charisma = 5,
				Perception = 5,
				Quickness = 5,
				Wisdom = 5,
				Willpower = 5
			},
			new CrewMember (random)
			{
				Body = 7,
				Charisma = 7,
				Perception = 7,
				Quickness = 3,
				Wisdom = 3,
				Willpower = 3
			}
			};
			foreach (var crewMember in crew)
			{
				bool unqiue = false;
				while (!unqiue)
				{
					if (crew.Count(c => c.Name == crewMember.Name) > 1 || crewMember.Name == managerName)
					{
						crewMember.Name = crewMember.SelectNewName(crewMember.Gender, random);
					} else
					{
						unqiue = true;
					}
				}
			}
			return crew.ToList();
		}

		/// <summary>
		/// Get the ScenarioName from every .iat file stored in the directory provided
		/// </summary>
		public List<string> GetGameNames(string storageLocation)
		{
			var folders = Directory.GetDirectories(storageLocation);
			List<string> gameNames = new List<string>();
			foreach (var folder in folders)
			{
				var files = Directory.GetFiles(folder, "*.iat");
				foreach(var file in files)
				{
					var name = IntegratedAuthoringToolAsset.LoadFromFile(LocalStorageProvider.Instance, file).ScenarioName;
					gameNames.Add(name);
				}
			}
			return gameNames;
		}

		/// <summary>
		/// Check if the information provided contains an existing game
		/// </summary>
		public bool CheckIfGameExists(string storageLocation, string gameName)
		{
			return Directory.Exists(Path.Combine(storageLocation, gameName.Replace(" ", "")));
		}

		/// <summary>
		/// Load an existing game
		/// </summary>
		public void LoadGame(IStorageProvider storagePorvider, string storageLocation, string boatName)
		{
			UnloadGame();
			Boat = new Boat();
			storageLocation = Path.Combine(storageLocation, boatName.Replace(" ", ""));
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
			LoadLineUpHistory();
		}

		/// <summary>
		/// Unload the current game
		/// </summary>
		public void UnloadGame()
		{
			Boat = null;
		}

		/// <summary>
		/// Assign the CrewMember with the name provided to the Position with the name provided
		/// </summary>
		public void AssignCrew(string positionName, string crewName)
		{
			BoatPosition position = Boat.BoatPositions.SingleOrDefault(p => p.Position.Name == positionName);
			CrewMember crewMember = Boat.GetAllCrewMembers().SingleOrDefault(c => c.Name == crewName);
			Boat.AssignCrew(position, crewMember);
		}

		/// <summary>
		/// Remove all CrewMember from their Position
		/// </summary>
		public void RemoveAllCrew()
		{
			Boat.RemoveAllCrew();
		}

		/// <summary>
		/// Load the history of line-ups from the manager's EA file
		/// </summary>
		public void LoadLineUpHistory()
		{
			LineUpHistory = new List<Boat>();
			var managerEvents = Boat.Manager.EmotionalAppraisal.EventRecords;
			var lineUpEvents = managerEvents.Where(e => e.Event.Contains("SelectedLineUp")).Select(e => e.Event);
			foreach (var lineup in lineUpEvents)
			{
				var splitAfter = lineup.Split('(')[2];
				splitAfter = splitAfter.Split(')')[0];
				var subjectSplit = splitAfter.Split(',');
				Boat boat = new Boat();
				boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + subjectSplit[0]));
				for (int i = 0; i < boat.BoatPositions.Count; i++)
				{
					boat.BoatPositions[i].CrewMember = Boat.GetAllCrewMembersIncludingRetired().SingleOrDefault(c => c.Name.Replace(" ", "") == subjectSplit[((i + 1) * 2) - 1].Replace(" ", ""));
					boat.BoatPositions[i].PositionScore = int.Parse(subjectSplit[(i + 1) * 2]);
				}
				LineUpHistory.Add(boat);
			}
		}

		/// <summary>
		/// Save the current boat line-up to the manager's EA file
		/// </summary>
		public void SaveLineUp()
		{
			var manager = Boat.Manager;
			var spacelessName = manager.EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventStringUnformatted = "SelectedLineUp({0},{1})";
			var boatType = Boat.GetType().Name;
			var crew = "";
			foreach (var boatPosition in Boat.BoatPositions)
			{
				if (!string.IsNullOrEmpty(crew))
				{
					crew += ",";
				}
				if (boatPosition.CrewMember != null)
				{
					crew += boatPosition.CrewMember.Name.Replace(" ", "");
					crew += "," + boatPosition.PositionScore;
				} else
				{
					crew += "null,0";
				}
			}
			var eventString = String.Format(eventStringUnformatted, boatType, crew);
			manager.EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			manager.SaveStatus();
			LineUpHistory.Add(Boat);
		}

		/// <summary>
		/// Save current line-up and update Crewmember's opinions and mood based on this line-up
		/// </summary>
		public void ConfirmLineUp()
		{
			Boat.ConfirmChanges();
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all crew for this boat
		/// </summary>
		public string[] SendCrewEvent(DialogueStateActionDTO selected)
		{
			var replies = EventController.SelectEvent(selected, Boat.GetAllCrewMembers(), Boat);
			Boat.UpdateBoatScore();
			return replies.ToArray();
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all assigned crew on this boat
		/// </summary>
		public string[] SendAssignedCrewEvent(DialogueStateActionDTO selected)
		{
			List<CrewMember> currentMembers = new List<CrewMember>();
			foreach (BoatPosition bp in Boat.BoatPositions)
			{
				if (bp.CrewMember != null)
				{
					currentMembers.Add(bp.CrewMember);
				}
			}
			var replies = EventController.SelectEvent(selected, currentMembers, Boat);
			Boat.UpdateBoatScore();
			return replies.ToArray();
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all unassigned crew on this boat
		/// </summary>
		public string[] SendUnassignedCrewEvent(DialogueStateActionDTO selected)
		{
			var replies = EventController.SelectEvent(selected, Boat.UnassignedCrew, Boat);
			Boat.UpdateBoatScore();
			return replies.ToArray();
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all crew within the list
		/// </summary>
		public string[] SendBoatMembersEvent(DialogueStateActionDTO selected, List<CrewMember> members)
		{
			var replies = EventController.SelectEvent(selected, members, Boat);
			Boat.UpdateBoatScore();
			return replies.ToArray();
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all crew within the list
		/// </summary>
		public string[] SendRecruitMembersEvent(DialogueStateActionDTO selected, List<CrewMember> members)
		{
			var replies = EventController.SelectEvent(selected, members, Boat);
			Boat.UpdateBoatScore();
			return replies.ToArray();
		}
	}
}

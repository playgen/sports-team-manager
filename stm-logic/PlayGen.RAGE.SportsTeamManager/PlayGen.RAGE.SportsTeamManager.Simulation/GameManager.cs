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
	//Two starting crew members and random starting opinions currently commented out
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
			bool initialCrew = false;
			if (crew == null)
			{
				initialCrew = true;
				crew = CreateInitialCrew(managerName);
			}

			Random rand = new Random();

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

			foreach (CrewMember member in crew)
			{
				member.CreateFile(iat, templateStorage, storagePorvider, storageLocation);
				Boat.AddCrew(member);
				/*if (initialCrew)
				{
					foreach (CrewMember otherMember in crew)
					{
						if (member != otherMember)
						{
							member.AddOrUpdateOpinion(otherMember, rand.Next(-4, 5));
						}
						member.AddOrUpdateOpinion(Boat.Manager, rand.Next(-3, 4));
					}
				}*/
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}

			iat.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceBoatName + ".iat"));
			EventController = new EventController(iat);
			LineUpHistory = new List<Boat>();
			Boat.GetIdealCrew();
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
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, random.Next(1, 4)},
					{CrewMemberSkill.Perception, random.Next(1, 4)},
					{CrewMemberSkill.Quickness, random.Next(1, 4)},
					{CrewMemberSkill.Charisma, random.Next(7, 11)},
					{CrewMemberSkill.Wisdom, random.Next(7, 11)},
					{CrewMemberSkill.Willpower, random.Next(7, 11)}
				},
			},
			new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, random.Next(2, 6)},
					{CrewMemberSkill.Charisma, random.Next(2, 6)},
					{CrewMemberSkill.Quickness, random.Next(2, 6)},
					{CrewMemberSkill.Willpower, random.Next(2, 6)},
					{CrewMemberSkill.Perception, random.Next(8, 11)},
					{CrewMemberSkill.Wisdom, random.Next(8, 11)}
				}
			},
			new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Charisma, random.Next(1, 4)},
					{CrewMemberSkill.Perception, random.Next(1, 4)},
					{CrewMemberSkill.Wisdom, random.Next(1, 4)},
					{CrewMemberSkill.Body, random.Next(7, 11)},
					{CrewMemberSkill.Quickness, random.Next(7, 11)},
					{CrewMemberSkill.Willpower, random.Next(7, 11)}
				}
			},
			new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, random.Next(4, 8)},
					{CrewMemberSkill.Charisma, random.Next(4, 8)},
					{CrewMemberSkill.Perception, random.Next(4, 8)},
					{CrewMemberSkill.Quickness, random.Next(4, 8)},
					{CrewMemberSkill.Wisdom, random.Next(4, 8)},
					{CrewMemberSkill.Willpower, random.Next(4, 8)}
				}
			},
			/*new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, random.Next(4, 8)},
					{CrewMemberSkill.Charisma, random.Next(4, 8)},
					{CrewMemberSkill.Perception, random.Next(4, 8)},
					{CrewMemberSkill.Quickness, random.Next(4, 8)},
					{CrewMemberSkill.Wisdom, random.Next(4, 8)},
					{CrewMemberSkill.Willpower, random.Next(4, 8)}
				}
			},*/
			new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, random.Next(6, 10)},
					{CrewMemberSkill.Charisma, random.Next(6, 10)},
					{CrewMemberSkill.Perception, random.Next(6, 10)},
					{CrewMemberSkill.Quickness, random.Next(2, 6)},
					{CrewMemberSkill.Wisdom, random.Next(2, 6)},
					{CrewMemberSkill.Willpower, random.Next(2, 6)}
				}
			},
			new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, random.Next(2, 6)},
					{CrewMemberSkill.Charisma, random.Next(2, 6)},
					{CrewMemberSkill.Perception, random.Next(2, 6)},
					{CrewMemberSkill.Quickness, random.Next(6, 10)},
					{CrewMemberSkill.Wisdom, random.Next(6, 10)},
					{CrewMemberSkill.Willpower, random.Next(6, 10)}
				}
			},
			/*new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Body, random.Next(2, 5)},
					{CrewMemberSkill.Charisma, random.Next(2, 5)},
					{CrewMemberSkill.Perception, random.Next(2, 5)},
					{CrewMemberSkill.Quickness, random.Next(2, 5)},
					{CrewMemberSkill.Wisdom, random.Next(8, 11)},
					{CrewMemberSkill.Willpower, random.Next(8, 11)}
				}
			}*/
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
			Boat.GetIdealCrew();
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
				boat.IdealMatchScore = float.Parse(subjectSplit[subjectSplit.Length - 1]);
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
			crew += "," + Boat.IdealMatchScore;
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

		public void PostRaceRest()
		{
			Boat.PostRaceRest();
		}

		public string[] GetEventStrings(string eventKey)
		{
			return EventController.GetEventStrings(eventKey);
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all crew within the list
		/// </summary>
		public string[] SendBoatMembersEvent(DialogueStateActionDTO selected, List<CrewMember> members)
		{
			var replies = EventController.SelectEvent(selected, members, Boat);
			Boat.UpdateBoatScore();
			Boat.GetIdealCrew();
			return replies.ToArray();
		}

		public string[] SendBoatMembersEvent(string eventType, string eventName, List<CrewMember> members)
		{
			var replies = EventController.SelectEvent(eventType, eventName, members, Boat);
			Boat.UpdateBoatScore();
			Boat.GetIdealCrew();
			return replies.ToArray();
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all members within the list
		/// </summary>
		public string[] SendRecruitMembersEvent(DialogueStateActionDTO selected, List<CrewMember> members)
		{
			var replies = EventController.SelectEvent(selected, members, Boat);
			Boat.UpdateBoatScore();
			Boat.GetIdealCrew();
			return replies.ToArray();
		}
	}
}

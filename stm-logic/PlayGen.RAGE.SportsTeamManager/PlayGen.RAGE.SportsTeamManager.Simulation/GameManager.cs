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
		public int ActionAllowance { get; set; } = 20;
		public int CrewEditAllowance { get; set; }
		public event EventHandler AllowanceUpdated = delegate { };

		private IntegratedAuthoringToolAsset _iat { get; set; }
		private IStorageProvider _storagePorvider;
		private string _storageLocation;

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

			Person manager = new Person
			{
				Name = managerName,
				Age = int.Parse(managerAge),
				Gender = managerGender
			};

			manager.CreateFile(iat, templateStorage, storagePorvider, storageLocation);
			manager.UpdateBeliefs("Manager");
			manager.UpdateSingleBelief("Value(BoatType)", Boat.GetType().Name, "SELF");
			manager.UpdateSingleBelief("Action(Allowance)", ActionAllowance.ToString(), "SELF");
			manager.UpdateSingleBelief("CrewEdit(Allowance)", Boat.BoatPositions.Count.ToString(), "SELF");
			CrewEditAllowance = Boat.BoatPositions.Count;
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
				foreach (CrewMember otherMember in crew)
				{
					if (member != otherMember)
					{
						member.AddOrUpdateOpinion(otherMember, 0);
						member.AddOrUpdateRevealedOpinion(otherMember, 0);
					}
					member.AddOrUpdateOpinion(Boat.Manager, 0);
					member.AddOrUpdateRevealedOpinion(Boat.Manager, 0);
				}
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}

			iat.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceBoatName + ".iat"));
			EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
			_storagePorvider = storagePorvider;
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
					{CrewMemberSkill.Body, random.Next(1, 5)},
					{CrewMemberSkill.Perception, random.Next(1, 5)},
					{CrewMemberSkill.Quickness, random.Next(1, 5)},
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
					{CrewMemberSkill.Perception, random.Next(7, 11)},
					{CrewMemberSkill.Wisdom, random.Next(7, 11)}
				}
			},
			new CrewMember (random)
			{
				Skills = new Dictionary<CrewMemberSkill, int>()
				{
					{CrewMemberSkill.Charisma, random.Next(1, 5)},
					{CrewMemberSkill.Perception, random.Next(1, 5)},
					{CrewMemberSkill.Wisdom, random.Next(1, 5)},
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
					{CrewMemberSkill.Body, random.Next(2, 6)},
					{CrewMemberSkill.Charisma, random.Next(2, 6)},
					{CrewMemberSkill.Perception, random.Next(2, 6)},
					{CrewMemberSkill.Quickness, random.Next(2, 6)},
					{CrewMemberSkill.Wisdom, random.Next(7, 11)},
					{CrewMemberSkill.Willpower, random.Next(7, 11)}
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
					ActionAllowance = int.Parse(person.EmotionalAppraisal.GetBeliefValue("Action(Allowance)"));
					CrewEditAllowance = int.Parse(person.EmotionalAppraisal.GetBeliefValue("CrewEdit(Allowance)"));
					Boat.Name = iat.ScenarioName;
					Boat.Manager = person;
					continue;
				}
				CrewMember crewMember = new CrewMember(storagePorvider, rpc);
				crewList.Add(crewMember);
			}
			crewList.ForEach(cm => Boat.AddCrew(cm));
			crewList.ForEach(cm => cm.LoadBeliefs(Boat));
			EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
			_storagePorvider = storagePorvider;
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
			ResetActionAllowance(20);
			ResetCrewEditAllowance();
		}

		public void PostRaceRest()
		{
			Boat.PostRaceRest();
		}

		void DeductCost(int cost)
		{
			ActionAllowance -= cost;
			Boat.Manager.UpdateSingleBelief("Action(Allowance)", ActionAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		void ResetActionAllowance(int amount)
		{
			ActionAllowance = amount;
			Boat.Manager.UpdateSingleBelief("Action(Allowance)", ActionAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		void DeductCrewEditAllowance(int cost = 1)
		{
			CrewEditAllowance -= cost;
			Boat.Manager.UpdateSingleBelief("CrewEdit(Allowance)", CrewEditAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		void ResetCrewEditAllowance()
		{
			CrewEditAllowance = Boat.BoatPositions.Count;
			Boat.Manager.UpdateSingleBelief("CrewEdit(Allowance)", CrewEditAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		public void CreateRecruits(int amount)
		{
			if (Boat.Recruits != null)
			{
				Boat.Recruits.Clear();
			} else
			{
				Boat.Recruits = new List<CrewMember>();
			}
			Random rand = new Random();
			for (int i = 0; i < amount; i++)
			{
				CrewMember newMember = new CrewMember(rand);
				bool unqiue = false;
				while (!unqiue)
				{
					if (Boat.GetAllCrewMembers().Count(c => c.Name == newMember.Name) > 1 || newMember.Name == Boat.Manager.Name)
					{
						newMember.Name = newMember.SelectNewName(newMember.Gender, rand);
					}
					else
					{
						unqiue = true;
					}
				}
				int positionValue = rand.Next(0, Boat.BoatPositions.Count + 1);
				Position selectedPerferred = positionValue < Boat.BoatPositions.Count ? Boat.BoatPositions[positionValue].Position : null;
				newMember.Skills = new Dictionary<CrewMemberSkill, int>();
				foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
				{
					if (selectedPerferred != null)
					{
						if (selectedPerferred.RequiredSkills.Contains(skill))
						{
							newMember.Skills.Add(skill,rand.Next(7, 11));
						}
						else
						{
							newMember.Skills.Add(skill, rand.Next(1, 5));
						}
					}
					else
					{
						newMember.Skills.Add(skill, rand.Next(3, 9));
					}
				}
				Boat.Recruits.Add(newMember);
			}
		}

		public bool CanAddToCrew()
		{
			if (Boat.GetAllCrewMembers().Count + 1 > (Boat.BoatPositions.Count + 1) * 2)
			{
				return false;
			}
			return true;
		}

		public void AddRecruit(CrewMember member, int cost)
		{
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && CanAddToCrew())
			{
				TemplateStorageProvider templateStorage = new TemplateStorageProvider();
				member.CreateFile(_iat, templateStorage, _storagePorvider, _storageLocation);
				Boat.AddCrew(member);
				foreach (CrewMember otherMember in Boat.GetAllCrewMembers())
				{
					if (member != otherMember)
					{
						member.AddOrUpdateOpinion(otherMember, 0);
						member.AddOrUpdateRevealedOpinion(otherMember, 0);
						otherMember.AddOrUpdateOpinion(member, 0);
						otherMember.AddOrUpdateRevealedOpinion(member, 0);
					}
					member.AddOrUpdateOpinion(Boat.Manager, 0);
					member.AddOrUpdateRevealedOpinion(Boat.Manager, 0);
				}
				member.UpdateBeliefs("null");
				member.SaveStatus();
				DeductCost(cost);
				_iat.SaveToFile(_storagePorvider, _iat.AssetFilePath);
				DeductCrewEditAllowance();
			}
		}

		public bool CanRemoveFromCrew()
		{
			if (Boat.GetAllCrewMembers().Count - 1 < Boat.BoatPositions.Count)
			{
				return false;
			}
			return true;
		}

		public void RetireCrewMember(CrewMember crewMember, int cost)
		{
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && CanRemoveFromCrew())
			{
				Boat.RetireCrew(crewMember);
				DeductCost(cost);
				DeductCrewEditAllowance();
			}
		}

		public string[] GetEventStrings(string eventKey)
		{
			return EventController.GetEventStrings(_iat, eventKey);
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all crew within the list
		/// </summary>
		public string[] SendBoatMembersEvent(DialogueStateActionDTO selected, List<CrewMember> members, int cost)
		{
			if (cost <= ActionAllowance)
			{
				var replies = EventController.SelectBoatMemberEvent(_iat, selected, members, Boat);
				Boat.UpdateBoatScore();
				Boat.GetIdealCrew();
				DeductCost(cost);
				return replies.ToArray();
			}
			return new string[0];
		}

		public string[] SendBoatMembersEvent(string eventType, string eventName, List<CrewMember> members, int cost)
		{
			if (cost <= ActionAllowance)
			{
				var replies = EventController.SelectBoatMemberEvent(_iat, eventType, eventName, members, Boat);
				Boat.UpdateBoatScore();
				Boat.GetIdealCrew();
				DeductCost(cost);
				return replies.ToArray();
			}
			return new string[0];
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all members within the list
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitMembersEvent(CrewMemberSkill skill, List<CrewMember> members, int cost)
		{
			if (cost <= ActionAllowance)
			{
				var replies = EventController.SelectRecruitEvent(_iat, skill, members);
				DeductCost(cost);
				return replies;
			}
			else
			{
				Dictionary<CrewMember, string> replies = new Dictionary<CrewMember, string>();
				foreach (CrewMember member in members)
				{
					replies.Add(member, "");
				}
				return replies;
			}
		}
	}
}

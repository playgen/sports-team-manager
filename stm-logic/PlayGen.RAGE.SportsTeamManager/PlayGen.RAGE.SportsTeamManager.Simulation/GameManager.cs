using EmotionalAppraisal;
using IntegratedAuthoringTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AssetManagerPackage;
using IntegratedAuthoringTool.DTOs;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to access functionality contained within other classes and create/load/save games
	/// </summary>
	public class GameManager
	{
		private readonly ConfigStore config;
		private EventController eventController;

		public Team Team { get; private set; }
		public int ActionAllowance { get; private set; }
		public int CrewEditAllowance { get; private set; }
		public int RaceSessionLength { get; private set; }
		public EventController EventController => eventController;

		/// <summary>
		/// GameManager Constructor
		/// </summary>
		public GameManager()
		{
			config = new ConfigStore();
		}

		/// <summary>
		/// Create a new game
		/// </summary>
		public void NewGame(string storageLocation, string name, byte[] teamColorsPrimary, byte[] teamColorsSecondary, string managerName, string managerAge, string managerGender, List<CrewMember> crew = null)
		{
			UnloadGame();
			AssetManager.Instance.Bridge = new TemplateBridge();
			//create folder and iat file for game
			var combinedStorageLocation = Path.Combine(storageLocation, name);
			Directory.CreateDirectory(combinedStorageLocation);
			var iat = IntegratedAuthoringToolAsset.LoadFromFile("template_iat");
			//set up boat and team
			var initialType = config.GameConfig.PromotionTriggers.First(pt => pt.StartType == "Start").NewType;
			var boat = new Boat(config, initialType);
			Team = new Team(iat, storageLocation, config, name, boat);
			var positionCount = boat.Positions.Count;
			Team.TeamColorsPrimary = new Color(teamColorsPrimary[0], teamColorsPrimary[1], teamColorsPrimary[2], 255);
			Team.TeamColorsSecondary = new Color(teamColorsSecondary[0], teamColorsSecondary[1], teamColorsSecondary[2], 255);
			iat.ScenarioName = name;
			AssetManager.Instance.Bridge = new BaseBridge();
			iat.SaveToFile(Path.Combine(combinedStorageLocation, name + ".iat"));
			//create manager
			var manager = new Person
			{
				Name = managerName,
				Age = Convert.ToInt32(managerAge),
				Gender = managerGender
			};
			Team.Manager = manager;
			//create the initial crew members
			var initialCrew = false;
			if (crew == null)
			{
				initialCrew = true;
				for (var i = 0; i < positionCount * 2; i++)
				{
					var newMember = new CrewMember(boat.GetWeakPosition(Team.CrewMembers.Values.Concat(Team.Recruits.Values).ToList()), config);
					Team.UniqueNameCheck(newMember);
					Team.AddCrewMember(newMember);
				}
			}
			if (!initialCrew)
			{
				crew.ForEach(cm => Team.AddCrewMember(cm));
			}
			ActionAllowance = (int)config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * positionCount);
			CrewEditAllowance = (int)config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * positionCount;
			RaceSessionLength = (int)config.ConfigValues[ConfigKeys.RaceSessionLength];
			//create manager files and store game attribute details
			manager.CreateFile(iat, combinedStorageLocation);
			manager.UpdateBeliefs("Manager");
			manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), boat.Type);
			manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorRedPrimary.GetDescription(), teamColorsPrimary[0].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorGreenPrimary.GetDescription(), teamColorsPrimary[1].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorBluePrimary.GetDescription(), teamColorsPrimary[2].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorRedSecondary.GetDescription(), teamColorsSecondary[0].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorGreenSecondary.GetDescription(), teamColorsSecondary[1].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorBlueSecondary.GetDescription(), teamColorsSecondary[2].ToString());
			
			manager.SaveStatus();

			var names = Team.CrewMembers.Keys.ToList();
			names.Add(managerName);

			//set up files and details for each CrewMember
			foreach (var member in Team.CrewMembers.Values)
			{
				member.CreateFile(iat, combinedStorageLocation);
				member.Avatar = new Avatar(member);
				Team.SetCrewColors(member.Avatar);
				if (!initialCrew)
				{
					foreach (var otherMember in names)
					{
						if (member.Name != otherMember)
						{
							member.AddOrUpdateOpinion(otherMember, 0);
							member.AddOrUpdateRevealedOpinion(otherMember, 0);
						}
					}
				}
				else
				{
					member.CreateInitialOpinions(names);
				}
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}
			iat.SaveToFile(Path.Combine(combinedStorageLocation, name + ".iat"));
			eventController = new EventController(iat);
			Team.CreateRecruits();
		}

		/// <summary>
		/// Get the name of every folder stored in the directory provided
		/// </summary>
		public List<string> GetGameNames(string storageLocation)
		{
			var folders = Directory.GetDirectories(storageLocation);
			var gameNames = new List<string>();
			foreach (var folder in folders)
			{
				var file = Path.GetFileName(folder);
				gameNames.Add(file);
			}
			return gameNames;
		}

		/// <summary>
		/// Check if the location provided contains an existing game
		/// </summary>
		public bool CheckIfGameExists(string storageLocation, string gameName)
		{
			var gameExists = false;
			if (Directory.Exists(Path.Combine(storageLocation, gameName)))
			{
				var files = Directory.GetFiles(Path.Combine(storageLocation, gameName), "*.iat");
				foreach (var file in files)
				{
					try
					{
						AssetManager.Instance.Bridge = new BaseBridge();
						var game = IntegratedAuthoringToolAsset.LoadFromFile(file);
						if (game != null && game.ScenarioName == gameName)
						{
							gameExists = true;
							break;
						}
					}
					//do not want loading errors to result in exceptions, so catch all
					catch
					{

					}
				}
			}
			return gameExists;
		}

		/// <summary>
		/// Load an existing game
		/// </summary>
		public void LoadGame(string storageLocation, string boatName)
		{
			UnloadGame();
			//get the iat file and all characters for this game
			var combinedStorageLocation = Path.Combine(storageLocation, boatName);
			AssetManager.Instance.Bridge = new BaseBridge();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(Path.Combine(combinedStorageLocation, boatName + ".iat"));
			var rpcList = iat.GetAllCharacters();

			var crewList = new List<CrewMember>();
			var nameList = new List<string>();
			foreach (var rpc in rpcList)
			{
				var tempea = EmotionalAppraisalAsset.LoadFromFile(rpc.EmotionalAppraisalAssetSource);
				var position = tempea.GetBeliefValue(NPCBeliefs.Position.GetDescription());
				//if this character is the manager, load the game details from this file and set this character as the manager
				if (position == "Manager")
				{
					var person = new Person(rpc);
					nameList.Add(person.Name);
					var boat = new Boat(config, person.LoadBelief(NPCBeliefs.BoatType.GetDescription()));
					Team = new Team(iat, storageLocation, config, iat.ScenarioName, boat);
					ActionAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.ActionAllowance.GetDescription()));
					CrewEditAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.CrewEditAllowance.GetDescription()));
					RaceSessionLength = (int)config.ConfigValues[ConfigKeys.RaceSessionLength];
					var primary = new byte[3];
					primary[0] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorRedPrimary.GetDescription()));
					primary[1] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorGreenPrimary.GetDescription()));
					primary[2] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorBluePrimary.GetDescription()));
					Team.TeamColorsPrimary = new Color(primary[0], primary[1], primary[2], 255);
					var secondary = new byte[3];
					secondary[0] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorRedSecondary.GetDescription()));
					secondary[1] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorGreenSecondary.GetDescription()));
					secondary[2] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorBlueSecondary.GetDescription()));
					Team.Manager = person;
					Team.TeamColorsSecondary = new Color(secondary[0], secondary[1], secondary[2], 255);
					continue;
				}
				//set up every other character as a CrewManager, making sure to separate retired and recruits
				var crewMember = new CrewMember(rpc, config);
				nameList.Add(crewMember.Name);
				switch (position)
				{
					case "Retired":
						crewMember.Avatar = new Avatar(crewMember, false, true);
						Team.RetiredCrew.Add(crewMember.Name, crewMember);
						continue;
					case "Recruit":
						crewMember.Avatar = new Avatar(crewMember, false, true);
						Team.Recruits.Add(crewMember.Name, crewMember);
						continue;
				}
				crewList.Add(crewMember);
			}
			//add all non-retired and non-recruits to the list of crew
			crewList.ForEach(cm => Team.AddCrewMember(cm));
			//load the 'beliefs' (aka, stats and opinions) of all crew members
			foreach (var cm in Team.Recruits.Values)
			{
				cm.LoadBeliefs(nameList);
			}
			foreach (var cm in Team.RetiredCrew.Values)
			{
				cm.LoadBeliefs(nameList);
			}
			crewList.ForEach(cm => cm.LoadBeliefs(nameList));
			crewList.ForEach(cm => cm.LoadPosition(Team.Boat));
			//set up crew avatars
			crewList.ForEach(cm => cm.Avatar = new Avatar(cm, true, true));
			crewList.ForEach(cm => Team.SetCrewColors(cm.Avatar));
			eventController = new EventController(iat);
			LoadLineUpHistory();
		}

		/// <summary>
		/// Unload the current game
		/// </summary>
		public void UnloadGame()
		{
			Team = null;
		}

		/// <summary>
		/// Load the history of line-ups from the manager's EA file
		/// </summary>
		public void LoadLineUpHistory()
		{
			//get all events that feature 'SelectedLineUp' from their EA file
			var ea = EmotionalAppraisalAsset.LoadFromFile(Team.Manager.RolePlayCharacter.EmotionalAppraisalAssetSource);
			var managerEvents = ea.EventRecords;
			var lineUpEvents = managerEvents.Where(e => e.Event.Contains("SelectedLineUp")).Select(e => e.Event);
			var crewMembers = Team.CrewMembers.Values.ToList();
			foreach (var crewMember in Team.RetiredCrew.Values.ToList())
			{
				crewMembers.Add(crewMember);
			}

			foreach (var lineup in lineUpEvents)
			{
				//split up the string of details saved with this event
				var splitAfter = lineup.Split('(')[2];
				splitAfter = splitAfter.Split(')')[0];
				var subjectSplit = splitAfter.Split(',');
				//set up the boat
				var boat = new Boat(config, subjectSplit[0]);
				//position crew members and gather set-up information using details from split string
				for (var i = 0; i < boat.Positions.Count; i++)
				{
					if (subjectSplit[((i + 1) * 2) - 1].NoSpaces() != "null")
					{
						boat.PositionCrew.Add(boat.Positions[i], crewMembers.Single(c => c.Name.NoSpaces() == subjectSplit[((i + 1) * 2) - 1].NoSpaces()));
						boat.PositionScores.Add(boat.Positions[i], Convert.ToInt32(subjectSplit[(i + 1) * 2]));
					}
				}
				boat.IdealMatchScore = float.Parse(subjectSplit[(boat.Positions.Count * 2) + 1]);
				boat.Score = boat.PositionScores.Values.Sum();
				boat.SelectionMistakes = new List<string>();
				for (var i = (boat.Positions.Count + 1) * 2; i < subjectSplit.Length - 1; i++)
				{
					boat.SelectionMistakes.Add(subjectSplit[i].NoSpaces());
				}
				Team.HistoricTimeOffset.Add(Convert.ToInt32(subjectSplit[subjectSplit.Length - 1]));
				Team.LineUpHistory.Add(boat);
			}
		}

		/// <summary>
		/// Save the current boat line-up to the manager's EA file
		/// </summary>
		public void SaveLineUp(int offset)
		{
			//set-up boat for saving
			var boat = Team.Boat;
			var manager = Team.Manager;
			boat.UpdateBoatScore(manager.Name);
			boat.GetIdealCrew(Team.CrewMembers, manager.Name);
			var spacelessName = manager.RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventStringUnformatted = "SelectedLineUp({0},{1})";
			var boatType = boat.Type;
			var crew = "";
			//set up string to save
			foreach (var position in boat.Positions)
			{
				//add comma to split information if this isn't the first part of the string
				if (!string.IsNullOrEmpty(crew))
				{
					crew += ",";
				}
				//add positioned crewmembers and their position rating to the string
				if (boat.PositionCrew.ContainsKey(position))
				{
					crew += boat.PositionCrew[position].Name.NoSpaces();
					crew += "," + boat.PositionScores[position];
				}
				else
				{
					crew += "null,0";
				}
			}
			//add idealmatchscore to the string
			crew += "," + boat.IdealMatchScore;
			//add every selection mistake to the string 
			boat.SelectionMistakes.ForEach(sm => crew += "," + sm);
			//add time offset to the string 
			crew += "," + offset;
			//send event with string of information within
			var eventString = string.Format(eventStringUnformatted, boatType, crew);
			manager.EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
			var eventRpc = manager.RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
			if (eventRpc != null)
			{
				manager.RolePlayCharacter.ActionFinished(eventRpc);
			}
			manager.SaveStatus();
			//store saved details in new local boat copy
			var lastBoat = new Boat(config, boat.Type);
			foreach (var position in boat.Positions)
			{
				if (boat.PositionCrew.ContainsKey(position))
				{
					lastBoat.PositionCrew.Add(position, boat.PositionCrew[position]);
					lastBoat.PositionScores.Add(position, boat.PositionScores[position]);
				}
			}
			lastBoat.SelectionMistakes = boat.SelectionMistakes;
			lastBoat.IdealMatchScore = boat.IdealMatchScore;
			lastBoat.Score = lastBoat.PositionScores.Values.Sum();
			Team.LineUpHistory.Add(lastBoat);
			Team.HistoricTimeOffset.Add(offset);
			Team.TickCrewMembers((int)config.ConfigValues[ConfigKeys.TicksPerSession]);
		}

		/// <summary>
		/// Save current line-up and update CrewMember's opinions and mood based on this line-up
		/// </summary>
		public void ConfirmLineUp()
		{
			Team.ConfirmChanges(ActionAllowance);
			//reset the limits on actions and hiring/firing
			ResetActionAllowance();
			ResetCrewEditAllowance();
		}

		/// <summary>
		/// Select a random post-race event
		/// </summary>
		public List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> SelectPostRaceEvents()
		{
			//work out if it is currently directly after a race session and the chance of an event occurring as a result
			var afterRace = false;
			var chance = (int)config.ConfigValues[ConfigKeys.EventChance];
			if (Team.LineUpHistory.Count % RaceSessionLength == 0)
			{
				afterRace = true;
			}
			else
			{
				chance += (int)config.ConfigValues[ConfigKeys.PracticeEventChanceReduction];
			}
			return eventController.SelectPostRaceEvents(config, Team, chance, afterRace);
		}

		/// <summary>
		/// Send player dialogue to characters involved in the event and get their replies
		/// </summary>
		public Dictionary<CrewMember, string> SendPostRaceEvent(DialogueStateActionDTO dialogue, List<CrewMember> members)
		{
			return eventController.SendPostRaceEvent(dialogue, members, Team, Team.LineUpHistory.Last());
		}

		/// <summary>
		/// Deduct the cost of an action from the available allowance
		/// </summary>
		void DeductCost(int cost)
		{
			ActionAllowance -= cost;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Reset the amount of allowance actions
		/// </summary>
		void ResetActionAllowance()
		{
			ActionAllowance = GetStartingActionAllowance();
			Team.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Calculate how much ActionAllowance the player should start each race with
		/// </summary>
		public int GetStartingActionAllowance()
		{
			return (int)config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * Team.Boat.Positions.Count);
		}

		/// <summary>
		/// Deduct the cost of a hiring/firing action from the available allowance
		/// </summary>
		void DeductCrewEditAllowance()
		{
			CrewEditAllowance--;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Reset the amount of hiring/firing actions allowed
		/// </summary>
		void ResetCrewEditAllowance()
		{
			CrewEditAllowance = GetStartingCrewEditAllowance();
			Team.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Calculate how many hiring/firing actions player should start each race with
		/// </summary>
		public int GetStartingCrewEditAllowance()
		{
			return (int)config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * Team.Boat.Positions.Count;
		}

		/// <summary>
		/// If the player has enough time, hire the selected recruit and take the cost from the allowances
		/// </summary>
		public void AddRecruit(CrewMember member)
		{
			//if the player is able to take this action
			var cost = (int)config.ConfigValues[ConfigKeys.RecruitmentCost];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && Team.CanAddToCrew())
			{
				Team.AddRecruit(member);
				DeductCost(cost);
				DeductCrewEditAllowance();
			}
		}

		/// <summary>
		/// Remove a CrewMember from the crew if the player has the allowances to do so
		/// </summary>
		public void RetireCrewMember(CrewMember crewMember)
		{
			var cost = (int)config.ConfigValues[ConfigKeys.FiringCost];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && Team.CanRemoveFromCrew())
			{
				Team.RetireCrew(crewMember);
				DeductCost(cost);
				DeductCrewEditAllowance();
			}
		}

		/// <summary>
		/// Send player meeting dialogue to a CrewMember, getting their response in return
		/// </summary>
		public string SendMeetingEvent(string eventName, CrewMember member)
		{
			var cost = (int)GetConfigValue((ConfigKeys)Enum.Parse(typeof(ConfigKeys), eventName + "Cost"));
			if (cost <= ActionAllowance)
			{
				var reply = eventController.SendMeetingEvent(eventName, member, Team);
				DeductCost(cost);
				return reply;
			}
			return "";
		}

		/// <summary>
		/// Get the value from the config
		/// </summary>
		public float GetConfigValue(ConfigKeys eventKey)
		{
			return config.ConfigValues[eventKey];
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all recruits
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitMembersEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			var cost = (int)config.ConfigValues[ConfigKeys.SendRecruitmentQuestionCost];
			if (cost <= ActionAllowance)
			{
				return eventController.SendRecruitEvent(skill, members);
			}
			var replies = new Dictionary<CrewMember, string>();
			members.ForEach(member => replies.Add(member, ""));
			return replies;
		}
	}
}

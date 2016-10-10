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
	//Two starting crew members and random starting opinions currently commented out
	/// <summary>
	/// Used to access functionality contained within other classes
	/// </summary>
	public class GameManager
	{
		private readonly ConfigStore _config;
		private EventController _eventController;

		public Team Team { get; private set; }
		public int ActionAllowance { get; private set; }
		public int CrewEditAllowance { get; private set; }
		public int RaceSessionLength { get; private set; }
		public EventController EventController
		{
			get { return _eventController; }
		}

		/// <summary>
		/// GameManager Constructor
		/// </summary>
		public GameManager()
		{
			_config = new ConfigStore();
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
			//set up first boat
			var boat = new Boat(_config, "Dinghy");
			Team = new Team(iat, storageLocation, _config, name, boat);
			var positionCount = boat.BoatPositions.Count;
			Team.TeamColorsPrimary = new Color(teamColorsPrimary[0], teamColorsPrimary[1], teamColorsPrimary[2], 255);
			Team.TeamColorsSecondary = new Color(teamColorsSecondary[0], teamColorsSecondary[1], teamColorsSecondary[2], 255);
			iat.ScenarioName = name;
			AssetManager.Instance.Bridge = new BaseBridge();
			iat.SaveToFile(Path.Combine(combinedStorageLocation, name + ".iat"));
			var random = new Random();
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
					Team.AddCrewMember(new CrewMember(random, boat.GetWeakPosition(random, Team.CrewMembers.Values.ToList()), _config));
				}
			}
			if (!initialCrew)
			{
				crew.ForEach(cm => Team.AddCrewMember(cm));
			}
			ActionAllowance = (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * positionCount);
			CrewEditAllowance = (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * positionCount;
			RaceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength];
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
					member.CreateInitialOpinions(random, names);
				}
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}
			iat.SaveToFile(Path.Combine(combinedStorageLocation, name + ".iat"));
			_eventController = new EventController(iat);
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
				/*foreach(var file in files)
				{
					var name = IntegratedAuthoringToolAsset.LoadFromFile(LocalStorageProvider.Instance, file).ScenarioName;
					gameNames.Add(name);
				}*/
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
					var boat = new Boat(_config, person.LoadBelief(NPCBeliefs.BoatType.GetDescription()));
					Team = new Team(iat, storageLocation, _config, iat.ScenarioName, boat);
					ActionAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.ActionAllowance.GetDescription()));
					CrewEditAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.CrewEditAllowance.GetDescription()));
					RaceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength];
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
				var crewMember = new CrewMember(rpc, _config);
				nameList.Add(crewMember.Name);
				if (position == "Retired")
				{
					crewMember.Avatar = new Avatar(crewMember, false, true);
					Team.RetiredCrew.Add(crewMember.Name, crewMember);
					continue;
				}
				if (position == "Recruit")
				{
					crewMember.Avatar = new Avatar(crewMember, false, true);
					Team.Recruits.Add(crewMember.Name, crewMember);
					continue;
				}
				crewList.Add(crewMember);
			}
			//add all non-retired and non-recruits to the list of unassigned crew
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
			_eventController = new EventController(iat);
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
				//set up the version of boat this was
				var boat = new Boat(_config, subjectSplit[0]);
				//position crew members and gather set-up information using details from split string
				for (var i = 0; i < boat.BoatPositions.Count; i++)
				{
					boat.BoatPositionCrew.Add(boat.BoatPositions[i], crewMembers.Single(c => c.Name.NoSpaces() == subjectSplit[((i + 1) * 2) - 1].NoSpaces()));
					boat.BoatPositionScores.Add(boat.BoatPositions[i], Convert.ToInt32(subjectSplit[(i + 1) * 2]));
				}
				boat.IdealMatchScore = float.Parse(subjectSplit[(boat.BoatPositions.Count * 2) + 1]);
				boat.BoatScore = boat.BoatPositionScores.Values.Sum();
				boat.SelectionMistakes = new List<string>();
				for (var i = (boat.BoatPositions.Count + 1) * 2; i < subjectSplit.Length - 1; i++)
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
			foreach (var boatPosition in boat.BoatPositions)
			{
				if (!string.IsNullOrEmpty(crew))
				{
					crew += ",";
				}
				if (boat.BoatPositionCrew.ContainsKey(boatPosition))
				{
					crew += boat.BoatPositionCrew[boatPosition].Name.NoSpaces();
					crew += "," + boat.BoatPositionScores[boatPosition];
				} else
				{
					crew += "null,0";
				}
			}
			crew += "," + boat.IdealMatchScore;
			boat.SelectionMistakes.ForEach(sm => crew += "," + sm);
			crew += "," + offset;
			var eventString = string.Format(eventStringUnformatted, boatType, crew);
			manager.EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
			var eventRpc = manager.RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
			if (eventRpc != null)
			{
				manager.RolePlayCharacter.ActionFinished(eventRpc);
			}
			manager.SaveStatus();
			var lastBoat = new Boat(_config, boat.Type);
			foreach (var bp in boat.BoatPositions)
			{
				if (boat.BoatPositionCrew.ContainsKey(bp))
				{
					lastBoat.BoatPositionCrew.Add(bp, boat.BoatPositionCrew[bp]);
					lastBoat.BoatPositionScores.Add(bp, boat.BoatPositionScores[bp]);
				}
			}
			lastBoat.SelectionMistakes = boat.SelectionMistakes;
			lastBoat.IdealMatchScore = boat.IdealMatchScore;
			lastBoat.BoatScore = lastBoat.BoatPositionScores.Values.Sum();
			Team.LineUpHistory.Add(lastBoat);
			Team.HistoricTimeOffset.Add(offset);
			Team.TickCrewMembers((int)_config.ConfigValues[ConfigKeys.TicksPerSession]);
		}

		/// <summary>
		/// Save current line-up and update CrewMember's opinions and mood based on this line-up
		/// </summary>
		public void ConfirmLineUp()
		{
			Team.ConfirmChanges(ActionAllowance);
			//TODO: Change trigger for promotion
			if (((Team.LineUpHistory.Count + 1) / RaceSessionLength) % 2 != 0)
			{
				Team.PromoteBoat();
			}
			//update available recruits for the next race
			Team.CreateRecruits();
			//reset the limits on actions and hiring/firing
			ResetActionAllowance();
			ResetCrewEditAllowance();
		}

		/// <summary>
		/// Select a random post-race event
		/// </summary>
		public List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> SelectPostRaceEvents()
		{
			var afterRace = false;
			var chance = (int)_config.ConfigValues[ConfigKeys.EventChance];
			if (Team.LineUpHistory.Count % RaceSessionLength == 0)
			{
				afterRace = true;
			}
			else
			{
				chance += (int)_config.ConfigValues[ConfigKeys.PracticeEventChanceReduction];
			}
			var random = new Random();
			return _eventController.SelectPostRaceEvents(Team, chance, random, afterRace);
		}

		/// <summary>
		/// Send player dialogue to characters involved in the event and get their replies
		/// </summary>
		public Dictionary<CrewMember, string> SendPostRaceEvent(DialogueStateActionDTO dialogue, List<CrewMember> members)
		{
			var replies = _eventController.SendPostRaceEvent(dialogue, members, Team, Team.LineUpHistory.Last());
			return replies;
		}

		/// <summary>
		/// Deduct the cost of an action from the available allowance
		/// </summary>
		void DeductCost(int cost)
		{
			ActionAllowance -= cost;
			Team.TickCrewMembers(cost);
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
			return (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * Team.Boat.BoatPositions.Count);
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
			return (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * Team.Boat.BoatPositions.Count;
		}

		public void AddRecruit(CrewMember member)
		{
			//if the player is able to take this action
			var cost = (int)_config.ConfigValues[ConfigKeys.RecruitmentCost];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && Team.CanAddToCrew())
			{
				Team.AddRecruit(member);
				DeductCost(cost);
				DeductCrewEditAllowance();
			}
		}

		/// <summary>
		/// Remove a CrewMember from the crew
		/// </summary>
		public void RetireCrewMember(CrewMember crewMember)
		{
			var cost = (int)_config.ConfigValues[ConfigKeys.FiringCost];
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
				var reply = _eventController.SendMeetingEvent(eventName, member, Team);
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
			return _config.ConfigValues[eventKey];
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all members within the list
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitMembersEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			var cost = (int)_config.ConfigValues[ConfigKeys.SendRecruitmentQuestionCost];
			if (cost <= ActionAllowance)
			{
				return _eventController.SendRecruitEvent(skill, members);
			}
			var replies = new Dictionary<CrewMember, string>();
			members.ForEach(member => replies.Add(member, ""));
			return replies;
		}
	}
}

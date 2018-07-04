using IntegratedAuthoringTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using RolePlayCharacter;

using WellFormedNames;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to access functionality contained within other classes and create/load/save games
	/// </summary>
	public class GameManager
	{
		private ConfigStore _config;
		private Dictionary<int, Dictionary<string, string>> _customTutorialAttributes { get; set; }
		public Team Team { get; private set; }
		public int ActionAllowance { get; private set; }
		public int CrewEditAllowance { get; private set; }
		public int RaceSessionLength { get; private set; }
		public int CurrentRaceSession { get; private set; }
		public bool ShowTutorial { get; private set; }
		public int TutorialStage { get; private set; }
		public bool QuestionnaireCompleted { get; private set; }
		public EventController EventController { get; private set; }

		/// <summary>
		/// GameManager Constructor
		/// </summary>
		public GameManager(Platform platform = Platform.Windows)
		{
			ConfigLoad();
		}

		private async void ConfigLoad(Platform platform = Platform.Windows)
		{
			_config = await Task.Factory.StartNew(() => new ConfigStore(platform));
		}

		/// <summary>
		/// Validate that the provided game config should result in no errors and can be processed correctly
		/// </summary>
		private void ValidateGameConfig()
		{
			var invalidString = string.Empty;
			var promotionTriggers = _config.GameConfig.PromotionTriggers;
			//is there a promotion trigger that has a StartType of 'Start'?
			if (promotionTriggers.All(pt => pt.StartType != "Start"))
			{
				invalidString += "Game Config requires one PromotionTrigger with StartTpe \"Start\".\n";
			}
			
			foreach (var promotion in promotionTriggers)
			{
				//is there a promotion trigger that has a StartType of 'Start' and has a values less than 0 for ScoreMetSinceLast?
				if (promotion.StartType != "Start" && promotion.ScoreMetSinceLast <= 0)
				{
					invalidString += $"ScoreMetSinceLast for StartType {promotion.StartType} and NewType {promotion.NewType} should be greater than 0.\n";
				}
				//is there a promotion trigger that has a StartType that is the same as it's NewType?
				if (promotion.StartType == promotion.NewType)
				{
					invalidString += $"Invalid PromotionTrigger in Game Config for {promotion.StartType}, will result in changing to same boat type.\n";
				}
				//is there a promotion trigger that does not have a StartType of 'Start' and has a StartType that isn't provided in BoatConfig.json?
				if (promotion.StartType != "Start" && _config.BoatTypes.All(bt => bt.Key != promotion.StartType))
				{
					invalidString += $"StartType {promotion.StartType} is not an existing BoatType.\n";
				}
				//is there a promotion trigger that does not have a NewType of 'Finish' and has a NewType that isn't provided in BoatConfig.json?
				if (promotion.NewType != "Finish" && _config.BoatTypes.All(bt => bt.Key != promotion.NewType))
				{
					invalidString += $"NewType {promotion.NewType} is not an existing BoatType.\n";
				}
				//is there a promotion trigger that is impossible to trigger?
				if (promotionTriggers.Any(pt => pt != promotion && pt.StartType == promotion.StartType && pt.ScoreMetSinceLast <= promotion.ScoreMetSinceLast && pt.ScoreRequired <= promotion.ScoreRequired))
				{
					invalidString += $"PromotionTrigger with StartType {promotion.StartType}, NewType {promotion.NewType} will never be triggered.\n";
				}
				//is there a promotion trigger that is impossible to trigger?
				if (promotion.StartType != "Start" && promotionTriggers.All(pt => pt.NewType != promotion.StartType))
				{
					invalidString += $"PromotionTrigger with StartType {promotion.StartType}, NewType {promotion.NewType} will never be triggered.\n";
				}
			}
			var eventTriggers = _config.GameConfig.EventTriggers;
			var postRaceEvents = EventController.GetPossiblePostRaceDialogue();
			var postRaceNames = postRaceEvents.Select(pre => pre.NextState).ToList();
			foreach (var ev in eventTriggers)
			{
				//is there a post race event trigger that is not a valid name for an event?
				if (postRaceNames.All(prn => prn != "Player_" + ev.EventName))
				{
					invalidString += $"{ev.EventName} is not an existing event name.\n";
				}
				//is there a post race event trigger that contains an invalid boat type?
				if (ev.StartBoatType != null && _config.BoatTypes.All(bt => bt.Key != ev.StartBoatType))
				{
					invalidString += $"StartBoatType {ev.StartBoatType} is not an existing BoatType.\n";
				}
				//is there a post race event trigger that contains an invalid boat type?
				if (ev.EndBoatType != null && _config.BoatTypes.All(bt => bt.Key != ev.EndBoatType))
				{
					invalidString += $"EndBoatType {ev.EndBoatType} is not an existing BoatType.\n";
				}
				//is there a post race event trigger that has a RaceTrigger value less than 0?
				if (ev.RaceTrigger < 0)
				{
					invalidString += "RaceTrigger must be at least 0.\n";
				}
			}
			if (!string.IsNullOrEmpty(invalidString))
			{
				throw new Exception(invalidString);
			}
		}

		public async void NewGameTask(string storageLocation, string name, byte[] teamColorsPrimary, byte[] teamColorsSecondary, string managerName, bool showTutorial, string nation, Action<bool> completed, List<CrewMember> crew = null)
		{
			try
			{
				await Task.Factory.StartNew(() => NewGame(storageLocation, name, teamColorsPrimary, teamColorsSecondary, managerName, showTutorial, nation, crew));
				completed(true);
			}
			catch
			{
				completed(false);
				throw;
			}
		}

		/// <summary>
		/// Create a new game
		/// </summary>
		public void NewGame(string storageLocation, string name, byte[] teamColorsPrimary, byte[] teamColorsSecondary, string managerName, bool showTutorial, string nation, List<CrewMember> crew = null)
		{
			UnloadGame();
			//create folder and iat file for game
			var combinedStorageLocation = Path.Combine(storageLocation, name);
			Directory.CreateDirectory(combinedStorageLocation);
			var iat = ConfigStore.IntegratedAuthoringTool.Copy();
			EventController = new EventController(iat);
			ValidateGameConfig();
			//set up boat and team
			var initialType = _config.GameConfig.PromotionTriggers.First(pt => pt.StartType == "Start").NewType;
			var boat = new Boat(_config, initialType);
			Team = new Team(iat, storageLocation, _config, name, nation, boat);
			var positionCount = boat.Positions.Count;
			Team.TeamColorsPrimary = new Color(teamColorsPrimary[0], teamColorsPrimary[1], teamColorsPrimary[2], 255);
			Team.TeamColorsSecondary = new Color(teamColorsSecondary[0], teamColorsSecondary[1], teamColorsSecondary[2], 255);
			iat.ScenarioName = name;
			iat.SetFutureFilePath(Path.Combine(combinedStorageLocation, name + ".iat"));
			//create manager
			var manager = new Person(null)
							{
								Name = managerName
							};
			Team.Manager = manager;
			//create the initial crew members
			var initialCrew = false;
			if (crew == null)
			{
				initialCrew = true;
				for (var i = 0; i < positionCount * 2; i++)
				{
					var newMember = new CrewMember(boat.GetWeakPosition(Team.CrewMembers.Values.Concat(Team.Recruits.Values).ToList()), Team.Nationality, _config);
					Team.UniqueNameCheck(newMember);
					Team.AddCrewMember(newMember);
				}
			}
			if (!initialCrew)
			{
				crew.ForEach(cm => Team.AddCrewMember(cm));
			}
			//set up initial values
			ActionAllowance = (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * positionCount);
			CrewEditAllowance = (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * positionCount;
			RaceSessionLength = showTutorial ? (int)_config.ConfigValues[ConfigKeys.TutorialRaceSessionLength] : (int)_config.ConfigValues[ConfigKeys.RaceSessionLength];
			CurrentRaceSession = 0;
			ShowTutorial = showTutorial;
			TutorialStage = 0;
			_customTutorialAttributes = new Dictionary<int, Dictionary<string, string>>();
			QuestionnaireCompleted = false;
			//create manager files and store game attribute details
			manager.CreateFile(iat, combinedStorageLocation);
			manager.UpdateBeliefs("Manager");
			manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), boat.Type);
			manager.UpdateSingleBelief(NPCBeliefs.ShowTutorial.GetDescription(), ShowTutorial.ToString());
			manager.UpdateSingleBelief(NPCBeliefs.QuestionnaireCompleted.GetDescription(), QuestionnaireCompleted.ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TutorialStage.GetDescription(), TutorialStage.ToString());
			manager.UpdateSingleBelief(NPCBeliefs.Nationality.GetDescription(), nation);
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
							member.AddOrUpdateRevealedOpinion(otherMember, 0, false);
						}
					}
				}
				else
				{
					member.CreateInitialOpinions(names);
				}
				member.UpdateBeliefs(Name.NIL_STRING);
				member.SaveStatus();
			}
			Team.CreateRecruits();
		}

		/// <summary>
		/// Get the name of every folder stored in the directory provided
		/// </summary>
		public List<string> GetGameNames(string storageLocation)
		{
			try
			{
				var folders = Directory.GetDirectories(storageLocation).ToList();
				return folders.Select(Path.GetFileName).ToList();
			}
			catch
			{
				return new List<string>();
			}
		}

		public async void CheckIfGameExistsTask(string storageLocation, string boatName, Action<bool, bool> completed)
		{
			try
			{
				var task = await Task.Factory.StartNew(() => CheckIfGameExists(storageLocation, boatName, out var _));
				completed(true, task);
			}
			catch
			{
				completed(false, false);
				throw;
			}
		}

		/// <summary>
		/// Check if the location provided contains an existing game
		/// </summary>
		public bool CheckIfGameExists(string storageLocation, string gameName, out IntegratedAuthoringToolAsset iat)
		{
			if (Directory.Exists(Path.Combine(storageLocation, gameName)))
			{
				var files = Directory.GetFiles(Path.Combine(storageLocation, gameName), "*.iat");
				foreach (var file in files)
				{
					try
					{
						var game = IntegratedAuthoringToolAsset.LoadFromFile(file);
						if (game != null && game.ScenarioName.ToLower() == gameName.ToLower())
						{
							iat = game;
							return true;
						}
					}
					//do not want loading errors to result in exceptions, so catch all
					catch
					{
						iat = null;
						return false;
					}
				}
			}
			iat = null;
			return false;
		}

		public async void LoadGameTask(string storageLocation, string boatName, Action<bool> completed)
		{
			try
			{
				await Task.Factory.StartNew(() => LoadGame(storageLocation, boatName));
				completed(true);
			}
			catch
			{
				completed(false);
				throw;
			}
		}

		/// <summary>
		/// Load an existing game
		/// </summary>
		public void LoadGame(string storageLocation, string boatName)
		{
			UnloadGame();
			var valid = CheckIfGameExists(storageLocation, boatName, out var iat);
			if (!valid)
			{
				return;
			}
			//get the iat file and all characters for this game
			EventController = new EventController(iat);
			ValidateGameConfig();
			var characterList = iat.GetAllCharacterSources();

			var crewList = new List<CrewMember>();
			var nameList = new List<string>();
			foreach (var character in characterList)
			{
				var rpc = RolePlayCharacterAsset.LoadFromFile(character.Source);
				var position = rpc.GetBeliefValue(NPCBeliefs.Position.GetDescription());
				//if this character is the manager, load the game details from this file and set this character as the manager
				if (position == "Manager")
				{
					var person = new Person(rpc);
					nameList.Add(person.Name);
					var boat = new Boat(_config, person.LoadBelief(NPCBeliefs.BoatType.GetDescription()));
					var nation = person.LoadBelief(NPCBeliefs.Nationality.GetDescription());
					ShowTutorial = bool.Parse(person.LoadBelief(NPCBeliefs.ShowTutorial.GetDescription()));
					TutorialStage = int.Parse(person.LoadBelief(NPCBeliefs.TutorialStage.GetDescription()));
					_customTutorialAttributes = new Dictionary<int, Dictionary<string, string>>();
					QuestionnaireCompleted = bool.Parse(person.LoadBelief(NPCBeliefs.QuestionnaireCompleted.GetDescription()) ?? "false");
					Team = new Team(iat, storageLocation, _config, iat.ScenarioName, nation, boat);
					if (boat.Type == "Finish")
					{
						Team.Finished = true;
					}
					ActionAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.ActionAllowance.GetDescription()));
					CrewEditAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.CrewEditAllowance.GetDescription()));
					RaceSessionLength = ShowTutorial ? (int)_config.ConfigValues[ConfigKeys.TutorialRaceSessionLength] : (int)_config.ConfigValues[ConfigKeys.RaceSessionLength];
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
			LoadLineUpHistory();
			LoadCurrentEvents();
		}

		/// <summary>
		/// Unload the current game
		/// </summary>
		public void UnloadGame()
		{
			Team = null;
		}

		/// <summary>
		/// Load the history of line-ups from the manager's RPC file
		/// </summary>
		private void LoadLineUpHistory()
		{
			//get all events that feature 'SelectedLineUp' from their RPC file
			var managerEvents = Team.Manager.RolePlayCharacter.EventRecords;
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
				var boat = new Boat(_config, subjectSplit[0]);
				//position crew members and gather set-up information using details from split string
				for (var i = 0; i < boat.Positions.Count; i++)
				{
					if (subjectSplit[((i + 1) * 2) - 1].NoSpaces() != null)
					{
						boat.PositionCrew.Add(boat.Positions[i], crewMembers.Single(c => c.Name.NoSpaces() == subjectSplit[((i + 1) * 2) - 1].NoSpaces()));
						boat.PositionScores.Add(boat.Positions[i], Convert.ToInt32(subjectSplit[(i + 1) * 2]));
					}
				}
				boat.IdealMatchScore = float.Parse(subjectSplit[(boat.Positions.Count * 2) + 1]);
				boat.Score = boat.PositionScores.Values.Sum();
				boat.SelectionMistakes = new List<string>();
				for (var i = (boat.Positions.Count + 1) * 2; i < subjectSplit.Length - 2; i++)
				{
					boat.SelectionMistakes.Add(subjectSplit[i].NoSpaces());
				}
				Team.HistoricTimeOffset.Add(Convert.ToInt32(subjectSplit[subjectSplit.Length - 2]));
				Team.HistoricSessionNumber.Add(Convert.ToInt32(subjectSplit[subjectSplit.Length - 1]));
				Team.LineUpHistory.Add(boat);
			}
			CurrentRaceSession = Team.HistoricSessionNumber.LastOrDefault();
		}

		/// <summary>
		/// Load the currently running post-race events (if any)
		/// </summary>
		private void LoadCurrentEvents()
		{
			var noEventFound = false;
			var eventsFound = 0;
			var eventSectionsFound = 0;
			while (!noEventFound)
			{
				var crewMemberName = Team.Manager.LoadBelief($"PRECrew{eventsFound}({eventSectionsFound})");
				if (crewMemberName != null)
				{
					var crewMember = Team.CrewMembers.FirstOrDefault(cm => cm.Key.NoSpaces() == crewMemberName).Value
									?? Team.RetiredCrew.FirstOrDefault(cm => cm.Key.NoSpaces() == crewMemberName).Value;
					if (crewMember != null)
					{
						var evName = Team.Manager.LoadBelief($"PREEvent{eventsFound}({eventSectionsFound})");
						if (evName != null)
						{
							var ev = EventController.GetPossibleAgentDialogue(evName).FirstOrDefault()
								?? EventController.GetPossibleAgentDialogue("PostRaceEventStart").FirstOrDefault(e => e.NextState == evName);
							if (eventSectionsFound == 0)
							{
								EventController.PostRaceEvents.Add(new List<PostRaceEventState>());
							}
							var subjectString = Team.Manager.LoadBelief($"PRESubject{eventsFound}({eventSectionsFound})");
							var subjects = subjectString?.Split('_').ToList() ?? new List<string>();
							EventController.PostRaceEvents[eventsFound].Add(new PostRaceEventState(crewMember, ev, subjects));
							eventSectionsFound++;
							continue;
						}
					}
				}
				if (eventSectionsFound == 0)
				{
					noEventFound = true;
				}
				else
				{
					eventsFound++;
					eventSectionsFound = 0;
				}
			}
		}

		/// <summary>
		/// Skips all remaining practice sessions (if any)
		/// </summary>
		public void SkipToRace()
		{
			CurrentRaceSession = RaceSessionLength - 1;
		}

		public async void SaveLineUpTask(int offset, Action<bool> completed)
		{
			try
			{
				await Task.Factory.StartNew(() => SaveLineUp(offset));
				completed(true);
			}
			catch
			{
				completed(false);
				throw;
			}
		}

		/// <summary>
		/// Save the current boat line-up to the manager's RPC file
		/// </summary>
		public void SaveLineUp(int offset)
		{
			//set-up boat for saving
			var boat = Team.Boat;
			var manager = Team.Manager;
			boat.UpdateBoatScore(manager.Name);
			boat.GetIdealCrew(Team.CrewMembers, manager.Name);
			var spacelessName = manager.RolePlayCharacter.CharacterName;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventStringUnformatted = "SelectedLineUp({0},{1})";
			var boatType = boat.Type;
			var crew = string.Empty;
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
			CurrentRaceSession++;
			if (RaceSessionLength == CurrentRaceSession)
			{
				CurrentRaceSession = 0;
			}
			crew += "," + CurrentRaceSession;
			//send event with string of information within
			var eventString = string.Format(eventStringUnformatted, boatType, crew);
			manager.RolePlayCharacter.Perceive((Name)string.Format(eventBase, eventString, spacelessName));
			manager.SaveStatus();
			//store saved details in new local boat copy
			var lastBoat = new Boat(_config, boat.Type);
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
			Team.HistoricSessionNumber.Add(CurrentRaceSession);
			if (CurrentRaceSession == 0)
			{
				Team.TickCrewMembers((int)_config.ConfigValues[ConfigKeys.TicksPerSession], false);
				SelectPostRaceEvents();
				ConfirmLineUp();
			}
			else
			{
				Team.TickCrewMembers();
			}
		}

		/// <summary>
		/// Save current line-up and update CrewMember's opinions and mood based on this line-up
		/// </summary>
		private void ConfirmLineUp()
		{
			Team.ConfirmChanges();
			//reset the limits on actions and hiring/firing
			ResetAllowances();
		}

		/// <summary>
		/// Get the keys for the available Post Race Events
		/// </summary>
		public List<string> GetPostRaceEventKeys()
		{
			return EventController.GetEventKeys();
		}

		/// <summary>
		/// Select post-race events
		/// </summary>
		private void SelectPostRaceEvents()
		{
			var chance = (int)_config.ConfigValues[ConfigKeys.EventChance];
			EventController.SelectPostRaceEvents(_config, Team, chance);
		}

		/// <summary>
		/// Send player dialogue to characters involved in the event and get their replies
		/// </summary>
		public List<PostRaceEventState> SendPostRaceEvent(List<PostRaceEventState> selected)
		{
			return EventController.SendPostRaceEvent(selected, Team);
		}

		/// <summary>
		/// Deduct the cost of an action from the available allowance
		/// </summary>
		private void DeductCost(int cost)
		{
			if (ShowTutorial && _customTutorialAttributes.ContainsKey(TutorialStage) && _customTutorialAttributes[TutorialStage].ContainsKey("cost") && _customTutorialAttributes[TutorialStage]["cost"] == "false")
			{
				return;
			}
			ActionAllowance -= cost;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
		}

		/// <summary>
		/// Reset the allowances
		/// </summary>
		private void ResetAllowances()
		{
			ActionAllowance = GetStartingActionAllowance();
			CrewEditAllowance = GetStartingCrewEditAllowance();
			Team.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			Team.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Calculate how much ActionAllowance the player should start each race with
		/// </summary>
		public int GetStartingActionAllowance()
		{
			if (Team.Boat.Positions.Count == 0)
			{
				return 0;
			}
			return (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * Team.Boat.Positions.Count);
		}

		/// <summary>
		/// Deduct the cost of a hiring/firing action from the available allowance
		/// </summary>
		private void DeductCrewEditAllowance()
		{
			CrewEditAllowance--;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
		}

		/// <summary>
		/// Calculate how many hiring/firing actions player should start each race with
		/// </summary>
		public int GetStartingCrewEditAllowance()
		{
			if (Team.Boat.Positions.Count == 0)
			{
				return 0;
			}
			return (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * Team.Boat.Positions.Count;
		}

		/// <summary>
		/// If the player has enough time, hire the selected recruit and take the cost from the allowances
		/// </summary>
		public void AddRecruit(CrewMember member)
		{
			//if the player is able to take this action
			var cost = (int)_config.ConfigValues[ConfigKeys.RecruitmentCost];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && Team.CanAddToCrew())
			{
				Team.AddRecruit(member);
				DeductCrewEditAllowance();
				DeductCost(cost);
				Team.Manager.SaveStatus();
			}
		}

		/// <summary>
		/// Remove a CrewMember from the crew if the player has the allowances to do so
		/// </summary>
		public void RetireCrewMember(CrewMember crewMember)
		{
			var cost = (int)_config.ConfigValues[ConfigKeys.FiringCost];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && Team.CanRemoveFromCrew())
			{
				Team.RetireCrew(crewMember);
				DeductCrewEditAllowance();
				DeductCost(cost);
				Team.Manager.SaveStatus();
			}
		}

		/// <summary>
		/// Send player meeting dialogue to a CrewMember, getting their response in return
		/// </summary>
		public List<string> SendMeetingEvent(string eventName, CrewMember member)
		{
			var cost = (int)GetConfigValue((ConfigKeys)Enum.Parse(typeof(ConfigKeys), eventName + "Cost"), member);
			if (cost <= ActionAllowance)
			{
				var reply = EventController.SendMeetingEvent(eventName, member, Team);
				DeductCost(cost);
				Team.Manager.SaveStatus();
				return reply;
			}
			return new List<string>();
		}

		/// <summary>
		/// Get the value from the config
		/// </summary>
		public float GetConfigValue(ConfigKeys eventKey, CrewMember member = null)
		{
			if (eventKey == ConfigKeys.StatRevealCost && member != null)
			{
				return ((int)(member.RevealedSkills.Count(s => s.Value != 0) * GetConfigValue(ConfigKeys.StatRevealCost))) + (member.RevealedSkills.All(s => s.Value != 0) ? 0 : 1);
			}
			return _config.ConfigValues[eventKey];
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all recruits
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitMembersEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			var cost = (int)_config.ConfigValues[ConfigKeys.SendRecruitmentQuestionCost];
			if (cost <= ActionAllowance)
			{
				DeductCost(cost);
				Team.Manager.SaveStatus();
				return EventController.SendRecruitEvent(skill, members);
			}
			var replies = new Dictionary<CrewMember, string>();
			members.ForEach(member => replies.Add(member, string.Empty));
			return replies;
		}

		/// <summary>
		/// Set custom attributes for the tutorial
		/// </summary>
		public void SetCustomTutorialAttributes(int stage, Dictionary<string, string> attributes)
		{
			foreach (var att in attributes)
			{
				if (!_customTutorialAttributes.ContainsKey(stage))
				{
					_customTutorialAttributes.Add(stage, new Dictionary<string, string>());
				}
				if (_customTutorialAttributes[stage].ContainsKey(att.Key))
				{
					_customTutorialAttributes[stage][att.Key] = att.Value;
				}
				else
				{
					_customTutorialAttributes[stage].Add(att.Key, att.Value);
				}
			}
		}

		/// <summary>
		/// Save the player's current progress through the tutorial for this game
		/// </summary>
		public void SaveTutorialProgress(int saveIndex, bool finished = false)
		{
			var stageToSave = saveIndex;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.TutorialStage.GetDescription(), stageToSave.ToString());
			TutorialStage++;
			if (finished)
			{
				ShowTutorial = false;
				RaceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength];
				Team.Manager.UpdateSingleBelief(NPCBeliefs.ShowTutorial.GetDescription(), ShowTutorial.ToString());
				_customTutorialAttributes.Clear();
			}
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Save the results of the questionnaire
		/// </summary>
		public void SaveQuestionnaireResults(Dictionary<string, int> results)
		{
			foreach (var result in results)
			{
				Team.Manager.UpdateSingleBelief($"QuestionnaireMeaning({result.Key})", result.Value.ToString());
			}
			QuestionnaireCompleted = true;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.QuestionnaireCompleted.GetDescription(), QuestionnaireCompleted.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Get the percentage that the player selected each management style during the game events and post game questionnaire
		/// </summary>
		public Dictionary<string, float> GatherManagementStyles()
		{
			var managerBeliefs = Team.Manager.RolePlayCharacter.GetAllBeliefs().ToList();
			var possibleBeliefs = EventController.GetPlayerEventStyles().Select(s => s.ToLower()).ToArray();
			var gameMeanings = managerBeliefs.Where(b => b.Name.StartsWith("Meaning")).ToList();
			var managementStyles = gameMeanings.Select(b => new KeyValuePair<string, int>(b.Name.Split('(', ')')[1].ToLower(), int.Parse(b.Value))).ToDictionary(b => b.Key, b => b.Value);
			var managementTotal = managementStyles.Values.ToList().Sum();
			foreach (var bel in possibleBeliefs)
			{
				if (!managementStyles.ContainsKey(bel))
				{
					managementStyles.Add(bel, 0);
				}
			}

			var managementPercentage = managementStyles.Select(m => new KeyValuePair<string, float>(m.Key, m.Value / (managementTotal * 2f))).ToDictionary(m => m.Key, m => m.Value);

			var questionnaireMeanings = managerBeliefs.Where(b => b.Name.StartsWith("QuestionnaireMeaning")).ToList();
			managementStyles = questionnaireMeanings.Select(b => new KeyValuePair<string, int>(b.Name.Split('(', ')')[1].ToLower(), int.Parse(b.Value))).ToDictionary(b => b.Key, b => b.Value);
			managementTotal = managementStyles.Values.ToList().Sum();
			foreach (var style in managementStyles)
			{
				managementPercentage[style.Key] += style.Value / (managementTotal * 2f);
			}
			managementPercentage = managementPercentage.OrderByDescending(m => m.Value).ToDictionary(b => b.Key, b => b.Value);

			return managementPercentage;
		}

		/// <summary>
		/// Get the percentage that the player selected each leadership style based on their selected management styles
		/// </summary>
		public Dictionary<string, float> GatherLeadershipStyles()
		{
			var managementStyles = GatherManagementStyles();
			var managementPercentage = new Dictionary<string, float>
											{
												{ "laissez-faire", managementStyles["avoiding"] + managementStyles["competing"] },
												{ "transformational", managementStyles["collaborating"] + managementStyles["accommodating"] },
												{ "transactional", managementStyles["compromising"] }
											};
			return managementPercentage;
		}

		/// <summary>
		/// Get the most commonly selected leadership style(s)
		/// </summary>
		public string[] GetPrevalentLeadershipStyle()
		{
			var managementStyles = GatherLeadershipStyles();
			return managementStyles.Where(m => Math.Abs(m.Value - managementStyles.Values.Max()) < 0.01f).ToDictionary(b => b.Key, b => b.Value).Keys.ToArray();
		}

		public int GetTotalRaceCount()
		{
			return _config.GameConfig.PromotionTriggers.Sum(p => p.ScoreMetSinceLast);
		}
	}
}

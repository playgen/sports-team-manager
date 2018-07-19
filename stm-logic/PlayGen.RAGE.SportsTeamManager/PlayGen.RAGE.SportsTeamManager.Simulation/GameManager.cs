using IntegratedAuthoringTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RolePlayCharacter;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to access functionality contained within other classes and create/load/save games
	/// </summary>
	public class GameManager
	{
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
			ConfigLoad(platform);
		}

		private async void ConfigLoad(Platform platform = Platform.Windows)
		{
			await Task.Factory.StartNew(() => new ConfigStore(platform));
		}

		/// <summary>
		/// Validate that the provided game config should result in no errors and can be processed correctly
		/// </summary>
		private void ValidateGameConfig()
		{
			var invalidString = string.Empty;
			var promotionTriggers = ConfigStore.GameConfig.PromotionTriggers;
			//is there a promotion trigger that has a StartType of 'Start'?
			if (promotionTriggers.All(pt => pt.StartType != "Start"))
			{
				invalidString += "Game Config requires one PromotionTrigger with StartTpe \"Start\".\n";
			}
			
			foreach (var promotion in promotionTriggers)
			{
				//is there a promotion trigger that has a StartType of 'Start' and has a value less than 0 for ScoreMetSinceLast?
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
				if (promotion.StartType != "Start" && ConfigStore.BoatTypes.All(bt => bt.Key != promotion.StartType))
				{
					invalidString += $"StartType {promotion.StartType} is not an existing BoatType.\n";
				}
				//is there a promotion trigger that does not have a NewType of 'Finish' and has a NewType that isn't provided in BoatConfig.json?
				if (promotion.NewType != "Finish" && ConfigStore.BoatTypes.All(bt => bt.Key != promotion.NewType))
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
			var eventTriggers = ConfigStore.GameConfig.EventTriggers;
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
				if (ev.StartBoatType != null && ConfigStore.BoatTypes.All(bt => bt.Key != ev.StartBoatType))
				{
					invalidString += $"StartBoatType {ev.StartBoatType} is not an existing BoatType.\n";
				}
				//is there a post race event trigger that contains an invalid boat type?
				if (ev.EndBoatType != null && ConfigStore.BoatTypes.All(bt => bt.Key != ev.EndBoatType))
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

		public async void NewGameTask(string storageLocation, string name, Color teamColorsPrimary, Color teamColorsSecondary, string managerName, bool showTutorial, string nation, Action<bool> completed, List<CrewMember> crew = null)
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
		public void NewGame(string storageLocation, string name, Color teamColorsPrimary, Color teamColorsSecondary, string managerName, bool showTutorial, string nation, List<CrewMember> crew = null)
		{
			UnloadGame();
			//create folder and iat file for game
			var combinedStorageLocation = Path.Combine(storageLocation, name);
			Directory.CreateDirectory(combinedStorageLocation);
			var iat = ConfigStore.IntegratedAuthoringTool.Copy();
			EventController = new EventController(iat);
			ValidateGameConfig();
			//set up boat and team
			var initialType = ConfigStore.GameConfig.PromotionTriggers.First(pt => pt.StartType == "Start").NewType;
			var boat = new Boat(initialType);
			Team = new Team(iat, storageLocation, name, nation, boat);
			var positionCount = boat.PositionCount;
			Team.TeamColorsPrimary = teamColorsPrimary;
			Team.TeamColorsSecondary = teamColorsSecondary;
			iat.ScenarioName = name;
			iat.SetFutureFilePath(Path.Combine(combinedStorageLocation, name + ".iat"));
			//create manager
			var manager = new Person
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
					var newMember = new CrewMember(boat.GetWeakestPosition(Team.CrewMembers.Values.ToList()), Team.Nationality);
					Team.UniqueNameCheck(newMember);
					Team.AddCrewMember(newMember);
				}
			}
			if (!initialCrew)
			{
				crew.ForEach(cm => Team.AddCrewMember(cm));
			}
			//set up initial values
			ActionAllowance = GetStartingActionAllowance();
			CrewEditAllowance = GetStartingCrewEditAllowance();
			RaceSessionLength = showTutorial ? ConfigKey.TutorialRaceSessionLength.GetIntValue() : ConfigKey.RaceSessionLength.GetIntValue();
			CurrentRaceSession = 0;
			ShowTutorial = showTutorial;
			TutorialStage = 0;
			_customTutorialAttributes = new Dictionary<int, Dictionary<string, string>>();
			QuestionnaireCompleted = false;
			//create manager files and store game attribute details
			manager.CreateFile(iat, combinedStorageLocation);
			manager.UpdateSingleBelief(NPCBelief.Position, "Manager");
			manager.UpdateSingleBelief(NPCBelief.BoatType, boat.Type);
			manager.UpdateSingleBelief(NPCBelief.ShowTutorial, ShowTutorial);
			manager.UpdateSingleBelief(NPCBelief.QuestionnaireCompleted, QuestionnaireCompleted);
			manager.UpdateSingleBelief(NPCBelief.TutorialStage, TutorialStage);
			manager.UpdateSingleBelief(NPCBelief.Nationality, nation);
			manager.UpdateSingleBelief(NPCBelief.ActionAllowance, ActionAllowance);
			manager.UpdateSingleBelief(NPCBelief.CrewEditAllowance, CrewEditAllowance);
			manager.UpdateSingleBelief(NPCBelief.TeamColorRedPrimary, teamColorsPrimary.R);
			manager.UpdateSingleBelief(NPCBelief.TeamColorGreenPrimary, teamColorsPrimary.G);
			manager.UpdateSingleBelief(NPCBelief.TeamColorBluePrimary, teamColorsPrimary.B);
			manager.UpdateSingleBelief(NPCBelief.TeamColorRedSecondary, teamColorsSecondary.R);
			manager.UpdateSingleBelief(NPCBelief.TeamColorGreenSecondary, teamColorsSecondary.G);
			manager.UpdateSingleBelief(NPCBelief.TeamColorBlueSecondary, teamColorsSecondary.B);
			manager.SaveStatus();

			var names = Team.CrewMembers.Keys.ToList();
			names.Add(managerName);

			//set up files and details for each CrewMember
			foreach (var member in Team.CrewMembers.Values)
			{
				member.CreateTeamMemberFile(iat, combinedStorageLocation, names, Team.TeamColorsPrimary, Team.TeamColorsSecondary, initialCrew);
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
						if (game != null && string.Equals(game.ScenarioName, gameName, StringComparison.CurrentCultureIgnoreCase))
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
				var position = rpc.GetBeliefValue(NPCBelief.Position.Description());
				nameList.Add(rpc.BodyName);
				//if this character is the manager, load the game details from this file and set this character as the manager
				if (position == "Manager")
				{
					var person = new Person(rpc);
					var boat = new Boat(person.LoadBelief(NPCBelief.BoatType));
					var nation = person.LoadBelief(NPCBelief.Nationality);
					ShowTutorial = bool.Parse(person.LoadBelief(NPCBelief.ShowTutorial));
					TutorialStage = Convert.ToInt32(person.LoadBelief(NPCBelief.TutorialStage));
					_customTutorialAttributes = new Dictionary<int, Dictionary<string, string>>();
					QuestionnaireCompleted = bool.Parse(person.LoadBelief(NPCBelief.QuestionnaireCompleted) ?? "false");
					Team = new Team(iat, storageLocation, iat.ScenarioName, nation, boat);
					if (boat.Type == "Finish")
					{
						Team.Finished = true;
					}
					ActionAllowance = Convert.ToInt32(person.LoadBelief(NPCBelief.ActionAllowance));
					CrewEditAllowance = Convert.ToInt32(person.LoadBelief(NPCBelief.CrewEditAllowance));
					RaceSessionLength = ShowTutorial ? ConfigKey.TutorialRaceSessionLength.GetIntValue() : ConfigKey.RaceSessionLength.GetIntValue();
					Team.TeamColorsPrimary = new Color(Convert.ToInt32(person.LoadBelief(NPCBelief.TeamColorRedPrimary)), Convert.ToInt32(person.LoadBelief(NPCBelief.TeamColorGreenPrimary)), Convert.ToInt32(person.LoadBelief(NPCBelief.TeamColorBluePrimary)));
					Team.TeamColorsSecondary = new Color(Convert.ToInt32(person.LoadBelief(NPCBelief.TeamColorRedSecondary)), Convert.ToInt32(person.LoadBelief(NPCBelief.TeamColorGreenSecondary)), Convert.ToInt32(person.LoadBelief(NPCBelief.TeamColorBlueSecondary)));
					Team.Manager = person;
					continue;
				}
				//set up every other character as a CrewManager, making sure to separate retired and recruits
				var crewMember = new CrewMember(rpc);
				switch (position)
				{
					case "Retired":
						Team.RetiredCrew.Add(crewMember.Name, crewMember);
						Team.RetiredCrew.Values.ToList().ForEach(cm => cm.LoadBeliefs(nameList));
						continue;
					case "Recruit":
						Team.Recruits.Add(crewMember.Name, crewMember);
						Team.Recruits.Values.ToList().ForEach(cm => cm.LoadBeliefs(nameList));
						continue;
				}
				crewList.Add(crewMember);
			}
			crewList.ForEach(cm => Team.AddCrewMember(cm));
			crewList.ForEach(cm => cm.LoadBeliefs(nameList, Team));
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
			var lineUpEvents = Team.Manager.RolePlayCharacter.EventRecords.Where(e => e.Event.Contains("SelectedLineUp")).Select(e => e.Event).ToList();
			var crewMembers = Team.CrewMembers.Values.ToList();
			crewMembers.AddRange(Team.RetiredCrew.Values.ToList());

			foreach (var lineup in lineUpEvents)
			{
				//split up the string of details saved with this event
				var splitAfter = lineup.Split('(')[2];
				splitAfter = splitAfter.Split(')')[0];
				var subjectSplit = splitAfter.Split(',');
				//set up the boat
				var boat = new Boat(subjectSplit[0]);
				//position crew members and gather set-up information using details from split string
				for (var i = 0; i < boat.PositionCount; i++)
				{
					var crewMember = subjectSplit[((i + 1) * 2) - 1];
					if (!string.IsNullOrEmpty(crewMember))
					{
						boat.PositionCrew.Add(boat.Positions[i], crewMembers.Single(c => c.Name.NoSpaces() == crewMember.NoSpaces()));
						boat.PositionScores.Add(boat.Positions[i], Convert.ToInt32(subjectSplit[(i + 1) * 2]));
					}
				}
				boat.PerfectSelections = Convert.ToInt32(subjectSplit[(boat.PositionCount * 2) + 1]);
				boat.ImperfectSelections = Convert.ToInt32(subjectSplit[(boat.PositionCount * 2) + 2]);
				boat.Score = boat.PositionScores.Values.Sum();
				boat.SelectionMistakes = new List<string>();
				for (var i = (boat.PositionCount * 2) + 3; i < subjectSplit.Length - 2; i++)
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
					var crewMember = Team.CrewMembers.FirstOrDefault(cm => cm.Key.NoSpaces() == crewMemberName).Value ?? Team.RetiredCrew.FirstOrDefault(cm => cm.Key.NoSpaces() == crewMemberName).Value;
					if (crewMember != null)
					{
						var evName = Team.Manager.LoadBelief($"PREEvent{eventsFound}({eventSectionsFound})");
						if (evName != null)
						{
							var ev = EventController.GetPossibleAgentDialogue(evName).FirstOrDefault() ?? EventController.GetPossibleAgentDialogue("PostRaceEventStart").FirstOrDefault(e => e.NextState == evName);
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
			boat.UpdateScore(manager.Name);
			boat.GetIdealCrew(Team.CrewMembers, manager.Name);
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
			crew += "," + boat.PerfectSelections + "," + boat.ImperfectSelections;
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
			var eventString = EventHelper.ActionStart("Player", $"SelectedLineUp({boatType},{crew})", manager.Name.NoSpaces());
			manager.RolePlayCharacter.Perceive(eventString);
			manager.SaveStatus();
			//store saved details in new local boat copy
			var lastBoat = boat.Copy();
			lastBoat.PositionCrew.Clear();
			lastBoat.PositionScores.Clear();
			foreach (var position in boat.Positions)
			{
				if (boat.PositionCrew.ContainsKey(position))
				{
					lastBoat.PositionCrew.Add(position, boat.PositionCrew[position]);
					lastBoat.PositionScores.Add(position, boat.PositionScores[position]);
				}
			}
			Team.LineUpHistory.Add(lastBoat);
			Team.HistoricTimeOffset.Add(offset);
			Team.HistoricSessionNumber.Add(CurrentRaceSession);
			if (CurrentRaceSession == 0)
			{
				Team.TickCrewMembers(ConfigKey.TicksPerSession.GetIntValue(), false);
				EventController.SelectPostRaceEvents(Team, ConfigKey.EventChance.GetIntValue());
				Team.ConfirmChanges();
				ResetAllowances();
			}
			else
			{
				Team.TickCrewMembers();
			}
		}

		/// <summary>
		/// Send player dialogue to characters involved in the event and get their replies
		/// </summary>
		public List<PostRaceEventState> SendPostRaceEvent(List<PostRaceEventState> selected)
		{
			return EventController.SendPostRaceEvent(selected, Team);
		}

		/// <summary>
		/// Reset the allowances
		/// </summary>
		private void ResetAllowances()
		{
			ActionAllowance = GetStartingActionAllowance();
			CrewEditAllowance = GetStartingCrewEditAllowance();
			Team.Manager.UpdateSingleBelief(NPCBelief.ActionAllowance, ActionAllowance);
			Team.Manager.UpdateSingleBelief(NPCBelief.CrewEditAllowance, CrewEditAllowance);
			Team.Manager.SaveStatus();
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
			Team.Manager.UpdateSingleBelief(NPCBelief.ActionAllowance, ActionAllowance);
		}

		/// <summary>
		/// Calculate how much ActionAllowance the player should start each race with
		/// </summary>
		public int GetStartingActionAllowance()
		{
			if (Team.Boat.PositionCount == 0)
			{
				return 0;
			}
			return ConfigKey.DefaultActionAllowance.GetIntValue() + (ConfigKey.ActionAllowancePerPosition.GetIntValue() * Team.Boat.PositionCount);
		}

		/// <summary>
		/// Deduct the cost of a hiring/firing action from the available allowance
		/// </summary>
		private void DeductCrewEditAllowance()
		{
			CrewEditAllowance--;
			Team.Manager.UpdateSingleBelief(NPCBelief.CrewEditAllowance, CrewEditAllowance);
		}

		/// <summary>
		/// Calculate how many hiring/firing actions player should start each race with
		/// </summary>
		public int GetStartingCrewEditAllowance()
		{
			if (Team.Boat.PositionCount == 0)
			{
				return 0;
			}
			return ConfigKey.CrewEditAllowancePerPosition.GetIntValue() * Team.Boat.PositionCount;
		}

		/// <summary>
		/// If the player has enough time, hire the selected recruit and take the cost from the allowances
		/// </summary>
		public void AddRecruit(CrewMember member)
		{
			//if the player is able to take this action
			var cost = ConfigKey.RecruitmentCost.GetIntValue();
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
			var cost = ConfigKey.FiringCost.GetIntValue();
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
			var cost = (int)GetConfigValue((ConfigKey)Enum.Parse(typeof(ConfigKey), eventName + "Cost"), member);
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
		public float GetConfigValue(ConfigKey eventKey, CrewMember member = null)
		{
			if (eventKey == ConfigKey.StatRevealCost && member != null)
			{
				return (int)(member.RevealedSkills.Count(s => s.Value != 0) * GetConfigValue(ConfigKey.StatRevealCost)) + (member.RevealedSkills.All(s => s.Value != 0) ? 0 : 1);
			}
			return eventKey.GetValue();
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all recruits
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitmentEvent(Skill skill)
		{
			var members = Team.Recruits.Values.ToList();
			var cost = ConfigKey.SendRecruitmentQuestionCost.GetIntValue();
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
			Team.Manager.UpdateSingleBelief(NPCBelief.TutorialStage, stageToSave);
			TutorialStage++;
			if (finished)
			{
				ShowTutorial = false;
				RaceSessionLength = ConfigKey.RaceSessionLength.GetIntValue();
				Team.Manager.UpdateSingleBelief(NPCBelief.ShowTutorial, ShowTutorial);
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
				Team.Manager.UpdateSingleBelief($"QuestionnaireMeaning({result.Key})", result.Value);
			}
			QuestionnaireCompleted = true;
			Team.Manager.UpdateSingleBelief(NPCBelief.QuestionnaireCompleted, QuestionnaireCompleted);
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
			var managementStyles = gameMeanings.Select(b => new KeyValuePair<string, int>(b.Name.Split('(', ')')[1].ToLower(), Convert.ToInt32(b.Value))).ToDictionary(b => b.Key, b => b.Value);
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
			managementStyles = questionnaireMeanings.Select(b => new KeyValuePair<string, int>(b.Name.Split('(', ')')[1].ToLower(), Convert.ToInt32(b.Value))).ToDictionary(b => b.Key, b => b.Value);
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
			return ConfigStore.GameConfig.PromotionTriggers.Sum(p => p.ScoreMetSinceLast);
		}
	}
}

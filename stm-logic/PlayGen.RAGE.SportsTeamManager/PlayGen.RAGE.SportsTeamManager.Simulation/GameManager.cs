using IntegratedAuthoringTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RolePlayCharacter;

using WellFormedNames;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Used to access functionality contained within other classes and create/load/save games
	/// </summary>
	public class GameManager
	{
		private readonly ConfigStore config;
		private EventController eventController;

		private Dictionary<string, string> _customTutorialAttributes { get; set; }
		public Team Team { get; private set; }
		public int ActionAllowance { get; private set; }
		public int CrewEditAllowance { get; private set; }
		public int RaceSessionLength { get; private set; }
		public int CurrentRaceSession { get; private set; }
		public bool ShowTutorial { get; private set; }
		public int TutorialStage { get; private set; }
		public bool QuestionnaireCompleted { get; private set; }
		public EventController EventController => eventController;

		/// <summary>
		/// GameManager Constructor
		/// </summary>
		public GameManager(bool android = false)
		{
			config = new ConfigStore(android);
		}

		private void ValidateGameConfig()
		{
			var invalidString = string.Empty;
			var promotionTriggers = config.GameConfig.PromotionTriggers;
			if (promotionTriggers.All(pt => pt.StartType != "Start"))
			{
				invalidString += "Game Config requires one PromotionTrigger with StartTpe \"Start\".\n";
			}
			
			foreach (var promotion in promotionTriggers)
			{
				if (promotion.StartType != "Start" && promotion.ScoreMetSinceLast <= 0)
				{
					invalidString += string.Format("ScoreMetSinceLast for StartType {0} and NewType {1} should be greater than 0.\n", promotion.StartType, promotion.NewType);
				}
				if (promotion.StartType == promotion.NewType)
				{
					invalidString += string.Format("Invalid PromotionTrigger in Game Config for {0}, will result in changing to same boat type.\n", promotion.StartType);
				}
				if (promotion.StartType != "Start" && config.BoatTypes.All(bt => bt.Key != promotion.StartType))
				{
					invalidString += string.Format("StartType {0} is not an existing BoatType.\n", promotion.StartType);
				}
				if (promotion.NewType != "Finish" && config.BoatTypes.All(bt => bt.Key != promotion.NewType))
				{
					invalidString += string.Format("NewType {0} is not an existing BoatType.\n", promotion.NewType);
				}
				if (promotionTriggers.Any(pt => pt != promotion && pt.StartType == promotion.StartType && pt.ScoreMetSinceLast <= promotion.ScoreMetSinceLast && pt.ScoreRequired <= promotion.ScoreRequired))
				{
					invalidString += string.Format("PromotionTrigger with StartType {0}, NewType {1} will never be triggered.\n", promotion.StartType, promotion.NewType);
				}
				if (promotion.StartType != "Start" && promotionTriggers.All(pt => pt.NewType != promotion.StartType))
				{
					invalidString += string.Format("PromotionTrigger with StartType {0}, NewType {1} will never be triggered.\n", promotion.StartType, promotion.NewType);
				}
			}
			var eventTriggers = config.GameConfig.EventTriggers;
			var postRaceEvents = EventController.GetPossiblePostRaceDialogue();
			var postRaceNames = postRaceEvents.Select(pre => pre.NextState).ToList();
			foreach (var ev in eventTriggers)
			{
				if (postRaceNames.All(prn => prn != ev.EventName))
				{
					invalidString += string.Format("{0} is not an existing event name.\n", ev.EventName);
				}
				if (ev.StartBoatType != null && config.BoatTypes.All(bt => bt.Key != ev.StartBoatType))
				{
					invalidString += string.Format("StartBoatType {0} is not an existing BoatType.\n", ev.StartBoatType);
				}
				if (ev.EndBoatType != null && config.BoatTypes.All(bt => bt.Key != ev.EndBoatType))
				{
					invalidString += string.Format("EndBoatType {0} is not an existing BoatType.\n", ev.EndBoatType);
				}
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

		/// <summary>
		/// Create a new game
		/// </summary>
		public void NewGame(string storageLocation, string name, byte[] teamColorsPrimary, byte[] teamColorsSecondary, string managerName, bool showTutorial, string nation, List<CrewMember> crew = null)
		{
			UnloadGame();
			//create folder and iat file for game
			var combinedStorageLocation = Path.Combine(storageLocation, name);
			Directory.CreateDirectory(combinedStorageLocation);
			var iat = ConfigStore.IntegratedAuthoringTool;
			var help = ConfigStore.HelpIntegratedAuthoringTool.GetDialogueActionsByState(IATConsts.AGENT, "LearningPill").ToList();
			eventController = new EventController(iat, help);
			ValidateGameConfig();
			//set up boat and team
			var initialType = config.GameConfig.PromotionTriggers.First(pt => pt.StartType == "Start").NewType;
			var boat = new Boat(config, initialType);
			Team = new Team(iat, storageLocation, config, name, nation, boat);
			var positionCount = boat.Positions.Count;
			Team.TeamColorsPrimary = new Color(teamColorsPrimary[0], teamColorsPrimary[1], teamColorsPrimary[2], 255);
			Team.TeamColorsSecondary = new Color(teamColorsSecondary[0], teamColorsSecondary[1], teamColorsSecondary[2], 255);
			iat.ScenarioName = name;
			iat.SaveToFile(Path.Combine(combinedStorageLocation, name + ".iat"));
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
					var newMember = new CrewMember(boat.GetWeakPosition(Team.CrewMembers.Values.Concat(Team.Recruits.Values).ToList()), Team.Nationality, config);
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
			RaceSessionLength = showTutorial ? (int)config.ConfigValues[ConfigKeys.TutorialRaceSessionLength] : (int)config.ConfigValues[ConfigKeys.RaceSessionLength];
			CurrentRaceSession = 0;
			ShowTutorial = showTutorial;
			TutorialStage = 0;
			_customTutorialAttributes = new Dictionary<string, string>();
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
			Team.CreateRecruits();
			config.LoadAssets();

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

		/// <summary>
		/// Check if the location provided contains an existing game
		/// </summary>
		public bool CheckIfGameExists(string storageLocation, string gameName)
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
							return true;
						}
					}
					//do not want loading errors to result in exceptions, so catch all
					catch
					{
						return false;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Load an existing game
		/// </summary>
		public void LoadGame(string storageLocation, string boatName)
		{
			UnloadGame();
			var help = ConfigStore.HelpIntegratedAuthoringTool.GetDialogueActionsByState(IATConsts.AGENT, "LearningPill").ToList();
			//get the iat file and all characters for this game
			var combinedStorageLocation = Path.Combine(storageLocation, boatName);
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(Path.Combine(combinedStorageLocation, boatName + ".iat"));
			eventController = new EventController(iat, help);
			ValidateGameConfig();
			var characterList = iat.GetAllCharacterSources();

			var crewList = new List<CrewMember>();
			var nameList = new List<string>();
			foreach (var character in characterList)
			{
				var rpc = RolePlayCharacterAsset.LoadFromFile(character.Source);
				rpc.LoadAssociatedAssets();
				var position = rpc.GetBeliefValue(NPCBeliefs.Position.GetDescription());
				//if this character is the manager, load the game details from this file and set this character as the manager
				if (position == "Manager")
				{
					var person = new Person(rpc);
					nameList.Add(person.Name);
					var boat = new Boat(config, person.LoadBelief(NPCBeliefs.BoatType.GetDescription()));
					var nation = person.LoadBelief(NPCBeliefs.Nationality.GetDescription());
					ShowTutorial = bool.Parse(person.LoadBelief(NPCBeliefs.ShowTutorial.GetDescription()));
					TutorialStage = int.Parse(person.LoadBelief(NPCBeliefs.TutorialStage.GetDescription()));
					_customTutorialAttributes = new Dictionary<string, string>();
					QuestionnaireCompleted = bool.Parse(person.LoadBelief(NPCBeliefs.QuestionnaireCompleted.GetDescription()) ?? "false");
					Team = new Team(iat, storageLocation, config, iat.ScenarioName, nation, boat);
					if (boat.Type == "Finish")
					{
						Team.Finished = true;
					}
					ActionAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.ActionAllowance.GetDescription()));
					CrewEditAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.CrewEditAllowance.GetDescription()));
					RaceSessionLength = ShowTutorial ? (int)config.ConfigValues[ConfigKeys.TutorialRaceSessionLength] : (int)config.ConfigValues[ConfigKeys.RaceSessionLength];
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
		/// Load the history of line-ups from the manager's EA file
		/// </summary>
		private void LoadLineUpHistory()
		{
			//get all events that feature 'SelectedLineUp' from their EA file
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

		private void LoadCurrentEvents()
		{
			bool noEventFound = false;
			int eventsFound = 0;
			int eventSectionsFound = 0;
			while (!noEventFound)
			{
				var crewMemberName = Team.Manager.LoadBelief(String.Format("PRECrew{0}({1})", eventsFound, eventSectionsFound));
				if (crewMemberName != "null")
				{
					var crewMember = Team.CrewMembers.FirstOrDefault(cm => cm.Key.NoSpaces() == crewMemberName).Value
									?? Team.RetiredCrew.FirstOrDefault(cm => cm.Key.NoSpaces() == crewMemberName).Value;
					if (crewMember != null)
					{
						var evName = Team.Manager.LoadBelief(String.Format("PREEvent{0}({1})", eventsFound, eventSectionsFound));
						if (evName != "null")
						{
							var ev = eventController.GetPossibleAgentDialogue(evName).FirstOrDefault()
								?? eventController.GetPossibleAgentDialogue("PostRaceEventStart").FirstOrDefault(e => e.NextState == evName);
							if (eventSectionsFound == 0)
							{
								eventController.PostRaceEvents.Add(new List<PostRaceEventState>());
							}
							var subjectString = Team.Manager.LoadBelief(String.Format("PRESubject{0}({1})", eventsFound, eventSectionsFound));
							var subjects = subjectString != "null" ? subjectString.Split('_').ToList() : new List<string>();
							eventController.PostRaceEvents[eventsFound].Add(new PostRaceEventState(crewMember, ev, subjects));
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
			manager.RolePlayCharacter.Perceive(new[] { (Name)string.Format(eventBase, eventString, spacelessName) });
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
			Team.HistoricSessionNumber.Add(CurrentRaceSession);
			Team.TickCrewMembers(0);
			if (CurrentRaceSession == 0)
			{
				Team.TickCrewMembers((int)config.ConfigValues[ConfigKeys.TicksPerSession]);
				SelectPostRaceEvents();
				ConfirmLineUp();
			}
		}

		/// <summary>
		/// Save current line-up and update CrewMember's opinions and mood based on this line-up
		/// </summary>
		private void ConfirmLineUp()
		{
			Team.ConfirmChanges(ActionAllowance);
			//reset the limits on actions and hiring/firing
			ResetActionAllowance();
			ResetCrewEditAllowance();
		}

		/// <summary>
		/// Select a random post-race event
		/// </summary>
		private void SelectPostRaceEvents()
		{
			var chance = (int)config.ConfigValues[ConfigKeys.EventChance];
			eventController.SelectPostRaceEvents(config, Team, chance);
		}

		/// <summary>
		/// Send player dialogue to characters involved in the event and get their replies
		/// </summary>
		public List<PostRaceEventState> SendPostRaceEvent(List<PostRaceEventState> selected)
		{
			return eventController.SendPostRaceEvent(selected, Team);
		}

		/// <summary>
		/// Deduct the cost of an action from the available allowance
		/// </summary>
		private void DeductCost(int cost)
		{
			if (_customTutorialAttributes.ContainsKey("cost") && _customTutorialAttributes["cost"] == "false")
			{
				return;
			}
			ActionAllowance -= cost;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Reset the amount of allowance actions
		/// </summary>
		private void ResetActionAllowance()
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
			if (Team.Boat.Positions.Count == 0)
			{
				return 0;
			}
			return (int)config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * Team.Boat.Positions.Count);
		}

		/// <summary>
		/// Deduct the cost of a hiring/firing action from the available allowance
		/// </summary>
		private void DeductCrewEditAllowance()
		{
			CrewEditAllowance--;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			Team.Manager.SaveStatus();
		}

		/// <summary>
		/// Reset the amount of hiring/firing actions allowed
		/// </summary>
		private void ResetCrewEditAllowance()
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
			if (Team.Boat.Positions.Count == 0)
			{
				return 0;
			}
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
		public List<string> SendMeetingEvent(string eventName, CrewMember member)
		{
			var cost = (int)GetConfigValue((ConfigKeys)Enum.Parse(typeof(ConfigKeys), eventName + "Cost"));
			if (cost <= ActionAllowance)
			{
				var reply = eventController.SendMeetingEvent(eventName, member, Team);
				DeductCost(cost);
				return reply;
			}
			return new List<string>();
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
				DeductCost(cost);
				return eventController.SendRecruitEvent(skill, members);
			}
			var replies = new Dictionary<CrewMember, string>();
			members.ForEach(member => replies.Add(member, string.Empty));
			return replies;
		}

		public void SetCustomTutorialAttributes(Dictionary<string, string> attributes)
		{
			foreach (var att in attributes)
			{
				if (_customTutorialAttributes.ContainsKey(att.Key))
				{
					_customTutorialAttributes[att.Key] = att.Value;
				}
				else
				{
					_customTutorialAttributes.Add(att.Key, att.Value);
				}
			}
		}

		public void SaveTutorialProgress(int saveAmount, bool finished = false)
		{
			var stageToSave = TutorialStage + saveAmount;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.TutorialStage.GetDescription(), stageToSave.ToString());
			TutorialStage++;
			if (finished)
			{
				ShowTutorial = false;
				RaceSessionLength = (int)config.ConfigValues[ConfigKeys.RaceSessionLength];
				Team.Manager.UpdateSingleBelief(NPCBeliefs.ShowTutorial.GetDescription(), ShowTutorial.ToString());
				_customTutorialAttributes.Clear();
			}
			Team.Manager.SaveStatus();
		}

		public void SaveQuestionnaireResults(Dictionary<string, int> results)
		{
			foreach (var result in results)
			{
				Team.Manager.UpdateSingleBelief(string.Format("QuestionnaireMeaning({0})", result.Key), result.Value.ToString());
			}
			QuestionnaireCompleted = true;
			Team.Manager.UpdateSingleBelief(NPCBeliefs.QuestionnaireCompleted.GetDescription(), QuestionnaireCompleted.ToString());
			Team.Manager.SaveStatus();
		}

		public Dictionary<string, float> GatherManagementStyles()
		{
			var managerBeliefs = Team.Manager.RolePlayCharacter.GetAllBeliefs().ToList();
			var possibleBeliefs = eventController.GetPlayerEventStyles();
			var gameMeanings = managerBeliefs.Where(b => b.Name.StartsWith("PossibleMeaning")).ToList();
			var managementStyles = gameMeanings.Select(b => new KeyValuePair<string, int>(b.Name.Split('(', ')')[1], int.Parse(b.Value))).ToDictionary(b => b.Key, b => b.Value);
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
			managementStyles = questionnaireMeanings.Select(b => new KeyValuePair<string, int>(b.Name.Split('(', ')')[1], int.Parse(b.Value))).ToDictionary(b => b.Key, b => b.Value);
			managementTotal = managementStyles.Values.ToList().Sum();
			foreach (var style in managementStyles)
			{
				managementPercentage[style.Key] += style.Value / (managementTotal * 2f);
			}
			managementPercentage = managementPercentage.OrderByDescending(m => m.Value).ToDictionary(b => b.Key, b => b.Value);

			return managementPercentage;
		}

		public string[] GetPrevalentLeadershipStyle()
		{
			var managerBeliefs = Team.Manager.RolePlayCharacter.GetAllBeliefs();
			managerBeliefs = managerBeliefs.Where(b => b.Name.StartsWith("Style")).ToList();
			var managementStyles = managerBeliefs.Select(b => new KeyValuePair<string, int>(b.Name.Split('(', ')')[1], int.Parse(b.Value))).ToDictionary(b => b.Key, b => b.Value);
			managementStyles = managementStyles.OrderByDescending(m => m.Value).ToDictionary(b => b.Key, b => b.Value);
			managementStyles = managementStyles.Where(m => m.Value == managementStyles.Values.Max()).ToDictionary(b => b.Key, b => b.Value);
			return managementStyles.Keys.ToArray();
		}
	}
}

﻿using EmotionalAppraisal;
using IntegratedAuthoringTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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
		private IntegratedAuthoringToolAsset _iat;
		private string _storageLocation;
		private readonly ConfigStore _config;

		public Boat Boat { get; private set; }
		public EventController EventController { get; private set; }
		public List<Boat> LineUpHistory { get; private set; }
		public List<int> HistoricTimeOffset { get; private set; }
		public int ActionAllowance { get; private set; }
		public int CrewEditAllowance { get; private set; }
		public int RaceSessionLength { get; private set; }

		public bool Running { get { return _threadRunning; } }
		private bool _threadRunning;
		Thread _thread;

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
		public void NewGame(string storageLocation, string boatName, byte[] teamColorsPrimary, byte[] teamColorsSecondary, string managerName, string managerAge, string managerGender, List<CrewMember> crew = null)
		{
			UnloadGame();
			AssetManager.Instance.Bridge = new TemplateBridge();
			//create folder and iat file for game
			var combinedStorageLocation = Path.Combine(storageLocation, boatName);
			Directory.CreateDirectory(combinedStorageLocation);
			var iat = IntegratedAuthoringToolAsset.LoadFromFile("template_iat");
			//set up first boat
			Boat = new Dinghy(_config);
			Boat.Name = boatName;
			Boat.TeamColorsPrimary = teamColorsPrimary;
			Boat.TeamColorsSecondary = teamColorsSecondary;
			iat.ScenarioName = Boat.Name;
			AssetManager.Instance.Bridge = new BaseBridge();
			iat.SaveToFile(Path.Combine(combinedStorageLocation, boatName + ".iat"));
			var random = new Random();
			var manager = new Person
			{
				Name = managerName,
				Age = Convert.ToInt32(managerAge),
				Gender = managerGender
			};
			Boat.Manager = manager;
			//create the initial crew members
			var initialCrew = false;
			if (crew == null)
			{
				initialCrew = true;
				crew = new List<CrewMember>();
				for (var i = 0; i < Boat.BoatPositions.Count * 2; i++)
				{
					Boat.AddCrew(new CrewMember(random, Boat, _config));
				}
			}
			if (!initialCrew)
			{
				crew.ForEach(cm => Boat.AddCrew(cm));
			}
			ActionAllowance = (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * Boat.BoatPositions.Count);
			CrewEditAllowance = (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * Boat.BoatPositions.Count;
			RaceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength];
			//create manager files and store game attribute details
			manager.CreateFile(iat, combinedStorageLocation);
			manager.UpdateBeliefs("Manager");
			manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), Boat.GetType().Name);
			manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorRedPrimary.GetDescription(), teamColorsPrimary[0].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorGreenPrimary.GetDescription(), teamColorsPrimary[1].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorBluePrimary.GetDescription(), teamColorsPrimary[2].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorRedSecondary.GetDescription(), teamColorsSecondary[0].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorGreenSecondary.GetDescription(), teamColorsSecondary[1].ToString());
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorBlueSecondary.GetDescription(), teamColorsSecondary[2].ToString());
			
			manager.SaveStatus();

			//set up files and details for each CrewMember
			foreach (var member in Boat.GetAllCrewMembers())
			{
				member.CreateFile(iat, combinedStorageLocation);
				member.Avatar = new Avatar(member);
				Boat.SetCrewColors(member.Avatar);
				if (!initialCrew)
				{
					foreach (var otherMember in crew)
					{
						if (member != otherMember)
						{
							member.AddOrUpdateOpinion(otherMember, 0);
							member.AddOrUpdateRevealedOpinion(otherMember, 0);
						}
						member.AddOrUpdateOpinion(Boat.Manager, 0);
						member.AddOrUpdateRevealedOpinion(Boat.Manager, 0);
					}
				}
				else
				{
					member.CreateInitialOpinions(random, Boat);
				}
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}
			iat.SaveToFile(Path.Combine(combinedStorageLocation, boatName + ".iat"));
			EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
			LineUpHistory = new List<Boat>();
			HistoricTimeOffset = new List<int>();
			Boat.CreateRecruits(iat, combinedStorageLocation);
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
			Boat = new Boat(_config);
			//get the iat file and all characters for this game
			var combinedStorageLocation = Path.Combine(storageLocation, boatName);
			AssetManager.Instance.Bridge = new BaseBridge();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(Path.Combine(combinedStorageLocation, boatName + ".iat"));
			var rpcList = iat.GetAllCharacters();

			var crewList = new List<CrewMember>();

			foreach (var rpc in rpcList)
			{
				var tempea = EmotionalAppraisalAsset.LoadFromFile(rpc.EmotionalAppraisalAssetSource);
				var position = tempea.GetBeliefValue(NPCBeliefs.Position.GetDescription());
				//if this character is the manager, load the game details from this file and set this character as the manager
				if (position == "Manager")
				{
					var person = new Person(rpc);
					Boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + person.LoadBelief(NPCBeliefs.BoatType.GetDescription())), _config);
					ActionAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.ActionAllowance.GetDescription()));
					CrewEditAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.CrewEditAllowance.GetDescription()));
					RaceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength];
					Boat.Name = iat.ScenarioName;
					Boat.TeamColorsPrimary = new byte[3];
					Boat.TeamColorsPrimary[0] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorRedPrimary.GetDescription()));
					Boat.TeamColorsPrimary[1] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorGreenPrimary.GetDescription()));
					Boat.TeamColorsPrimary[2] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorBluePrimary.GetDescription()));
					Boat.TeamColorsSecondary = new byte[3];
					Boat.TeamColorsSecondary[0] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorRedSecondary.GetDescription()));
					Boat.TeamColorsSecondary[1] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorGreenSecondary.GetDescription()));
					Boat.TeamColorsSecondary[2] = Convert.ToByte(person.LoadBelief(NPCBeliefs.TeamColorBlueSecondary.GetDescription()));
					Boat.Manager = person;
					continue;
				}
				//set up every other character as a CrewManager, making sure to separate retired and recruits
				var crewMember = new CrewMember(rpc, _config);
				if (position == "Retired")
				{
					crewMember.Avatar = new Avatar(crewMember, false, true);
					Boat.RetiredCrew.Add(crewMember);
					continue;
				}
				if (position == "Recruit")
				{
					crewMember.Avatar = new Avatar(crewMember, false, true);
					Boat.Recruits.Add(crewMember);
					continue;
				}
				crewList.Add(crewMember);
			}
			//add all non-retired and non-recruits to the list of unassigned crew
			crewList.ForEach(cm => Boat.UnassignedCrew.Add(cm));
			//load the 'beliefs' (aka, stats and opinions) of all crew members
			Boat.Recruits.ForEach(cm => cm.LoadBeliefs(Boat));
			Boat.RetiredCrew.ForEach(cm => cm.LoadBeliefs(Boat));
			crewList.ForEach(cm => cm.LoadBeliefs(Boat));
			crewList.ForEach(cm => cm.LoadPosition(Boat));
			//set up crew avatars
			crewList.ForEach(cm => cm.Avatar = new Avatar(cm, true, true));
			crewList.ForEach(cm => cm.Avatar.PrimaryOutfitColor = new Color(Boat.TeamColorsPrimary[0], Boat.TeamColorsPrimary[1], Boat.TeamColorsPrimary[2], 255));
			crewList.ForEach(cm => cm.Avatar.SecondaryOutfitColor = new Color(Boat.TeamColorsPrimary[0], Boat.TeamColorsPrimary[1], Boat.TeamColorsPrimary[2], 255));
			EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
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
		/// Assign the CrewMember provided to the Position provided
		/// </summary>
		public void AssignCrew(Position position, CrewMember crewMember)
		{
			var boatPosition = Boat.BoatPositions.SingleOrDefault(p => p.Position == position);
			Boat.AssignCrew(boatPosition, crewMember);
		}

		/// <summary>
		/// Load the history of line-ups from the manager's EA file
		/// </summary>
		public void LoadLineUpHistory()
		{
			LineUpHistory = new List<Boat>();
			HistoricTimeOffset = new List<int>();
			//get all events that feature 'SelectedLineUp' from their EA file
			var ea = EmotionalAppraisalAsset.LoadFromFile(Boat.Manager.RolePlayCharacter.EmotionalAppraisalAssetSource);
			var managerEvents = ea.EventRecords;
			var lineUpEvents = managerEvents.Where(e => e.Event.Contains("SelectedLineUp")).Select(e => e.Event);
			foreach (var lineup in lineUpEvents)
			{
				//split up the string of details saved with this event
				var splitAfter = lineup.Split('(')[2];
				splitAfter = splitAfter.Split(')')[0];
				var subjectSplit = splitAfter.Split(',');
				//set up the version of boat this was
				var boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + subjectSplit[0]), _config);
				//position crew members and gather set-up information using details from split string
				for (var i = 0; i < boat.BoatPositions.Count; i++)
				{
					boat.BoatPositions[i].CrewMember = Boat.GetAllCrewMembersIncludingRetired().Single(c => c.Name.Replace(" ", "") == subjectSplit[((i + 1) * 2) - 1].Replace(" ", ""));
					boat.BoatPositions[i].PositionScore = Convert.ToInt32(subjectSplit[(i + 1) * 2]);
				}
				boat.IdealMatchScore = float.Parse(subjectSplit[(boat.BoatPositions.Count * 2) + 1]);
				boat.BoatScore = boat.BoatPositions.Sum(bp => bp.PositionScore);
				boat.SelectionMistakes = new List<string>();
				for (var i = (boat.BoatPositions.Count + 1) * 2; i < subjectSplit.Length - 1; i++)
				{
					boat.SelectionMistakes.Add(subjectSplit[i].Replace(" ", ""));
				}
				HistoricTimeOffset.Add(Convert.ToInt32(subjectSplit[subjectSplit.Length - 1]));
				LineUpHistory.Add(boat);
			}
		}

		/// <summary>
		/// Change the current type of boat to a different type
		/// </summary>
		public void PromoteBoat()
		{
			Boat newBoat;
			switch (Boat.GetType().Name)
			{
				case "Dinghy":
					newBoat = new AltDinghy(_config);
					break;
				case "AltDinghy":
					newBoat = new BiggerDinghy(_config);
					break;
				case "BiggerDinghy":
					newBoat = new BiggestDinghy(_config);
					break;
				default:
					return;
			}
			//store that the boat type has been changed
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), newBoat.GetType().Name);
			Boat.Manager.SaveStatus();
			//calculate how many new members should be created
			var extraMembers = (newBoat.BoatPositions.Count - Boat.BoatPositions.Count) * 2;
			//reload the current game
			LoadGame(_storageLocation, Boat.Name);
			var rand = new Random();
			for (var i = 0; i < extraMembers; i++)
			{
			//only create a new CrewMember if the crew limit can support it
				if (CanAddToCrew())
				{
					var newMember = new CrewMember(rand, Boat, _config);
					var combinedStorageLocation = Path.Combine(_storageLocation, Boat.Name);
					newMember.CreateFile(_iat, combinedStorageLocation);
					newMember.Avatar = new Avatar(newMember);
					Boat.SetCrewColors(newMember.Avatar);
					newMember.CreateInitialOpinions(rand, Boat);
					Boat.GetAllCrewMembers().ForEach(cm => cm.CreateInitialOpinion(rand, newMember));
					newMember.UpdateBeliefs("null");
					newMember.SaveStatus();
					AssetManager.Instance.Bridge = new BaseBridge();
					_iat.SaveToFile(_iat.AssetFilePath);
					//if the boat is under-staffed for the current boat size, this new CrewMember is not counted
					if (!CanRemoveFromCrew())
					{
						i--;
					}
					Boat.AddCrew(newMember);
				}
			}
		}

		/// <summary>
		/// Get the amount provided of current selection mistakes
		/// </summary>
		public List<string> GetAssignmentMistakes(int amount)
		{
			return Boat.GetAssignmentMistakes(amount);
		}

		public void SaveLineUp(int offset)
		{
			Boat.UpdateBoatScore();
			_thread = new Thread(SaveLineUpThreaded);
			_threadRunning = true;
			_thread.Start(offset);
		}

		/// <summary>
		/// Save the current boat line-up to the manager's EA file
		/// </summary>
		private void SaveLineUpThreaded(object offset)
		{
			var manager = Boat.Manager;
			var spacelessName = manager.RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventStringUnformatted = "SelectedLineUp({0},{1})";
			var boatType = Boat.GetType().Name;
			var crew = "";
			//set up string to save
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
				}
				else
				{
					crew += "null,0";
				}
			}
			Boat.GetIdealCrew();
			crew += "," + Boat.IdealMatchScore;
			Boat.SelectionMistakes.ForEach(sm => crew += "," + sm);
			crew += "," + offset;
			var eventString = string.Format(eventStringUnformatted, boatType, crew);
			manager.EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
			var eventRpc = manager.RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
			if (eventRpc != null)
			{
				manager.RolePlayCharacter.ActionFinished(eventRpc);
			}
			manager.SaveStatus();
			var lastBoat = new Boat(_config);
			foreach (var bp in Boat.BoatPositions)
			{
				lastBoat.BoatPositions.Add(new BoatPosition
				{
					Position = bp.Position,
					CrewMember = bp.CrewMember,
					PositionScore = bp.PositionScore
				});
			}
			lastBoat.SelectionMistakes = Boat.SelectionMistakes;
			lastBoat.IdealMatchScore = Boat.IdealMatchScore;
			lastBoat.BoatScore = lastBoat.BoatPositions.Sum(bp => bp.PositionScore);
			lastBoat.Manager = Boat.Manager;
			LineUpHistory.Add(lastBoat);
			HistoricTimeOffset.Add((int)offset);
			_threadRunning = false;
		}

		/// <summary>
		/// Save current line-up and update CrewMember's opinions and mood based on this line-up
		/// </summary>
		public void ConfirmLineUp()
		{
			Boat.ConfirmChanges(ActionAllowance);
			//TODO: Change trigger for promotion
			if (((LineUpHistory.Count + 1) / RaceSessionLength) % 2 != 0)
			{
				PromoteBoat();
			}
			//update available recruits for the next race
			Boat.CreateRecruits(_iat, Path.Combine(_storageLocation, Boat.Name));
			//reset the limits on actions and hiring/firing
			ResetActionAllowance();
			ResetCrewEditAllowance();
		}

		/// <summary>
		/// Select a random post-race event
		/// </summary>
		public List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> SelectPostRaceEvent()
		{
			var afterRace = false;
			var chance = (int)_config.ConfigValues[ConfigKeys.EventChance];
			if (LineUpHistory.Count % RaceSessionLength == 0)
			{
				afterRace = true;
			}
			else
			{
				chance += (int)_config.ConfigValues[ConfigKeys.PracticeEventChanceReduction];
			}
			Boat.TickCrewMembers((int)_config.ConfigValues[ConfigKeys.TicksPerSession]);
			var reactionEvents = new List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>>();
			foreach (var crewMember in Boat.GetAllCrewMembers())
			{
				var delayedReactions = crewMember.CurrentEventCheck(Boat, _iat, afterRace);
				foreach (var reply in delayedReactions)
				{
					reactionEvents.Add(new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(new List<CrewMember> { crewMember }, reply));
				}
			}
			var random = new Random();
			var selectedEvents = new List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>>();
			var findEvents = true;
			while (findEvents)
			{
				//attempt to select a random post-race event
				var postRaceEvent = EventController.SelectPostRaceEvent(_iat, chance, selectedEvents.Count, random, afterRace);
				//if no post-race event was selected, stop searching for events
				if (postRaceEvent == null)
				{
					findEvents = false;
					continue;
				}
				var eventMembers = new List<CrewMember>();
				var allCrew = Boat.GetAllCrewMembers();
				var allCrewRemovals = new List<CrewMember>();
				switch (postRaceEvent.NextState)
				{
					case "NotPicked":
						//for this event, select a crew member who was not selected in the previous race
						foreach (var bp in LineUpHistory.Last().BoatPositions)
						{
							if (bp.CrewMember != null)
							{
								allCrew.Remove(allCrew.First(ac => ac.Name == bp.CrewMember.Name));
							}
						}
						foreach (var kvp in selectedEvents)
						{
							foreach (var crewMember in kvp.Key)
							{
								allCrew.Remove(crewMember);
							}
						}
						foreach (var crewMember in allCrew)
						{
							if ((crewMember.LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) ?? "null").ToLower() == "true")
							{
								allCrewRemovals.Add(crewMember);
							}
							else if (crewMember.LoadBelief("Event(Retire)") != null)
							{
								allCrewRemovals.Add(crewMember);
							}
						}
						foreach (var crewMember in allCrewRemovals)
						{
							allCrew.Remove(crewMember);
						}
						if (allCrew.Count == 0)
						{
							findEvents = false;
							continue;
						}
						var notSelected = allCrew.OrderBy(c => Guid.NewGuid()).First();
						eventMembers.Add(notSelected);
						selectedEvents.Add(new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(eventMembers, postRaceEvent));
						continue;
					case "Retirement":
						allCrew = allCrew.Where(cm => cm.RestCount <= -5).ToList();
						foreach (var crewMember in allCrew)
						{
							if ((crewMember.LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) ?? "null").ToLower() == "true")
							{
								allCrewRemovals.Add(crewMember);
							}
							else if (crewMember.LoadBelief("Event(Retire)") != null)
							{
								allCrewRemovals.Add(crewMember);
							}
						}
						foreach (var crewMember in allCrewRemovals)
						{
							allCrew.Remove(crewMember);
						}
						if (allCrew.Count == 0)
						{
							findEvents = false;
							continue;
						}
						var retiree = allCrew.OrderBy(c => Guid.NewGuid()).First();
						retiree.UpdateSingleBelief("Event(Retire)", "1");
						eventMembers.Add(retiree);
						selectedEvents.Add(new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(eventMembers, postRaceEvent));
						continue;
					default:
						findEvents = false;
						continue;
				}
			}
			return reactionEvents.Concat(selectedEvents).ToList();
		}

		/// <summary>
		/// Set the player dialogue state
		/// </summary>
		public void SetPlayerState(DialogueStateActionDTO currentEvent)
		{
			_iat.SetDialogueState("Player", currentEvent.NextState);
		}

		/// <summary>
		/// Get all player dialogue for their current state
		/// </summary>
		public DialogueStateActionDTO[] GetPostRaceEvents()
		{
			return EventController.GetEvents(_iat, _iat.GetCurrentDialogueState("Player"));
		}

		/// <summary>
		/// Send player dialogue to characters involved in the event and get their replies
		/// </summary>
		public Dictionary<CrewMember, string> SendPostRaceEvent(DialogueStateActionDTO dialogue, List<CrewMember> members)
		{
			var replies = EventController.SendPostRaceEvent(_iat, dialogue, members, Boat, LineUpHistory.Last());
			return replies;
		}

		/// <summary>
		/// Deduct the cost of an action from the available allowance
		/// </summary>
		void DeductCost(int cost)
		{
			ActionAllowance -= cost;
			Boat.TickCrewMembers(cost);
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			Boat.Manager.SaveStatus();
		}

		/// <summary>
		/// Reset the amount of allowance actions
		/// </summary>
		void ResetActionAllowance()
		{
			ActionAllowance = GetStartingActionAllowance();
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString());
			Boat.Manager.SaveStatus();
		}

		/// <summary>
		/// Calculate how much ActionAllowance the player should start each race with
		/// </summary>
		public int GetStartingActionAllowance()
		{
			return (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition] * Boat.BoatPositions.Count);
		}

		/// <summary>
		/// Deduct the cost of a hiring/firing action from the available allowance
		/// </summary>
		void DeductCrewEditAllowance()
		{
			CrewEditAllowance--;
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			Boat.Manager.SaveStatus();
		}

		/// <summary>
		/// Reset the amount of hiring/firing actions allowed
		/// </summary>
		void ResetCrewEditAllowance()
		{
			CrewEditAllowance = GetStartingCrewEditAllowance();
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString());
			Boat.Manager.SaveStatus();
		}

		/// <summary>
		/// Calculate how many hiring/firing actions player should start each race with
		/// </summary>
		public int GetStartingCrewEditAllowance()
		{
			return (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition] * Boat.BoatPositions.Count;
		}

		/// <summary>
		/// Calculate if the player should be able to hire new characters into their crew
		/// </summary>
		public bool CanAddToCrew()
		{
			if (Boat.GetAllCrewMembers().Count + 1 > (Boat.BoatPositions.Count + 1) * 2 || Boat.Recruits.Count == 0)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Calculate how many hiring actions the player can still perform before reaching the limit
		/// </summary>
		public int CrewLimitLeft()
		{
			return ((Boat.BoatPositions.Count + 1) * 2) - Boat.GetAllCrewMembers().Count;
		}

		public void AddRecruit(CrewMember member)
		{
			//if the player is able to take this action
			var cost = (int)_config.ConfigValues[ConfigKeys.RecruitmentCost];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && CanAddToCrew())
			{
			//remove recruit from the current list of characters in the game
				_iat.RemoveCharacters(new List<string>() { member.Name });
				//set up recruit as a 'proper' character in the game
				member.CreateFile(_iat, Path.Combine(_storageLocation, Boat.Name));
				member.Avatar.UpdateAvatarBeliefs(member);
				member.Avatar = new Avatar(member, true, true);
				Boat.SetCrewColors(member.Avatar);
				var random = new Random();
				member.CreateInitialOpinions(random, Boat);
				Boat.GetAllCrewMembers().ForEach(cm => cm.CreateInitialOpinion(random, member));
				Boat.AddCrew(member);
				member.UpdateBeliefs("null");
				member.SaveStatus();
				DeductCost(cost);
				Boat.Recruits.Remove(member);
				AssetManager.Instance.Bridge = new BaseBridge();
				_iat.SaveToFile(_iat.AssetFilePath);
				DeductCrewEditAllowance();
			}
		}

		/// <summary>
		/// Calculate if the player should be able to fire characters from their crew
		/// </summary>
		public bool CanRemoveFromCrew()
		{
			if (Boat.GetAllCrewMembers().Count - 1 < Boat.BoatPositions.Count)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Remove a CrewMember from the crew
		/// </summary>
		public void RetireCrewMember(CrewMember crewMember)
		{
			var cost = (int)_config.ConfigValues[ConfigKeys.FiringCost];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && CanRemoveFromCrew())
			{
				Boat.RetireCrew(crewMember);
				DeductCost(cost);
				DeductCrewEditAllowance();
			}
		}

		/// <summary>
		/// Get event utterances for player dialogue that match the eventKey provided
		/// </summary>
		public string[] GetEventStrings(string eventKey)
		{
			return EventController.GetEventStrings(_iat, eventKey);
		}

		/// <summary>
		/// Send player meeting dialogue to a CrewMember, getting their response in return
		/// </summary>
		public string SendMeetingEvent(string eventName, CrewMember member)
		{
			var cost = GetQuestionCost(eventName);
			if (cost <= ActionAllowance)
			{
				var reply = member.SendMeetingEvent(_iat, eventName, Boat);
				DeductCost(cost);
				return reply;
			}
			return "";
		}

		/// <summary>
		/// Get the cost of sending the below questions
		/// </summary>
		public int GetQuestionCost(string eventName)
		{
			switch (eventName)
			{
				case "StatReveal":
					return (int)_config.ConfigValues[ConfigKeys.SkillRevealCost];
				case "RoleReveal":
					return (int)_config.ConfigValues[ConfigKeys.RoleRevealCost];
				case "OpinionRevealPositive":
					return (int)_config.ConfigValues[ConfigKeys.OpinionPositiveRevealCost];
				case "OpinionRevealNegative":
					return (int)_config.ConfigValues[ConfigKeys.OpinionNegativeRevealCost];
				default:
					return 0;
			}
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
				var replies = new Dictionary<CrewMember, string>();
				foreach (var member in members)
				{
					var reply = member.SendRecruitEvent(_iat, skill);
					replies.Add(member, reply ?? "");
				}
				DeductCost(cost);
				return replies;
			}
			else
			{
				var replies = new Dictionary<CrewMember, string>();
				members.ForEach(member => replies.Add(member, ""));
				return replies;
			}
		}
	}
}

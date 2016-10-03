﻿using EmotionalAppraisal;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using System;
using System.Collections.Generic;
using System.Drawing;
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
		public Boat Boat { get; private set; }
		public EventController EventController { get; private set; }

		public List<Boat> LineUpHistory { get; private set; }
		public List<int> HistoricTimeOffset { get; private set; }
		public int ActionAllowance { get; private set; }
		public int CrewEditAllowance { get; private set; }
		public int RaceSessionLength { get; private set; }
		private IntegratedAuthoringToolAsset _iat { get; set; }
		private string _storageLocation { get; set; }

		private ConfigStore _config { get; }

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
		public void NewGame(string storageLocation, string boatName, int[] teamColorsPrimary, int[] teamColorsSecondary, string managerName, string managerAge, string managerGender, List<CrewMember> crew = null)
		{
			UnloadGame();
			AssetManager.Instance.Bridge = new TemplateBridge();
			//create folder and iat file for game
			string combinedStorageLocation = Path.Combine(storageLocation, boatName);
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
			Random random = new Random();
			Person manager = new Person
			{
				Name = managerName,
				Age = Convert.ToInt32(managerAge),
				Gender = managerGender
			};
			Boat.Manager = manager;
			//create the initial crew members
			bool initialCrew = false;
			if (crew == null)
			{
				initialCrew = true;
				crew = new List<CrewMember>();
				for (int i = 0; i < Boat.BoatPositions.Count * 2; i++)
				{
					crew.Add(new CrewMember(random, Boat, _config));
				}
			}
			ActionAllowance = (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance.ToString()] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition.ToString()] * Boat.BoatPositions.Count);
			CrewEditAllowance = (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition.ToString()] * Boat.BoatPositions.Count;
			RaceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength.ToString()];
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
			foreach (CrewMember member in crew)
			{
				member.CreateFile(iat, combinedStorageLocation);
				member.Avatar = new Avatar(member);
				Boat.AddCrew(member);
				Boat.SetCrewColors(member.Avatar);
				if (!initialCrew)
				{
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
				}
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}

			if (initialCrew)
			{
				Boat.AllCrew.ForEach(cm => cm.CreateInitialOpinions(random, Boat));
			}
			iat.SaveToFile(Path.Combine(combinedStorageLocation, boatName + ".iat"));
			EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
			LineUpHistory = new List<Boat>();
			HistoricTimeOffset = new List<int>();
			Boat.GetIdealCrew();
			Boat.CreateRecruits(iat, combinedStorageLocation);
		}

		/// <summary>
		/// Get the name of every folder stored in the directory provided
		/// </summary>
		public List<string> GetGameNames(string storageLocation)
		{
			var folders = Directory.GetDirectories(storageLocation);
			List<string> gameNames = new List<string>();
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
			bool gameExists = false;
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
			string combinedStorageLocation = Path.Combine(storageLocation, boatName);
			AssetManager.Instance.Bridge = new BaseBridge();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(Path.Combine(combinedStorageLocation, boatName + ".iat"));
			var rpcList = iat.GetAllCharacters();

			List<CrewMember> crewList = new List<CrewMember>();

			foreach (RolePlayCharacterAsset rpc in rpcList)
			{
				var tempea = EmotionalAppraisalAsset.LoadFromFile(rpc.EmotionalAppraisalAssetSource);
				string position = tempea.GetBeliefValue(NPCBeliefs.Position.GetDescription());
				//if this character is the manager, load the game details from this file and set this character as the manager
				if (position == "Manager")
				{
					Person person = new Person(rpc);
					Boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + person.LoadBelief(NPCBeliefs.BoatType.GetDescription())), _config);
					ActionAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.ActionAllowance.GetDescription()));
					CrewEditAllowance = Convert.ToInt32(person.LoadBelief(NPCBeliefs.CrewEditAllowance.GetDescription()));
					RaceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength.ToString()];
					Boat.Name = iat.ScenarioName;
					Boat.TeamColorsPrimary = new int[3];
					Boat.TeamColorsPrimary[0] = Convert.ToInt32(person.LoadBelief(NPCBeliefs.TeamColorRedPrimary.GetDescription()));
					Boat.TeamColorsPrimary[1] = Convert.ToInt32(person.LoadBelief(NPCBeliefs.TeamColorGreenPrimary.GetDescription()));
					Boat.TeamColorsPrimary[2] = Convert.ToInt32(person.LoadBelief(NPCBeliefs.TeamColorBluePrimary.GetDescription()));
					Boat.TeamColorsSecondary = new int[3];
					Boat.TeamColorsSecondary[0] = Convert.ToInt32(person.LoadBelief(NPCBeliefs.TeamColorRedSecondary.GetDescription()));
					Boat.TeamColorsSecondary[1] = Convert.ToInt32(person.LoadBelief(NPCBeliefs.TeamColorGreenSecondary.GetDescription()));
					Boat.TeamColorsSecondary[2] = Convert.ToInt32(person.LoadBelief(NPCBeliefs.TeamColorBlueSecondary.GetDescription()));
					Boat.Manager = person;
					continue;
				}
				//set up every other character as a CrewManager, making sure to separate retired and recruits
				CrewMember crewMember = new CrewMember(rpc, _config);
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
			//add all non-retired and non-recruits to the list of unsassigned crew
			crewList.ForEach(cm => Boat.UnassignedCrew.Add(cm));
            crewList.ForEach(cm => Boat.AllCrew.Add(cm));
            //load the 'beliefs' (aka, stats and opinions) of all crew members
            Boat.Recruits.ForEach(cm => cm.LoadBeliefs(Boat));
			Boat.RetiredCrew.ForEach(cm => cm.LoadBeliefs(Boat));
			crewList.ForEach(cm => cm.LoadBeliefs(Boat));
			crewList.ForEach(cm => cm.LoadPosition(Boat));
			//set up crew avatars
			crewList.ForEach(cm => cm.Avatar = new Avatar(cm, true, true));
			crewList.ForEach(cm => cm.Avatar.PrimaryOutfitColor = Color.FromArgb(255, Boat.TeamColorsPrimary[0], Boat.TeamColorsPrimary[1], Boat.TeamColorsPrimary[2]));
			crewList.ForEach(cm => cm.Avatar.SecondaryOutfitColor = Color.FromArgb(255, Boat.TeamColorsPrimary[0], Boat.TeamColorsPrimary[1], Boat.TeamColorsPrimary[2]));
			EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
			LoadLineUpHistory();
			Boat.GetIdealCrew();
			Boat.UpdateBoatScore();
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
			BoatPosition boatPosition = Boat.BoatPositions.SingleOrDefault(p => p.Position == position);
			Boat.AssignCrew(boatPosition, crewMember);
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
				Boat boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + subjectSplit[0]), _config);
				//position crew members and gather set-up information using details from split string
				for (int i = 0; i < boat.BoatPositions.Count; i++)
				{
					boat.BoatPositions[i].CrewMember = Boat.GetAllCrewMembersIncludingRetired().SingleOrDefault(c => c.Name.Replace(" ", "") == subjectSplit[((i + 1) * 2) - 1].Replace(" ", ""));
					boat.BoatPositions[i].PositionScore = Convert.ToInt32(subjectSplit[(i + 1) * 2]);
				}
				boat.IdealMatchScore = float.Parse(subjectSplit[(boat.BoatPositions.Count * 2) + 1]);
				boat.SelectionMistakes = new List<string>();
				for (int i = (boat.BoatPositions.Count + 1) * 2; i < subjectSplit.Length - 1; i++)
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
				/*case "BiggerDinghy":
					newBoat = new BiggestDinghy(_config);
					break;*/
				default:
					return;
			}
			//store that the boat type has been changed
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), newBoat.GetType().Name);
			Boat.Manager.SaveStatus();
			//calculate how many new members should be created
			int extraMembers = (newBoat.BoatPositions.Count - Boat.BoatPositions.Count) * 2;
			//reload the current game
			LoadGame(_storageLocation, Boat.Name);
			Random rand = new Random();
			for (int i = 0; i < extraMembers; i++)
			{
			//only create a new CrewMember if the crew limit can support it
				if (CanAddToCrew())
				{
					CrewMember newMember = new CrewMember(rand, Boat, _config);
					string combinedStorageLocation = Path.Combine(_storageLocation, Boat.Name);
					newMember.CreateFile(_iat, combinedStorageLocation);
					newMember.Avatar = new Avatar(newMember);
					Boat.SetCrewColors(newMember.Avatar);
					newMember.CreateInitialOpinions(rand, Boat);
					Boat.AllCrew.ForEach(cm => cm.CreateInitialOpinion(rand, newMember));
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
			Boat.GetIdealCrew();
		}

		/// <summary>
		/// Get the amount provided of current selection mistakes
		/// </summary>
		public List<string> GetAssignmentMistakes(int amount)
		{
			return Boat.GetAssignmentMistakes(amount);
		}

		/// <summary>
		/// Save the current boat line-up to the manager's EA file
		/// </summary>
		public void SaveLineUp(int offset)
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
				} else
				{
					crew += "null,0";
				}
			}
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
			Boat lastBoat = new Boat(_config);
			//Boat lastBoat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + Boat.GetType().Name), _config);
			foreach (BoatPosition bp in Boat.BoatPositions)
			{
				lastBoat.BoatPositions.Add(new BoatPosition
				{
					Position = bp.Position,
					CrewMember = bp.CrewMember,
					PositionScore = bp.PositionScore
				});
			}
			lastBoat.IdealMatchScore = lastBoat.IdealMatchScore;
			lastBoat.Manager = Boat.Manager;
			LineUpHistory.Add(lastBoat);
			HistoricTimeOffset.Add(offset);
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
			Boat.TickCrewMembers((int)_config.ConfigValues[ConfigKeys.TicksPerSession.ToString()]);
			Random random = new Random();
			List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> selectedEvents = new List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>>();
			bool afterRace = false;
			var chance = (int)_config.ConfigValues[ConfigKeys.EventChance.ToString()];
			if (LineUpHistory.Count % RaceSessionLength == 0)
			{
				afterRace = true;
			}
			else
			{
			   chance += (int)_config.ConfigValues[ConfigKeys.PracticeEventChanceReduction.ToString()];
			}
            List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>> reactionEvents = new List<KeyValuePair<List<CrewMember>, DialogueStateActionDTO>>();
            foreach (CrewMember crewMember in Boat.AllCrew)
            {
                DialogueStateActionDTO[] delayedReactions = crewMember.CurrentEventCheck(Boat, _iat, afterRace);
                foreach (DialogueStateActionDTO reply in delayedReactions)
                {
                    reactionEvents.Add(new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(new List<CrewMember> { crewMember }, reply));
                }
            }
            bool findEvents = true;
			while (findEvents)
			{
				DialogueStateActionDTO postRaceEvent;
				//attempt a random post-race event
				postRaceEvent = EventController.SelectPostRaceEvent(_iat, chance, selectedEvents.Count, random, afterRace);
				//if no post-race event was selected, stop searching for events
				if (postRaceEvent == null)
				{
					findEvents = false;
					continue;
				}
				List<CrewMember> eventMembers = new List<CrewMember>();
				List<CrewMember> allCrew = Boat.AllCrew;
				List<CrewMember> allCrewRemovals = new List<CrewMember>();
				switch (postRaceEvent.NextState)
				{
					case "NotPicked":
						//for this event, select a crew member who was not selected in the previous race
						
						foreach (var bp in LineUpHistory.LastOrDefault().BoatPositions)
						{
							if (bp.CrewMember != null)
							{
								allCrew.Remove(allCrew.First(ac => ac.Name == bp.CrewMember.Name));
							}
						}
						foreach (KeyValuePair<List<CrewMember>, DialogueStateActionDTO> kvp in selectedEvents)
						{
							foreach (CrewMember crewMember in kvp.Key)
							{
								allCrew.Remove(crewMember);
							}
						}
						foreach (CrewMember crewMember in allCrew)
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
						CrewMember notSelected = allCrew.OrderBy(c => Guid.NewGuid()).First();
						eventMembers.Add(notSelected);
						selectedEvents.Add(new KeyValuePair<List<CrewMember>, DialogueStateActionDTO>(eventMembers, postRaceEvent));
						continue;
					case "Retirement":
						allCrew = allCrew.Where(cm => cm.RestCount <= -5).ToList();
						foreach (CrewMember crewMember in allCrew)
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
						CrewMember retiree = allCrew.OrderBy(c => Guid.NewGuid()).First();
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
			var replies = EventController.SendPostRaceEvent(_iat, dialogue, members, Boat, LineUpHistory.LastOrDefault());
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
			return (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance.ToString()] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition.ToString()] * Boat.BoatPositions.Count);
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
			return (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition.ToString()] * Boat.BoatPositions.Count;
		}

		/// <summary>
		/// Calculate if the player should be able to hire new characters into their crew
		/// </summary>
		public bool CanAddToCrew()
		{
			if (Boat.AllCrew.Count + 1 > (Boat.BoatPositions.Count + 1) * 2 || Boat.Recruits.Count == 0)
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
			return ((Boat.BoatPositions.Count + 1) * 2) - Boat.AllCrew.Count;
		}

		public void AddRecruit(CrewMember member)
		{
			//if the player is able to take this action
			int cost = (int)_config.ConfigValues[ConfigKeys.RecruitmentCost.ToString()];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && CanAddToCrew())
			{
			//remove recruit from the current list of characters in the game
				_iat.RemoveCharacters(new List<string>() { member.Name });
				//set up recruit as a 'proper' character in the game
				member.CreateFile(_iat, Path.Combine(_storageLocation, Boat.Name));
				member.Avatar.UpdateAvatarBeliefs(member);
				member.Avatar = new Avatar(member, true, true);
				Boat.SetCrewColors(member.Avatar);
				Random random = new Random();
				member.CreateInitialOpinions(random, Boat);
				Boat.AllCrew.ForEach(cm => cm.CreateInitialOpinion(random, member));
				Boat.AddCrew(member);
				member.UpdateBeliefs("null");
				member.SaveStatus();
				DeductCost(cost);
				Boat.Recruits.Remove(member);
				AssetManager.Instance.Bridge = new BaseBridge();
				_iat.SaveToFile(_iat.AssetFilePath);
				DeductCrewEditAllowance();
				Boat.GetIdealCrew();
			}
		}

		/// <summary>
		/// Calculate if the player should be able to fire characters from their crew
		/// </summary>
		public bool CanRemoveFromCrew()
		{
			if (Boat.AllCrew.Count - 1 < Boat.BoatPositions.Count)
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
			int cost = (int)_config.ConfigValues[ConfigKeys.FiringCost.ToString()];
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
			int cost = GetQuestionCost(eventName);
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
					return (int)_config.ConfigValues[ConfigKeys.SkillRevealCost.ToString()];
				case "RoleReveal":
					return (int)_config.ConfigValues[ConfigKeys.RoleRevealCost.ToString()];
				case "OpinionRevealPositive":
					return (int)_config.ConfigValues[ConfigKeys.OpinionPositiveRevealCost.ToString()];
				case "OpinionRevealNegative":
					return (int)_config.ConfigValues[ConfigKeys.OpinionNegativeRevealCost.ToString()];
				default:
					return 0;
			}
		}

		/// <summary>
		/// Get the value from the config
		/// </summary>
		public float GetConfigValue(ConfigKeys eventKey)
		{
			return _config.ConfigValues[eventKey.ToString()];
		}

		/// <summary>
		/// Send an event to the EventController that'll be triggered for all members within the list
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitMembersEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			int cost = (int)_config.ConfigValues[ConfigKeys.SendRecruitmentQuestionCost.ToString()];
			if (cost <= ActionAllowance)
			{
				Dictionary<CrewMember, string> replies = new Dictionary<CrewMember, string>();
				foreach (CrewMember member in members)
				{
					var reply = member.SendRecruitEvent(_iat, skill);
					if (reply != null)
					{
						replies.Add(member, reply);
					} else
					{
						replies.Add(member, "");
					}
				}
				DeductCost(cost);
				return replies;
			}
			else
			{
				Dictionary<CrewMember, string> replies = new Dictionary<CrewMember, string>();
				members.ForEach(member => replies.Add(member, ""));
				return replies;
			}
		}
	}
}

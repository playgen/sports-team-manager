using EmotionalAppraisal;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
		public List<int> HistoricTimeOffset { get; set; }
		public int ActionAllowance { get; set; }
		public int CrewEditAllowance { get; set; }
		private int _raceSessionLength { get; set; }

		private IntegratedAuthoringToolAsset _iat { get; set; }
		private IStorageProvider _storageProvider { get; set; }
		private string _storageLocation { get; set; }

		private ConfigStore _config { get; set; }

		public GameManager()
		{
			_config = new ConfigStore();
		}

		/// <summary>
		/// Create a new game
		/// </summary>
		public void NewGame(IStorageProvider storagePorvider, string storageLocation, string boatName, int[] teamColorsPrimary, int[] teamColorsSecondary, string managerName, string managerAge, string managerGender, List<CrewMember> crew = null)
		{
			UnloadGame();
			string combinedStorageLocation = Path.Combine(storageLocation, boatName);
			Directory.CreateDirectory(combinedStorageLocation);
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(templateStorage, "template_iat");
			Boat = new Dinghy(_config);
			Boat.Name = boatName;
			Boat.TeamColorsPrimary = teamColorsPrimary;
			Boat.TeamColorsSecondary = teamColorsSecondary;
			iat.ScenarioName = Boat.Name;
			Random random = new Random();
			bool initialCrew = false;
			if (crew == null)
			{
				initialCrew = true;
				crew = CreateInitialCrew(managerName, random);
			}

			Person manager = new Person
			{
				Name = managerName,
				Age = int.Parse(managerAge),
				Gender = managerGender
			};
			ActionAllowance = (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance.ToString()] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition.ToString()] * Boat.BoatPositions.Count);
			CrewEditAllowance = (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition.ToString()] * Boat.BoatPositions.Count;
			_raceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength.ToString()];
			manager.CreateFile(iat, templateStorage, storagePorvider, combinedStorageLocation);
			manager.UpdateBeliefs("Manager");
			manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), Boat.GetType().Name, "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString(), "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString(), "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorRedPrimary.GetDescription(), teamColorsPrimary[0].ToString(), "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorGreenPrimary.GetDescription(), teamColorsPrimary[1].ToString(), "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorBluePrimary.GetDescription(), teamColorsPrimary[2].ToString(), "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorRedSecondary.GetDescription(), teamColorsSecondary[0].ToString(), "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorGreenSecondary.GetDescription(), teamColorsSecondary[1].ToString(), "SELF");
			manager.UpdateSingleBelief(NPCBeliefs.TeamColorBlueSecondary.GetDescription(), teamColorsSecondary[2].ToString(), "SELF");
			Boat.Manager = manager;
			manager.SaveStatus();

			foreach (CrewMember member in crew)
			{
				member.CreateFile(iat, templateStorage, storagePorvider, combinedStorageLocation);
				member.Avatar = new Avatar(member);
				Boat.AddCrew(member);
                if (initialCrew)
                {
                    foreach (CrewMember otherMember in crew)
                    {
                        if (member != otherMember)
                        {
                            member.AddOrUpdateOpinion(otherMember, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
                            member.AddOrUpdateRevealedOpinion(otherMember, 0);
                        }
                        member.AddOrUpdateOpinion(Boat.Manager, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
                        member.AddOrUpdateRevealedOpinion(Boat.Manager, 0);
                    }
                } else
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

			iat.SaveToFile(storagePorvider, Path.Combine(combinedStorageLocation, boatName + ".iat"));
			EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
			_storageProvider = storagePorvider;
			LineUpHistory = new List<Boat>();
			HistoricTimeOffset = new List<int>();
			Boat.GetIdealCrew();
			Boat.CreateRecruits(iat, templateStorage, storagePorvider, combinedStorageLocation);
		}

		/// <summary>
		/// Create the CrewMember for the start of every game
		/// </summary>
		public List<CrewMember> CreateInitialCrew(string managerName, Random random)
		{
			CrewMember[] crew = {
			new CrewMember (random, _config)
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
			new CrewMember (random, _config)
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
			new CrewMember (random, _config)
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
			new CrewMember (random, _config)
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
			new CrewMember (random, _config)
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
			new CrewMember (random, _config)
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
		/// Check if the information provided contains an existing game
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
						var game = IntegratedAuthoringToolAsset.LoadFromFile(LocalStorageProvider.Instance, file);
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
		public void LoadGame(IStorageProvider storagePorvider, string storageLocation, string boatName)
		{
			UnloadGame();
			Boat = new Boat(_config);
			string combinedStorageLocation = Path.Combine(storageLocation, boatName);
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(storagePorvider, Path.Combine(combinedStorageLocation, boatName + ".iat"));
			var rpcList = iat.GetAllCharacters();

			List<CrewMember> crewList = new List<CrewMember>();

			foreach (RolePlayCharacterAsset rpc in rpcList)
			{
				var tempea = EmotionalAppraisalAsset.LoadFromFile(storagePorvider, rpc.EmotionalAppraisalAssetSource);
				string position = tempea.GetBeliefValue(NPCBeliefs.Position.GetDescription());
				if (position == "Manager")
				{
					Person person = new Person(storagePorvider, rpc);
					Boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.BoatType.GetDescription())), _config);
					ActionAllowance = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.ActionAllowance.GetDescription()));
					CrewEditAllowance = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.CrewEditAllowance.GetDescription()));
					this._raceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength.ToString()];
					Boat.Name = iat.ScenarioName;
					Boat.TeamColorsPrimary = new int[3];
					Boat.TeamColorsPrimary[0] = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorRedPrimary.GetDescription()));
					Boat.TeamColorsPrimary[1] = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorGreenPrimary.GetDescription()));
					Boat.TeamColorsPrimary[2] = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorBluePrimary.GetDescription()));
					Boat.TeamColorsSecondary = new int[3];
					Boat.TeamColorsSecondary[0] = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorRedSecondary.GetDescription()));
					Boat.TeamColorsSecondary[1] = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorGreenSecondary.GetDescription()));
					Boat.TeamColorsSecondary[2] = int.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorBlueSecondary.GetDescription()));
					Boat.Manager = person;
					continue;
				}
				CrewMember crewMember = new CrewMember(storagePorvider, rpc, _config);
				if (position == "Retired")
				{
					crewMember.LoadBeliefs(Boat);
					crewMember.Avatar = new Avatar(crewMember, false, true);
					Boat.RetiredCrew.Add(crewMember);
					
					continue;
				}
				if (position == "Recruit")
				{
					crewMember.LoadBeliefs(Boat);
					crewMember.Avatar = new Avatar(crewMember, false, true);
					Boat.Recruits.Add(crewMember);
					
					continue;
				}
				crewList.Add(crewMember);
			}
			crewList.ForEach(cm => cm.Avatar = new Avatar(cm, true, true));
			crewList.ForEach(cm => Boat.UnassignedCrew.Add(cm));
			crewList.ForEach(cm => cm.LoadBeliefs(Boat));
            crewList.ForEach(cm => cm.LoadPosition(Boat));
            crewList.ForEach(cm => cm.Avatar.PrimaryOutfitColor = Color.FromArgb(255, Boat.TeamColorsPrimary[0], Boat.TeamColorsPrimary[1], Boat.TeamColorsPrimary[2]));
            crewList.ForEach(cm => cm.Avatar.SecondaryOutfitColor = Color.FromArgb(255, Boat.TeamColorsPrimary[0], Boat.TeamColorsPrimary[1], Boat.TeamColorsPrimary[2]));
            EventController = new EventController();
			_iat = iat;
			_storageLocation = storageLocation;
			this._storageProvider = storagePorvider;
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
			var managerEvents = Boat.Manager.EmotionalAppraisal.EventRecords;
			var lineUpEvents = managerEvents.Where(e => e.Event.Contains("SelectedLineUp")).Select(e => e.Event);
			foreach (var lineup in lineUpEvents)
			{
				var splitAfter = lineup.Split('(')[2];
				splitAfter = splitAfter.Split(')')[0];
				var subjectSplit = splitAfter.Split(',');
				Boat boat = new Boat(_config);
				boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + subjectSplit[0]), _config);
				for (int i = 0; i < boat.BoatPositions.Count; i++)
				{
					boat.BoatPositions[i].CrewMember = Boat.GetAllCrewMembersIncludingRetired().SingleOrDefault(c => c.Name.Replace(" ", "") == subjectSplit[((i + 1) * 2) - 1].Replace(" ", ""));
					boat.BoatPositions[i].PositionScore = int.Parse(subjectSplit[(i + 1) * 2]);
				}
				boat.IdealMatchScore = float.Parse(subjectSplit[(boat.BoatPositions.Count * 2) + 1]);
				boat.SelectionMistakes = new List<string>();
				for (int i = (boat.BoatPositions.Count + 1) * 2; i < subjectSplit.Length - 1; i++)
				{
					boat.SelectionMistakes.Add(subjectSplit[i].Replace(" ", ""));
				}
				HistoricTimeOffset.Add(int.Parse(subjectSplit[subjectSplit.Length - 1]));
				LineUpHistory.Add(boat);
			}
		}

		public void PromoteBoat()
		{
			Boat newBoat = new Boat(_config);
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
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), newBoat.GetType().Name, "SELF");
			Boat.Manager.SaveStatus();
            int extraMembers = 0;//(newBoat.BoatPositions.Count - Boat.BoatPositions.Count) * 2;
			LoadGame(_storageProvider, _storageLocation, Boat.Name);
			Random rand = new Random();
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			for (int i = 0; i < extraMembers; i++)
			{
				if (CanAddToCrew())
				{
					CrewMember newMember = Boat.CreateNewMember(rand);
                    string combinedStorageLocation = Path.Combine(_storageLocation, Boat.Name);
                    newMember.CreateFile(_iat, templateStorage, _storageProvider, combinedStorageLocation);
                    newMember.Avatar = new Avatar(newMember);
					foreach (CrewMember otherMember in Boat.GetAllCrewMembers())
					{
						if (newMember != otherMember)
						{
							newMember.AddOrUpdateOpinion(otherMember, rand.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
							newMember.AddOrUpdateRevealedOpinion(otherMember, 0);
							otherMember.AddOrUpdateOpinion(newMember, rand.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
							otherMember.AddOrUpdateRevealedOpinion(newMember, 0);
						}
						newMember.AddOrUpdateOpinion(Boat.Manager, 0);
						newMember.AddOrUpdateRevealedOpinion(Boat.Manager, 0);
					}
					newMember.UpdateBeliefs("null");
					newMember.SaveStatus();
					_iat.SaveToFile(_storageProvider, _iat.AssetFilePath);
					if (!CanRemoveFromCrew())
					{
						i--;
					}
					Boat.AddCrew(newMember);
				}
			}
			Boat.GetIdealCrew();
		}

		public int GetRaceSessionLength()
		{
			return _raceSessionLength;
		}

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
			for (int i = 0; i < Boat.SelectionMistakes.Count; i++)
			{
				crew += "," + Boat.SelectionMistakes[i];
			}
			crew += "," + offset.ToString();
			var eventString = String.Format(eventStringUnformatted, boatType, crew);
			manager.EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
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
		/// Save current line-up and update Crewmember's opinions and mood based on this line-up
		/// </summary>
		public void ConfirmLineUp()
		{
			Boat.ConfirmChanges(ActionAllowance);
            if (((LineUpHistory.Count + 1) / _raceSessionLength) % 2 != 0)
            {
                PromoteBoat();
            }
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			Boat.CreateRecruits(_iat, templateStorage, _storageProvider, Path.Combine(_storageLocation, Boat.Name));
			ResetActionAllowance();
			ResetCrewEditAllowance();
		}

		public KeyValuePair<List<CrewMember>, string> SelectPostRaceEvent()
		{
            if ((LineUpHistory.Count / _raceSessionLength) % 2 != 0)
            {
                return new KeyValuePair<List<CrewMember>, string>(null, null);
            }
            DialogueStateActionDTO postRaceEvent = EventController.SelectPostRaceEvent(_iat, (int)_config.ConfigValues[ConfigKeys.EventChance.ToString()]);
			if (postRaceEvent == null)
			{
				return new KeyValuePair<List<CrewMember>, string>(null, null);
			}
			List<CrewMember> eventMembers = new List<CrewMember>();
			switch (postRaceEvent.Style)
			{
				case "NotPicked":
                    List<CrewMember> allCrew = Boat.GetAllCrewMembers();
                    foreach (var bp in LineUpHistory.LastOrDefault().BoatPositions)
                    {
                        if (bp.CrewMember != null)
                        {
                            allCrew.Remove(allCrew.First(ac => ac.Name == bp.CrewMember.Name));
                        }
                    }
                    if (allCrew.Count == 0)
					{
						return new KeyValuePair<List<CrewMember>, string>(null, null);
					}
					CrewMember notSelected = allCrew.OrderBy(c => Guid.NewGuid()).First();
					eventMembers.Add(notSelected);
					if (postRaceEvent.NextState != "-")
					{
						_iat.SetDialogueState("Player", postRaceEvent.NextState);
					}
					string reply = postRaceEvent.Utterance;
					return new KeyValuePair<List<CrewMember>, string>(eventMembers, reply);
				default:
					return new KeyValuePair<List<CrewMember>, string>(null, null);
			}
		}

		public DialogueStateActionDTO[] GetPostRaceEvents()
		{
			return EventController.GetEvents(_iat, _iat.GetCurrentDialogueState("Player"));
		}

		public Dictionary<CrewMember, string> SendPostRaceEvent(DialogueStateActionDTO dialogue, List<CrewMember> members)
		{
			var replies = EventController.SendPostRaceEvent(_iat, dialogue, members, Boat, LineUpHistory.LastOrDefault());
			return replies;
		}

		void DeductCost(int cost)
		{
			ActionAllowance -= cost;
			Boat.TickCrewMembers(cost);
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
		}

		void ResetActionAllowance()
		{
			ActionAllowance = GetStartingActionAllowance();
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
		}

		public int GetStartingActionAllowance()
		{
			return (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance.ToString()] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition.ToString()] * Boat.BoatPositions.Count);
		}

		void DeductCrewEditAllowance()
		{
			CrewEditAllowance--;
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
		}

		void ResetCrewEditAllowance()
		{
			CrewEditAllowance = GetStartingCrewEditAllowance();
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
		}

		public int GetStartingCrewEditAllowance()
		{
			return (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition.ToString()] * Boat.BoatPositions.Count;
		}

		public bool CanAddToCrew()
		{
			if (Boat.GetAllCrewMembers().Count + 1 > (Boat.BoatPositions.Count + 1) * 2 || Boat.Recruits.Count == 0)
			{
				return false;
			}
			return true;
		}

		public int CrewLimitLeft()
		{
			return ((Boat.BoatPositions.Count + 1) * 2) - Boat.GetAllCrewMembers().Count;
		}

		public void AddRecruit(CrewMember member)
		{
			int cost = (int)_config.ConfigValues[ConfigKeys.RecruitmentCost.ToString()];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && CanAddToCrew())
			{
				_iat.RemoveCharacters(new List<string>() { member.Name });
				TemplateStorageProvider templateStorage = new TemplateStorageProvider();
				member.CreateFile(_iat, templateStorage, _storageProvider, Path.Combine(_storageLocation, Boat.Name));
				member.Avatar.UpdateAvatarBeliefs(member);
				member.Avatar = new Avatar(member, true, true);
				Random random = new Random();
				foreach (CrewMember otherMember in Boat.GetAllCrewMembers())
				{
					if (member != otherMember)
					{
						member.AddOrUpdateOpinion(otherMember, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
						member.AddOrUpdateRevealedOpinion(otherMember, 0);
						otherMember.AddOrUpdateOpinion(member, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
						otherMember.AddOrUpdateRevealedOpinion(member, 0);
					}
					member.AddOrUpdateOpinion(Boat.Manager, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
					member.AddOrUpdateRevealedOpinion(Boat.Manager, 0);
				}
				Boat.AddCrew(member);
				member.UpdateBeliefs("null");
				member.SaveStatus();
				DeductCost(cost);
				Boat.Recruits.Remove(member);
				_iat.SaveToFile(this._storageProvider, _iat.AssetFilePath);
				DeductCrewEditAllowance();
				Boat.GetIdealCrew();
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

		public string[] GetEventStrings(string eventKey)
		{
			return EventController.GetEventStrings(_iat, eventKey);
		}

		public string[] SendMeetingEvent(string eventType, string eventName, List<CrewMember> members)
		{
			int cost = GetQuestionCost(eventName);
			if (cost <= ActionAllowance)
			{
				var replies = EventController.SendMeetingEvent(_iat, eventType, eventName, members, Boat);
				DeductCost(cost);
				return replies.ToArray();
			}
			return new string[0];
		}

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
		/// Send an event to the EventController that'll be triggered for all members within the list
		/// </summary>
		public Dictionary<CrewMember, string> SendRecruitMembersEvent(CrewMemberSkill skill, List<CrewMember> members)
		{
			int cost = (int)_config.ConfigValues[ConfigKeys.SendRecruitmentQuestionCost.ToString()];
			if (cost <= ActionAllowance)
			{
				var replies = EventController.SendRecruitEvent(_iat, skill, members);
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

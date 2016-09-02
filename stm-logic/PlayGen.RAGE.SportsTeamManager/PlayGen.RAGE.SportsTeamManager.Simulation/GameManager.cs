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
		public int ActionAllowance { get; set; }
		public int CrewEditAllowance { get; set; }
		private int _raceSessionLength { get; set; }
		public event EventHandler AllowanceUpdated = delegate { };

		private IntegratedAuthoringToolAsset _iat { get; set; }
		private IStorageProvider _storagePorvider { get; set; }
		private string _storageLocation { get; set; }

		private ConfigStore _config { get; set; }

		public GameManager()
		{
			_config = new ConfigStore();
		}

		/// <summary>
		/// Create a new game
		/// </summary>
		public void NewGame(IStorageProvider storagePorvider, string storageLocation, string boatName, float[] teamColorsPrimary, float[] teamColorsSecondary, string managerName, string managerAge, string managerGender, List<CrewMember> crew = null)
		{
			UnloadGame();
			var noSpaceBoatName = boatName.Replace(" ", "");
			storageLocation = Path.Combine(storageLocation, noSpaceBoatName);
			Directory.CreateDirectory(storageLocation);
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
			this._raceSessionLength = (int)_config.ConfigValues[ConfigKeys.RaceSessionLength.ToString()];
			manager.CreateFile(iat, templateStorage, storagePorvider, storageLocation);
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
						member.AddOrUpdateOpinion(otherMember, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
						member.AddOrUpdateRevealedOpinion(otherMember, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
					}
					member.AddOrUpdateOpinion(Boat.Manager, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
					member.AddOrUpdateRevealedOpinion(Boat.Manager, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
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
			Boat.CreateRecruits(iat, templateStorage, storagePorvider, storageLocation);
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
			Boat = new Boat(_config);
			storageLocation = Path.Combine(storageLocation, boatName.Replace(" ", ""));
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, boatName.Replace(" ", "") + ".iat"));
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
					Boat.TeamColorsPrimary = new float[3];
					Boat.TeamColorsPrimary[0] = float.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorRedPrimary.GetDescription()));
					Boat.TeamColorsPrimary[1] = float.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorGreenPrimary.GetDescription()));
					Boat.TeamColorsPrimary[2] = float.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorBluePrimary.GetDescription()));
					Boat.TeamColorsSecondary = new float[3];
					Boat.TeamColorsSecondary[0] = float.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorRedSecondary.GetDescription()));
					Boat.TeamColorsSecondary[1] = float.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorGreenSecondary.GetDescription()));
					Boat.TeamColorsSecondary[2] = float.Parse(person.EmotionalAppraisal.GetBeliefValue(NPCBeliefs.TeamColorBlueSecondary.GetDescription()));
					Boat.Manager = person;
					continue;
				}
				CrewMember crewMember = new CrewMember(storagePorvider, rpc, _config);
				if (position == "Retired")
				{
					Boat.RetiredCrew.Add(crewMember);
					continue;
				}
				if (position == "Recruit")
				{
					Boat.Recruits.Add(crewMember);
					continue;
				}
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
				Boat boat = new Boat(_config);
				boat = (Boat)Activator.CreateInstance(Type.GetType("PlayGen.RAGE.SportsTeamManager.Simulation." + subjectSplit[0]), _config);
				for (int i = 0; i < boat.BoatPositions.Count; i++)
				{
					boat.BoatPositions[i].CrewMember = Boat.GetAllCrewMembersIncludingRetired().SingleOrDefault(c => c.Name.Replace(" ", "") == subjectSplit[((i + 1) * 2) - 1].Replace(" ", ""));
					boat.BoatPositions[i].PositionScore = int.Parse(subjectSplit[(i + 1) * 2]);
				}
				boat.IdealMatchScore = float.Parse(subjectSplit[subjectSplit.Length - 1]);
				LineUpHistory.Add(boat);
			}
		}

		public int GetRaceSessionLength()
		{
			return _raceSessionLength;
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
			Boat lastBoat = new Boat(_config);
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
			LineUpHistory.Add(lastBoat);
		}

		/// <summary>
		/// Save current line-up and update Crewmember's opinions and mood based on this line-up
		/// </summary>
		public void ConfirmLineUp()
		{
			Boat.TickCrewMembers(ActionAllowance);
			Boat.ConfirmChanges();
			Boat.PostRaceRest();
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			Boat.CreateRecruits(_iat, templateStorage, _storagePorvider, _storageLocation);
			ResetActionAllowance();
			ResetCrewEditAllowance();
		}

		public KeyValuePair<List<CrewMember>, string> SelectPostRaceEvent()
		{
			DialogueStateActionDTO postRaceEvent = EventController.SelectPostRaceEvent(_iat, (int)_config.ConfigValues[ConfigKeys.EventChance.ToString()]);
			if (postRaceEvent == null)
			{
				return new KeyValuePair<List<CrewMember>, string>(null, null);
			}
			List<CrewMember> eventMembers = new List<CrewMember>();
			switch (postRaceEvent.Style)
			{
				case "NotPicked":
					if (Boat.UnassignedCrew.Count == 0)
					{
						return new KeyValuePair<List<CrewMember>, string>(null, null);
					}
					CrewMember notSelected = Boat.UnassignedCrew.OrderBy(c => Guid.NewGuid()).First();
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
			var replies = EventController.SendPostRaceEvent(_iat, dialogue, members, Boat);
			return replies;
		}

		void DeductCost(int cost)
		{
			ActionAllowance -= cost;
			Boat.TickCrewMembers(cost);
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		void ResetActionAllowance()
		{
			ActionAllowance = (int)_config.ConfigValues[ConfigKeys.DefaultActionAllowance.ToString()] + ((int)_config.ConfigValues[ConfigKeys.ActionAllowancePerPosition.ToString()] * Boat.BoatPositions.Count);
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.ActionAllowance.GetDescription(), ActionAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		void DeductCrewEditAllowance()
		{
			CrewEditAllowance--;
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		void ResetCrewEditAllowance()
		{
			CrewEditAllowance = (int)_config.ConfigValues[ConfigKeys.CrewEditAllowancePerPosition.ToString()] * Boat.BoatPositions.Count;
			Boat.Manager.UpdateSingleBelief(NPCBeliefs.CrewEditAllowance.GetDescription(), CrewEditAllowance.ToString(), "SELF");
			Boat.Manager.SaveStatus();
			AllowanceUpdated(this, new EventArgs());
		}

		public bool CanAddToCrew()
		{
			if (Boat.GetAllCrewMembers().Count + 1 > (Boat.BoatPositions.Count + 1) * 2)
			{
				return false;
			}
			return true;
		}

		public void AddRecruit(CrewMember member)
		{
			int cost = (int)_config.ConfigValues[ConfigKeys.RecruitmentCost.ToString()];
			if (cost <= ActionAllowance && CrewEditAllowance > 0 && CanAddToCrew())
			{
				_iat.RemoveCharacters(new List<string>() { member.Name });
				TemplateStorageProvider templateStorage = new TemplateStorageProvider();
				member.CreateFile(_iat, templateStorage, _storagePorvider, _storageLocation);
				Boat.AddCrew(member);
				Random random = new Random();
				foreach (CrewMember otherMember in Boat.GetAllCrewMembers())
				{
					if (member != otherMember)
					{
						member.AddOrUpdateOpinion(otherMember, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
						member.AddOrUpdateRevealedOpinion(otherMember, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
						otherMember.AddOrUpdateOpinion(member, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
						otherMember.AddOrUpdateRevealedOpinion(member, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
					}
					member.AddOrUpdateOpinion(Boat.Manager, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
					member.AddOrUpdateRevealedOpinion(Boat.Manager, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
				}
				member.UpdateBeliefs("null");
				member.SaveStatus();
				DeductCost(cost);
				Boat.Recruits.Remove(member);
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
				Boat.UpdateBoatScore();
				Boat.GetIdealCrew();
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

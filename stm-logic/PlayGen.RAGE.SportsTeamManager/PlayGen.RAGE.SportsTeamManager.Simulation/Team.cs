using System.Collections.Generic;
using System.IO;
using System.Linq;
using IntegratedAuthoringTool;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Stores crew details and contains functionality related to adjusting and adding to crew
	/// </summary>
	public class Team
	{
		private readonly IntegratedAuthoringToolAsset iat;
		private readonly string storageLocation;
		private readonly Dictionary<string, CrewMember> crewMembers;

		public Boat Boat { get; }
		public List<Boat> LineUpHistory { get; internal set; }
		public Boat PreviousSession => LineUpHistory.LastOrDefault();
		public List<int> HistoricTimeOffset { get; internal set; }
		public List<int> HistoricSessionNumber { get; internal set; }
		public List<Boat> RaceHistory => LineUpHistory.Where((boat, i) => HistoricSessionNumber[i] == 0).ToList();
		public string Name { get; }
		public string Nationality { get; }
		public Color TeamColorsPrimary { get; internal set; }
		public Color TeamColorsSecondary { get; internal set; }
		public Dictionary<string, CrewMember> CrewMembers => crewMembers.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
		public Dictionary<string, CrewMember> RetiredCrew { get; }
		public Dictionary<string, CrewMember> Recruits { get; }
		public Person Manager { get; internal set; }
		public string ManagerName => Manager.Name;
		public bool Finished { get; internal set; }

		/// <summary>
		/// Team constructor
		/// </summary>
		internal Team (IntegratedAuthoringToolAsset i, string storage, string name, string nation, Boat boat)
		{
			iat = i;
			storageLocation = Path.Combine(storage, name);
			Name = name;
			Nationality = nation;
			Boat = boat;
			crewMembers = new Dictionary<string, CrewMember>();
			RetiredCrew = new Dictionary<string, CrewMember>();
			Recruits = new Dictionary<string, CrewMember>();
			LineUpHistory = new List<Boat>();
			HistoricTimeOffset = new List<int>();
			HistoricSessionNumber = new List<int>();
		}

		/// <summary>
		/// Add a CrewMember to the list of CrewMembers
		/// </summary>
		internal void AddCrewMember(CrewMember crewMember)
		{
			crewMembers.Add(crewMember.Name, crewMember);
		}

		/// <summary>
		/// Check that a CrewMember name is unique within this Team
		/// </summary>
		internal void UniqueNameCheck(CrewMember cm)
		{
			var unqiue = false;
			var partialFailCount = 0;
			//if the name is already in use by another character, reset their name
			while (!unqiue)
			{
				if (partialFailCount < 5)
				{
					partialFailCount++;
					var firstNames = crewMembers.Select(c => c.Value.FirstName).Concat(RetiredCrew.Select(c => c.Value.FirstName)).Concat(Recruits.Select(c => c.Value.FirstName)).ToList();
					var lastNames = crewMembers.Select(c => c.Value.LastName).Concat(RetiredCrew.Select(c => c.Value.LastName)).Concat(Recruits.Select(c => c.Value.LastName)).ToList();
					if (firstNames.Contains(cm.FirstName) || lastNames.Contains(cm.LastName) || cm.Name == ManagerName)
					{
						cm.Name = cm.SelectRandomName();
					}
					else
					{
						unqiue = true;
					}
				}
				else
				{
					if (crewMembers.ContainsKey(cm.Name) || RetiredCrew.ContainsKey(cm.Name) || Recruits.ContainsKey(cm.Name) || cm.Name == ManagerName)
					{
						cm.Name = cm.SelectRandomName();
					}
					else
					{
						unqiue = true;
					}
				}
			}
		}

		/// <summary>
		/// Calculate if the player should be able to hire new characters into their crew
		/// </summary>
		public bool CanAddToCrew()
		{
			if (Boat.PositionCount == 0)
			{
				return false;
			}
			return crewMembers.Count + 1 <= (Boat.PositionCount + 1) * 2 && Recruits.Count != 0;
		}

		/// <summary>
		/// Calculate how many hiring actions the player can still perform before reaching the limit
		/// </summary>
		public int CrewLimitLeft()
		{
			if (Boat.PositionCount == 0)
			{
				return 0;
			}
			return ((Boat.PositionCount + 1) * 2) - crewMembers.Count;
		}

		/// <summary>
		/// Update the set of recruits
		/// </summary>
		internal void CreateRecruits()
		{
			var recuritsToRemove = new List<string>();
			//remove recruits from iat and randomly select to remove them from pool of available recruits
			foreach (var member in Recruits)
			{
				var path = Path.Combine(storageLocation, member.Value.RolePlayCharacter.VoiceName + ".rpc").Replace("\\", "/");
				iat.RemoveCharacters(new List<int> { iat.GetAllCharacterSources().First(c => c.Source.Replace("\\", "/") == path).Id });
				if (StaticRandom.Int(0, 100) % ConfigKey.RecruitChangeChance.GetIntValue() != 0)
				{
					recuritsToRemove.Add(member.Key);
				}
			}
			foreach (var member in recuritsToRemove)
			{
				Recruits.Remove(member);
			}

			//for the amount of empty recruit spaces, create a new recruit
			var amount = ConfigKey.RecruitCount.GetIntValue() - Recruits.Count;
			for (var i = 0; i < amount; i++)
			{
				var position = Boat.GetWeakestPosition(CrewMembers.Values.Concat(Recruits.Values).ToList());
				var newMember = new CrewMember(position, Nationality);
				UniqueNameCheck(newMember);
				Recruits.Add(newMember.Name, newMember);
			}
			var storeNum = 0;
			//set up the files for each recruit and their avatar
			foreach (var recruit in Recruits)
			{
				recruit.Value.CreateRecruitFile(iat, storageLocation, storeNum);
				storeNum++;
			}
			iat.Save();
		}

		/// <summary>
		/// Add a recruit into crewMembers
		/// </summary>
		internal void AddRecruit(CrewMember member)
		{
			//remove recruit from the current list of characters in the game
			var path = Path.Combine(storageLocation, member.RolePlayCharacter.VoiceName + ".rpc").Replace("\\", "/");
			iat.RemoveCharacters(new List<int> { iat.GetAllCharacterSources().First(c => c.Source.Replace("\\", "/") == path).Id });
			//set up recruit as a 'proper' character in the game
			var currentNames = crewMembers.Keys.ToList();
			currentNames.Add(ManagerName);
			member.CreateTeamMemberFile(iat, storageLocation, currentNames, TeamColorsPrimary, TeamColorsSecondary);
			foreach (var cm in crewMembers.Values)
			{
				cm.CreateInitialOpinions(new List<string> { member.Name }, true);
			}
			AddCrewMember(member);
			Recruits.Remove(member.Name);
			iat.Save();
		}

		/// <summary>
		/// Calculate if the player should be able to fire characters from their crew
		/// </summary>
		public bool CanRemoveFromCrew()
		{
			if (Boat.PositionCount == 0)
			{
				return false;
			}
			return CrewMembers.Count - 1 >= Boat.PositionCount;
		}

		/// <summary>
		/// Retire a CrewMember, meaning they can no longer be assigned to a position (mostly used for historical positions)
		/// </summary>
		internal void RetireCrew(CrewMember crewMember)
		{
			var current = Boat.GetCrewMemberPosition(crewMember);
			if (current != Position.Null)
			{
				Boat.AssignCrewMember(current, null);
			}
			crewMembers.Remove(crewMember.Name);
			RetiredCrew.Add(crewMember.Name, crewMember);
			crewMember.Retire();
			foreach (var cm in crewMembers.Values)
			{
				cm.CrewOpinions.Remove(crewMember.Name);
				cm.RevealedCrewOpinions.Remove(crewMember.Name);
			}
		}

		//Tick all CrewMembers in crewMembers and Retired
		internal void TickCrewMembers(int amount = 0, bool save = true)
		{
			foreach (var cm in crewMembers.Values)
			{
				cm.TickUpdate(amount, save);
			}
			foreach (var cm in RetiredCrew.Values)
			{
				cm.TickUpdate(amount, save);
			}
		}

		/// <summary>
		/// Save the current status of each CrewMember for this Team
		/// </summary>
		internal void ConfirmChanges()
		{
			PostRaceRest();
			PromoteBoat();
			//update available recruits for the next race
			CreateRecruits();
			TickCrewMembers();
			Manager.SaveStatus();
		}

		/// <summary>
		/// Update the current type of Boat to use for this Team
		/// </summary>
		internal void PromoteBoat()
		{
			var extraMembers = Boat.PositionCount;
			var newType = PromotionTriggerCheck();
			if (string.IsNullOrEmpty(newType))
			{
				return;
			}
			Boat.Promote(newType);
			if (Boat.Type == "Finished")
			{
				Finished = true;
				return;
			}
			//store that the boat type has been changed
			Manager.UpdateSingleBelief(NPCBelief.BoatType, Boat.Type);
			//calculate how many new members should be created
			extraMembers = (Boat.PositionCount - extraMembers) * 2;
			//reset the positions on the boat to those for the new type
			for (var i = 0; i < extraMembers; i++)
			{
				var currentNames = crewMembers.Keys.ToList();
				currentNames.Add(ManagerName);
				//only create a new CrewMember if the crew limit can support it
				if (CanAddToCrew())
				{
					var position = Boat.GetWeakestPosition(CrewMembers.Values.Concat(Recruits.Values).ToList());
					var newMember = new CrewMember(position, Nationality);
					UniqueNameCheck(newMember);
					newMember.CreateTeamMemberFile(iat, storageLocation, currentNames, TeamColorsPrimary, TeamColorsSecondary);
					foreach (var cm in crewMembers.Values)
					{
						cm.CreateInitialOpinions(new List<string> { newMember.Name });
					}
					//if the boat is under-staffed for the current boat size, this new CrewMember is not counted
					if (!CanRemoveFromCrew())
					{
						i--;
					}
					AddCrewMember(newMember);
				}
			}
		}

		private string PromotionTriggerCheck()
		{
			var possibleTypes = ConfigStore.GameConfig.PromotionTriggers.Where(pt => pt.StartType == Boat.Type);
			var validRaces = RaceHistory.Where(pb => pb.Type == Boat.Type).ToList();
			foreach (var type in possibleTypes)
			{
				var consecutiveMatches = 0;
				foreach (var boat in validRaces)
				{
					if (boat.Score >= type.ScoreRequired)
					{
						consecutiveMatches++;
						if (consecutiveMatches >= type.ScoreMetSinceLast)
						{
							return type.NewType;
						}
					}
					else
					{
						consecutiveMatches = 0;
					}
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Get the average mood of the crew
		/// </summary>
		public float AverageMood()
		{
			var mood = 0f;
			foreach (var crewMember in crewMembers.Values)
			{
				mood += crewMember.GetMood();
			}
			mood = mood / crewMembers.Count;
			return mood;
		}

		/// <summary>
		/// Get the average manager opinion of the crew
		/// </summary>
		public float AverageManagerOpinion()
		{
			var opinion = 0f;
			foreach (var crewMember in crewMembers.Values)
			{
				if (crewMember.CrewOpinions.ContainsKey(ManagerName))
				{
					opinion += crewMember.CrewOpinions[ManagerName];
				}
			}
			opinion = opinion / crewMembers.Count;
			return opinion;
		}

		/// <summary>
		/// Get the average opinion of the crew
		/// </summary>
		public float AverageOpinion()
		{
			var opinion = 0f;
			foreach (var crewMember in crewMembers.Values)
			{
				var crewOpinion = 0f;
				foreach (var otherMember in crewMembers.Keys)
				{
					if (otherMember != crewMember.Name && crewMember.CrewOpinions.ContainsKey(otherMember))
					{
						crewOpinion += crewMember.CrewOpinions[otherMember];
					}
				}
				crewOpinion = crewOpinion / (crewMembers.Count - 1);
				opinion += crewOpinion;
			}
			opinion = opinion / crewMembers.Count;
			return opinion;
		}

		/// <summary>
		/// Set all CrewMembers who raced to not be available for the set amount of races
		/// </summary>
		private void PostRaceRest()
		{
			foreach (var crewMember in crewMembers.Values)
			{
				crewMember.RaceRest(Boat.PositionCrew.ContainsValue(crewMember));
			}
		}
	}
}

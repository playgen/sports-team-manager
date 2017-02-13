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
		private readonly ConfigStore config;
		private readonly Dictionary<string, CrewMember> crewMembers;

		public Boat Boat { get; }
		public List<Boat> LineUpHistory { get; internal set; }
		public List<int> HistoricTimeOffset { get; internal set; }
		public List<int> HistoricSessionNumber { get; internal set; }
		public List<Boat> RaceHistory
		{
			get
			{
				return LineUpHistory.Where((boat, i) => HistoricSessionNumber[i] == 0).ToList();
			}
		}
		public string Name { get; }
		public string Nationality { get; }
		public Color TeamColorsPrimary { get; internal set; }
		public Color TeamColorsSecondary { get; internal set; }
		public Dictionary<string, CrewMember> CrewMembers
		{
			get { return crewMembers.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value); }
		}
		public Dictionary<string, CrewMember> RetiredCrew { get; }
		public Dictionary<string, CrewMember> Recruits { get; }
		public Person Manager { get; internal set; }
		public bool Finished { get; internal set; }

		/// <summary>
		/// Team constructor
		/// </summary>
		internal Team (IntegratedAuthoringToolAsset i, string storage, ConfigStore con, string name, string nation, Boat boat)
		{
			iat = i;
			storageLocation = Path.Combine(storage, name);
			config = con;
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
		/// Set the avatar outfit colors to match the team colors
		/// </summary>
		internal void SetCrewColors(Avatar avatar)
		{
			avatar.PrimaryOutfitColor = TeamColorsPrimary;
			avatar.SecondaryOutfitColor = TeamColorsSecondary;
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
					var splitName = cm.Name.Split(new [] { ' ' }, 2);
					if (splitName.Any(n => crewMembers.Keys.Any(k => k.Contains(n)) || RetiredCrew.Keys.Any(k => k.Contains(n)) || Recruits.Keys.Any(k => k.Contains(n))) || cm.Name == Manager.Name)
					{
						cm.Name = cm.SelectNewName();
					}
					else
					{
						unqiue = true;
					}
				}
				else
				{
					if (crewMembers.ContainsKey(cm.Name) || RetiredCrew.ContainsKey(cm.Name) || Recruits.ContainsKey(cm.Name) || cm.Name == Manager.Name)
					{
						cm.Name = cm.SelectNewName();
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
			if (Boat.Positions.Count == 0)
			{
				return false;
			}
			return crewMembers.Count + 1 <= (Boat.Positions.Count + 1) * 2 && Recruits.Count != 0;
		}

		/// <summary>
		/// Calculate how many hiring actions the player can still perform before reaching the limit
		/// </summary>
		public int CrewLimitLeft()
		{
			if (Boat.Positions.Count == 0)
			{
				return 0;
			}
			return ((Boat.Positions.Count + 1) * 2) - crewMembers.Count;
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
				iat.RemoveCharacters(new List<string> { member.Key });
				if (StaticRandom.Int(0, 100) % (int)config.ConfigValues[ConfigKeys.RecruitChangeChance] != 0)
				{
					recuritsToRemove.Add(member.Key);
				}
			}
			foreach (var member in recuritsToRemove)
			{
				Recruits.Remove(member);
			}

			//for the amount of empty recruit spaces, create a new recruit
			var amount = (int)config.ConfigValues[ConfigKeys.RecruitCount] - Recruits.Count;
			for (var i = 0; i < amount; i++)
			{
				var position = Boat.GetWeakPosition(CrewMembers.Values.Concat(Recruits.Values).ToList());
				var newMember = new CrewMember(position, Nationality, config);
				UniqueNameCheck(newMember);
				Recruits.Add(newMember.Name, newMember);
			}
			var storeNum = 0;
			//set up the files for each recruit and their avatar
			foreach (var recruit in Recruits)
			{
				recruit.Value.CreateFile(iat, storageLocation, "Recruit" + storeNum);
				storeNum++;
				recruit.Value.UpdateBeliefs("Recruit");
				recruit.Value.Avatar = new Avatar(recruit.Value, false);
			}
			iat.SaveConfigurationToFile(iat.AssetFilePath);
		}

		/// <summary>
		/// Add a recruit into crewMembers
		/// </summary>
		internal void AddRecruit(CrewMember member)
		{
			//remove recruit from the current list of characters in the game
			iat.RemoveCharacters(new List<string> { member.Name });
			//set up recruit as a 'proper' character in the game
			member.CreateFile(iat, storageLocation);
			member.Avatar.UpdateAvatarBeliefs(member);
			member.Avatar = new Avatar(member, true, true);
			SetCrewColors(member.Avatar);
			var currentNames = crewMembers.Keys.ToList();
			currentNames.Add(Manager.Name);
			member.CreateInitialOpinions(currentNames);
			foreach (var cm in crewMembers.Values)
			{
				cm.CreateInitialOpinion(member.Name);
			}
			AddCrewMember(member);
			member.UpdateBeliefs("null");
			member.SaveStatus();
			Recruits.Remove(member.Name);
			iat.SaveConfigurationToFile(iat.AssetFilePath);
		}

		/// <summary>
		/// Calculate if the player should be able to fire characters from their crew
		/// </summary>
		public bool CanRemoveFromCrew()
		{
			if (Boat.Positions.Count == 0)
			{
				return false;
			}
			return CrewMembers.Count - 1 >= Boat.Positions.Count;
		}

		/// <summary>
		/// Retire a CrewMember, meaning they can no longer be assigned to a position (mostly used for historical positions)
		/// </summary>
		internal void RetireCrew(CrewMember crewMember)
		{
			var current = crewMember.GetBoatPosition(Boat.PositionCrew);
			if (current != Position.Null)
			{
				Boat.UnassignCrewMember(current);
			}
			crewMembers.Remove(crewMember.Name);
			RetiredCrew.Add(crewMember.Name, crewMember);
			crewMember.Retire();
			foreach (var cm in crewMembers.Values)
			{
				cm.CrewOpinions.Remove(crewMember.Name);
				cm.RevealedCrewOpinions.Remove(crewMember.Name);
				cm.RevealedCrewOpinionAges.Remove(crewMember.Name);
			}
		}

		//Tick all CrewMembers in crewMembers and Retired
		internal void TickCrewMembers(int amount)
		{
			foreach (var cm in crewMembers.Values)
			{
				cm.TickUpdate(amount);
			}
			foreach (var cm in RetiredCrew.Values)
			{
				cm.TickUpdate(amount);
			}
		}

		/// <summary>
		/// Save the current status of each CrewMember for this Team
		/// </summary>
		internal void ConfirmChanges(int actionAllowance)
		{
			foreach (var crewMember in crewMembers.Values)
			{
				crewMember.TickRevealedOpinionAge();
			}
			Manager.SaveStatus();
			PostRaceRest();
			TickCrewMembers(0);
			PromoteBoat();
			//update available recruits for the next race
			CreateRecruits();
		}

		/// <summary>
		/// Update the current type of Boat to use for this Team
		/// </summary>
		public void PromoteBoat()
		{
			var extraMembers = Boat.Positions.Count;
			if (!Boat.PromoteBoat(RaceHistory))
			{
				return;
			}
			if (Boat.Type == "Finished")
			{
				Finished = true;
				return;
			}
			//store that the boat type has been changed
			Manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), Boat.Type);
			Manager.SaveStatus();
			//calculate how many new members should be created
			extraMembers = (Boat.Positions.Count - extraMembers) * 2;
			//reset the positions on the boat to those for the new type
			for (var i = 0; i < extraMembers; i++)
			{
				var currentNames = crewMembers.Keys.ToList();
				currentNames.Add(Manager.Name);
				//only create a new CrewMember if the crew limit can support it
				if (CanAddToCrew())
				{
					var position = Boat.GetWeakPosition(CrewMembers.Values.Concat(Recruits.Values).ToList());
					var newMember = new CrewMember(position, Nationality, config);
					UniqueNameCheck(newMember);
					newMember.CreateFile(iat, storageLocation);
					newMember.Avatar = new Avatar(newMember);
					SetCrewColors(newMember.Avatar);
					newMember.CreateInitialOpinions(currentNames);
					foreach (var cm in crewMembers.Values)
					{
						cm.CreateInitialOpinion(newMember.Name);
					}
					newMember.UpdateBeliefs("null");
					newMember.SaveStatus();
					iat.SaveConfigurationToFile(iat.AssetFilePath);
					//if the boat is under-staffed for the current boat size, this new CrewMember is not counted
					if (!CanRemoveFromCrew())
					{
						i--;
					}
					AddCrewMember(newMember);
				}
			}
		}

		/// <summary>
		/// Get the average mood of the crew
		/// </summary>
		public float AverageTeamMood()
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
		public float AverageTeamManagerOpinion()
		{
			var opinion = 0f;
			foreach (var crewMember in crewMembers.Values)
			{
				if (crewMember.CrewOpinions.ContainsKey(Manager.Name))
				{
					opinion += crewMember.CrewOpinions[Manager.Name];
				}
			}
			opinion = opinion / crewMembers.Count;
			return opinion;
		}

		/// <summary>
		/// Get the average opinion of the crew
		/// </summary>
		public float AverageTeamOpinion()
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

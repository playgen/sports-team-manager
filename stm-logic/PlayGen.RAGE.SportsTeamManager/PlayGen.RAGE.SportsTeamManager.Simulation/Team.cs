using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AssetManagerPackage;
using IntegratedAuthoringTool;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Team
	{
		private readonly IntegratedAuthoringToolAsset iat;
		private readonly string storageLocation;
		private readonly ConfigStore config;
		private readonly Dictionary<string, CrewMember> crewMembers;

		public Boat Boat { get; }
		public List<Boat> LineUpHistory { get; set; }
		public List<int> HistoricTimeOffset { get; set; }
		public string Name { get; }
		public Color TeamColorsPrimary { get; set; }
		public Color TeamColorsSecondary { get; set; }
		public Dictionary<string, CrewMember> CrewMembers
		{
			get { return crewMembers.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value); }
		}
		public Dictionary<string, CrewMember> RetiredCrew { get; }
		public Dictionary<string, CrewMember> Recruits { get; }
		public Person Manager { get; set; }

		public Team (IntegratedAuthoringToolAsset i, string storage, ConfigStore con, string name, Boat boat)
		{
			iat = i;
			storageLocation = Path.Combine(storage, name);
			config = con;
			Name = name;
			Boat = boat;
			crewMembers = new Dictionary<string, CrewMember>();
			RetiredCrew = new Dictionary<string, CrewMember>();
			Recruits = new Dictionary<string, CrewMember>();
			LineUpHistory = new List<Boat>();
			HistoricTimeOffset = new List<int>();
		}

		/// <summary>
		/// Add a CrewMember to the list of CrewMembers
		/// </summary>
		public void AddCrewMember(CrewMember crewMember)
		{
			crewMembers.Add(crewMember.Name, crewMember);
		}

		/// <summary>
		/// Set the avatar outfit colors to match the team colors
		/// </summary>
		public void SetCrewColors(Avatar avatar)
		{
			avatar.PrimaryOutfitColor = TeamColorsPrimary;
			avatar.SecondaryOutfitColor = TeamColorsSecondary;
		}

		public void UniqueNameCheck(CrewMember cm)
		{
			var unqiue = false;
			//if the name is already in use by another character, reset their name
			while (!unqiue)
			{
				if (crewMembers.ContainsKey(cm.Name) || RetiredCrew.ContainsKey(cm.Name) || Recruits.ContainsKey(cm.Name) || cm.Name == Manager.Name)
				{
					cm.Name = cm.SelectNewName(cm.Gender);
				}
				else
				{
					unqiue = true;
				}
			}
		}

		/// <summary>
		/// Calculate if the player should be able to hire new characters into their crew
		/// </summary>
		public bool CanAddToCrew()
		{
			return crewMembers.Count + 1 <= (Boat.BoatPositions.Count + 1) * 2 && Recruits.Count != 0;
		}

		/// <summary>
		/// Calculate how many hiring actions the player can still perform before reaching the limit
		/// </summary>
		public int CrewLimitLeft()
		{
			return ((Boat.BoatPositions.Count + 1) * 2) - crewMembers.Count;
		}

		/// <summary>
		/// Update the set of recruits for this Boat
		/// </summary>
		public void CreateRecruits()
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
				var position = Boat.GetWeakPosition(CrewMembers.Values.ToList());
				var newMember = new CrewMember(position, config);
				UniqueNameCheck(newMember);
				Recruits.Add(newMember.Name, newMember);
			}
			var storeNum = 0;
			foreach (var recruit in Recruits)
			{
				recruit.Value.CreateFile(iat, storageLocation, "Recruit" + storeNum);
				storeNum++;
				recruit.Value.Avatar = new Avatar(recruit.Value, false);
				recruit.Value.UpdateBeliefs("Recruit");
				recruit.Value.SaveStatus();
			}
			AssetManager.Instance.Bridge = new BaseBridge();
			iat.SaveToFile(iat.AssetFilePath);
		}

		public void AddRecruit(CrewMember member)
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
			AssetManager.Instance.Bridge = new BaseBridge();
			iat.SaveToFile(iat.AssetFilePath);
		}

		/// <summary>
		/// Calculate if the player should be able to fire characters from their crew
		/// </summary>
		public bool CanRemoveFromCrew()
		{
			return CrewMembers.Count - 1 >= Boat.BoatPositions.Count;
		}

		/// <summary>
		/// Retire a CrewMember, meaning they can no longer be assigned to a position (used for historical positions)
		/// </summary>
		public void RetireCrew(CrewMember crewMember)
		{
			var current = crewMember.GetBoatPosition(Boat.BoatPositionCrew);
			if (current != Position.Null)
			{
				Boat.RemoveCrew(current);
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

		//Tick all CrewMembers
		public void TickCrewMembers(int amount)
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
		/// Save the current status of each CrewMember for this Boat
		/// </summary>
		public void ConfirmChanges(int actionAllowance)
		{
			foreach (var crewMember in crewMembers.Values)
			{
				crewMember.DecisionFeedback(Boat);
			}
			Manager.SaveStatus();
			PostRaceRest();
			TickCrewMembers(0);
		}

		public void PromoteBoat()
		{
			var extraMembers = Boat.BoatPositions.Count;
			Boat.PromoteBoat();
			//store that the boat type has been changed
			Manager.UpdateSingleBelief(NPCBeliefs.BoatType.GetDescription(), Boat.Type);
			Manager.SaveStatus();
			//calculate how many new members should be created
			extraMembers = (Boat.BoatPositions.Count - extraMembers) * 2;
			//reset the positions on the boat to those for the new type
			for (var i = 0; i < extraMembers; i++)
			{
				var currentNames = crewMembers.Keys.ToList();
				currentNames.Add(Manager.Name);
				//only create a new CrewMember if the crew limit can support it
				if (CanAddToCrew())
				{
					var position = Boat.GetWeakPosition(CrewMembers.Values.ToList());
					var newMember = new CrewMember(position, config);
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
					AssetManager.Instance.Bridge = new BaseBridge();
					iat.SaveToFile(iat.AssetFilePath);
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
		/// Set all CrewMembers who raced to not be available for the set amount of races
		/// </summary>
		private void PostRaceRest()
		{
			foreach (var crewMember in crewMembers.Values)
			{
				crewMember.RaceRest(Boat.BoatPositionCrew.ContainsValue(crewMember));
			}
		}
	}
}

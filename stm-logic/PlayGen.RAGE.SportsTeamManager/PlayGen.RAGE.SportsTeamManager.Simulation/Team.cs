using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using AssetManagerPackage;

using IntegratedAuthoringTool;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Team
	{
		private IntegratedAuthoringToolAsset _iat;
		private string _storageLocation;
		private readonly ConfigStore _config;
		private readonly Dictionary<string, CrewMember> _crewMembers;

		public Boat Boat { get; }
		public List<Boat> LineUpHistory { get; set; }
		public List<int> HistoricTimeOffset { get; set; }
		public string Name { get; }
		public Color TeamColorsPrimary { get; set; }
		public Color TeamColorsSecondary { get; set; }
		public Dictionary<string, CrewMember> CrewMembers
		{
			get { return _crewMembers.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value); }
		}
		public Dictionary<string, CrewMember> RetiredCrew { get; }
		public Dictionary<string, CrewMember> Recruits { get; }
		public Person Manager { get; set; }

		public Team (IntegratedAuthoringToolAsset iat, string storageLocation, ConfigStore config, string name, Boat boat)
		{
			_iat = iat;
			_storageLocation = Path.Combine(storageLocation, name);
			_config = config;
			Name = name;
			Boat = boat;
			_crewMembers = new Dictionary<string, CrewMember>();
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
			_crewMembers.Add(crewMember.Name, crewMember);
		}

		/// <summary>
		/// Set the avatar outfit colors to match the team colors
		/// </summary>
		public void SetCrewColors(Avatar avatar)
		{
			avatar.PrimaryOutfitColor = TeamColorsPrimary;
			avatar.SecondaryOutfitColor = TeamColorsSecondary;
		}

		public void UniqueNameCheck(Random random, CrewMember cm)
		{
			var unqiue = false;
			//if the name is already in use by another character, reset their name
			while (!unqiue)
			{
				if (_crewMembers.ContainsKey(cm.Name) || RetiredCrew.ContainsKey(cm.Name) || Recruits.ContainsKey(cm.Name) || cm.Name == Manager.Name)
				{
					cm.Name = cm.SelectNewName(cm.Gender, random);
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
			if (_crewMembers.Count + 1 > (Boat.BoatPositions.Count + 1) * 2 || Recruits.Count == 0)
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
			return ((Boat.BoatPositions.Count + 1) * 2) - _crewMembers.Count;
		}

		/// <summary>
		/// Update the set of recruits for this Boat
		/// </summary>
		public void CreateRecruits()
		{
			var rand = new Random();
			var recuritsToRemove = new List<string>();
			//remove recruits from iat and randomly select to remove them from pool of available recruits
			foreach (var member in Recruits)
			{
				_iat.RemoveCharacters(new List<string> { member.Key });
				if (rand.Next(0, 100) % (int)_config.ConfigValues[ConfigKeys.RecruitChangeChance] != 0)
				{
					recuritsToRemove.Add(member.Key);
				}
			}
			foreach (var member in recuritsToRemove)
			{
				Recruits.Remove(member);
			}

			//for the amount of empty recruit spaces, create a new recruit
			var amount = (int)_config.ConfigValues[ConfigKeys.RecruitCount] - Recruits.Count;
			for (var i = 0; i < amount; i++)
			{
				var position = Boat.GetWeakPosition(rand, CrewMembers.Values.ToList());
				var newMember = new CrewMember(rand, position, _config);
				UniqueNameCheck(rand, newMember);
				Recruits.Add(newMember.Name, newMember);
			}
			var storeNum = 0;
			foreach (var recruit in Recruits)
			{
				recruit.Value.CreateFile(_iat, _storageLocation, "Recruit" + storeNum);
				storeNum++;
				recruit.Value.Avatar = new Avatar(recruit.Value, false);
				recruit.Value.UpdateBeliefs("Recruit");
				recruit.Value.SaveStatus();
			}
			AssetManager.Instance.Bridge = new BaseBridge();
			_iat.SaveToFile(_iat.AssetFilePath);
		}

		public void AddRecruit(CrewMember member)
		{
			//remove recruit from the current list of characters in the game
			_iat.RemoveCharacters(new List<string> { member.Name });
			//set up recruit as a 'proper' character in the game
			member.CreateFile(_iat, _storageLocation);
			member.Avatar.UpdateAvatarBeliefs(member);
			member.Avatar = new Avatar(member, true, true);
			SetCrewColors(member.Avatar);
			var random = new Random();
			var currentNames = _crewMembers.Keys.ToList();
			currentNames.Add(Manager.Name);
			member.CreateInitialOpinions(random, currentNames);
			foreach (var cm in _crewMembers.Values)
			{
				cm.CreateInitialOpinion(random, member.Name);
			}
			AddCrewMember(member);
			member.UpdateBeliefs("null");
			member.SaveStatus();
			Recruits.Remove(member.Name);
			AssetManager.Instance.Bridge = new BaseBridge();
			_iat.SaveToFile(_iat.AssetFilePath);
		}

		/// <summary>
		/// Calculate if the player should be able to fire characters from their crew
		/// </summary>
		public bool CanRemoveFromCrew()
		{
			if (CrewMembers.Count - 1 < Boat.BoatPositions.Count)
			{
				return false;
			}
			return true;
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
			_crewMembers.Remove(crewMember.Name);
			RetiredCrew.Add(crewMember.Name, crewMember);
			crewMember.Retire();
			foreach (var cm in _crewMembers.Values)
			{
				cm.CrewOpinions.Remove(crewMember.Name);
				cm.RevealedCrewOpinions.Remove(crewMember.Name);
			}
		}

		//Tick all CrewMembers
		public void TickCrewMembers(int amount)
		{
			foreach (var cm in _crewMembers.Values)
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
			foreach (var crewMember in _crewMembers.Values)
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
			var rand = new Random();
			for (var i = 0; i < extraMembers; i++)
			{
				var currentNames = _crewMembers.Keys.ToList();
				currentNames.Add(Manager.Name);
				//only create a new CrewMember if the crew limit can support it
				if (CanAddToCrew())
				{
					var position = Boat.GetWeakPosition(rand, CrewMembers.Values.ToList());
					var newMember = new CrewMember(rand, position, _config);
					newMember.CreateFile(_iat, _storageLocation);
					newMember.Avatar = new Avatar(newMember);
					SetCrewColors(newMember.Avatar);
					newMember.CreateInitialOpinions(rand, currentNames);
					foreach (var cm in _crewMembers.Values)
					{
						cm.CreateInitialOpinion(rand, newMember.Name);
					}
					newMember.UpdateBeliefs("null");
					newMember.SaveStatus();
					AssetManager.Instance.Bridge = new BaseBridge();
					_iat.SaveToFile(_iat.AssetFilePath);
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
			foreach (var crewMember in _crewMembers.Values)
			{
				crewMember.RaceRest(Boat.BoatPositionCrew.ContainsValue(crewMember));
			}
		}
	}
}

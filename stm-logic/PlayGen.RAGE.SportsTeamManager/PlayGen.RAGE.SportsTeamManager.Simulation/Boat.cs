using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GAIPS.Rage;
using IntegratedAuthoringTool;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Boat
	{
		public string Name { get; set; }
		public int[] TeamColorsPrimary { get; set; }
		public int[] TeamColorsSecondary { get; set; }
		public List<BoatPosition> BoatPositions { get; set; }
		public List<List<BoatPosition>> IdealCrew { get; set; }
		public List<CrewMember> UnassignedCrew { get; set; }
		public List<CrewMember> RetiredCrew { get; set; }
		public List<CrewMember> Recruits { get; set; }
		public int BoatScore { get; set; }
		public float IdealMatchScore { get; set; }
		public Person Manager { get; set; }
		private ConfigStore _config { get;}

		/// <summary>
		/// Boat constructor
		/// </summary>
		public Boat(ConfigStore config)
		{
			BoatPositions = new List<BoatPosition>();
			UnassignedCrew = new List<CrewMember>();
			RetiredCrew = new List<CrewMember>();
			IdealCrew = new List<List<BoatPosition>>();
			Recruits = new List<CrewMember>();
			_config = config;
		}

		/// <summary>
		/// Get a list of all the CrewMember assigned to this Boat, including those currently not in a position
		/// </summary>
		public List<CrewMember> GetAllCrewMembers()
		{
			List<CrewMember> crew = new List<CrewMember>();
			foreach (CrewMember crewMember in UnassignedCrew)
			{
				crew.Add(crewMember);
			}
			foreach (BoatPosition boatPosition in BoatPositions)
			{
				if (boatPosition.CrewMember != null)
				{
					crew.Add(boatPosition.CrewMember);
				}
			}
			crew = crew.OrderBy(c => c.Name).ToList();
			return crew;
		}

		/// <summary>
		/// Get a list of all CrewMembers assigned to this boat, including those marked as 'retired' and thus cannot go into a position
		/// </summary>
		public List<CrewMember> GetAllCrewMembersIncludingRetired()
		{
			List<CrewMember> crew = GetAllCrewMembers();
			foreach (CrewMember crewMember in RetiredCrew)
			{
				crew.Add(crewMember);
			}
			crew = crew.OrderBy(c => c.Name).ToList();
			return crew;
		}

		/// <summary>
		/// Add a CrewMember to the list of UnassignedCrew
		/// </summary>
		public void AddCrew(CrewMember crewMember)
		{
			var currentPosition = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (currentPosition != null)
			{
				return;
			}
			var current = UnassignedCrew.SingleOrDefault(c => c == crewMember);
			if (current != null)
			{
				return;
			}
			UnassignedCrew.Add(crewMember);
			UpdateBoatScore();
			crewMember.Avatar.PrimaryOutfitColor = Color.FromArgb(255, TeamColorsPrimary[0], TeamColorsPrimary[1], TeamColorsPrimary[2]);
			crewMember.Avatar.SecondaryOutfitColor = Color.FromArgb(255, TeamColorsPrimary[0], TeamColorsPrimary[1], TeamColorsPrimary[2]);
		}

		/// <summary>
		/// Assign a CrewMember to a BoatPosition
		/// </summary>
		public void AssignCrew(BoatPosition boatPosition, CrewMember crewMember)
		{
			if (crewMember != null)
			{
				var current = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
				if (current != null)
				{
					RemoveCrew(current);
				}
			}
			if (boatPosition != null)
			{
				if (UnassignedCrew.Contains(crewMember))
				{
					UnassignedCrew.Remove(crewMember);
				}
				if (boatPosition.CrewMember != null)
				{
					RemoveCrew(boatPosition);
				}
				boatPosition.CrewMember = crewMember;
				if (boatPosition.CrewMember != null)
				{
					crewMember.OpinionChange += new EventHandler(OnOpinionChange);
				}
			}
			if (crewMember != null && boatPosition != null)
			{
				crewMember.UpdateBeliefs(boatPosition.Position.Name);
			}
			UpdateBoatScore();
		}

		/// <summary>
		/// Remove a Crewmember from their BoatPosition and add them to the list of UnassignedCrew
		/// </summary>
		public void RemoveCrew(BoatPosition boatPosition)
		{
			boatPosition.CrewMember.OpinionChange -= new EventHandler(OnOpinionChange);
			UnassignedCrew.Add(boatPosition.CrewMember);
			boatPosition.CrewMember.UpdateBeliefs("null");
			boatPosition.CrewMember = null;
		}

		/// <summary>
		/// Remove all assigned CrewMembers
		/// </summary>
		public void RemoveAllCrew()
		{
			foreach (BoatPosition boatPosition in BoatPositions)
			{
				if (boatPosition.CrewMember != null)
				{
					RemoveCrew(boatPosition);
				}
			}
		}

		public void CreateRecruits(IntegratedAuthoringToolAsset iat, IStorageProvider templateStorage, IStorageProvider savedStorage, string storageLocation)
		{
			Random rand = new Random();
			List<CrewMember> recuritsToRemove = new List<CrewMember>();
			foreach (CrewMember member in Recruits)
			{
				iat.RemoveCharacters(new List<string>() { member.Name });
				if (rand.Next(0, 100) % 3 != 0)
				{
					recuritsToRemove.Add(member);
				}
			}
			foreach (CrewMember member in recuritsToRemove)
			{
				Recruits.Remove(member);
			}
			
			int amount = BoatPositions.Count + 1 - Recruits.Count;
			for (int i = 0; i < amount; i++)
			{
				CrewMember newMember = CreateNewMember(rand);
				Recruits.Add(newMember);
			}
			for (int i = 0; i < Recruits.Count; i++)
			{
				Recruits[i].CreateFile(iat, templateStorage, savedStorage, storageLocation, "Recruit" + i);
				Recruits[i].Avatar = new Avatar(Recruits[i], false);
				Recruits[i].UpdateBeliefs("Recruit");
				Recruits[i].SaveStatus();
			}
			iat.SaveToFile(savedStorage, iat.AssetFilePath);
		}

		public CrewMember CreateNewMember(Random rand)
		{
			CrewMember newMember = new CrewMember(rand, _config);
			bool unqiue = false;
			while (!unqiue)
			{
				if (GetAllCrewMembers().Count(c => c.Name == newMember.Name) > 0 || RetiredCrew.Count(c => c.Name == newMember.Name) > 0 || Recruits.Count(c => c.Name == newMember.Name) > 0 || newMember.Name == Manager.Name)
				{
					newMember.Name = newMember.SelectNewName(newMember.Gender, rand);
				}
				else
				{
					unqiue = true;
				}
			}
			Position selectedPerferred = null;
			Dictionary<Position, int> positionStrength = GetPositionStrength();
			if (positionStrength.OrderBy(kvp => kvp.Value).Last().Value - positionStrength.OrderBy(kvp => kvp.Value).First().Value == 0)
			{
				int positionValue = rand.Next(0, BoatPositions.Count + 1);
				selectedPerferred = positionValue < BoatPositions.Count ? BoatPositions[positionValue].Position : null;
			}
			else
			{
				int lowValue = positionStrength.OrderBy(kvp => kvp.Value).First().Value;
				Position[] lowPositions = positionStrength.Where(kvp => kvp.Value == lowValue).Select(kvp => kvp.Key).ToArray();
				selectedPerferred = lowPositions.OrderBy(p => Guid.NewGuid()).First();
			}
			newMember.Skills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (selectedPerferred != null)
				{
					if (selectedPerferred.RequiredSkills.Contains(skill))
					{
						newMember.Skills.Add(skill, rand.Next((int)_config.ConfigValues[ConfigKeys.GoodPositionRating.ToString()], 11));
					}
					else
					{
						newMember.Skills.Add(skill, rand.Next(1, (int)_config.ConfigValues[ConfigKeys.BadPositionRating.ToString()] + 1));
					}
				}
				else
				{
					newMember.Skills.Add(skill, rand.Next((int)_config.ConfigValues[ConfigKeys.RandomSkillLow.ToString()], (int)_config.ConfigValues[ConfigKeys.RandomSkillHigh.ToString()] + 1));
				}
			}
			return newMember;
		}

		Dictionary<Position, int> GetPositionStrength()
		{
			Dictionary<Position, int> positionStrength = new Dictionary<Position, int>();
			foreach (Position pos in BoatPositions.Select(bp => bp.Position))
			{
				positionStrength.Add(pos, 0);
				foreach (CrewMember cm in GetAllCrewMembers())
				{
					if (pos.GetPositionRating(cm) >= (int)_config.ConfigValues[ConfigKeys.GoodPositionRating.ToString()])
					{
						positionStrength[pos]++;
					}
				}
				foreach (CrewMember cm in Recruits)
				{
					if (pos.GetPositionRating(cm) >= (int)_config.ConfigValues[ConfigKeys.GoodPositionRating.ToString()])
					{
						positionStrength[pos]++;
					}
				}
			}
			return positionStrength;
		}

		/// <summary>
		/// Retire a CrewMember, meaning they can no longer be assigned to a position (used for historical positions)
		/// </summary>
		public void RetireCrew(CrewMember crewMember)
		{
			var currentPosition = BoatPositions.SingleOrDefault(bp => bp.CrewMember == crewMember);
			if (currentPosition != null)
			{
				RemoveCrew(currentPosition);
			}
			var current = UnassignedCrew.SingleOrDefault(c => c == crewMember);
			if (current != null)
			{
				UnassignedCrew.Remove(crewMember);
			}
			RetiredCrew.Add(crewMember);
			crewMember.Retire();
			foreach (CrewMember cm in GetAllCrewMembers())
			{
				CrewOpinion opinionToRemove = cm.CrewOpinions.FirstOrDefault(co => co.Person.Name == crewMember.Name);
				if (opinionToRemove != null)
				{
					cm.CrewOpinions.Remove(opinionToRemove);
				}
				CrewOpinion revealedToRemove = cm.RevealedCrewOpinions.FirstOrDefault(co => co.Person.Name == crewMember.Name);
				if (opinionToRemove != null)
				{
					cm.RevealedCrewOpinions.Remove(revealedToRemove);
				}
			}
			GetIdealCrew();
			UpdateBoatScore();
		}

		/// <summary>
		/// Triggered when a Crewmember's opinion on a Person changes in order to update the Boat's score to an accurate value
		/// </summary>
		void OnOpinionChange(object sender, EventArgs e)
		{
			UpdateBoatScore();
		}

		/// <summary>
		/// Update the score in each BoatPosition in order to get the score for this Boat
		/// </summary>
		public void UpdateBoatScore()
		{
			foreach (BoatPosition bp in BoatPositions)
			{
				bp.UpdateCrewMemberScore(this, _config);
			}
			BoatScore = BoatPositions.Sum(bp => bp.PositionScore);
			UpdateIdealScore();
		}

		public void GetIdealCrew()
		{
			Boat tempBoat = (Boat)Activator.CreateInstance(this.GetType(), _config);
			tempBoat.Manager = Manager;
			IEnumerable<CrewMember> availableCrew = GetAllCrewMembers().Where(cm => cm.restCount <= 0);
			IEnumerable<IEnumerable<CrewMember>> crewCombos = GetPermutations(availableCrew, BoatPositions.Count - 1);
			int bestScore = 0;
			List<List<BoatPosition>> bestCrew = new List<List<BoatPosition>>();
			foreach (IEnumerable<CrewMember> possibleCrew in crewCombos)
			{
				List<CrewMember> crewList = possibleCrew.ToList();
				for (int i = 0; i < tempBoat.BoatPositions.Count; i++)
				{
					tempBoat.BoatPositions[i].CrewMember = crewList[i];
				}
				tempBoat.UpdateBoatScore();
				if (tempBoat.BoatScore >= bestScore)
				{
					List<BoatPosition> thisCrew = new List<BoatPosition>();
					foreach (BoatPosition bp in tempBoat.BoatPositions)
					{
						thisCrew.Add(new BoatPosition
						{
							Position = bp.Position,
							CrewMember = bp.CrewMember,
							PositionScore = bp.PositionScore
						});
					}
					if (tempBoat.BoatScore > bestScore)
					{
						bestCrew.Clear();
						bestScore = tempBoat.BoatScore;
					}
					bestCrew.Add(thisCrew);
					
				} 
			}
			IdealCrew = bestCrew;
			UpdateIdealScore();
		}

		public void UpdateIdealScore()
		{
			IdealMatchScore = 0;
			foreach (List<BoatPosition> crew in IdealCrew)
			{
				float currentIdealMatch = 0;
				for (int i = 0; i < crew.Count; i++)
				{
					if (crew[i].CrewMember == BoatPositions[i].CrewMember)
					{
						currentIdealMatch++;
					}
					else
					{
						foreach (BoatPosition ideal in crew)
						{
							if (ideal.CrewMember == BoatPositions[i].CrewMember)
							{
								currentIdealMatch += 0.1f;
							}
						}
					}
				}
				if (currentIdealMatch > IdealMatchScore)
				{
					IdealMatchScore = currentIdealMatch;
				}
			}
		}

		public void TickCrewMembers(int amount)
		{
			foreach (CrewMember cm in GetAllCrewMembersIncludingRetired())
			{
				cm.TickUpdate(amount);
			}
		}

		/// <summary>
		/// Save the current status of each CrewMember for this Boat
		/// </summary>
		public void ConfirmChanges(int actionAllowance)
		{
			TickCrewMembers(actionAllowance);
			List<CrewMember> crew = GetAllCrewMembers();
			crew = crew.OrderBy(p => p.Name).ToList();
			crew.ForEach(p => p.DecisionFeedback(this));
			Manager.SaveStatus();
			PostRaceRest();
			GetIdealCrew();
			UpdateBoatScore();
			TickCrewMembers(0);
		}

		public void PostRaceRest()
		{
			List<CrewMember> crew = GetAllCrewMembers();
			crew.ForEach(p => p.RaceRest(BoatPositions.SingleOrDefault(bp => bp.CrewMember == p) != null));
		}

		private IEnumerable<IEnumerable<T>>GetPermutations<T>(IEnumerable<T> list, int length)
		{
			if (length == 0)
			{
				return list.Select(t => new T[] { t }.AsEnumerable());
				
			}
			return GetPermutations(list, length - 1)
				.SelectMany(t => list.Where(o => !t.Contains(o)),
					(t1, t2) => t1.Concat(new T[] { t2 }));
		}
	}
}
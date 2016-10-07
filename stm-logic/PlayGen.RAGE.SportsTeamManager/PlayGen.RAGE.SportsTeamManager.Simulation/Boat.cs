using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssetManagerPackage;
using IntegratedAuthoringTool;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Stores boat and crew details and contains functionality related to adjusting either
	/// </summary>
	public class Boat
	{
		private readonly List<List<KeyValuePair<CrewMember, int>>> _idealCrew;
		private List<KeyValuePair<CrewMember, int>> _nearestIdealMatch;
		private readonly ConfigStore _config;

		public string Type { get; private set; }
		public string Name { get; private set; }
		public byte[] TeamColorsPrimary { get; set; }
		public byte[] TeamColorsSecondary { get; set; }
		public List<Position> BoatPositions { get; set; }
		public Dictionary<Position, CrewMember> BoatPositionCrew { get; set; }
		public Dictionary<Position, int> BoatPositionScores { get; set; }
		public Dictionary<string, CrewMember> UnassignedCrew { get; }
		public Dictionary<string, CrewMember> RetiredCrew { get; }
		public Dictionary<string, CrewMember> Recruits { get; }
		public int BoatScore { get; set; }
		public float IdealMatchScore { get; set; }
		public List<string> SelectionMistakes { get; set; }
		public Person Manager { get; set; }

		/// <summary>
		/// Boat constructor
		/// </summary>
		public Boat(ConfigStore config, string name, string type)
		{
			BoatPositions = new List<Position>();
			BoatPositionCrew = new Dictionary<Position, CrewMember>();
			BoatPositionScores = new Dictionary<Position, int>();
			UnassignedCrew = new Dictionary<string, CrewMember>();
			RetiredCrew = new Dictionary<string, CrewMember>();
			_idealCrew = new List<List<KeyValuePair<CrewMember, int>>>();
			Recruits = new Dictionary<string, CrewMember>();
			SelectionMistakes = new List<string>();
			_config = config;
			Name = name;
			Type = type;
			GetPositions();
		}

		public void ChangeBoatType(string type)
		{
			Type = type;
			GetPositions();
		}

		private void GetPositions()
		{
			BoatPositions.Clear();
			BoatPositions = _config.BoatTypes[Type];
			var oldPositions = BoatPositionCrew.Keys.Where(bp => !BoatPositions.Contains(bp)).ToList();
			foreach (var oldPosition in oldPositions)
			{
				RemoveCrew(oldPosition);
			}
		}

		/// <summary>
		/// Get a list of all the CrewMember assigned to this Boat, including those currently not in a position
		/// </summary>
		public Dictionary<string, CrewMember> GetAllCrewMembers()
		{
			var crew = new Dictionary<string, CrewMember>();
			foreach (var crewMember in UnassignedCrew)
			{
				crew.Add(crewMember.Key, crewMember.Value);
			}
			foreach (var boatPosition in BoatPositionCrew)
			{
				crew.Add(boatPosition.Value.Name, boatPosition.Value);
			}
			crew = crew.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
			return crew;
		}

		/// <summary>
		/// Get a list of all CrewMembers assigned to this boat, including those marked as 'retired' and thus cannot go into a position
		/// </summary>
		public Dictionary<string, CrewMember> GetAllCrewMembersIncludingRetired()
		{
			var crew = GetAllCrewMembers();
			foreach (var crewMember in RetiredCrew)
			{
				crew.Add(crewMember.Key, crewMember.Value);
			}
			crew = crew.OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);
			return crew;
		}

		/// <summary>
		/// Add a CrewMember to the list of UnassignedCrew
		/// </summary>
		public void AddCrew(CrewMember crewMember)
		{
			var currentPosition = crewMember.GetBoatPosition(this);
			if (currentPosition != Position.Null)
			{
				return;
			}
			if (UnassignedCrew.ContainsKey(crewMember.Name))
			{
				return;
			}
			UnassignedCrew.Add(crewMember.Name, crewMember);
		}

		/// <summary>
		/// Set the avatar outfit colors to match the team colors
		/// </summary>
		public void SetCrewColors(Avatar avatar)
		{
			avatar.PrimaryOutfitColor = new Color(TeamColorsPrimary[0], TeamColorsPrimary[1], TeamColorsPrimary[2], 255);
			avatar.SecondaryOutfitColor = new Color(TeamColorsPrimary[0], TeamColorsPrimary[1], TeamColorsPrimary[2], 255);
		}

		/// <summary>
		/// Assign a CrewMember to a BoatPosition
		/// </summary>
		public void AssignCrew(Position position, CrewMember crewMember)
		{
			if (crewMember != null)
			{
				var current = crewMember.GetBoatPosition(this);
				if (current != Position.Null)
				{
					RemoveCrew(current);
				}
			}
			if (position != Position.Null)
			{
				if (UnassignedCrew.ContainsKey(crewMember.Name))
				{
					UnassignedCrew.Remove(crewMember.Name);
				}
				if (BoatPositionCrew.ContainsKey(position))
				{
					RemoveCrew(position);
				}
				BoatPositionCrew.Add(position, crewMember);
				BoatPositionScores.Add(position, 0);
			}
			if (crewMember != null && position != Position.Null)
			{
				crewMember.UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), position.GetName());
			}
		}

		/// <summary>
		/// Remove a CrewMember from their BoatPosition and add them to the list of UnassignedCrew
		/// </summary>
		private void RemoveCrew(Position boatPosition)
		{
			UnassignedCrew.Add(BoatPositionCrew[boatPosition].Name, BoatPositionCrew[boatPosition]);
			BoatPositionCrew[boatPosition].UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), "null");
			BoatPositionCrew.Remove(boatPosition);
			BoatPositionScores.Remove(boatPosition);
		}

		public void UniqueNameCheck(Random random, CrewMember cm)
		{
			var unqiue = false;
			//if the name is already in use by another character, reset their name
			while (!unqiue)
			{
				if (GetAllCrewMembers().ContainsKey(cm.Name) || RetiredCrew.ContainsKey(cm.Name) || Recruits.ContainsKey(cm.Name) || cm.Name == Manager.Name)
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
		/// Update the set of recruits for this Boat
		/// </summary>
		public void CreateRecruits(IntegratedAuthoringToolAsset iat, string storageLocation)
		{
			var rand = new Random();
			var recuritsToRemove = new List<string>();
			//remove recruits from iat and randomly select to remove them from pool of available recruits
			foreach (var member in Recruits)
			{
				iat.RemoveCharacters(new List<string> { member.Key });
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
				var newMember = new CrewMember(rand, this, _config);
				Recruits.Add(newMember.Name, newMember);
			}
			//for each recruit, save their asset files, avatar and save files/add to iat
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

		public Position GetWeakPosition(Random random)
		{
			var positionStrength = GetPositionStrength().OrderBy(kvp => kvp.Value).ToDictionary(p => p.Key, p => p.Value);
			//if there is no position that has more available than another, select one at random
			if (positionStrength.Values.Max() - positionStrength.Values.Min() == 0)
			{
				var positionValue = random.Next(0, BoatPositions.Count + 1);
				return positionValue < BoatPositions.Count ? BoatPositions[positionValue] : Position.Null;
			}
			//select from weaker positions if at least one position has less available members than another 
			var lowValue = positionStrength.Values.Min();
			var lowPositions = positionStrength.Where(kvp => kvp.Value == lowValue).Select(kvp => kvp.Key).ToArray();
			return lowPositions.OrderBy(p => Guid.NewGuid()).First();
		}

		/// <summary>
		/// Get the amount of current crew and recruits that are capable of going into each position
		/// </summary>
		private Dictionary<Position, int> GetPositionStrength()
		{
			var positionStrength = new Dictionary<Position, int>();
			foreach (var pos in BoatPositions)
			{
				positionStrength.Add(pos, 0);
				foreach (var cm in GetAllCrewMembers().Values)
				{
					if (pos.GetPositionRating(cm) >= (int)_config.ConfigValues[ConfigKeys.GoodPositionRating])
					{
						positionStrength[pos]++;
					}
				}
				foreach (var cm in Recruits.Values)
				{
					if (pos.GetPositionRating(cm) >= (int)_config.ConfigValues[ConfigKeys.GoodPositionRating])
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
			var currentPosition = crewMember.GetBoatPosition(this);
			if (currentPosition != Position.Null)
			{
				RemoveCrew(currentPosition);
			}
			if (UnassignedCrew.ContainsKey(crewMember.Name))
			{
				UnassignedCrew.Remove(crewMember.Name);
			}
			RetiredCrew.Add(crewMember.Name, crewMember);
			crewMember.Retire();
			foreach (var cm in GetAllCrewMembers().Values)
			{
				cm.CrewOpinions.Remove(crewMember.Name);
				cm.RevealedCrewOpinions.Remove(crewMember.Name);
			}
		}

		/// <summary>
		/// Update the score in each BoatPosition in order to get the score for this Boat
		/// </summary>
		public void UpdateBoatScore()
		{
			foreach (var bp in BoatPositions)
			{
				if (BoatPositionCrew.ContainsKey(bp))
				{
					UpdateCrewMemberScore(bp, BoatPositionCrew[bp]);
				}
			}
			BoatScore = BoatPositionScores.Values.Sum();
		}

		/// <summary>
		/// Get the current score for this Position on this Boat for this CrewMember
		/// </summary>
		public void UpdateCrewMemberScore(Position position, CrewMember crewMember)
		{
			//Get the average skill rating for this CrewMember in this Position
			var crewScore = position.GetPositionRating(crewMember);

			var opinion = 0;
			var opinionCount = 0;
			var managerOpinion = 0;

			//get the average opinion of every other positioned crew member and the manager
			if (crewMember.CrewOpinions != null && crewMember.CrewOpinions.Count > 0)
			{
				foreach (var bp in BoatPositionCrew)
				{
					if (bp.Key != position)
					{
						opinion += crewMember.CrewOpinions[bp.Value.Name];
						opinionCount++;
					}
				}
				managerOpinion += crewMember.CrewOpinions[Manager.Name];
			}

			if (opinionCount > 0)
			{
				opinion = opinion / opinionCount;
			}

			//add average opinion, manager opinion and current mood to score
			crewScore += (int)(opinion * _config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);

			crewScore += (int)(managerOpinion * _config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]);

			crewScore += (int)(crewMember.GetMood() * _config.ConfigValues[ConfigKeys.MoodRatingWeighting]);

			BoatPositionScores[position] = crewScore;
		}

		/// <summary>
		/// Get the crew set-up(s) that would be worth the highest BoatScore
		/// </summary>
		public void GetIdealCrew()
		{
			var availableCrew = GetAllCrewMembers().Where(cm => cm.Value.RestCount <= 0).ToDictionary(ac => ac.Key, ac => ac.Value);
			if (availableCrew.Count < BoatPositions.Count)
			{
				return;
			}
			//get all crew combinations
			var positionCrewCombos = new Dictionary<string, int>(availableCrew.Count * BoatPositions.Count);
			foreach (var bp in BoatPositions)
			{
				foreach (var cm in availableCrew)
				{
					positionCrewCombos.Add(string.Concat(bp.GetName(), cm.Key), bp.GetPositionRating(cm.Value) + (int)(cm.Value.GetMood() * _config.ConfigValues[ConfigKeys.MoodRatingWeighting]) + (int)(cm.Value.CrewOpinions[Manager.Name] * _config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]));
				}
			}
			var crewCombos = GetPermutations(availableCrew.Keys.ToList(), BoatPositions.Count - 1);
			var crewOpinions = new Dictionary<string, int>(crewCombos.Count);
			foreach (var possibleCrew in crewCombos)
			{
				var crewComboKey = possibleCrew.Aggregate(new StringBuilder(), (sb, s) => sb.Append(s)).ToString();
				crewOpinions.Add(crewComboKey, 0);
				foreach (var crewMember in possibleCrew)
				{
					var opinion = 0;
					var opinionCount = 0;
					foreach (var otherMember in possibleCrew)
					{
						if (crewMember != otherMember)
						{
							opinion += availableCrew[crewMember].CrewOpinions[otherMember];
							opinionCount++;
						}
					}
					opinion = (int)((opinion / opinionCount) * _config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);
					crewOpinions[crewComboKey] += opinion;
				}
			}
			var opinionMax = crewOpinions.Values.Max();
			var positionNames = BoatPositions.Select(bp => bp.GetName()).ToList();
			var bestScore = 0;
			var bestCrew = new List<List<string>>();
			var crewPositionScores = new Dictionary<string, int>();
			int score;
			//for each possible combination
			foreach (var possibleCrew in crewCombos)
			{
				crewPositionScores.Clear();
				positionNames.ForEach(pn => possibleCrew.ForEach(pc => crewPositionScores.Add(string.Concat(pn, pc), positionCrewCombos[string.Concat(pn, pc)])));
				crewPositionScores = crewPositionScores.OrderByDescending(cps => cps.Value).ToDictionary(cps => cps.Key, cps => cps.Value);
				if (crewPositionScores.Values.Take(positionNames.Count).Sum() + opinionMax >= bestScore)
				{
					if (positionNames.Select(pn => crewPositionScores.First(cps => cps.Key.Contains(pn)).Value).Sum() + opinionMax < bestScore)
					{
						continue;
					}
					if (possibleCrew.Select(pc => crewPositionScores.First(cps => cps.Key.Contains(pc)).Value).Sum() + opinionMax < bestScore)
					{
						continue;
					}
					var combos = GetOrderedPermutations(possibleCrew, positionNames.Count - 1);
					foreach (var combo in combos)
					{
						score = 0;
						//assign crew members to their positions and get the score for this set-up
						for (var i = 0; i < combo.Count; i++)
						{
							score += crewPositionScores[string.Concat(positionNames[i], combo[i])];
						}
						if (score + opinionMax < bestScore)
						{
							continue;
						}
						score += crewOpinions[possibleCrew.Aggregate(new StringBuilder(), (sb, s) => sb.Append(s)).ToString()];
						//if the score for this set-up is higher or equal than the current highest, set up a new list of BoatPositions for the set-up
						if (score >= bestScore)
						{
							//if the set-up has a higher score, clear the current list
							if (score > bestScore)
							{
								bestCrew.Clear();
								bestScore = score;
							}
							//add this set-up to the list of best crews
							bestCrew.Add(combo);
						}
					}
				}
			}
			_idealCrew.Clear();
			foreach (var crew in bestCrew)
			{
				var positionedCrew = new List<KeyValuePair<CrewMember, int>>();
				for (var i = 0; i < crew.Count; i++)
				{
					positionedCrew.Add(new KeyValuePair<CrewMember, int>(availableCrew[crew[i]], positionCrewCombos[string.Concat(positionNames[i], crew[i])]));
				}
				_idealCrew.Add(positionedCrew);
			}
			UpdateIdealScore();
		}

		/// <summary>
		/// Find how close the current crew is to being an 'ideal' set-up
		/// </summary>
		private void UpdateIdealScore()
		{
			//reset current values
			IdealMatchScore = 0;
			_nearestIdealMatch = null;
			if (BoatPositionCrew.Count < BoatPositions.Count)
			{
				return;
			}
			//check the current positioned crew against every ideal crew layout
			foreach (var crew in _idealCrew)
			{
				float currentIdealMatch = 0;
				for (var i = 0; i < crew.Count; i++)
				{
					//if the CrewMembers match in both the current and the ideal, add 1 to the currentIdealMatch score
					if (crew[i].Key == BoatPositionCrew[BoatPositions[i]])
					{
						currentIdealMatch++;
					}
					//otherwise, check if this CrewMember is meant to be positioned elsewhere in an ideal set-up. If so, add 0.1f to the currentIdealMatch score
					else
					{
						foreach (var ideal in crew)
						{
							if (ideal.Key == BoatPositionCrew[BoatPositions[i]])
							{
								currentIdealMatch += 0.1f;
							}
						}
					}
				}
				//if the final currentIdealMatch score is higher than the current IdealMatchScore, or _nearestIdealMatch is null (meaning no other ideals have been checked), set this ideal crew to the nearest match
				if (currentIdealMatch > IdealMatchScore || _nearestIdealMatch == null)
				{
					IdealMatchScore = currentIdealMatch;
					_nearestIdealMatch = crew;
				}
			}
			FindAssignmentMistakes();
		}

		/// <summary>
		/// Find all the reasons this current crew is not an 'ideal' crew
		/// </summary>
		private void FindAssignmentMistakes()
		{
			//create a list of all possible 'mistakes' for known values and hidden values
			var mistakeScores = new Dictionary<string, float>();
			var hiddenScores = new Dictionary<string, float>();
			foreach (var skillName in Enum.GetNames(typeof(CrewMemberSkill)))
			{
				mistakeScores.Add(skillName, 0);
				hiddenScores.Add(skillName, 0);
			}
			mistakeScores.Add("CrewOpinion", 0);
			mistakeScores.Add("ManagerOpinion", 0);
			mistakeScores.Add("Mood", 0);
			hiddenScores.Add("CrewOpinion", 0);
			hiddenScores.Add("ManagerOpinion", 0);
			hiddenScores.Add("Mood", 0);
			for (var i = 0; i < BoatPositions.Count; i++)
			{
				//if the position is correctly assigned, there's no need for further checks
				if (BoatPositionCrew[BoatPositions[i]] == _nearestIdealMatch[i].Key)
				{
					continue;
				}
				//for each required skill for this position, get the difference between the rating of the ideal and the current and add to mistakeScores
				foreach (var skill in BoatPositions[i].RequiredSkills())
				{
					mistakeScores[skill.ToString()] += (_nearestIdealMatch[i].Key.Skills[skill] - BoatPositionCrew[BoatPositions[i]].Skills[skill])/(float)BoatPositions[i].RequiredSkills().Count();
					//if the rating of the current positioned CrewMember is not known to the player, add to hiddenScores
					if (BoatPositionCrew[BoatPositions[i]].RevealedSkills[skill] == 0)
					{
						hiddenScores[skill.ToString()] += (_nearestIdealMatch[i].Key.Skills[skill] - BoatPositionCrew[BoatPositions[i]].Skills[skill]) / (float)BoatPositions[i].RequiredSkills().Count();
					}
				}
				//add the difference in opinion of the manager to mistakeScores
				mistakeScores["ManagerOpinion"] += (int)((_nearestIdealMatch[i].Key.CrewOpinions[Manager.Name] - BoatPositionCrew[BoatPositions[i]].CrewOpinions[Manager.Name]) * _config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]);
				//if the player does not know this opinion, add the difference to hiddenScores
				if (BoatPositionCrew[BoatPositions[i]].CrewOpinions[Manager.Name] != BoatPositionCrew[BoatPositions[i]].RevealedCrewOpinions[Manager.Name])
				{
					hiddenScores["ManagerOpinion"] += (int)((_nearestIdealMatch[i].Key.CrewOpinions[Manager.Name] - BoatPositionCrew[BoatPositions[i]].CrewOpinions[Manager.Name]) * _config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]);
				}
				//add the difference in mood to mistakeScores
				mistakeScores["Mood"] += (int)((_nearestIdealMatch[i].Key.GetMood() - BoatPositionCrew[BoatPositions[i]].GetMood()) * _config.ConfigValues[ConfigKeys.MoodRatingWeighting]);
			//calculate the average opinion for this position in the ideal crew
			var idealOpinion = 0;
				foreach (var bp in _nearestIdealMatch)
				{
					if (bp.Key != _nearestIdealMatch[i].Key)
					{
						idealOpinion += _nearestIdealMatch[i].Key.CrewOpinions[bp.Key.Name];
					}
				}
				idealOpinion /= BoatPositions.Count - 1;
				//calculate the average opinion for this position in the current crew and how many opinions are currently unknown
				var currentOpinion = 0;
				var unknownCrewOpinions = 0;
				foreach (var bp in BoatPositions)
				{
					if (BoatPositionCrew[bp] != BoatPositionCrew[BoatPositions[i]])
					{
						currentOpinion += BoatPositionCrew[BoatPositions[i]].CrewOpinions[BoatPositionCrew[bp].Name];
						if (BoatPositionCrew[BoatPositions[i]].CrewOpinions[BoatPositionCrew[bp].Name] != BoatPositionCrew[BoatPositions[i]].RevealedCrewOpinions[BoatPositionCrew[bp].Name])
						{
							unknownCrewOpinions++;
						}
					}
				}
				currentOpinion /= BoatPositions.Count - 1;
				//add the difference to mistakeScores
				mistakeScores["CrewOpinion"] += (int)((idealOpinion - currentOpinion) * _config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);
				//if the percentage of unknown opinions is above the given amount, add the difference to hiddenScores
				if (unknownCrewOpinions >= (BoatPositions.Count - 1) * _config.ConfigValues[ConfigKeys.HiddenOpinionLimit])
				{
					hiddenScores["CrewOpinion"] += (int)((idealOpinion - currentOpinion) * _config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);
				}
			}
			//sort the 'mistakes' by their values, removing all with a score of 0 and below (aka, equal or better to the ideal crew)
			var mistakes = mistakeScores.OrderByDescending(ms => ms.Value).Where(ms => ms.Value > 0).Select(ms => ms.Key).ToList();
			//if the value of the mistake in hiddenScores is more than the given percentage of that in mistakeScores, set this mistake to be 'hidden'
			for (var i = 0; i < mistakes.Count; i++)
			{
				if (hiddenScores[mistakes[i]] >= mistakeScores[mistakes[i]] * _config.ConfigValues[ConfigKeys.HiddenMistakeLimit])
				{
					mistakes[i] = "Hidden";
				}
			}
			//store the current range of mistakes
			SelectionMistakes = mistakes;
		}

		/// <summary>
		/// Get the amount of mistakes requested, adding 'Correct' if not enough exist
		/// </summary>
		public List<string> GetAssignmentMistakes(int returnAmount)
		{
			var mistakes = SelectionMistakes.Take(returnAmount).ToList();
			while (mistakes.Count < returnAmount) {
				mistakes.Insert(0, "Correct");
			}
			return mistakes;
		}

		//Tick all CrewMembers
		public void TickCrewMembers(int amount)
		{
			foreach (var cm in GetAllCrewMembersIncludingRetired().Values)
			{
				cm.TickUpdate(amount);
			}
		}

		/// <summary>
		/// Save the current status of each CrewMember for this Boat
		/// </summary>
		public void ConfirmChanges(int actionAllowance)
		{
			foreach (var crewMember in GetAllCrewMembers().Values)
			{
				crewMember.DecisionFeedback(this);
			}
			Manager.SaveStatus();
			PostRaceRest();
			TickCrewMembers(0);
		}

		/// <summary>
		/// Set all CrewMembers who raced to not be available for the set amount of races
		/// </summary>
		private void PostRaceRest()
		{
			foreach (var crewMember in GetAllCrewMembers().Values)
			{
				crewMember.RaceRest(BoatPositionCrew.ContainsValue(crewMember));
			}
		}

		/// <summary>
		/// Get every possible combination of CrewMembers in every order
		/// </summary>
		private List<List<T>>GetOrderedPermutations<T>(List<T> list, int length)
		{
		   if (length == 0)
			{
				return list.Select(t => new [] { t }.ToList()).ToList();
				
			}
			return GetOrderedPermutations(list, length - 1)
				.SelectMany(t => list.Where(o => !t.Contains(o)),
					(t1, t2) => t1.Concat(new [] { t2 }).ToList()).ToList();
		}

		/// <summary>
		/// Get every possible combination of CrewMembers in no order
		/// </summary>
		private List<List<T>> GetPermutations<T>(List<T> list, int length) where T : IComparable<T>
		{
			if (length == 0)
			{
				return list.Select(t => new[] { t }.ToList()).ToList();

			}
			return GetPermutations(list, length - 1)
				.SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
					(t1, t2) => t1.Concat(new[] { t2 }).ToList()).ToList();
		}
	}
}
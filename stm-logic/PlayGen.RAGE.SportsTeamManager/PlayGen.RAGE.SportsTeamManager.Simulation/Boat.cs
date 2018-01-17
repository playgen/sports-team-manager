using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Stores boat details and contains functionality related to adjusting currently selected crew
	/// </summary>
	public class Boat
	{
		private readonly ConfigStore config;

		public string Type { get; private set; }
		public List<Position> Positions { get; private set; }
		public Dictionary<Position, CrewMember> PositionCrew { get; internal set; }
		public Dictionary<Position, int> PositionScores { get; internal set; }
		public int Score { get; internal set; }
		public float IdealMatchScore { get; internal set; }
		public List<string> SelectionMistakes { get; internal set; }

		/// <summary>
		/// Boat constructor
		/// </summary>
		internal Boat(ConfigStore con, string type)
		{
			Positions = new List<Position>();
			PositionCrew = new Dictionary<Position, CrewMember>();
			PositionScores = new Dictionary<Position, int>();
			SelectionMistakes = new List<string>();
			config = con;
			Type = type;
			GetPositions();
		}

		/// <summary>
		/// Change the current type of boat to a different type and get the positions for this new type
		/// </summary>
		internal bool PromoteBoat(List<Boat> previous)
		{
			var possibleTypes = config.GameConfig.PromotionTriggers.Where(pt => pt.StartType == Type);
			previous = previous.Where(pb => pb.Type == Type).ToList();
			foreach (var type in possibleTypes)
			{
				var consecutiveMatches = 0;
				foreach (var boat in previous)
				{
					if (boat.Score >= type.ScoreRequired)
					{
						consecutiveMatches++;
						if (consecutiveMatches >= type.ScoreMetSinceLast)
						{
							Type = type.NewType;
							GetPositions();
							return true;
						}
					}
					else
					{
						consecutiveMatches = 0;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Get the positions for this boat type, keeping crew members in existing positions in the process
		/// </summary>
		private void GetPositions()
		{
			Positions.Clear();
			if (Type != "Finish")
			{
				Positions = new List<Position>(config.BoatTypes[Type]);
			}
			var oldPositions = PositionCrew.Keys.Where(position => !Positions.Contains(position)).ToList();
			foreach (var oldPosition in oldPositions)
			{
				UnassignCrewMember(oldPosition);
			}
		}

		/// <summary>
		/// Assign a CrewMember to a Position
		/// </summary>
		public void AssignCrewMember(Position position, CrewMember crewMember)
		{
			if (crewMember != null)
			{
				//if a CrewMember is provided and is assigned to a position, remove them from that position
				var current = crewMember.GetBoatPosition(PositionCrew);
				if (current != Position.Null)
				{
					UnassignCrewMember(current);
				}
			}
			if (position != Position.Null)
			{
				//if a Position is provided and has a CrewMember already assigned to it, remove them from that position
				if (PositionCrew.ContainsKey(position))
				{
					UnassignCrewMember(position);
				}
			}
			if (crewMember != null && position != Position.Null)
			{
				//add combination of CrewMember and Position to PositionCrew dictionary
				PositionCrew.Add(position, crewMember);
				PositionScores.Add(position, 0);
				crewMember.UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), position.ToString());
			}
		}

		/// <summary>
		/// Remove a CrewMember from their Position
		/// </summary>
		internal void UnassignCrewMember(Position position)
		{
			PositionCrew[position].UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), "null");
			PositionCrew.Remove(position);
			PositionScores.Remove(position);
		}

		/// <summary>
		/// Get the Position on this Boat with the least CrewMembers able to perform well in it
		/// </summary>
		internal Position GetWeakPosition(List<CrewMember> crewMembers)
		{
			if (Positions.Count == 0)
			{
				return Position.Null;
			}
			//get number of CrewMembers with position rating above set 'good' rating in each position
			var positionStrength = new Dictionary<Position, int>();
			foreach (var pos in Positions)
			{
				positionStrength.Add(pos, 0);
				foreach (var cm in crewMembers)
				{
					if (pos.GetPositionRating(cm) >= (int)config.ConfigValues[ConfigKeys.GoodPositionRating])
					{
						positionStrength[pos]++;
					}
				}
			}
			positionStrength = positionStrength.OrderBy(kvp => kvp.Value).ToDictionary(p => p.Key, p => p.Value);
			//if there is no position that has more available than another, select one at random. Possible to select no position (Position.Null)
			if (positionStrength.Values.Max() - positionStrength.Values.Min() == 0)
			{
				var positionValue = StaticRandom.Int(0, Positions.Count + 1);
				return positionValue < Positions.Count ? Positions[positionValue] : Position.Null;
			}
			//select from weaker positions if at least one position has less available members than another 
			var lowValue = positionStrength.Values.Min();
			var lowPositions = positionStrength.Where(kvp => kvp.Value == lowValue).Select(kvp => kvp.Key).ToArray();
			return lowPositions.OrderBy(p => Guid.NewGuid()).First();
		}

		/// <summary>
		/// Update the score in each Position in order to get the score for this Boat
		/// </summary>
		public void UpdateBoatScore(string managerName)
		{
			foreach (var position in Positions)
			{
				if (PositionCrew.ContainsKey(position))
				{
					UpdateCrewMemberScore(position, PositionCrew[position], managerName);
				}
			}
			Score = PositionScores.Values.Sum();
		}

		/// <summary>
		/// Get the current score for this Position on this Boat for this CrewMember
		/// </summary>
		internal void UpdateCrewMemberScore(Position position, CrewMember crewMember, string managerName)
		{
			//Get the average skill rating for this CrewMember in this Position
			var crewScore = position.GetPositionRating(crewMember);

			var opinion = 0;
			var opinionCount = 0;
			var managerOpinion = 0;

			//get the average opinion of every other positioned crew member and the manager
			if (crewMember.CrewOpinions != null && crewMember.CrewOpinions.Count > 0)
			{
				foreach (var pair in PositionCrew)
				{
					if (pair.Key != position)
					{
						opinion += crewMember.CrewOpinions[pair.Value.Name];
						opinionCount++;
					}
				}
				managerOpinion += crewMember.CrewOpinions[managerName];
			}

			if (opinionCount > 0)
			{
				opinion = (int)Math.Round((float)opinion / opinionCount);
			}

			//add weighted average opinion, manager opinion and current mood to get score for crew member
			crewScore += (int)(opinion * config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);

			crewScore += (int)(managerOpinion * config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]);

			crewScore += (int)(crewMember.GetMood() * config.ConfigValues[ConfigKeys.MoodRatingWeighting]);

			PositionScores[position] = crewScore;
		}

		/// <summary>
		/// Get the average mood of the selected crew
		/// </summary>
		public float AverageBoatMood()
		{
			var mood = 0f;
			foreach (var crewMember in PositionCrew.Values)
			{
				mood += crewMember.GetMood();
			}
			mood = mood / PositionCrew.Count;
			return mood;
		}

		/// <summary>
		/// Get the average manager opinion of the selected crew
		/// </summary>
		public float AverageBoatManagerOpinion(string managerName)
		{
			var opinion = 0f;
			foreach (var crewMember in PositionCrew.Values)
			{
				if (crewMember.CrewOpinions.ContainsKey(managerName))
				{
					opinion += crewMember.CrewOpinions[managerName];
				}
			}
			opinion = opinion / PositionCrew.Count;
			return opinion;
		}

		/// <summary>
		/// Get the average opinion of the selected crew
		/// </summary>
		public float AverageBoatOpinion()
		{
			var opinion = 0f;
			foreach (var crewMember in PositionCrew.Values)
			{
				var crewOpinion = 0f;
				foreach (var otherMember in PositionCrew.Values)
				{
					if (otherMember.Name != crewMember.Name && crewMember.CrewOpinions.ContainsKey(otherMember.Name))
					{
						crewOpinion += crewMember.CrewOpinions[otherMember.Name];
					}
				}
				crewOpinion = crewOpinion / (PositionCrew.Count - 1);
				opinion += crewOpinion;
			}
			opinion = opinion / PositionCrew.Count;
			return opinion;
		}

		/// <summary>
		/// Get the crew set-up(s) that would be worth the highest Score
		/// </summary>
		internal void GetIdealCrew(Dictionary<string, CrewMember> crewMembers, string managerName)
		{
			//remove CrewMembers that are currently resting
			var availableCrew = crewMembers.Where(cm => cm.Value.RestCount <= 0).ToDictionary(ac => ac.Key, ac => ac.Value);
			//if there are not enough active CrewMembers to race,do not perform the rest of the method
			if (Positions.Count == 0 || availableCrew.Count < Positions.Count)
			{
				return;
			}
			//get the combined score of mood, manager opinion and position rating for every crewmember in every position
			var positionCrewCombos = new Dictionary<string, int>(availableCrew.Count * Positions.Count);
			foreach (var position in Positions)
			{
				foreach (var cm in availableCrew)
				{
					positionCrewCombos.Add(string.Concat(position.ToString(), cm.Key), position.GetPositionRating(cm.Value) + (int)(cm.Value.GetMood() * config.ConfigValues[ConfigKeys.MoodRatingWeighting]) + (int)(cm.Value.CrewOpinions[managerName] * config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]));
				}
			}
			var positionNames = Positions.Select(position => position.ToString()).ToList();
			//get every crewmember combination (unordered)
			var crewCombos = GetPermutations(availableCrew.Keys.ToList(), Positions.Count - 1).ToList();
			//get the combined average opinion for every combination
			var crewOpinions = new Dictionary<string, int>();
			var crewMaxScores = new Dictionary<List<string>, int>();
			var crewPositionScores = new Dictionary<string, int>();
			foreach (var possibleCrew in crewCombos)
			{
				var crewComboKey = string.Concat(possibleCrew.ToArray());
				var opinionTotal = 0;
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
					// ReSharper disable once PossibleLossOfFraction
					opinion = (int)(Math.Round((float)opinion / opinionCount) * config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);
					opinionTotal += opinion;
				}
				crewOpinions.Add(crewComboKey, opinionTotal);
				//get every position rating for every crewmember in this combination in every position
				crewPositionScores.Clear();
				positionNames.ForEach(pn => possibleCrew.ForEach(pc => crewPositionScores.Add(string.Concat(pn, pc), positionCrewCombos[string.Concat(pn, pc)])));
				crewMaxScores.Add(possibleCrew, crewPositionScores.Values.OrderByDescending(v => v).Take(positionNames.Count).Sum() + opinionTotal);
			}
			var bestScore = 0;
			var bestCrew = new List<List<string>>(crewCombos.Count);
			var crewMaxSorted = crewMaxScores.OrderByDescending(m => m.Value).ToList();
			//for each possible combination
			foreach (var possibleCrew in crewMaxSorted)
			{
				//if the combined total of the top scores and highest possible opinion could beat the current top score, continue. Otherwise, this combination can never have the best combination, so stop
				if (possibleCrew.Value >= bestScore)
				{
					//get every position rating for every crewmember in this combination in every position
					crewPositionScores.Clear();
					positionNames.ForEach(pn => possibleCrew.Key.ForEach(pc => crewPositionScores.Add(string.Concat(pn, pc), positionCrewCombos[string.Concat(pn, pc)])));
					//sort these position ratings from highest to lowest
					crewPositionScores = crewPositionScores.OrderByDescending(cps => cps.Value).ToDictionary(cps => cps.Key, cps => cps.Value);
					//find the combined average opinion for this combination
					var opinionScore = crewOpinions[string.Concat(possibleCrew.Key.ToArray())];
					//if the highest score in each position plus the highest possible opinion is lower than the best score so far, stop this loop
					if (positionNames.Select(pn => crewPositionScores.First(cps => cps.Key.Contains(pn)).Value).Sum() + opinionScore < bestScore)
					{
						continue;
					}
					//if the highest score for each crewmember plus the highest possible opinion is lower than the best score so far, stop this loop
					if (possibleCrew.Key.Select(pc => crewPositionScores.First(cps => cps.Key.Contains(pc)).Value).Sum() + opinionScore < bestScore)
					{
						continue;
					}
					//get every combination in every order for these crewmembers
					var combos = GetOrderedPermutations(possibleCrew.Key, positionNames.Count - 1);
					//for each combination
					foreach (var combo in combos)
					{
						var score = 0;
						//assign crew members to their positions and get the score for this set-up
						for (var i = 0; i < combo.Count; i++)
						{
							score += crewPositionScores[string.Concat(positionNames[i], combo[i])];
						}
						//add the combined average opinion for this combination
						score += opinionScore;
						//if the score for this set-up is higher or equal than the current highest
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
			//for each of the best crews, create a list of kvp of the crewmembers and their position/mood/manager opinion scores
			var idealCrew = new List<List<KeyValuePair<CrewMember, int>>>();
			foreach (var crew in bestCrew)
			{
				var positionedCrew = new List<KeyValuePair<CrewMember, int>>();
				for (var i = 0; i < crew.Count; i++)
				{
					positionedCrew.Add(new KeyValuePair<CrewMember, int>(availableCrew[crew[i]], positionCrewCombos[string.Concat(positionNames[i], crew[i])]));
				}
				idealCrew.Add(positionedCrew);
			}
			var nearestIdealMatch = UpdateIdealScore(idealCrew);
			if (nearestIdealMatch.Count != 0)
			{
				FindAssignmentMistakes(nearestIdealMatch, managerName);
			}
		}

		/// <summary>
		/// Find how close the current crew is to being an 'ideal' set-up
		/// </summary>
		private List<KeyValuePair<CrewMember, int>> UpdateIdealScore(List<List<KeyValuePair<CrewMember, int>>> idealCrew)
		{
			//reset current values
			IdealMatchScore = 0;
			var nearestIdealMatch = new List<KeyValuePair<CrewMember, int>>();
			//if not enough crewmembers are currently positioned, do not perform the rest of this method
			if (PositionCrew.Count < Positions.Count)
			{
				return nearestIdealMatch;
			}
			//check the current positioned crew against every ideal crew layout
			foreach (var crew in idealCrew)
			{
				float currentIdealMatch = 0;
				for (var i = 0; i < crew.Count; i++)
				{
					//if the CrewMembers match in both the current and the ideal, add 1 to the currentIdealMatch score
					if (crew[i].Key == PositionCrew[Positions[i]])
					{
						currentIdealMatch++;
					}
					//otherwise, check if this CrewMember is meant to be positioned elsewhere in an ideal set-up. If so, add 0.1f to the currentIdealMatch score
					else
					{
						foreach (var ideal in crew)
						{
							if (ideal.Key == PositionCrew[Positions[i]])
							{
								currentIdealMatch += 0.1f;
							}
						}
					}
				}
				//if the final currentIdealMatch score is higher than the current IdealMatchScore, or nearestIdealMatch is null (meaning no other ideals have been checked), set this ideal crew to the nearest match
				if (currentIdealMatch > IdealMatchScore || nearestIdealMatch.Count == 0)
				{
					IdealMatchScore = currentIdealMatch;
					nearestIdealMatch = crew;
				}
			}
			return nearestIdealMatch;
		}

		/// <summary>
		/// Find all the reasons this current crew is not an 'ideal' crew
		/// </summary>
		private void FindAssignmentMistakes(List<KeyValuePair<CrewMember, int>> nearestIdealMatch, string managerName)
		{
			if (PositionCrew.Count < Positions.Count)
			{
				return;
			}
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
			for (var i = 0; i < Positions.Count; i++)
			{
				//if the position is correctly assigned, there's no need for further checks
				if (PositionCrew[Positions[i]] == nearestIdealMatch[i].Key)
				{
					continue;
				}
				//for each required skill for this position, get the difference between the rating of the ideal and the current and add to mistakeScores
				foreach (var skill in Positions[i].RequiredSkills())
				{
					mistakeScores[skill.ToString()] += (nearestIdealMatch[i].Key.Skills[skill] - PositionCrew[Positions[i]].Skills[skill]) / (float)Positions[i].RequiredSkills().Count();
					//if the rating of the current positioned CrewMember is not known to the player, add to hiddenScores
					if (PositionCrew[Positions[i]].RevealedSkills[skill] == 0)
					{
						hiddenScores[skill.ToString()] += (nearestIdealMatch[i].Key.Skills[skill] - PositionCrew[Positions[i]].Skills[skill]) / (float)Positions[i].RequiredSkills().Count();
					}
				}
				//add the difference in opinion of the manager to mistakeScores
				mistakeScores["ManagerOpinion"] += (int)((nearestIdealMatch[i].Key.CrewOpinions[managerName] - PositionCrew[Positions[i]].CrewOpinions[managerName]) * config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]);
				//if the player does not know this opinion, add the difference to hiddenScores
				if (PositionCrew[Positions[i]].CrewOpinions[managerName] != PositionCrew[Positions[i]].RevealedCrewOpinions[managerName])
				{
					hiddenScores["ManagerOpinion"] += (int)((nearestIdealMatch[i].Key.CrewOpinions[managerName] - PositionCrew[Positions[i]].CrewOpinions[managerName]) * config.ConfigValues[ConfigKeys.ManagerOpinionRatingWeighting]);
				}
				//add the difference in mood to mistakeScores
				mistakeScores["Mood"] += (int)((nearestIdealMatch[i].Key.GetMood() - PositionCrew[Positions[i]].GetMood()) * config.ConfigValues[ConfigKeys.MoodRatingWeighting]);
				//calculate the average opinion for this position in the ideal crew
				var idealOpinion = 0f;
				foreach (var pair in nearestIdealMatch)
				{
					if (pair.Key != nearestIdealMatch[i].Key)
					{
						idealOpinion += nearestIdealMatch[i].Key.CrewOpinions[pair.Key.Name];
						idealOpinion += pair.Key.CrewOpinions[nearestIdealMatch[i].Key.Name];
					}
				}
				idealOpinion /= (Positions.Count - 1) * 2;
				idealOpinion = (float)Math.Round(idealOpinion);
				//calculate the average opinion for this position in the current crew and how many opinions are currently unknown
				var currentOpinion = 0f;
				var unknownCrewOpinions = 0;
				foreach (var position in Positions)
				{
					if (PositionCrew[position] != PositionCrew[Positions[i]])
					{
						currentOpinion += PositionCrew[Positions[i]].CrewOpinions[PositionCrew[position].Name];
						if (PositionCrew[Positions[i]].CrewOpinions[PositionCrew[position].Name] != PositionCrew[Positions[i]].RevealedCrewOpinions[PositionCrew[position].Name])
						{
							unknownCrewOpinions++;
						}
						currentOpinion += PositionCrew[position].CrewOpinions[PositionCrew[Positions[i]].Name];
						if (PositionCrew[position].CrewOpinions[PositionCrew[Positions[i]].Name] != PositionCrew[position].RevealedCrewOpinions[PositionCrew[Positions[i]].Name])
						{
							unknownCrewOpinions++;
						}
					}
				}
				currentOpinion /= (Positions.Count - 1) * 2;
				currentOpinion = (float)Math.Round(currentOpinion);
				//add the difference to mistakeScores
				mistakeScores["CrewOpinion"] += (int)((idealOpinion - currentOpinion) * config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);
				//if the percentage of unknown opinions is above the given amount, add the difference to hiddenScores
				if (unknownCrewOpinions >= ((Positions.Count - 1) * 2) * config.ConfigValues[ConfigKeys.HiddenOpinionLimit])
				{
					hiddenScores["CrewOpinion"] += (int)((idealOpinion - currentOpinion) * config.ConfigValues[ConfigKeys.OpinionRatingWeighting]);
				}
			}
			//sort the 'mistakes' by their values, removing all with a score of 0 and below (aka, equal or better to the ideal crew)
			var mistakes = mistakeScores.OrderByDescending(ms => ms.Value).Where(ms => ms.Value > 0).Select(ms => ms.Key).ToList();
			//if the value of the mistake in hiddenScores is more than the given percentage of that in mistakeScores, set this mistake to be 'hidden'
			for (var i = 0; i < mistakes.Count; i++)
			{
				if (hiddenScores[mistakes[i]] >= mistakeScores[mistakes[i]] * config.ConfigValues[ConfigKeys.HiddenMistakeLimit])
				{
					mistakes[i] = "Hidden";
				}
			}
			//store the current range of mistakes
			SelectionMistakes = mistakes;
		}

		/// <summary>
		/// Get the amount of mistakes requested
		/// </summary>
		public List<string> GetAssignmentMistakes(int returnAmount)
		{
			var mistakes = SelectionMistakes.Take(returnAmount).ToList();
			return mistakes;
		}

		/// <summary>
		/// Get every possible combination of CrewMembers in every order
		/// </summary>
		private List<List<string>> GetOrderedPermutations(List<string> list, int length)
		{
			if (length == 0)
			{
				return list.Select(t => new List<string> { t }).ToList();
			}

			var newList = new List<List<string>>();
			var perms = GetOrderedPermutations(list, length - 1);
			foreach (var t in perms)
			{
				foreach (var o in list)
				{
					if (!t.Contains(o))
					{
						var permCopy = CopyPermList(t);
						permCopy.Add(o);
						newList.Add(permCopy);
					}
				}
			}

			return newList;
		}

		/// <summary>
		/// Get every possible combination of CrewMembers in no order
		/// </summary>
		private List<List<string>> GetPermutations(List<string> list, int length)
		{
			if (length == 0)
			{
				return list.Select(t => new List<string> { t }).ToList();
			}

			var newList = new List<List<string>>();
			var perms = GetPermutations(list, length - 1);
			foreach (var t in perms)
			{
				foreach (var o in list)
				{
					if (o.CompareTo(t.Last()) > 0)
					{
						var permCopy = CopyPermList(t);
						permCopy.Add(o);
						newList.Add(permCopy);
					}
				}
			}

			return newList;
		}

		private List<string> CopyPermList(List<string> list)
		{
			return new List<string>(list);
		}
	}
}
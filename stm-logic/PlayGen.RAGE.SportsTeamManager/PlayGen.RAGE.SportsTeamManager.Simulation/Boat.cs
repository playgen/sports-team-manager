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
		private struct PositionKey
		{
			public string Position;
			public string Crew;
		}

		public string Type { get; private set; }
		public List<Position> Positions { get; private set; }
		public int PositionCount => Positions.Count;
		public Dictionary<Position, CrewMember> PositionCrew { get; internal set; }
		public Dictionary<Position, int> PositionScores { get; internal set; }
		public int Score { get; internal set; }
		public int PerfectSelections { get; internal set; }
		public int ImperfectSelections { get; internal set; }
		public int IncorrectSelections => PositionCount - PerfectSelections - ImperfectSelections;
		public List<string> SelectionMistakes { get; internal set; }

		/// <summary>
		/// Boat constructor
		/// </summary>
		internal Boat(string type)
		{
			Positions = new List<Position>();
			PositionCrew = new Dictionary<Position, CrewMember>();
			PositionScores = new Dictionary<Position, int>();
			SelectionMistakes = new List<string>();
			Type = type;
			GetPositions();
		}

		/// <summary>
		/// Get the positions for this boat type, keeping crew members in existing positions in the process
		/// </summary>
		private void GetPositions()
		{
			Positions.Clear();
			if (Type != "Finish")
			{
				Positions = new List<Position>(ConfigStore.BoatTypes[Type]);
			}
			var oldPositions = PositionCrew.Keys.Where(position => !Positions.Contains(position)).ToList();
			foreach (var oldPosition in oldPositions)
			{
				AssignCrewMember(oldPosition, null);
			}
		}

		/// <summary>
		/// Change the current type of boat to a different type and get the positions for this new type
		/// </summary>
		internal void Promote(string newType)
		{
			Type = newType;
			GetPositions();
		}

		/// <summary>
		/// Assign a CrewMember to a Position
		/// </summary>
		public void AssignCrewMember(Position position, CrewMember crewMember)
		{
			if (position != Position.Null)
			{
				//if a Position is provided and has a CrewMember already assigned to it, remove them from that position
				if (PositionCrew.ContainsKey(position))
				{
					AssignCrewMember(Position.Null, PositionCrew[position]);
				}
			}
			if (crewMember != null)
			{
				//if a CrewMember is provided and is assigned to a position, remove them from that position
				var current = GetCrewMemberPosition(crewMember);
				if (current != Position.Null)
				{
					PositionCrew.Remove(current);
					PositionScores.Remove(current);
				}
				if (position != Position.Null)
				{
					//add combination of CrewMember and Position to PositionCrew dictionary
					PositionCrew.Add(position, crewMember);
					PositionScores.Add(position, 0);
					crewMember.UpdateSingleBelief(NPCBelief.Position, position);
				}
				else
				{
					crewMember.UpdateSingleBelief(NPCBelief.Position);
				}
			}
		}

		public Position GetCrewMemberPosition(CrewMember crewMember)
		{
			return PositionCrew.SingleOrDefault(pair => pair.Value == crewMember).Key;
		}

		/// <summary>
		/// Get the Position on this Boat with the least CrewMembers able to perform well in it
		/// </summary>
		internal Position GetWeakestPosition(List<CrewMember> crewMembers)
		{
			if (PositionCount == 0)
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
					if (pos.GetPositionRating(cm) >= ConfigKey.GoodPositionRating.GetIntValue())
					{
						positionStrength[pos]++;
					}
				}
			}
			//if there is no position that has more available than another, make it possible to select no position (Position.Null)
			if (positionStrength.Values.Max() - positionStrength.Values.Min() == 0)
			{
				positionStrength.Add(Position.Null, positionStrength.Values.Min());
			}
			//select from weaker positions if at least one position has less available members than another 
			var lowValue = positionStrength.Values.Min();
			var lowPositions = positionStrength.Where(kvp => kvp.Value == lowValue).Select(kvp => kvp.Key).ToArray();
			return lowPositions.OrderBy(p => Guid.NewGuid()).First();
		}

		/// <summary>
		/// Update the score in each Position in order to get the score for this Boat
		/// </summary>
		internal void UpdateScore(string managerName)
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
			//Get the combined average skill rating, mood and manager opinion for this CrewMember in this Position
			var crewScore = GetPositionRating(crewMember, position, managerName);

			//add weighted average opinion to get score for crew member
			crewScore += crewMember.GetOpinionRating(PositionCrew.Values.Select(c => c.Name).ToList());

			PositionScores[position] = crewScore;
		}

		internal int GetPositionRating(CrewMember member, Position pos, string managerName)
		{
			return pos.GetPositionRating(member) + (int)(member.GetMood() * ConfigKey.MoodRatingWeighting.GetValue()) + (int)(member.CrewOpinions[managerName] * ConfigKey.ManagerOpinionRatingWeighting.GetValue());
		}

		/// <summary>
		/// Get the crew set-up(s) that would be worth the highest Score
		/// </summary>
		internal void GetIdealCrew(Dictionary<string, CrewMember> crewMembers, string managerName)
		{
			if (PositionCount == 0)
			{
				return;
			}
				
			//remove CrewMembers that are currently resting
			var availableCrew = crewMembers.Where(cm => cm.Value.RestCount <= 0).ToDictionary(ac => ac.Key, ac => ac.Value);
			//if there are not enough active CrewMembers to race,do not perform the rest of the method
			if (availableCrew.Count < PositionCount)
			{
				return;
			}

			var positionKey = new PositionKey();

			//get the combined score of mood, manager opinion and position rating for every crewmember in every position
			var positionCrewCombos = new Dictionary<PositionKey, int>();
			
			var positionNames = new List<string>();
			foreach (var position in Positions)
			{
				positionNames.Add(position.ToString());
				foreach (var cm in availableCrew)
				{
					positionKey.Position = position.ToString();
					positionKey.Crew = cm.Key;
					positionCrewCombos.Add(positionKey, GetPositionRating(cm.Value, position, managerName));
				}
			}
			//get every crewmember combination (unordered)
			var sortedCrew = availableCrew.Keys.OrderBy(c => c).ToList();
			var crewCombos = GetPermutations(sortedCrew, PositionCount - 1).ToList();
			//get the combined average opinion for every combination
			var crewOpinions = new Dictionary<string, int>();
			var crewMaxScores = new Dictionary<List<string>, int>();
			var crewPositionScores = new Dictionary<PositionKey, int>();
			foreach (var possibleCrew in crewCombos)
			{
				var crewComboKey = string.Concat(possibleCrew.ToArray());
				var opinionTotal = possibleCrew.Sum(c => availableCrew[c].GetOpinionRating(possibleCrew));
				crewOpinions.Add(crewComboKey, opinionTotal);
				//get every position rating for every crewmember in this combination in every position
				crewPositionScores.Clear();
				positionNames.ForEach(pn => possibleCrew.ForEach(pc =>
				{
					positionKey.Position = pn;
					positionKey.Crew = pc;
					crewPositionScores.Add(positionKey, positionCrewCombos[positionKey]);
				}));
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
					positionNames.ForEach(pn => possibleCrew.Key.ForEach(pc =>
					{
						positionKey.Position = pn;
						positionKey.Crew = pc;
						crewPositionScores.Add(positionKey, positionCrewCombos[positionKey]);
					}));
					//sort these position ratings from highest to lowest
					crewPositionScores = crewPositionScores.OrderByDescending(cps => cps.Value).ToDictionary(cps => cps.Key, cps => cps.Value);
					//find the combined average opinion for this combination
					var opinionScore = crewOpinions[string.Concat(possibleCrew.Key.ToArray())];
					//if the highest score in each position plus the highest possible opinion is lower than the best score so far, stop this loop
					if (positionNames.Select(pn => crewPositionScores.First(cps => cps.Key.Position == pn).Value).Sum() + opinionScore < bestScore)
					{
						continue;
					}
					//if the highest score for each crewmember plus the highest possible opinion is lower than the best score so far, stop this loop
					if (possibleCrew.Key.Select(pc => crewPositionScores.First(cps => cps.Key.Crew == pc).Value).Sum() + opinionScore < bestScore)
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
							positionKey.Position = positionNames[i];
							positionKey.Crew = combo[i];
							score += crewPositionScores[positionKey];
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
			var idealCrew = new List<List<CrewMember>>();
			foreach (var crew in bestCrew)
			{
				var positionedCrew = new List<CrewMember>();
				for (var i = 0; i < crew.Count; i++)
				{
					positionKey.Position = positionNames[i];
					positionKey.Crew = crew[i];
					positionedCrew.Add(availableCrew[crew[i]]);
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
		private List<CrewMember> UpdateIdealScore(List<List<CrewMember>> idealCrew)
		{
			//reset current values
			PerfectSelections = 0;
			ImperfectSelections = 0;
			var perfectCount = 0;
			var imperfectCount = 0;
			var nearestIdealMatch = new List<CrewMember>();
			//if not enough crewmembers are currently positioned, do not perform the rest of this method
			if (PositionCrew.Count < PositionCount)
			{
				return nearestIdealMatch;
			}
			//check the current positioned crew against every ideal crew layout
			foreach (var crew in idealCrew)
			{
				var currentPerfectCount = 0;
				var currentImperfectCount = 0;
				for (var i = 0; i < crew.Count; i++)
				{
					//if the CrewMembers match in both the current and the ideal, add 1 to the currentIdealMatch score
					if (crew[i] == PositionCrew[Positions[i]])
					{
						currentPerfectCount ++;
					}
					//otherwise, check if this CrewMember is meant to be positioned elsewhere in an ideal set-up. If so, add 0.1f to the currentIdealMatch score
					else
					{
						foreach (var ideal in crew)
						{
							if (ideal == PositionCrew[Positions[i]])
							{
								currentImperfectCount++;
							}
						}
					}
				}
				//if the final currentIdealMatch score is higher than the current IdealMatchScore, or nearestIdealMatch is null (meaning no other ideals have been checked), set this ideal crew to the nearest match
				if (currentPerfectCount + currentImperfectCount >= perfectCount + imperfectCount || nearestIdealMatch.Count == 0)
				{
					if (currentPerfectCount + currentImperfectCount > perfectCount + imperfectCount || currentPerfectCount > perfectCount || nearestIdealMatch.Count == 0)
					{
						perfectCount = currentPerfectCount;
						imperfectCount = currentImperfectCount;
						nearestIdealMatch = crew;
					}
				}
			}
			PerfectSelections = perfectCount;
			ImperfectSelections = imperfectCount;
			return nearestIdealMatch;
		}

		/// <summary>
		/// Find all the reasons this current crew is not an 'ideal' crew
		/// </summary>
		private void FindAssignmentMistakes(List<CrewMember> nearestIdealMatch, string managerName)
		{
			if (PositionCrew.Count < PositionCount)
			{
				return;
			}
			//create a list of all possible 'mistakes' for known values and hidden values
			var mistakeScores = new Dictionary<string, float>();
			var hiddenScores = new Dictionary<string, float>();
			foreach (var skillName in Enum.GetNames(typeof(Skill)))
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
			for (var i = 0; i < PositionCount; i++)
			{
				//if the position is correctly assigned, there's no need for further checks
				if (PositionCrew[Positions[i]] == nearestIdealMatch[i])
				{
					continue;
				}
				//for each required skill for this position, get the difference between the rating of the ideal and the current and add to mistakeScores
				foreach (var skill in Positions[i].RequiredSkills())
				{
					mistakeScores[skill.ToString()] += (nearestIdealMatch[i].Skills[skill] - PositionCrew[Positions[i]].Skills[skill]) / (float)Positions[i].RequiredSkills().Count();
					//if the rating of the current positioned CrewMember is not known to the player, add to hiddenScores
					if (PositionCrew[Positions[i]].RevealedSkills[skill] != PositionCrew[Positions[i]].Skills[skill])
					{
						hiddenScores[skill.ToString()] += (nearestIdealMatch[i].Skills[skill] - PositionCrew[Positions[i]].Skills[skill]) / (float)Positions[i].RequiredSkills().Count();
					}
				}
				//add the difference in opinion of the manager to mistakeScores
				mistakeScores["ManagerOpinion"] += (int)((nearestIdealMatch[i].CrewOpinions[managerName] - PositionCrew[Positions[i]].CrewOpinions[managerName]) * ConfigKey.ManagerOpinionRatingWeighting.GetValue());
				//if the player does not know this opinion, add the difference to hiddenScores
				if (PositionCrew[Positions[i]].CrewOpinions[managerName] != PositionCrew[Positions[i]].RevealedCrewOpinions[managerName])
				{
					hiddenScores["ManagerOpinion"] += (int)((nearestIdealMatch[i].CrewOpinions[managerName] - PositionCrew[Positions[i]].CrewOpinions[managerName]) * ConfigKey.ManagerOpinionRatingWeighting.GetValue());
				}
				//add the difference in mood to mistakeScores
				mistakeScores["Mood"] += (int)((nearestIdealMatch[i].GetMood() - PositionCrew[Positions[i]].GetMood()) * ConfigKey.MoodRatingWeighting.GetValue());
				//calculate the average opinion for this position in the ideal crew
				var idealOpinion = 0f;
				foreach (var crewMember in nearestIdealMatch)
				{
					if (crewMember != nearestIdealMatch[i])
					{
						idealOpinion += nearestIdealMatch[i].CrewOpinions[crewMember.Name];
						idealOpinion += crewMember.CrewOpinions[nearestIdealMatch[i].Name];
					}
				}
				idealOpinion /= (PositionCount - 1) * 2;
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
				currentOpinion /= (PositionCount - 1) * 2;
				currentOpinion = (float)Math.Round(currentOpinion);
				//add the difference to mistakeScores
				mistakeScores["CrewOpinion"] += (int)((idealOpinion - currentOpinion) * ConfigKey.OpinionRatingWeighting.GetValue());
				//if the percentage of unknown opinions is above the given amount, add the difference to hiddenScores
				if (unknownCrewOpinions >= ((PositionCount - 1) * 2) * ConfigKey.HiddenOpinionLimit.GetValue())
				{
					hiddenScores["CrewOpinion"] += (int)((idealOpinion - currentOpinion) * ConfigKey.OpinionRatingWeighting.GetValue());
				}
			}
			//sort the 'mistakes' by their values, removing all with a score of 0 and below (aka, equal or better to the ideal crew)
			var mistakes = mistakeScores.OrderByDescending(ms => ms.Value).Where(ms => ms.Value > 0).Select(ms => ms.Key).ToList();
			//if the value of the mistake in hiddenScores is more than the given percentage of that in mistakeScores, set this mistake to be 'hidden'
			for (var i = 0; i < mistakes.Count; i++)
			{
				if (hiddenScores[mistakes[i]] >= mistakeScores[mistakes[i]] * ConfigKey.HiddenMistakeLimit.GetValue())
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
					if (string.Compare(o, t.Last(), StringComparison.Ordinal) > 0)
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

		/// <summary>
		/// Get the average mood of the selected crew
		/// </summary>
		public float AverageMood()
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
		/// Get the average opinion of the selected crew of a person
		/// </summary>
		public float AverageOpinion(string name)
		{
			var opinion = 0f;
			foreach (var crewMember in PositionCrew.Values)
			{
				if (crewMember.CrewOpinions.ContainsKey(name))
				{
					opinion += crewMember.CrewOpinions[name];
				}
			}
			opinion = opinion / PositionCrew.Count;
			return opinion;
		}

		/// <summary>
		/// Get the average opinion of the selected crew
		/// </summary>
		public float AverageOpinion()
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
	}
}
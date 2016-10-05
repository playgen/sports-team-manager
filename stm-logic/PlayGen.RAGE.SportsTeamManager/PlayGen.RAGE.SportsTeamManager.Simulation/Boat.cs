using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

using AssetManagerPackage;
using IntegratedAuthoringTool;

using System.Threading;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Stores boat and crew details and contains functionality related to adjusting either
	/// </summary>
	public class Boat
	{
		private List<List<BoatPosition>> _idealCrew;
		private List<BoatPosition> _nearestIdealMatch;
		private readonly ConfigStore _config;

		public string Name { get; set; }
		public int[] TeamColorsPrimary { get; set; }
		public int[] TeamColorsSecondary { get; set; }
		public List<BoatPosition> BoatPositions { get; set; }
		public List<CrewMember> UnassignedCrew { get; }
		public List<CrewMember> RetiredCrew { get; }
		public List<CrewMember> Recruits { get; }
		public int BoatScore { get; set; }
		public float IdealMatchScore { get; set; }
		public List<string> SelectionMistakes { get; set; }
		public Person Manager { get; set; }

		public bool Running { get { return _threadRunning; } }
		private bool _threadRunning;
		Thread _thread;

		/// <summary>
		/// Boat constructor
		/// </summary>
		public Boat(ConfigStore config)
		{
			BoatPositions = new List<BoatPosition>();
			UnassignedCrew = new List<CrewMember>();
			RetiredCrew = new List<CrewMember>();
			_idealCrew = new List<List<BoatPosition>>();
			Recruits = new List<CrewMember>();
			SelectionMistakes = new List<string>();
			_config = config;
		}

		/// <summary>
		/// Get a list of all the CrewMember assigned to this Boat, including those currently not in a position
		/// </summary>
		public List<CrewMember> GetAllCrewMembers()
		{
			var crew = new List<CrewMember>();
			UnassignedCrew.ForEach(crewMember => crew.Add(crewMember));
			BoatPositions.Where(bp => bp.CrewMember != null).ToList().ForEach(boatPosition => crew.Add(boatPosition.CrewMember));
			crew = crew.OrderBy(c => c.Name).ToList();
			return crew;
		}

		/// <summary>
		/// Get a list of all CrewMembers assigned to this boat, including those marked as 'retired' and thus cannot go into a position
		/// </summary>
		public List<CrewMember> GetAllCrewMembersIncludingRetired()
		{
			var crew = GetAllCrewMembers();
			RetiredCrew.ForEach(crewMember => crew.Add(crewMember));
			crew = crew.OrderBy(c => c.Name).ToList();
			return crew;
		}

		/// <summary>
		/// Add a CrewMember to the list of UnassignedCrew
		/// </summary>
		public void AddCrew(CrewMember crewMember)
		{
			var currentPosition = crewMember.GetBoatPosition(this);
			if (currentPosition != null)
			{
				return;
			}
			if (UnassignedCrew.Any(c => c == crewMember))
			{
				return;
			}
			UnassignedCrew.Add(crewMember);
		}

		/// <summary>
		/// Set the avatar outfit colors to match the team colors
		/// </summary>
		public void SetCrewColors(Avatar avatar)
		{
			avatar.PrimaryOutfitColor = Color.FromArgb(255, TeamColorsPrimary[0], TeamColorsPrimary[1], TeamColorsPrimary[2]);
			avatar.SecondaryOutfitColor = Color.FromArgb(255, TeamColorsPrimary[0], TeamColorsPrimary[1], TeamColorsPrimary[2]);
		}

		/// <summary>
		/// Assign a CrewMember to a BoatPosition
		/// </summary>
		public void AssignCrew(BoatPosition boatPosition, CrewMember crewMember)
		{
			if (crewMember != null)
			{
				var current = crewMember.GetBoatPosition(this);
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
			}
			if (crewMember != null && boatPosition != null)
			{
				crewMember.UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), boatPosition.Position.Name);
			}
		}

		/// <summary>
		/// Remove a CrewMember from their BoatPosition and add them to the list of UnassignedCrew
		/// </summary>
		private void RemoveCrew(BoatPosition boatPosition)
		{
			UnassignedCrew.Add(boatPosition.CrewMember);
			boatPosition.CrewMember.UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), "null");
			boatPosition.CrewMember = null;
		}

		/// <summary>
		/// Remove all assigned CrewMembers
		/// </summary>
		public void RemoveAllCrew()
		{
			foreach (var boatPosition in BoatPositions)
			{
				if (boatPosition.CrewMember != null)
				{
					RemoveCrew(boatPosition);
				}
			}
		}

		public void UniqueNameCheck(Random random, CrewMember cm)
		{
			var unqiue = false;
			//if the name is already in use by another character, reset their name
			while (!unqiue)
			{
				if (GetAllCrewMembers().Count(c => c.Name == cm.Name) > 0 || RetiredCrew.Count(c => c.Name == cm.Name) > 0 || Recruits.Count(c => c.Name == cm.Name) > 0 || cm.Name == Manager.Name)
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
			var recuritsToRemove = new List<CrewMember>();
			//remove recruits from iat and randomly select to remove them from pool of available recruits
			foreach (var member in Recruits)
			{
				iat.RemoveCharacters(new List<string>() { member.Name });
				if (rand.Next(0, 100) % (int)_config.ConfigValues[ConfigKeys.RecruitChangeChance.ToString()] != 0)
				{
					recuritsToRemove.Add(member);
				}
			}
			foreach (var member in recuritsToRemove)
			{
				Recruits.Remove(member);
			}

			//for the amount of empty recruit spaces, create a new recruit
			var amount = (int)_config.ConfigValues[ConfigKeys.RecruitCount.ToString()] - Recruits.Count;
			for (var i = 0; i < amount; i++)
			{
				var newMember = new CrewMember(rand, this, _config);
				Recruits.Add(newMember);
			}
			//for each recruit, save their asset files, avatar and save files/add to iat
			for (var i = 0; i < Recruits.Count; i++)
			{
				Recruits[i].CreateFile(iat, storageLocation, "Recruit" + i);
				Recruits[i].Avatar = new Avatar(Recruits[i], false);
				Recruits[i].UpdateBeliefs("Recruit");
				Recruits[i].SaveStatus();
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
				return positionValue < BoatPositions.Count ? BoatPositions[positionValue].Position : null;
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
			foreach (var pos in BoatPositions.Select(bp => bp.Position))
			{
				positionStrength.Add(pos, 0);
				foreach (var cm in GetAllCrewMembers())
				{
					if (pos.GetPositionRating(cm) >= (int)_config.ConfigValues[ConfigKeys.GoodPositionRating.ToString()])
					{
						positionStrength[pos]++;
					}
				}
				foreach (var cm in Recruits)
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
			var currentPosition = crewMember.GetBoatPosition(this);
			if (currentPosition != null)
			{
				RemoveCrew(currentPosition);
			}
			if (UnassignedCrew.Any(c => c == crewMember))
			{
				UnassignedCrew.Remove(crewMember);
			}
			RetiredCrew.Add(crewMember);
			crewMember.Retire();
			foreach (var cm in GetAllCrewMembers())
			{
				cm.CrewOpinions.Remove(crewMember);
				cm.RevealedCrewOpinions.Remove(crewMember);
			}
		}

		/// <summary>
		/// Update the score in each BoatPosition in order to get the score for this Boat
		/// </summary>
		public void UpdateBoatScore()
		{
			foreach (var bp in BoatPositions)
			{
				bp.UpdateCrewMemberScore(this, _config);
			}
			BoatScore = BoatPositions.Sum(bp => bp.PositionScore);
		}

		public void GetIdealCrew()
		{
			_idealCrew = null;
			_thread = new Thread(GetIdealCrewThread);
			_threadRunning = true;
			_thread.Start();
		}

		/// <summary>
		/// Get the crew set-up(s) that would be worth the highest BoatScore
		/// </summary>
		public void GetIdealCrewThread()
		{
			var availableCrew = GetAllCrewMembers().Where(cm => cm.RestCount <= 0).ToList();
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
					positionCrewCombos.Add(string.Format("{0} {1}", bp.Position.Name, cm.Name), bp.Position.GetPositionRating(cm) + cm.GetMood() + cm.CrewOpinions[Manager]);
				}
			}
			var crewOpinionCombos = GetPermutations(availableCrew, BoatPositions.Count - 1);
			var crewOpinions = new Dictionary<string, int>(crewOpinionCombos.Count);
			foreach (var possibleCrew in crewOpinionCombos)
			{
				var crewList = possibleCrew.Select(pc => pc.Name);
				var crewComboKey = crewList.Aggregate(new StringBuilder(), (sb, s) => sb.Append(s)).ToString();
				crewOpinions.Add(crewComboKey, 0);
				foreach (var crewMember in possibleCrew)
				{
					var opinion = 0;
					var opinionCount = 0;
					foreach (var otherMember in possibleCrew)
					{
						if (crewMember != otherMember)
						{
							opinion += crewMember.CrewOpinions[otherMember];
							opinionCount++;
						}
					}
					opinion = opinion / opinionCount;
					crewOpinions[crewComboKey] += opinion;
				}
			}
			var opinionDifference = crewOpinions.Values.Max() - crewOpinions.Values.Min();
			var crewCombos = GetOrderedPermutations(availableCrew, BoatPositions.Count - 1);
			var bestScore = 0;
			var bestCrew = new List<List<BoatPosition>>();
			//for each possible combination
			foreach (var possibleCrew in crewCombos)
			{
				var score = 0;
				var crewList = possibleCrew.ToList();
				var positionedCrew = new List<BoatPosition>(BoatPositions.Count);
				//assign crew members to their positions and get the score for this set-up
				for (var i = 0; i < BoatPositions.Count; i++)
				{
					positionedCrew.Add(new BoatPosition
					{
						Position = BoatPositions[i].Position,
						CrewMember = crewList[i],
						PositionScore = positionCrewCombos[string.Format("{0} {1}", BoatPositions[i].Position.Name, crewList[i].Name)]
					});
					score += positionedCrew[i].PositionScore;
				}
				if (score + opinionDifference < bestScore)
				{
					continue;
				}
				var nameList = crewList.Select(pc => pc.Name).ToList();
				nameList.Sort();
				var crewComboKey = nameList.Aggregate(new StringBuilder(), (sb, s) => sb.Append(s)).ToString();
				var opinion = crewOpinions[crewComboKey];
				score += opinion;
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
					bestCrew.Add(positionedCrew);
				}
			}
			_idealCrew = bestCrew;
			UpdateIdealScore();
			_threadRunning = false;
		}

		/// <summary>
		/// Find how close the current crew is to being an 'ideal' set-up
		/// </summary>
		private void UpdateIdealScore()
		{
			//reset current values
			IdealMatchScore = 0;
			_nearestIdealMatch = null;
			//check the current positioned crew against every ideal crew layout
			foreach (var crew in _idealCrew)
			{
				float currentIdealMatch = 0;
				for (var i = 0; i < crew.Count; i++)
				{
					//if the CrewMembers match in both the current and the ideal, add 1 to the currentIdealMatch score
					if (crew[i].CrewMember == BoatPositions[i].CrewMember)
					{
						currentIdealMatch++;
					}
					//otherwise, check if this CrewMember is meant to be positioned elsewhere in an ideal set-up. If so, add 0.1f to the currentIdealMatch score
					else
					{
						foreach (var ideal in crew)
						{
							if (ideal.CrewMember == BoatPositions[i].CrewMember)
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
			//if any BoatPosition does not have a CrewMember, do not do this check
			foreach (var bp in BoatPositions)
			{
				if (bp.CrewMember == null)
				{
					return;
				}
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
			for (var i = 0; i < BoatPositions.Count; i++)
			{
				//if the position is correctly assigned, there's no need for further checks
				if (BoatPositions[i].CrewMember == _nearestIdealMatch[i].CrewMember)
				{
					continue;
				}
				//for each required skill for this position, get the difference between the rating of the ideal and the current and add to mistakeScores
				foreach (var skill in BoatPositions[i].Position.RequiredSkills)
				{
					mistakeScores[skill.ToString()] += (_nearestIdealMatch[i].CrewMember.Skills[skill] - BoatPositions[i].CrewMember.Skills[skill])/(float)BoatPositions[i].Position.RequiredSkills.Count;
					//if the rating of the current positioned CrewMember is not known to the player, add to hiddenScores
					if (BoatPositions[i].CrewMember.RevealedSkills[skill] == 0)
					{
						hiddenScores[skill.ToString()] += (_nearestIdealMatch[i].CrewMember.Skills[skill] - BoatPositions[i].CrewMember.Skills[skill]) / (float)BoatPositions[i].Position.RequiredSkills.Count;
					}
				}
				//add the difference in opinion of the manager to mistakeScores
				mistakeScores["ManagerOpinion"] += _nearestIdealMatch[i].CrewMember.CrewOpinions[Manager] - BoatPositions[i].CrewMember.CrewOpinions[Manager];
				//if the player does not know this opinion, add the difference to hiddenScores
				if (BoatPositions[i].CrewMember.CrewOpinions[Manager] != BoatPositions[i].CrewMember.RevealedCrewOpinions[Manager])
				{
					hiddenScores["ManagerOpinion"] += _nearestIdealMatch[i].CrewMember.CrewOpinions[Manager] - BoatPositions[i].CrewMember.CrewOpinions[Manager];
				}
				//add the difference in mood to mistakeScores
				mistakeScores["Mood"] += _nearestIdealMatch[i].CrewMember.GetMood() - BoatPositions[i].CrewMember.GetMood();
				//find the total score caused by crew opinion in the ideal set-up
				var idealOpinion = _nearestIdealMatch[i].PositionScore - _nearestIdealMatch[i].Position.GetPositionRating(_nearestIdealMatch[i].CrewMember) - _nearestIdealMatch[i].CrewMember.GetMood() - _nearestIdealMatch[i].CrewMember.CrewOpinions[Manager];
				//find the total score caused by crew opinion in the current set-up
				var currentOpinion = BoatPositions[i].PositionScore - BoatPositions[i].Position.GetPositionRating(BoatPositions[i].CrewMember) - BoatPositions[i].CrewMember.GetMood() - BoatPositions[i].CrewMember.CrewOpinions[Manager];
				//add the difference to mistakeScores
				mistakeScores["CrewOpinion"] += idealOpinion - currentOpinion;
				//if the percentage of unknown opinions is above the given amount, add the difference to hiddenScores
				var unknownCrewOpinions = 0;
				foreach (var bp in BoatPositions)
				{
					if (bp.CrewMember != BoatPositions[i].CrewMember)
					{
						if (BoatPositions[i].CrewMember.CrewOpinions[bp.CrewMember] != BoatPositions[i].CrewMember.RevealedCrewOpinions[bp.CrewMember])
						{
							unknownCrewOpinions++;
						}
					}
				}
				if (unknownCrewOpinions >= (BoatPositions.Count - 1) * _config.ConfigValues[ConfigKeys.HiddenOpinionLimit.ToString()])
				{
					hiddenScores["CrewOpinion"] += idealOpinion - currentOpinion;
				}
			}
			//sort the 'mistakes' by their values, removing all with a score of 0 and below (aka, equal or better to the ideal crew)
			var mistakes = mistakeScores.OrderByDescending(ms => ms.Value).Where(ms => ms.Value > 0).Select(ms => ms.Key).ToList();
			//if the value of the mistake in hiddenScores is more than the given percentage of that in mistakeScores, set this mistake to be 'hidden'
			for (var i = 0; i < mistakes.Count; i++)
			{
				if (hiddenScores[mistakes[i]] >= mistakeScores[mistakes[i]] * _config.ConfigValues[ConfigKeys.HiddenMistakeLimit.ToString()])
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
			foreach (var cm in GetAllCrewMembersIncludingRetired())
			{
				cm.TickUpdate(amount);
			}
		}

		/// <summary>
		/// Save the current status of each CrewMember for this Boat
		/// </summary>
		public void ConfirmChanges(int actionAllowance)
		{
			var crew = GetAllCrewMembers();
			crew.ForEach(p => p.DecisionFeedback(this));
			Manager.SaveStatus();
			PostRaceRest();
			TickCrewMembers(0);
		}

		/// <summary>
		/// Set all CrewMembers who raced to not be available for the set amount of races
		/// </summary>
		private void PostRaceRest()
		{
			var crew = GetAllCrewMembers();
			crew.ForEach(p => p.RaceRest(BoatPositions.Any(bp => bp.CrewMember == p)));
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
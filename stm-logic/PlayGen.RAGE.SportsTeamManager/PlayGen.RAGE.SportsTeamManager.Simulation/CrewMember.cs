using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;

using WellFormedNames;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	//Decision Feedback functionality to adjust opinions/mood based on placement currently commented out
	/// <summary>
	/// Stores values and functionality related to crew members
	/// </summary>
	public class CrewMember : Person, IComparable<CrewMember>
	{
		private readonly ConfigStore _config;

		public Dictionary<CrewMemberSkill, int> Skills { get; set; }
		public Dictionary<CrewMemberSkill, int> RevealedSkills { get; }
		public Dictionary<string, int> CrewOpinions { get; }
		public Dictionary<string, int> RevealedCrewOpinions { get; }
		public int RestCount { get; private set; }
		public Avatar Avatar { get; set; }

		/// <summary>
		/// Base constructor for creating a CrewMember
		/// </summary>
		public CrewMember(ConfigStore con, RolePlayCharacterAsset rpc) : base(rpc)
		{
			_config = con;
			RevealedSkills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				RevealedSkills.Add(skill, 0);
			}
			CrewOpinions = new Dictionary<string, int>();
			RevealedCrewOpinions = new Dictionary<string, int>();
		}

		/// <summary>
		/// Constructor for creating a CrewMember with a non-random age/gender/name
		/// </summary>
		public CrewMember(ConfigStore con) : this(con, null)
		{

		}

		/// <summary>
		/// Constructor for creating a CrewMember from a saved game
		/// </summary>
		public CrewMember(RolePlayCharacterAsset rpc, ConfigStore con) : this(con, rpc)
		{
			Skills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				Skills.Add(skill, 0);
			}
		}

		/// <summary>
		/// Constructor for creating a CrewMember with a random age/gender/name
		/// </summary>
		public CrewMember(Position position, string nationality, ConfigStore con) : this(con, null)
		{
			Gender = SelectGender();
			Age = StaticRandom.Int(18, 45);
			Nationality = nationality;
			Name = SelectRandomName();
			//set the skills of the new CrewMember according to the required skills for the selected position
			Skills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (position != Position.Null)
				{
					Skills.Add(skill, position.RequiresSkill(skill) ?
								StaticRandom.Int((int)_config.ConfigValues[ConfigKeys.GoodPositionRating], 11) :
								StaticRandom.Int(1, (int)_config.ConfigValues[ConfigKeys.BadPositionRating] + 1));
				}
				else
				{
					Skills.Add(skill, StaticRandom.Int((int)_config.ConfigValues[ConfigKeys.RandomSkillLow], (int)_config.ConfigValues[ConfigKeys.RandomSkillHigh] + 1));
				}
			}
		}

		/// <summary>
		/// Randomly select the gender of the CrewMember
		/// </summary>
		private string SelectGender()
		{
			return StaticRandom.Int(0, 1000) % 2 == 0 ? "M" : "F";
		}

		/// <summary>
		/// Randomly select a new name for this CrewMember
		/// </summary>
		internal string SelectNewName()
		{
			return SelectRandomName();
		}

		/// <summary>
		/// Randomly select a name for this CrewMember
		/// </summary>
		private string SelectRandomName()
		{
			var name = string.Empty;
			List<string> names;
			if (Gender == "M")
			{
				if (Nationality != null && _config.NameConfig.MaleForename.ContainsKey(Nationality))
				{
					names = _config.NameConfig.MaleForename[Nationality];
				}
				else
				{
					names = _config.NameConfig.MaleForename.Values.ToList().SelectMany(n => n).ToList();
				}
			}
			else
			{
				if (Nationality != null && _config.NameConfig.FemaleForename.ContainsKey(Nationality))
				{
					names = _config.NameConfig.FemaleForename[Nationality];
				}
				else
				{
					names = _config.NameConfig.FemaleForename.Values.ToList().SelectMany(n => n).ToList();
				}
			}
			name += names[StaticRandom.Int(0, names.Count)] + " ";

			if (Nationality != null && _config.NameConfig.Surname.ContainsKey(Nationality))
			{
				names = _config.NameConfig.Surname[Nationality];
			}
			else
			{
				names = _config.NameConfig.Surname.Values.ToList().SelectMany(n => n).ToList();
			}

			name += names[StaticRandom.Int(0, names.Count)];

			return name;
		}

		/// <summary>
		/// Update the EA file for this CrewMember with updated stats
		/// </summary>
		internal override void UpdateBeliefs(string position = null)
		{
			base.UpdateBeliefs(position);
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				UpdateSingleBelief(string.Format(NPCBeliefs.Skill.GetDescription(), skill), Skills[skill].ToString());
			}
		}

		/// <summary>
		/// Create opinions for everyone included in the list for this CrewMember
		/// </summary>
		internal void CreateInitialOpinions(List<string> people, bool save = false)
		{
			foreach (var person in people)
			{
				if (person == Name)
				{
					continue;
				}
				AddOrUpdateOpinion(person, StaticRandom.Int((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax] + 1));
				//if the two people share the same last name, give the bonus stated in the config to their opinion
				if (person.GetType() == typeof(CrewMember) && Name.Split(new[] { ' ' }, 2).Last() == person.Split(new[] { ' ' }, 2).Last())
				{
					AddOrUpdateOpinion(person, (int)_config.ConfigValues[ConfigKeys.LastNameBonusOpinion]);
				}
				AddOrUpdateRevealedOpinion(person, 0, false);
			}
			if (save)
			{
				SaveStatus();
			}
		}

		/// <summary>
		/// Adjust or overwrite an opinion on another Person
		/// </summary>
		public void AddOrUpdateOpinion(string person, int change, bool replace = false, bool load = false)
		{
			if (!CrewOpinions.ContainsKey(person))
			{
				CrewOpinions.Add(person, 0);
			}
			if (replace)
			{
				CrewOpinions[person] = change;
			}
			else
			{
				CrewOpinions[person] += change;
			}
			CrewOpinions[person] = LimitOpinionToRange(CrewOpinions[person]);
			if (!load)
			{
				UpdateSingleBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), person.NoSpaces()), CrewOpinions[person].ToString());
			}
		}

		/// <summary>
		/// Limit crew opinions to a certain range
		/// </summary>
		/// <param name="value"></param>
		/// <param name="inclusiveMinimum">default: -5</param>
		/// <param name="inclusiveMaximum">default: 5</param>
		/// <returns></returns>
		private int LimitOpinionToRange(int value, int inclusiveMinimum = -5, int inclusiveMaximum = 5)
		{
			if (value < inclusiveMinimum) { return inclusiveMinimum; }
			if (value > inclusiveMaximum) { return inclusiveMaximum; }
			return value;
		}


		/// <summary>
		/// Update the known opinion on this Person
		/// </summary>
		internal void AddOrUpdateRevealedOpinion(string person, int change, bool save = true)
		{
			if (!RevealedCrewOpinions.ContainsKey(person))
			{
				RevealedCrewOpinions.Add(person, 0);
			}
			RevealedCrewOpinions[person] = change;
			if (save)
			{
				UpdateSingleBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), person.NoSpaces()), RevealedCrewOpinions[person].ToString());
			}
		}

		/// <summary>
		/// Get the saved stats and opinions for this CrewMember
		/// </summary>
		internal void LoadBeliefs(List<string> people)
		{
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (Skills.ContainsKey(skill))
				{
					Skills[skill] = Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.Skill.GetDescription(), skill)));
				}
				else
				{
					Skills.Add(skill, 0);
				}
				if (RevealedSkills.ContainsKey(skill))
				{
					RevealedSkills[skill] = Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), skill)));
				}
				else
				{
					RevealedSkills.Add(skill, 0);
				}
			}
			foreach (var person in people)
			{
				AddOrUpdateOpinion(person, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), person.NoSpaces()))), true, true);
				AddOrUpdateRevealedOpinion(person, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), person.NoSpaces())) ?? "0"), false);
			}
			RestCount = Convert.ToInt32(LoadBelief(NPCBeliefs.Rest.GetDescription()));
		}

		/// <summary>
		/// Get the saved last position for this CrewMember
		/// </summary>
		internal void LoadPosition(Boat boat)
		{
			var pos = boat.Positions.FirstOrDefault(position => position.ToString() == LoadBelief(NPCBeliefs.Position.GetDescription()));
			if (pos != Position.Null)
			{
				boat.AssignCrewMember(pos, this);
			}
		}

		/// <summary>
		/// Get the current mood of this CrewMember
		/// </summary>
		public int GetMood()
		{
			var mood = 0;
			if (RolePlayCharacter != null)
			{
				//RolePlayCharacter.Decide();
				mood = (int)Math.Round(RolePlayCharacter.Mood);
			}
			return mood;
		}

		/// <summary>
		/// Get the current position (if any) for this CrewMember
		/// </summary>
		public Position GetBoatPosition(Dictionary<Position, CrewMember> currentPositioned)
		{
			return currentPositioned.SingleOrDefault(pair => pair.Value == this).Key;
		}

		/// <summary>
		/// Decrease rest amount and set rest amount if CrewMember has been used
		/// </summary>
		internal void RaceRest(bool assigned)
		{
			RestCount--;
			if (assigned)
			{
				RestCount = (int)_config.ConfigValues[ConfigKeys.PostRaceRest];
			}
			UpdateSingleBelief(NPCBeliefs.Rest.GetDescription(), RestCount.ToString());
		}

		/// <summary>
		/// Send an event to the EA/RPC to get CrewMember information
		/// </summary>
		internal List<string> SendMeetingEvent(IntegratedAuthoringToolAsset iat, string style, Team team)
		{
			var reply = new List<string>();
			switch (style)
			{
				case "StatReveal":
					//select a random skill that has not been displayed before or any random skill if all have been displayed
					var availableStats = RevealedSkills.Where(s => s.Value == 0).Select(s => s.Key).ToList();
					if (availableStats.Count == 0)
					{
						availableStats = RevealedSkills.Where(s => s.Value != Skills[s.Key]).Select(s => s.Key).ToList();
					}
					if (availableStats.Count == 0)
					{
						availableStats = RevealedSkills.Select(s => s.Key).ToList();
					}
					var randomStat = StaticRandom.Int(0, availableStats.Count);
					var statName = availableStats[randomStat].ToString();
					var selectedStat = (CrewMemberSkill)Enum.Parse(typeof(CrewMemberSkill), statName);
					var statValue = Skills[selectedStat];
					//add this skill rating to the dictionary to revealed skills
					RevealedSkills[selectedStat] = statValue;
					//get available dialogue based off of the rating in the skill
					style += statValue <= (int)_config.ConfigValues[ConfigKeys.BadSkillRating] ? "Bad" :
								statValue >= (int)_config.ConfigValues[ConfigKeys.GoodSkillRating] ? "Good" : "Middle";
					reply.Add(statName.ToLower());
					//save that this skill has been revealed
					UpdateSingleBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), statName), statValue.ToString());
					break;
				case "RoleReveal":
					//select a random position
					var pos = team.Boat.Positions[StaticRandom.Int(0, team.Boat.Positions.Count)];
					//get dialogue based on if they would be above or below mid-range in this position
					style += pos.GetPositionRating(this) <= 5 ? "Bad" : "Good";
					reply.Add(pos.ToString());
					break;
				case "OpinionRevealPositive":
					//get all opinions for active crewmembers and the manager
					var crewOpinionsPositive = CrewOpinions.Where(c => team.CrewMembers.ContainsKey(c.Key)).ToDictionary(p => p.Key, p => p.Value);
					crewOpinionsPositive.Add(team.Manager.Name, CrewOpinions[team.Manager.Name]);
					//get all opinions where the value is equal/greater than the OpinionLike value in the config
					var opinionsPositive = crewOpinionsPositive.Where(co => co.Value >= (int)_config.ConfigValues[ConfigKeys.OpinionLike]).ToDictionary(o => o.Key, o => o.Value);
					//if there are any positive opinions
					if (opinionsPositive.Any())
					{
						//select an opinion at random
						var pickedOpinionPositive = opinionsPositive.OrderBy(o => Guid.NewGuid()).First();
						if (pickedOpinionPositive.Value >= (int)_config.ConfigValues[ConfigKeys.OpinionStrongLike])
						{
							style += "High";
						}
						reply.Add(pickedOpinionPositive.Key != team.Manager.Name ? pickedOpinionPositive.Key : "you");
						AddOrUpdateRevealedOpinion(pickedOpinionPositive.Key, pickedOpinionPositive.Value);
					}
					//if there are no positive opinions, get dialogue based on that
					else
					{
						style += "None";
					}
					break;
				case "OpinionRevealNegative":
					var crewOpinionsNegative = CrewOpinions.Where(c => team.CrewMembers.ContainsKey(c.Key)).ToDictionary(p => p.Key, p => p.Value);
					crewOpinionsNegative.Add(team.Manager.Name, CrewOpinions[team.Manager.Name]);
					var opinionsNegative = crewOpinionsNegative.Where(co => co.Value <= (int)_config.ConfigValues[ConfigKeys.OpinionDislike]).ToDictionary(o => o.Key, o => o.Value);
					if (opinionsNegative.Any())
					{
						var pickedOpinionNegative = opinionsNegative.OrderBy(o => Guid.NewGuid()).First();
						if (pickedOpinionNegative.Value >= (int)_config.ConfigValues[ConfigKeys.OpinionStrongDislike])
						{
							style += "High";
						}
						reply.Add(pickedOpinionNegative.Key != team.Manager.Name ? pickedOpinionNegative.Key : "you");
						AddOrUpdateRevealedOpinion(pickedOpinionNegative.Key, pickedOpinionNegative.Value);
					}
					else
					{
						style += "None";
					}
					break;
			}
			SaveStatus();
			var dialogueOptions = iat.GetDialogueActionsByState("NPC_" + style).ToList();
			if (dialogueOptions.Any())
			{
				reply.Insert(0, dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance);
				return reply;
			}
			reply.Clear();
			return reply;
		}

		/// <summary>
		/// Get recruit reaction to statement based on their rating of that skill
		/// </summary>
		internal string SendRecruitEvent(IntegratedAuthoringToolAsset iat, CrewMemberSkill skill)
		{
			List<DialogueStateActionDTO> dialogueOptions;
			string state;
			if (Skills[skill] >= 9)
			{
				state = "NPC_StrongAgree";
			}
			else if (Skills[skill] >= 7)
			{
				state = "NPC_Agree";
			}
			else if (Skills[skill] >= 5)
			{
				state = "NPC_Neither";
			}
			else if (Skills[skill] >= 3)
			{
				state = "NPC_Disagree";
			}
			else
			{
				state = "NPC_StrongDisagree";
			}
			dialogueOptions = iat.GetDialogueActionsByState(state).ToList();

			return dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
		}

		/// <summary>
		/// Check to see if any events are about to be triggered
		/// </summary>
		internal void CurrentEventCheck(Team team, IntegratedAuthoringToolAsset iat)
		{
			var spacelessName = RolePlayCharacter.CharacterName;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			//if the crew member is expecting to be selected
			if (LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) != null && LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) != WellFormedNames.Name.NIL_STRING)
			{
				//if the crew member is not in a position
				if (GetBoatPosition(team.Boat.PositionCrew) == Position.Null)
				{
					//reduce opinion of the manager
					AddOrUpdateOpinion(team.Manager.Name, -3);
					//send event on record that this happened
					var eventString = "PostRace(NotPickedNotDone)";
					RolePlayCharacter.Perceive((Name)string.Format(eventBase, eventString, spacelessName));
				}
				//set their belief to 'null'
				UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), WellFormedNames.Name.NIL_STRING);
				TickUpdate(0);
			}
			//if the crew member is expecting to be selecting in a particular position
			if (LoadBelief(NPCBeliefs.ExpectedPosition.GetDescription()) != null)
			{
				var expected = LoadBelief(NPCBeliefs.ExpectedPosition.GetDescription());
				if (expected != WellFormedNames.Name.NIL_STRING && team.Boat.Positions.Any(p => p.ToString() == expected))
				{
					//if they are currently not in the position they expected to be in
					if (GetBoatPosition(team.Boat.PositionCrew).ToString() != expected)
					{
						//reduce opinion of the manager
						AddOrUpdateOpinion(team.Manager.Name, -3);
						//send event on record that this happened
						var eventString = "PostRace(PWNotDone)";
						RolePlayCharacter.Perceive((Name)string.Format(eventBase, eventString, spacelessName));
					}
					//set their belief to 'null'
					UpdateSingleBelief(NPCBeliefs.ExpectedPosition.GetDescription(), WellFormedNames.Name.NIL_STRING);
					TickUpdate(0);
				}
			}
			//if the crew member expects to be selected in a position after this current race, set them to instead be expecting to be selected for the current race
			if (LoadBelief(NPCBeliefs.ExpectedPositionAfter.GetDescription()) != null)
			{
				var expected = LoadBelief(NPCBeliefs.ExpectedPositionAfter.GetDescription());
				if (expected != WellFormedNames.Name.NIL_STRING)
				{
					UpdateSingleBelief(NPCBeliefs.ExpectedPositionAfter.GetDescription(), WellFormedNames.Name.NIL_STRING);
					UpdateSingleBelief(NPCBeliefs.ExpectedPosition.GetDescription(), expected);
					TickUpdate(0);
				}
			}
		}

		/// <summary>
		/// Get CrewMember reply to player dialogue during a post-race event
		/// </summary>
		internal DialogueStateActionDTO SendPostRaceEvent(IntegratedAuthoringToolAsset iat, DialogueStateActionDTO selected, Team team, List<string> subjects)
		{
			if (selected == null)
			{
				return null;
			}
			var nextState = selected.NextState;
			var dialogueOptions = iat.GetDialogueActionsByState(nextState).ToList();
			//get dialogue
			if (dialogueOptions.Any())
			{
				//select reply
				var selectedReply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
				PostRaceFeedback(selected.NextState, team, subjects);
				var styleSplit = selectedReply.Style.Split('_').Where(sp => !string.IsNullOrEmpty(sp)).ToList();
				if (styleSplit.Any(s => s != "-"))
				{
					styleSplit.ForEach(s => PostRaceFeedback(s, team, subjects));
				}
				return selectedReply;
			}
			
			return null;
		}

		/// <summary>
		/// Make changes based off of post-race events
		/// </summary>
		private void PostRaceFeedback(string ev, Team team, List<string> subjects)
		{
			var spacelessName = RolePlayCharacter.CharacterName;

			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = $"PostRace({ev})";
			if (ev.Contains("("))
			{
				eventString = string.Format(ev);
			}
			RolePlayCharacter.Perceive((Name)string.Format(eventBase, eventString, spacelessName));
			if (Enum.IsDefined(typeof(PostRaceEventImpact), ev))
			{
				//trigger different changes based off of what dialogue the player last picked
				switch ((PostRaceEventImpact)Enum.Parse(typeof(PostRaceEventImpact), ev))
				{
					case PostRaceEventImpact.ExpectedPosition:
						AddOrUpdateOpinion(team.Manager.Name, 1);
						UpdateSingleBelief(NPCBeliefs.ExpectedPosition.GetDescription(), subjects[0]);
						break;
					case PostRaceEventImpact.ExpectedPositionAfter:
						AddOrUpdateOpinion(team.Manager.Name, 1);
						UpdateSingleBelief(NPCBeliefs.ExpectedPositionAfter.GetDescription(), subjects[0]);
						break;
					case PostRaceEventImpact.ManagerOpinionWorse:
						AddOrUpdateOpinion(team.Manager.Name, -1);
						break;
					case PostRaceEventImpact.ManagerOpinionAllCrewWorse:
						foreach (var cm in team.CrewMembers)
						{
							cm.Value.AddOrUpdateOpinion(team.Manager.Name, -2);
							cm.Value.SaveStatus();
						}
						break;
					case PostRaceEventImpact.ManagerOpinionBetter:
						AddOrUpdateOpinion(team.Manager.Name, 1);
						break;
					case PostRaceEventImpact.ManagerOpinionAllCrewBetter:
						foreach (var cm in team.CrewMembers)
						{
							cm.Value.AddOrUpdateOpinion(team.Manager.Name, 2);
							cm.Value.SaveStatus();
						}
						break;
					case PostRaceEventImpact.ManagerOpinionMuchBetter:
						AddOrUpdateOpinion(team.Manager.Name, 5);
						break;
					case PostRaceEventImpact.ManagerOpinionMuchWorse:
						AddOrUpdateOpinion(team.Manager.Name, -5);
						break;
					case PostRaceEventImpact.RevealTwoSkills:
						AddOrUpdateOpinion(team.Manager.Name, 1);
						for (var i = 0; i < 2; i++)
						{
							var randomStat = Math.Pow(2, StaticRandom.Int(0, Skills.Count));
							var statName = ((CrewMemberSkill)randomStat).ToString();
							var statValue = Skills[(CrewMemberSkill)randomStat];
							RevealedSkills[(CrewMemberSkill)randomStat] = statValue;
							UpdateSingleBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), statName), statValue.ToString());
						}
						break;
					case PostRaceEventImpact.RevealFourSkills:
						AddOrUpdateOpinion(team.Manager.Name, 3);
						for (var i = 0; i < 4; i++)
						{
							var randomStat = Math.Pow(2, StaticRandom.Int(0, Skills.Count));
							var statName = ((CrewMemberSkill)randomStat).ToString();
							var statValue = Skills[(CrewMemberSkill)randomStat];
							RevealedSkills[(CrewMemberSkill)randomStat] = statValue;
							UpdateSingleBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), statName), statValue.ToString());
						}
						break;
					case PostRaceEventImpact.ImproveConflictOpinionGreatly:
						var subGreatHelp = Regex.Replace(subjects[0], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
						foreach (var cm in team.CrewMembers)
						{
							if (cm.Key != subGreatHelp)
							{
								cm.Value.AddOrUpdateOpinion(subGreatHelp, 2);
								cm.Value.AddOrUpdateRevealedOpinion(subGreatHelp, cm.Value.CrewOpinions[subGreatHelp]);
								cm.Value.SaveStatus();
							}
						}
						break;
					case PostRaceEventImpact.ImproveConflictTeamOpinion:
						var subHelp = Regex.Replace(subjects[0], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
						foreach (var cm in team.CrewMembers)
						{
							if (cm.Key != subHelp)
							{
								cm.Value.AddOrUpdateOpinion(subHelp, 1);
								cm.Value.AddOrUpdateRevealedOpinion(subHelp, cm.Value.CrewOpinions[subHelp]);
								cm.Value.SaveStatus();
							}
						}
						break;
					case PostRaceEventImpact.ImproveConflictKnowledge:
						var subKnow = Regex.Replace(subjects[0], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
						AddOrUpdateOpinion(team.Manager.Name, 1);
						foreach (var cm in team.CrewMembers)
						{
							if (cm.Key != subKnow)
							{
								cm.Value.AddOrUpdateRevealedOpinion(subKnow, cm.Value.CrewOpinions[subKnow]);
								cm.Value.SaveStatus();
							}
						}
						break;
					case PostRaceEventImpact.CausesSelectionAfter:
						AddOrUpdateOpinion(team.Manager.Name, 1);
						UpdateSingleBelief(NPCBeliefs.ExpectedPosition.GetDescription(), subjects[0]);
						var otherPlayer = Regex.Replace(subjects[1], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
						team.CrewMembers[otherPlayer].AddOrUpdateOpinion(team.Manager.Name, 1);
						team.CrewMembers[otherPlayer].UpdateSingleBelief(NPCBeliefs.ExpectedPositionAfter.GetDescription(), subjects[0]);
						team.CrewMembers[otherPlayer].SaveStatus();
						break;
					case PostRaceEventImpact.WholeTeamChange:
						AddOrUpdateOpinion(team.Manager.Name, 4);
						foreach (var cm in team.CrewMembers)
						{
							if (!team.LineUpHistory.Last().PositionCrew.Values.Select(v => v.Name).Contains(cm.Key))
							{
								cm.Value.AddOrUpdateOpinion(team.Manager.Name, 1);
								cm.Value.UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "true");
								cm.Value.SaveStatus();
							}
						}
						break;
				}
			}
			TickUpdate(0);
		}

		/// <summary>
		/// Retire this CrewMember
		/// </summary>
		internal void Retire()
		{
			UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), "Retired");
			var spacelessName = RolePlayCharacter.CharacterName;
			var eventBase = "Event(Action-Start,Player,Status(Retired),{0})";
			RolePlayCharacter.Perceive((Name)$"Event(Action-Start,Player,Status(Retired),{spacelessName})");
			Avatar = new Avatar(this, false, true);
			SaveStatus();
		}

		/// <summary>
		/// Get the current social importance rating fir this crew member
		/// </summary>
		public string GetSocialImportanceRating(string name)
		{
			var siValue = SocialImportance.GetSocialImportance(name.NoSpaces());
			return siValue < 5 
				? "Negative" 
				: siValue > 5 
					? "Positive" 
					: "Mid";
		}

		/// <summary>
		/// Compare this crew member to anothe by checking their names
		/// </summary>
		public int CompareTo(CrewMember other)
		{
			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}
	}
}

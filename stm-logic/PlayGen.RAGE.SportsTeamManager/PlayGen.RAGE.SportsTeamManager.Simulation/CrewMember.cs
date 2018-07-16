using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	//Decision Feedback functionality to adjust opinions/mood based on placement currently commented out
	/// <summary>
	/// Stores values and functionality related to crew members
	/// </summary>
	public class CrewMember : Person, IComparable<CrewMember>
	{
		public Dictionary<CrewMemberSkill, int> Skills { get; set; }
		public Dictionary<CrewMemberSkill, int> RevealedSkills { get; }
		public Dictionary<string, int> CrewOpinions { get; }
		public Dictionary<string, int> RevealedCrewOpinions { get; }
		public int RestCount { get; private set; }
		public Avatar Avatar { get; set; }

		/// <summary>
		/// Base constructor for creating a CrewMember
		/// </summary>
		internal CrewMember(RolePlayCharacterAsset rpc = null) : base(rpc)
		{
			Skills = new Dictionary<CrewMemberSkill, int>();
			RevealedSkills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				Skills.Add(skill, 0);
				RevealedSkills.Add(skill, 0);
			}
			CrewOpinions = new Dictionary<string, int>();
			RevealedCrewOpinions = new Dictionary<string, int>();
		}

		/// <summary>
		/// Constructor for creating a CrewMember with a random age/gender/name
		/// </summary>
		internal CrewMember(Position position, string nationality) : this()
		{
			Gender = SelectGender();
			Age = StaticRandom.Int(18, 45);
			Nationality = nationality;
			Name = SelectRandomName();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (position != Position.Null)
				{
					Skills[skill] = position.RequiresSkill(skill) ? StaticRandom.Int(ConfigKeys.GoodPositionRating.GetIntValue(), 11) : StaticRandom.Int(1, ConfigKeys.BadPositionRating.GetIntValue() + 1);
				}
				else
				{
					Skills[skill] = StaticRandom.Int(ConfigKeys.RandomSkillLow.GetIntValue(), ConfigKeys.RandomSkillHigh.GetIntValue() + 1);
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
		/// Randomly select a name for this CrewMember
		/// </summary>
		internal string SelectRandomName()
		{
			var names = Gender == "M" ? ConfigStore.NameConfig.MaleForename.ContainsKey(Nationality) ? ConfigStore.NameConfig.MaleForename[Nationality] : ConfigStore.NameConfig.MaleForename.Values.ToList().SelectMany(n => n).ToList() :
						ConfigStore.NameConfig.FemaleForename.ContainsKey(Nationality) ? ConfigStore.NameConfig.FemaleForename[Nationality] : ConfigStore.NameConfig.FemaleForename.Values.ToList().SelectMany(n => n).ToList();
			var name = names[StaticRandom.Int(0, names.Count)] + " ";
			names = ConfigStore.NameConfig.Surname.ContainsKey(Nationality) ? ConfigStore.NameConfig.Surname[Nationality] : ConfigStore.NameConfig.Surname.Values.ToList().SelectMany(n => n).ToList();
			name += names[StaticRandom.Int(0, names.Count)];
			return name;
		}

		private void CreateFile(IntegratedAuthoringToolAsset iat, string storageLocation, string fileName = "", bool recruit = false)
		{
			base.CreateFile(iat, storageLocation, fileName);
			UpdateSingleBelief(NPCBeliefs.Age, Age);
			UpdateSingleBelief(NPCBeliefs.Gender, Gender);
			UpdateSingleBelief(NPCBeliefs.Nationality, Nationality);
			UpdateSingleBelief(NPCBeliefs.Position, recruit ? "Recruit" : null);
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				UpdateSingleBelief(NPCBeliefs.Skill, Skills[skill], skill);
			}
			if (Avatar == null)
			{
				Avatar = new Avatar(this, !recruit);
			}
			else
			{
				Avatar.UpdateAvatarBeliefs(this);
				Avatar = new Avatar(this, !recruit);
			}
		}

		internal void CreateTeamMemberFile(IntegratedAuthoringToolAsset iat, string storageLocation, List<string> crewNames, Color primary, Color secondary, bool setOpinions = true, string fileName = "")
		{
			CreateFile(iat, storageLocation, fileName);
			Avatar.SetCrewColors(primary, secondary);
			if (setOpinions)
			{
				CreateInitialOpinions(crewNames);
			}
			else
			{
				foreach (var otherMember in crewNames)
				{
					if (Name != otherMember)
					{
						AddOrUpdateOpinion(otherMember, 0);
						AddOrUpdateRevealedOpinion(otherMember, 0, false);
					}
				}
			}
			SaveStatus();
		}

		internal void CreateRecruitFile(IntegratedAuthoringToolAsset iat, string storageLocation, int recruitNumber)
		{
			CreateFile(iat, storageLocation, "Recruit" + recruitNumber, true);
			SaveStatus();
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
				AddOrUpdateOpinion(person, StaticRandom.Int(ConfigKeys.DefaultOpinionMin.GetIntValue(), ConfigKeys.DefaultOpinionMax.GetIntValue() + 1));
				//if the two people share the same last name, give the bonus stated in the config to their opinion
				if (person.GetType() == typeof(CrewMember) && Name.Split(new[] { ' ' }, 2).Last() == person.Split(new[] { ' ' }, 2).Last())
				{
					AddOrUpdateOpinion(person, ConfigKeys.LastNameBonusOpinion.GetIntValue());
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
		internal void AddOrUpdateOpinion(string person, int change, bool save = true)
		{
			if (!CrewOpinions.ContainsKey(person))
			{
				CrewOpinions.Add(person, 0);
			}
			CrewOpinions[person] = LimitOpinionToRange(CrewOpinions[person] + change, -5, 5);
			if (!save)
			{
				UpdateSingleBelief(NPCBeliefs.Opinion, CrewOpinions[person], person.NoSpaces());
			}
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
				UpdateSingleBelief(NPCBeliefs.RevealedOpinion, RevealedCrewOpinions[person], person.NoSpaces());
			}
		}

		/// <summary>
		/// Get the saved stats and opinions for this CrewMember
		/// </summary>
		internal void LoadBeliefs(List<string> people, Team team = null)
		{
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				Skills[skill] = Convert.ToInt32(LoadBelief(NPCBeliefs.Skill, skill));
				RevealedSkills[skill] = Convert.ToInt32(LoadBelief(NPCBeliefs.RevealedSkill, skill));
			}
			foreach (var person in people)
			{
				AddOrUpdateOpinion(person, Convert.ToInt32(LoadBelief(NPCBeliefs.Opinion, person.NoSpaces()) ?? "0"), false);
				AddOrUpdateRevealedOpinion(person, Convert.ToInt32(LoadBelief(NPCBeliefs.RevealedOpinion, person.NoSpaces()) ?? "0"), false);
			}
			RestCount = Convert.ToInt32(LoadBelief(NPCBeliefs.Rest));
			Avatar = new Avatar(this, team != null);
			if (team != null)
			{
				Avatar.SetCrewColors(team.TeamColorsPrimary, team.TeamColorsSecondary);
				var pos = team.Boat.Positions.FirstOrDefault(position => position.ToString() == LoadBelief(NPCBeliefs.Position));
				if (pos != Position.Null)
				{
					team.Boat.AssignCrewMember(pos, this);
				}
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
				mood = (int)Math.Round(RolePlayCharacter.Mood);
			}
			return mood;
		}

		internal int GetOpinionRating(List<string> names)
		{
			var opinion = 0f;
			var opinionCount = 0f;

			//get the average opinion of every other positioned crew member and the manager
			if (CrewOpinions != null && CrewOpinions.Count > 0)
			{
				foreach (var name in names)
				{
					if (name != Name)
					{
						opinion += CrewOpinions[name];
						opinionCount++;
					}
				}
			}

			if (opinionCount > 0)
			{
				opinion = (float)Math.Round(opinion / opinionCount);
				opinion = opinion * ConfigKeys.OpinionRatingWeighting.GetValue();
			}
			return (int)opinion;
		}

		/// <summary>
		/// Decrease rest amount and set rest amount if CrewMember has been used
		/// </summary>
		internal void RaceRest(bool assigned)
		{
			RestCount--;
			if (assigned)
			{
				RestCount = ConfigKeys.PostRaceRest.GetIntValue();
			}
			UpdateSingleBelief(NPCBeliefs.Rest, RestCount);
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
					style += statValue <= ConfigKeys.BadSkillRating.GetIntValue() ? "Bad" : statValue >= ConfigKeys.GoodSkillRating.GetIntValue() ? "Good" : "Middle";
					reply.Add(statName.ToLower());
					//save that this skill has been revealed
					UpdateSingleBelief(NPCBeliefs.RevealedSkill, statValue, statName);
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
					var opinionsPositive = crewOpinionsPositive.Where(co => co.Value >= ConfigKeys.OpinionLike.GetIntValue()).ToDictionary(o => o.Key, o => o.Value);
					//if there are any positive opinions
					if (opinionsPositive.Any())
					{
						//select an opinion at random
						var pickedOpinionPositive = opinionsPositive.OrderBy(o => Guid.NewGuid()).First();
						if (pickedOpinionPositive.Value >= ConfigKeys.OpinionStrongLike.GetIntValue())
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
					var opinionsNegative = crewOpinionsNegative.Where(co => co.Value <= ConfigKeys.OpinionDislike.GetIntValue()).ToDictionary(o => o.Key, o => o.Value);
					if (opinionsNegative.Any())
					{
						var pickedOpinionNegative = opinionsNegative.OrderBy(o => Guid.NewGuid()).First();
						if (pickedOpinionNegative.Value >= ConfigKeys.OpinionStrongDislike.GetIntValue())
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
			string state;
			if (Skills[skill] >= 9)
			{
				state = "NPC_StronglyAgree";
			}
			else if (Skills[skill] >= 7)
			{
				state = "NPC_Agree";
			}
			else if (Skills[skill] >= 5)
			{
				state = "NPC_Neutral";
			}
			else if (Skills[skill] >= 3)
			{
				state = "NPC_Disagree";
			}
			else
			{
				state = "NPC_StronglyDisagree";
			}
			var dialogueOptions = iat.GetDialogueActionsByState(state).ToList();
			return dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
		}

		/// <summary>
		/// Check to see if any events are about to be triggered
		/// </summary>
		internal void CurrentEventCheck(Team team, IntegratedAuthoringToolAsset iat)
		{
			var spacelessName = Name.NoSpaces();
			//if the crew member is expecting to be selected
			if (LoadBelief(NPCBeliefs.ExpectedSelection) != null)
			{
				//if the crew member is not in a position
				if (team.Boat.GetCrewMemberPosition(this) == Position.Null)
				{
					//reduce opinion of the manager
					AddOrUpdateOpinion(team.Manager.Name, -3);
					//send event on record that this happened
					var eventString = EventHelper.ActionStart("Player", "PostRace(NotPickedNotDone)", spacelessName);
					RolePlayCharacter.Perceive(eventString);
					eventString = EventHelper.ActionStart("Player", "MoodChange(-3)", spacelessName);
					RolePlayCharacter.Perceive(eventString);
				}
				//set their belief to 'null'
				UpdateSingleBelief(NPCBeliefs.ExpectedSelection);
				TickUpdate(0);
			}
			//if the crew member is expecting to be selecting in a particular position
			if (LoadBelief(NPCBeliefs.ExpectedPosition) != null)
			{
				var expected = LoadBelief(NPCBeliefs.ExpectedPosition);
				if (expected != null && team.Boat.Positions.Any(p => p.ToString() == expected))
				{
					//if they are currently not in the position they expected to be in
					if (team.Boat.GetCrewMemberPosition(this).ToString() != expected)
					{
						//reduce opinion of the manager
						AddOrUpdateOpinion(team.Manager.Name, -3);
						//send event on record that this happened
						var eventString = EventHelper.ActionStart("Player", "PostRace(PWNotDone)", spacelessName);
						RolePlayCharacter.Perceive(eventString);
						eventString = EventHelper.ActionStart("Player", "MoodChange(-3)", spacelessName);
						RolePlayCharacter.Perceive(eventString);
					}
					//set their belief to 'null'
					UpdateSingleBelief(NPCBeliefs.ExpectedPosition);
					TickUpdate(0);
				}
			}
			//if the crew member expects to be selected in a position after this current race, set them to instead be expecting to be selected for the current race
			if (LoadBelief(NPCBeliefs.ExpectedPositionAfter) != null)
			{
				var expected = LoadBelief(NPCBeliefs.ExpectedPositionAfter);
				if (expected != null)
				{
					UpdateSingleBelief(NPCBeliefs.ExpectedPositionAfter);
					UpdateSingleBelief(NPCBeliefs.ExpectedPosition, expected);
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
				if (styleSplit.Any(s => s != WellFormedNames.Name.NIL_STRING))
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
			var spacelessName = Name.NoSpaces();
			var eventString = EventHelper.ActionStart("Player", $"PostRace({ev})", spacelessName);
			if (ev.Contains("("))
			{
				eventString = EventHelper.ActionStart("Player", ev, spacelessName);
			}
			RolePlayCharacter.Perceive(eventString);
			if (Enum.IsDefined(typeof(PostRaceEventImpact), ev))
			{
				//trigger different changes based off of what dialogue the player last picked
				switch ((PostRaceEventImpact)Enum.Parse(typeof(PostRaceEventImpact), ev))
				{
					case PostRaceEventImpact.ExpectedPosition:
						AddOrUpdateOpinion(team.Manager.Name, 1);
						UpdateSingleBelief(NPCBeliefs.ExpectedPosition, subjects[0]);
						break;
					case PostRaceEventImpact.ExpectedPositionAfter:
						AddOrUpdateOpinion(team.Manager.Name, 1);
						UpdateSingleBelief(NPCBeliefs.ExpectedPositionAfter, subjects[0]);
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
							UpdateSingleBelief(NPCBeliefs.RevealedSkill, statValue, statName);
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
							UpdateSingleBelief(NPCBeliefs.RevealedSkill, statValue, statName);
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
						UpdateSingleBelief(NPCBeliefs.ExpectedPosition, subjects[0]);
						var otherPlayer = Regex.Replace(subjects[1], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
						team.CrewMembers[otherPlayer].AddOrUpdateOpinion(team.Manager.Name, 1);
						team.CrewMembers[otherPlayer].UpdateSingleBelief(NPCBeliefs.ExpectedPositionAfter, subjects[0]);
						team.CrewMembers[otherPlayer].SaveStatus();
						break;
					case PostRaceEventImpact.WholeTeamChange:
						AddOrUpdateOpinion(team.Manager.Name, 4);
						foreach (var cm in team.CrewMembers)
						{
							if (!team.LineUpHistory.Last().PositionCrew.Values.Select(v => v.Name).Contains(cm.Key))
							{
								cm.Value.AddOrUpdateOpinion(team.Manager.Name, 1);
								cm.Value.UpdateSingleBelief(NPCBeliefs.ExpectedSelection, "true");
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
			UpdateSingleBelief(NPCBeliefs.Position, "Retired");
			var spacelessName = Name.NoSpaces();
			var eventString = EventHelper.ActionStart("Player", "Status(Retired)", spacelessName);
			RolePlayCharacter.Perceive(eventString);
			Avatar = new Avatar(this, false);
			SaveStatus();
		}

		/// <summary>
		/// Get the current social importance rating fir this crew member
		/// </summary>
		public string GetSocialImportanceRating(string name)
		{
			var siValue = SocialImportance.GetSocialImportance(name.NoSpaces());
			return siValue < 5 ? "Negative" : siValue > 5 ? "Positive" : "Mid";
		}

		/// <summary>
		/// Limit crew opinions to a certain range
		/// </summary>
		/// <param name="value"></param>
		/// <param name="inclusiveMinimum"></param>
		/// <param name="inclusiveMaximum"></param>
		/// <returns></returns>
		private int LimitOpinionToRange(int value, int inclusiveMinimum, int inclusiveMaximum)
		{
			if (value < inclusiveMinimum)
			{
				return inclusiveMinimum;
			}
			return value > inclusiveMaximum ? inclusiveMaximum : value;
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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Stores values and functionality related to crew members
	/// </summary>
	public class CrewMember : Person, IComparable<CrewMember>
	{
		public Dictionary<Skill, int> Skills { get; set; }
		public Dictionary<Skill, int> RevealedSkills { get; }
		public Dictionary<string, int> CrewOpinions { get; }
		public Dictionary<string, int> RevealedCrewOpinions { get; }
		public int RestCount { get; private set; }
		public Avatar Avatar { get; set; }

		public string FirstName => Name.Split(new[] { ' ' }, 2).First();
		public string LastName => Name.Split(new[] { ' ' }, 2).Last();

		/// <summary>
		/// Base constructor for creating a CrewMember
		/// </summary>
		internal CrewMember(RolePlayCharacterAsset rpc = null) : base(rpc)
		{
			Skills = new Dictionary<Skill, int>();
			RevealedSkills = new Dictionary<Skill, int>();
			foreach (Skill skill in Enum.GetValues(typeof(Skill)))
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
			SelectRandomName();
			foreach (Skill skill in Enum.GetValues(typeof(Skill)))
			{
				if (position != Position.Null)
				{
					Skills[skill] = position.RequiresSkill(skill) ? StaticRandom.Int(ConfigKey.GoodPositionRating.GetIntValue(), 11) : StaticRandom.Int(1, ConfigKey.BadPositionRating.GetIntValue() + 1);
				}
				else
				{
					Skills[skill] = StaticRandom.Int(ConfigKey.RandomSkillLow.GetIntValue(), ConfigKey.RandomSkillHigh.GetIntValue() + 1);
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
		internal void SelectRandomName()
		{
			var names = Gender == "M" ? ConfigStore.NameConfig.MaleForename.ContainsKey(Nationality) ? ConfigStore.NameConfig.MaleForename[Nationality] : ConfigStore.NameConfig.MaleForename.Values.ToList().SelectMany(n => n).ToList() :
						ConfigStore.NameConfig.FemaleForename.ContainsKey(Nationality) ? ConfigStore.NameConfig.FemaleForename[Nationality] : ConfigStore.NameConfig.FemaleForename.Values.ToList().SelectMany(n => n).ToList();
			var name = names[StaticRandom.Int(0, names.Count)] + " ";
			names = ConfigStore.NameConfig.Surname.ContainsKey(Nationality) ? ConfigStore.NameConfig.Surname[Nationality] : ConfigStore.NameConfig.Surname.Values.ToList().SelectMany(n => n).ToList();
			name += names[StaticRandom.Int(0, names.Count)];
			Name = name;
		}

		/// <summary>
		/// Create the FAtiMA files for this crew member, set belief values and create an avatar
		/// </summary>
		private void CreateFile(IntegratedAuthoringToolAsset iat, string storageLocation, string fileName = "", bool recruit = false)
		{
			base.CreateFile(iat, storageLocation, fileName);
			UpdateSingleBelief(NPCBelief.Age, Age);
			UpdateSingleBelief(NPCBelief.Gender, Gender);
			UpdateSingleBelief(NPCBelief.Nationality, Nationality);
			UpdateSingleBelief(NPCBelief.Position, recruit ? "Recruit" : null);
			foreach (Skill skill in Enum.GetValues(typeof(Skill)))
			{
				UpdateSingleBelief(NPCBelief.Skill, Skills[skill], skill);
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

		/// <summary>
		/// Set up a new crew member that is part of the team
		/// </summary>
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

		/// <summary>
		/// Set up a new crew member that is a recruit
		/// </summary>
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
				AddOrUpdateOpinion(person, StaticRandom.Int(ConfigKey.DefaultOpinionMin.GetIntValue(), ConfigKey.DefaultOpinionMax.GetIntValue() + 1));
				//if the two people share the same last name, give the bonus stated in the config to their opinion
				if (person.GetType() == typeof(CrewMember) && LastName == person.Split(new[] { ' ' }, 2).Last())
				{
					AddOrUpdateOpinion(person, ConfigKey.LastNameBonusOpinion.GetIntValue());
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
				UpdateSingleBelief(NPCBelief.Opinion, CrewOpinions[person], person.NoSpaces());
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
				UpdateSingleBelief(NPCBelief.RevealedOpinion, RevealedCrewOpinions[person], person.NoSpaces());
			}
		}

		/// <summary>
		/// Get the saved stats and opinions for this CrewMember
		/// </summary>
		internal void LoadBeliefs(List<string> people, Team team = null)
		{
			foreach (Skill skill in Enum.GetValues(typeof(Skill)))
			{
				Skills[skill] = Convert.ToInt32(LoadBelief(NPCBelief.Skill, skill));
				RevealedSkills[skill] = Convert.ToInt32(LoadBelief(NPCBelief.RevealedSkill, skill));
			}
			foreach (var person in people)
			{
				AddOrUpdateOpinion(person, Convert.ToInt32(LoadBelief(NPCBelief.Opinion, person.NoSpaces()) ?? "0"), false);
				AddOrUpdateRevealedOpinion(person, Convert.ToInt32(LoadBelief(NPCBelief.RevealedOpinion, person.NoSpaces()) ?? "0"), false);
			}
			RestCount = Convert.ToInt32(LoadBelief(NPCBelief.Rest));
			Avatar = new Avatar(this, team != null);
			if (team != null)
			{
				Avatar.SetCrewColors(team.TeamColorsPrimary, team.TeamColorsSecondary);
				var pos = team.Boat.Positions.FirstOrDefault(position => position.ToString() == LoadBelief(NPCBelief.Position));
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

		/// <summary>
		/// Get this crew member's average opinion of the other crew members and the manager
		/// </summary>
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
				opinion = opinion * ConfigKey.OpinionRatingWeighting.GetValue();
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
				RestCount = ConfigKey.PostRaceRest.GetIntValue();
			}
			UpdateSingleBelief(NPCBelief.Rest, RestCount);
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
					var selectedStat = (Skill)Enum.Parse(typeof(Skill), statName);
					var statValue = Skills[selectedStat];
					//add this skill rating to the dictionary to revealed skills
					RevealedSkills[selectedStat] = statValue;
					//get available dialogue based off of the rating in the skill
					style += statValue <= ConfigKey.BadSkillRating.GetIntValue() ? "Bad" : statValue >= ConfigKey.GoodSkillRating.GetIntValue() ? "Good" : "Middle";
					reply.Add(statName.ToLower());
					//save that this skill has been revealed
					UpdateSingleBelief(NPCBelief.RevealedSkill, statValue, statName);
					break;
				case "RoleReveal":
					//select a random position
					var pos = team.Boat.Positions[StaticRandom.Int(0, team.Boat.PositionCount)];
					//get dialogue based on if they would be above or below mid-range in this position
					style += pos.GetPositionRating(this) <= 5 ? "Bad" : "Good";
					reply.Add(pos.ToString());
					break;
				case "OpinionRevealPositive":
					//get all opinions for active crewmembers and the manager
					var crewOpinionsPositive = CrewOpinions.Where(c => team.CrewMembers.ContainsKey(c.Key)).ToDictionary(p => p.Key, p => p.Value);
					crewOpinionsPositive.Add(team.ManagerName, CrewOpinions[team.ManagerName]);
					//get all opinions where the value is equal/greater than the OpinionLike value in the config
					var opinionsPositive = crewOpinionsPositive.Where(co => co.Value >= ConfigKey.OpinionLike.GetIntValue()).ToDictionary(o => o.Key, o => o.Value);
					//if there are any positive opinions
					if (opinionsPositive.Any())
					{
						//select an opinion at random
						var pickedOpinionPositive = opinionsPositive.OrderBy(o => Guid.NewGuid()).First();
						if (pickedOpinionPositive.Value >= ConfigKey.OpinionStrongLike.GetIntValue())
						{
							style += "High";
						}
						reply.Add(pickedOpinionPositive.Key != team.ManagerName ? pickedOpinionPositive.Key : "you");
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
					crewOpinionsNegative.Add(team.ManagerName, CrewOpinions[team.ManagerName]);
					var opinionsNegative = crewOpinionsNegative.Where(co => co.Value <= ConfigKey.OpinionDislike.GetIntValue()).ToDictionary(o => o.Key, o => o.Value);
					if (opinionsNegative.Any())
					{
						var pickedOpinionNegative = opinionsNegative.OrderBy(o => Guid.NewGuid()).First();
						if (pickedOpinionNegative.Value >= ConfigKey.OpinionStrongDislike.GetIntValue())
						{
							style += "High";
						}
						reply.Add(pickedOpinionNegative.Key != team.ManagerName ? pickedOpinionNegative.Key : "you");
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
		internal string SendRecruitEvent(IntegratedAuthoringToolAsset iat, Skill skill)
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
			if (LoadBelief(NPCBelief.ExpectedSelection) != null)
			{
				//if the crew member is not in a position
				if (team.Boat.GetCrewMemberPosition(this) == Position.Null)
				{
					//reduce opinion of the manager
					AddOrUpdateOpinion(team.ManagerName, -3);
					//send event on record that this happened
					var eventString = EventHelper.ActionStart("Player", "PostRace(NotPickedNotDone)", spacelessName);
					RolePlayCharacter.Perceive(eventString);
					eventString = EventHelper.ActionStart("Player", "MoodChange(-3)", spacelessName);
					RolePlayCharacter.Perceive(eventString);
				}
				//set their belief to 'null'
				UpdateSingleBelief(NPCBelief.ExpectedSelection);
				TickUpdate(0);
			}
			//if the crew member is expecting to be selecting in a particular position
			if (LoadBelief(NPCBelief.ExpectedPosition) != null)
			{
				var expected = LoadBelief(NPCBelief.ExpectedPosition);
				if (expected != null && team.Boat.Positions.Any(p => p.ToString() == expected))
				{
					//if they are currently not in the position they expected to be in
					if (team.Boat.GetCrewMemberPosition(this).ToString() != expected)
					{
						//reduce opinion of the manager
						AddOrUpdateOpinion(team.ManagerName, -3);
						//send event on record that this happened
						var eventString = EventHelper.ActionStart("Player", "PostRace(PWNotDone)", spacelessName);
						RolePlayCharacter.Perceive(eventString);
						eventString = EventHelper.ActionStart("Player", "MoodChange(-3)", spacelessName);
						RolePlayCharacter.Perceive(eventString);
					}
					//set their belief to 'null'
					UpdateSingleBelief(NPCBelief.ExpectedPosition);
					TickUpdate(0);
				}
			}
			//if the crew member expects to be selected in a position after this current race, set them to instead be expecting to be selected for the current race
			if (LoadBelief(NPCBelief.ExpectedPositionAfter) != null)
			{
				var expected = LoadBelief(NPCBelief.ExpectedPositionAfter);
				if (expected != null)
				{
					UpdateSingleBelief(NPCBelief.ExpectedPositionAfter);
					UpdateSingleBelief(NPCBelief.ExpectedPosition, expected);
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
			//event perceived to trigger any mood change events kept in the EA file
			RolePlayCharacter.Perceive(eventString);
			if (Enum.IsDefined(typeof(PostRaceEventImpact), ev))
			{
				//trigger different changes based off of what dialogue the player last picked
				switch ((PostRaceEventImpact)Enum.Parse(typeof(PostRaceEventImpact), ev))
				{
					//improve opinion of manager, crew member now expects to be picked in the selected position (subjects[0]) next race
					case PostRaceEventImpact.ExpectedPosition:
						AddOrUpdateOpinion(team.ManagerName, 1);
						UpdateSingleBelief(NPCBelief.ExpectedPosition, subjects[0]);
						break;
					//improve opinion of manager, crew member now expects to be picked in the selected position (subjects[0]) in two races time
					case PostRaceEventImpact.ExpectedPositionAfter:
						AddOrUpdateOpinion(team.ManagerName, 1);
						UpdateSingleBelief(NPCBelief.ExpectedPositionAfter, subjects[0]);
						break;
					//make opinion of manager worse
					case PostRaceEventImpact.ManagerOpinionWorse:
						AddOrUpdateOpinion(team.ManagerName, -1);
						break;
					//make all crew members' opinion of manager worse
					case PostRaceEventImpact.ManagerOpinionAllCrewWorse:
						foreach (var cm in team.CrewMembers)
						{
							cm.Value.AddOrUpdateOpinion(team.ManagerName, -2);
							cm.Value.SaveStatus();
						}
						break;
					//improve opinion of manager
					case PostRaceEventImpact.ManagerOpinionBetter:
						AddOrUpdateOpinion(team.ManagerName, 1);
						break;
					//make all crew members' opinion of manager better
					case PostRaceEventImpact.ManagerOpinionAllCrewBetter:
						foreach (var cm in team.CrewMembers)
						{
							cm.Value.AddOrUpdateOpinion(team.ManagerName, 2);
							cm.Value.SaveStatus();
						}
						break;
					//improve opinion of manager greatly
					case PostRaceEventImpact.ManagerOpinionMuchBetter:
						AddOrUpdateOpinion(team.ManagerName, 5);
						break;
					//make opinion of manager much worse
					case PostRaceEventImpact.ManagerOpinionMuchWorse:
						AddOrUpdateOpinion(team.ManagerName, -5);
						break;
					//reveal two random skills for this crew member (can be already revealed skills)
					case PostRaceEventImpact.RevealTwoSkills:
						AddOrUpdateOpinion(team.ManagerName, 1);
						for (var i = 0; i < 2; i++)
						{
							var randomStat = Math.Pow(2, StaticRandom.Int(0, Skills.Count));
							var statName = ((Skill)randomStat).ToString();
							var statValue = Skills[(Skill)randomStat];
							RevealedSkills[(Skill)randomStat] = statValue;
							UpdateSingleBelief(NPCBelief.RevealedSkill, statValue, statName);
						}
						break;
					//reveal four random skills for this crew member (can be already revealed skills)
					case PostRaceEventImpact.RevealFourSkills:
						AddOrUpdateOpinion(team.ManagerName, 3);
						for (var i = 0; i < 4; i++)
						{
							var randomStat = Math.Pow(2, StaticRandom.Int(0, Skills.Count));
							var statName = ((Skill)randomStat).ToString();
							var statValue = Skills[(Skill)randomStat];
							RevealedSkills[(Skill)randomStat] = statValue;
							UpdateSingleBelief(NPCBelief.RevealedSkill, statValue, statName);
						}
						break;
					//improve all crew members' opinion of the crew member who was the subject of the event (subjects[0]) greatly and reveals their opinion.
					//Regex adds spaces back before each capital letter
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
					//improve all crew members' opinion of the crew member who was the subject of the event (subjects[0]) and reveals their opinion.
					//Regex adds spaces back before each capital letter
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
					//reveals all crew members' opinion of the crew member who was the subject of the event (subjects[0]) and slightly improves this the opinion of the manager for this crew member.
					//Regex adds spaces back before each capital letter
					case PostRaceEventImpact.ImproveConflictKnowledge:
						var subKnow = Regex.Replace(subjects[0], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
						AddOrUpdateOpinion(team.ManagerName, 1);
						foreach (var cm in team.CrewMembers)
						{
							if (cm.Key != subKnow)
							{
								cm.Value.AddOrUpdateRevealedOpinion(subKnow, cm.Value.CrewOpinions[subKnow]);
								cm.Value.SaveStatus();
							}
						}
						break;
					//improve opinion of manager, expects to be placed in perferred position (subjects[0]) next race
					//other crew member involved in this event (subjects[1]) - improve opinion of manager, expects to be placed in perferred position (subjects[0]) in two races times
					//Regex adds spaces back before each capital letter
					case PostRaceEventImpact.CausesSelectionAfter:
						AddOrUpdateOpinion(team.ManagerName, 1);
						UpdateSingleBelief(NPCBelief.ExpectedPosition, subjects[0]);
						var otherPlayer = Regex.Replace(subjects[1], @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
						team.CrewMembers[otherPlayer].AddOrUpdateOpinion(team.ManagerName, 1);
						team.CrewMembers[otherPlayer].UpdateSingleBelief(NPCBelief.ExpectedPositionAfter, subjects[0]);
						team.CrewMembers[otherPlayer].SaveStatus();
						break;
					//improves opinion of manager greatly, all unselected crew members' opinion of manager improves and expect to be selected next race
					case PostRaceEventImpact.WholeTeamChange:
						AddOrUpdateOpinion(team.ManagerName, 4);
						foreach (var cm in team.CrewMembers)
						{
							if (!team.PreviousSession.PositionCrew.Values.Select(v => v.Name).Contains(cm.Key))
							{
								cm.Value.AddOrUpdateOpinion(team.ManagerName, 1);
								cm.Value.UpdateSingleBelief(NPCBelief.ExpectedSelection, "true");
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
			UpdateSingleBelief(NPCBelief.Position, "Retired");
			var spacelessName = Name.NoSpaces();
			var eventString = EventHelper.ActionStart("Player", "Status(Retired)", spacelessName);
			RolePlayCharacter.Perceive(eventString);
			Avatar = new Avatar(this, false);
			SaveStatus();
		}

		/// <summary>
		/// Get the current social importance rating for this crew member
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
		/// Compare this crew member to another by checking their names
		/// </summary>
		public int CompareTo(CrewMember other)
		{
			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}
	}
}

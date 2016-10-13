using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly ConfigStore config;
		
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
			config = con;
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
		public CrewMember(Position position, ConfigStore con) : this(con, null)
		{
			Gender = SelectGender();
			Age = StaticRandom.Int(18, 45);
			Name = SelectRandomName(Gender);
			//set the skills of the new CrewMember according to the required skills for the selected position
			Skills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (position != Position.Null)
				{
					if (position.RequiresSkill(skill))
					{
						Skills.Add(skill, StaticRandom.Int((int)config.ConfigValues[ConfigKeys.GoodPositionRating], 11));
					}
					else
					{
						Skills.Add(skill, StaticRandom.Int(1, (int)config.ConfigValues[ConfigKeys.BadPositionRating] + 1));
					}
				}
				else
				{
					Skills.Add(skill, StaticRandom.Int((int)config.ConfigValues[ConfigKeys.RandomSkillLow], (int)config.ConfigValues[ConfigKeys.RandomSkillHigh] + 1));
				}
			}
		}

		/// <summary>
		/// Randomly select the gender of the CrewMember
		/// </summary>
		private string SelectGender()
		{
			return StaticRandom.Int(0, 1000) % 2 == 0 ? "Male" : "Female";
		}

		/// <summary>
		/// Randomly select a new name for this CrewMember
		/// </summary>
		public string SelectNewName(string gender)
		{
			return SelectRandomName(gender);
		}

		/// <summary>
		/// Randomly select a name for this CrewMember
		/// </summary>
		private string SelectRandomName(string gender)
		{
			var name = "";
			switch (gender)
			{
				case "Male":
					{
						var names = new []{"Oliver", "Jack", "Harry", "Jacob", "Charlie", "Thomas", "George", "Oscar", "James", "William", "Noah", "Alfie", "Joshua", "Muhammad", "Henry", "Leo", "Archie", "Ethan", "Joseph", "Freddie", "Samuel", "Alexander", "Logan", "Daniel", "Isaac", "Max", "Mohammed", "Benjamin", "Mason", "Lucas", "Edward", "Harrison", "Jake", "Dylan", "Riley", "Finley", "Theo", "Sebastian", "Adam", "Zachary", "Arthur", "Toby", "Jayden", "Luke", "Harley", "Lewis", "Tyler", "Harvey", "Matthew", "David", "Reuben", "Michael", "Elijah", "Kian", "Tommy", "Mohammad", "Blake", "Luca", "Theodore", "Stanley", "Jenson", "Nathan", "Charles", "Frankie", "Jude", "Teddy", "Louie", "Louis", "Ryan", "Hugo", "Bobby", "Elliott", "Dexter", "Ollie", "Alex", "Liam", "Kai", "Gabriel", "Connor", "Aaron", "Frederick", "Callum", "Elliot", "Albert", "Leon", "Ronnie", "Rory", "Jamie", "Austin", "Seth", "Ibrahim", "Owen", "Caleb", "Ellis", "Sonny", "Robert", "Joey", "Felix", "Finlay", "Jackson" };
						name += names[StaticRandom.Int(0, names.Length)];
					}
					break;
				case "Female":
					{
						var names = new [] {"Amelia", "Olivia", "Isla", "Emily", "Poppy", "Ava", "Isabella", "Jessica", "Lily", "Sophie", "Grace", "Sophia", "Mia", "Evie", "Ruby", "Ella", "Scarlett", "Isabelle", "Chloe", "Sienna", "Freya", "Phoebe", "Charlotte", "Daisy", "Alice", "Florence", "Eva", "Sofia", "Millie", "Lucy", "Evelyn", "Elsie", "Rosie", "Imogen", "Lola", "Matilda", "Elizabeth", "Layla", "Holly", "Lilly", "Molly", "Erin", "Ellie", "Maisie", "Maya", "Abigail", "Eliza", "Georgia", "Jasmine", "Esme", "Willow", "Bella", "Annabelle", "Ivy", "Amber", "Emilia", "Emma", "Summer", "Hannah", "Eleanor", "Harriet", "Rose", "Amelie", "Lexi", "Megan", "Gracie", "Zara", "Lacey", "Martha", "Anna", "Violet", "Darcey", "Maria", "Maryam", "Brooke", "Aisha", "Katie", "Leah", "Thea", "Darcie", "Hollie", "Amy", "Mollie", "Heidi", "Lottie", "Bethany", "Francesca", "Faith", "Harper", "Nancy", "Beatrice", "Isabel", "Darcy", "Lydia", "Sarah", "Sara", "Julia", "Victoria", "Zoe", "Robyn" };
						name += names[StaticRandom.Int(0, names.Length)];
					}
					break;
			}
			name += " ";
			var surnames = new [] {"Smith", "Jones", "Williams", "Taylor", "Brown", "Davies", "Evans", "Thomas", "Wilson", "Johnson", "Roberts", "Robinson", "Thompson", "Wright", "Walker", "White", "Edwards", "Hughes", "Green", "Hall", "Lewis", "Harris", "Clarke", "Patel", "Jackson", "Wood", "Turner", "Martin", "Cooper", "Hill", "Morris", "Ward", "Moore", "Clark", "Baker", "Harrison", "King", "Morgan", "Lee", "Allen", "James", "Phillips", "Scott", "Watson", "Davis", "Parker", "Bennett", "Price", "Griffiths", "Young"};
			name += surnames[StaticRandom.Int(0, surnames.Length)];
			return name;
		}

		/// <summary>
		/// Update the EA file for this CrewMember with updated stats
		/// </summary>
		public override void UpdateBeliefs(string position = null)
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
		public void CreateInitialOpinions(List<string> people)
		{
			foreach (var person in people)
			{
				CreateInitialOpinion(person);
			}
		}

		/// <summary>
		/// Create an initial opinion on this person based on the given allowed range from the config
		/// </summary>
		public void CreateInitialOpinion(string person)
		{
			if (person == Name)
			{
				return;
			}
			AddOrUpdateOpinion(person, StaticRandom.Int((int)config.ConfigValues[ConfigKeys.DefaultOpinionMin], (int)config.ConfigValues[ConfigKeys.DefaultOpinionMax] + 1));
			//if the two people share the same last name, give the bonus stated in the config to their opinion
			if (person.GetType() == typeof(CrewMember) && Name.Split(' ').Last() == person.Split(' ').Last())
			{
				AddOrUpdateOpinion(person, (int)config.ConfigValues[ConfigKeys.LastNameBonusOpinion]);
			}
			AddOrUpdateRevealedOpinion(person, 0);
			SaveStatus();
		}

		/// <summary>
		/// Adjust or overwrite an opinion on another Person
		/// </summary>
		public void AddOrUpdateOpinion(string person, int change, bool replace = false)
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
			if (CrewOpinions[person] < -5)
			{
				CrewOpinions[person] = -5;
			}
			if (CrewOpinions[person] > 5)
			{
				CrewOpinions[person] = 5;
			}
			UpdateSingleBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), person.NoSpaces()), CrewOpinions[person].ToString());
		}

		/// <summary>
		/// Update the known opinion on this Person
		/// </summary>
		public void AddOrUpdateRevealedOpinion(string person, int change)
		{
			if (!RevealedCrewOpinions.ContainsKey(person))
			{
				RevealedCrewOpinions.Add(person, 0);
			}
			RevealedCrewOpinions[person] = change;
			UpdateSingleBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), person.NoSpaces()), RevealedCrewOpinions[person].ToString());
		}

		/// <summary>
		/// Get the saved stats and opinions for this CrewMember
		/// </summary>
		public void LoadBeliefs(List<string> people)
		{
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (Skills.ContainsKey(skill))
				{
					Skills[skill] = Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.Skill.GetDescription(), skill)));
				} else
				{
					Skills.Add(skill, 0);
				}
				if (RevealedSkills.ContainsKey(skill))
				{
					RevealedSkills[skill] = Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), skill)));
				}
				else {
					RevealedSkills.Add(skill, 0);
				}
			}
			foreach (var person in people)
			{
				AddOrUpdateOpinion(person, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), person.NoSpaces()))), true);
			}
			foreach (var person in people)
			{
				AddOrUpdateRevealedOpinion(person, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), person.NoSpaces()))));
			}
			RestCount = Convert.ToInt32(LoadBelief(NPCBeliefs.Rest.GetDescription()));
		}

		/// <summary>
		/// Get the saved last position for this CrewMember
		/// </summary>
		public void LoadPosition(Boat boat)
		{
			var pos = boat.Positions.FirstOrDefault(position => position.GetName() == LoadBelief(NPCBeliefs.Position.GetDescription()));
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
		/// Pass events to see what this CrewMember thinks of the current crew line-up and save these and any other changes
		/// </summary>
		public void DecisionFeedback(Boat boat)
		{
			/*var spacelessName = RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var currentPosition = boat.Positions.Single(bp => bp.CrewMember == this);
			int positionScore = currentPosition != null ? currentPosition.Position.GetPositionRating(this) : 0;
			var eventString = string.Format("PositionRating({0})", positionScore);
			var positionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			if (positionRpc != null)
			{
				var positionKey = positionRpc.Parameters.First().GetValue().ToString();
				switch (positionKey)
				{
					case "Good":
						AddOrUpdateOpinion(boat.Manager, 1);
						break;
					case "Bad":
						AddOrUpdateOpinion(boat.Manager, -1);
						break;
					case "VeryBad":
						AddOrUpdateOpinion(boat.Manager, -1);
						break;
				}
				RolePlayCharacter.ActionFinished(positionRpc);
			}
			TickUpdate();

			eventString = string.Format("ManagerOpinionCheck({0})", boat.Manager.Name.NoSpaces());
			var managerOpinionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			if (managerOpinionRpc != null)
			{
				RolePlayCharacter.ActionFinished(managerOpinionRpc);
			}
			TickUpdate();

			foreach (BoatPosition boatPosition in boat.Positions.OrderBy(b => b.Position.Name))
			{
				if (boatPosition.CrewMember != null && boatPosition.CrewMember != this)
				{
					int possiblePositionScore = boatPosition.Position.GetPositionRating(this);
					eventString = string.Format("OpinionCheck({0},{1},{2})", boatPosition.CrewMember.Name.NoSpaces(), possiblePositionScore, positionScore);
					if (positionScore == 0)
					{
						int theirPositionScore = boatPosition.Position.GetPositionRating(boatPosition.CrewMember);
						eventString = string.Format("OpinionCheck({0},{1},{2})", boatPosition.CrewMember.Name.NoSpaces(), possiblePositionScore, theirPositionScore);
					}
					var opinionRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
					EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
					if (opinionRpc != null)
					{
						var opinionKey = opinionRpc.Parameters.First().GetValue().ToString();
						switch (opinionKey)
						{
							case "DislikedInBetter":
								AddOrUpdateOpinion(boatPosition.CrewMember, -1);
								AddOrUpdateOpinion(team.Manager, -1);
								break;
						}
						RolePlayCharacter.ActionFinished(opinionRpc);
					}
					TickUpdate();
				}
			}*/
		}

		/// <summary>
		/// Decrease rest amount and set rest amount if CrewMember has been used
		/// </summary>
		public void RaceRest(bool assigned)
		{
			RestCount--;
			if (assigned)
			{
				RestCount = (int)config.ConfigValues[ConfigKeys.PostRaceRest];
			}
			UpdateSingleBelief(NPCBeliefs.Rest.GetDescription(), RestCount.ToString());
		}

		/// <summary>
		/// Send an event to the EA/RPC to get CrewMember information
		/// </summary>
		public string SendMeetingEvent(IntegratedAuthoringToolAsset iat, string style, Team team)
		{
			var reply = "";
			var dialogueOptions = new List<DialogueStateActionDTO>();
			switch (style)
			{
				case "StatReveal":
					//select a random skill
					var randomStat = Math.Pow(2, StaticRandom.Int(0, Skills.Count));
					var statName = ((CrewMemberSkill)randomStat).ToString();
					var statValue = Skills[(CrewMemberSkill)randomStat];
					//add this skill rating to the dictionary to revealed skills
					RevealedSkills[(CrewMemberSkill)randomStat] = statValue;
					//get available dialogue based off of the rating in the skill
					if (statValue <= (int)config.ConfigValues[ConfigKeys.BadSkillRating])
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Bad").ToName()).ToList();
					}
					else if (statValue >= (int)config.ConfigValues[ConfigKeys.GoodSkillRating])
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Good").ToName()).ToList();
					}
					else
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Middle").ToName()).ToList();
					}
					//select a random reply from those available
					if (dialogueOptions.Any())
					{
						reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, statName.ToLower());
					}
					UpdateSingleBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), statName), statValue.ToString());
					break;
				case "RoleReveal":
					//select a random position
					var pos = team.Boat.Positions[StaticRandom.Int(0, team.Boat.Positions.Count)];
					//get dialogue based on if they would be above or below mid-range in this position
					if (pos.GetPositionRating(this) <= 5)
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Bad").ToName()).ToList();
					}
					else if (pos.GetPositionRating(this) >= 6)
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Good").ToName()).ToList();
					}
					if (dialogueOptions.Any())
					{
						reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pos.GetName());
					}
					break;
				case "OpinionRevealPositive":
					//get all opinions for active crewmembers and the manager
					var crewOpinionsPositive = CrewOpinions.Where(c => team.CrewMembers.ContainsKey(c.Key)).ToDictionary(p => p.Key, p => p.Value);
					crewOpinionsPositive.Add(team.Manager.Name, CrewOpinions[team.Manager.Name]);
					//get all opinions where the value is equal/greater than the OpinionLike value in the config
					var opinionsPositive = crewOpinionsPositive.Where(co => co.Value >= (int)config.ConfigValues[ConfigKeys.OpinionLike]).ToDictionary(o => o.Key, o => o.Value);
					//if there are any positive opinions
					if (opinionsPositive.Any())
					{
						//select an opinion at random
						var pickedOpinionPositive = opinionsPositive.OrderBy(o => Guid.NewGuid()).First();
						//get available dialogue based on strength of opinion
						if (pickedOpinionPositive.Value >= (int)config.ConfigValues[ConfigKeys.OpinionStrongLike])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "High").ToName()).ToList();
						}
						else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, style.ToName()).ToList();
						}
						if (dialogueOptions.Any())
						{
							reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pickedOpinionPositive.Key);
							if (pickedOpinionPositive.Key == team.Manager.Name)
							{
								reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, "you");
							}
						}
						AddOrUpdateRevealedOpinion(pickedOpinionPositive.Key, pickedOpinionPositive.Value);
					}
					//if there are no positive opinions, get dialogue based on that
					else
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "None").ToName()).ToList();
						if (dialogueOptions.Any())
						{
							reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
						}
					}
					break;
				case "OpinionRevealNegative":
					var crewOpinionsNegative = CrewOpinions.Where(c => team.CrewMembers.ContainsKey(c.Key)).ToDictionary(p => p.Key, p => p.Value);
					crewOpinionsNegative.Add(team.Manager.Name, CrewOpinions[team.Manager.Name]);
					var opinionsNegative = crewOpinionsNegative.Where(co => co.Value <= (int)config.ConfigValues[ConfigKeys.OpinionDislike]).ToDictionary(o => o.Key, o => o.Value);
					if (opinionsNegative.Any())
					{
						var pickedOpinionNegative = opinionsNegative.OrderBy(o => Guid.NewGuid()).First();
						if (pickedOpinionNegative.Value <= (int)config.ConfigValues[ConfigKeys.OpinionStrongDislike])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "High").ToName()).ToList();
						}
						else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, style.ToName()).ToList();
						}
						if (dialogueOptions.Any())
						{
							reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pickedOpinionNegative.Key);
							if (pickedOpinionNegative.Key == team.Manager.Name)
							{
								reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, "you");
							}
						}
						AddOrUpdateRevealedOpinion(pickedOpinionNegative.Key, pickedOpinionNegative.Value);
					}
					else
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "None").ToName()).ToList();
						if (dialogueOptions.Any())
						{
							reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
						}
					}
					break;
			}
			SaveStatus();
			return reply;
		}

		/// <summary>
		/// Get recruit reaction to statement based on their rating of that skill
		/// </summary>
		public string SendRecruitEvent(IntegratedAuthoringToolAsset iat, CrewMemberSkill skill)
		{
			List<DialogueStateActionDTO> dialogueOptions;
			if (Skills[skill] >= 9)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "StrongAgree".ToName()).ToList();
			}
			else if (Skills[skill] >= 7)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Agree".ToName()).ToList();
			}
			else if (Skills[skill] >= 5)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Neither".ToName()).ToList();
			}
			else if (Skills[skill] >= 3)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Disagree".ToName()).ToList();
			}
			else
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "StrongDisagree".ToName()).ToList();
			}
			return dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
		}

		/// <summary>
		/// Check to see if any events are about to be triggered
		/// </summary>
		public DialogueStateActionDTO[] CurrentEventCheck(Team team, IntegratedAuthoringToolAsset iat, bool afterRaceSession)
		{
			var replies = new List<DialogueStateActionDTO>();
			List<DialogueStateActionDTO> dialogueOptions;
			var spacelessName = RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			//if this CrewMember is expecting to be picked for the next race and this is after that race
			if (afterRaceSession && (LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) ?? "null").ToLower() == "true")
			{
				//if they are currently unpositioned
				if (GetBoatPosition(team.Boat.PositionCrew) == Position.Null)
				{
					//reduce opinion of the manager
					AddOrUpdateOpinion(team.Manager.Name, -3);
					//set their belief to 'false'
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "false");
					//send event on record that this happened
					var eventString = "PostRace(NotPickedAfterSorry)";
					EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
					var eventRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
					if (eventRpc != null)
					{
						RolePlayCharacter.ActionFinished(eventRpc);
					}
					//get dialogue for this happening
					dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "NotPickedAfterSorry".ToName()).ToList();
				}
				//if they were positioned
				else
				{
					//set their belief to 'not'
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "not");
					//get dialogue for this happening
					dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "PickedAfterSorry".ToName()).ToList();
				}
				//if there are any dialogue options, select one at random and add to the list of replies
				if (dialogueOptions.Any())
				{
					var selectedNext = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
					replies.Add(selectedNext);
				}
				TickUpdate();
			}
			//if the CrewMember is planning to retire
			if (afterRaceSession && !string.IsNullOrEmpty(LoadBelief("Event(Retire)")))
			{
				//update retirement countdown belief
				UpdateSingleBelief("Event(Retire)", (Convert.ToInt32(LoadBelief("Event(Retire)")) - 1).ToString());
				//if countdown has reached zero
				if (Convert.ToInt32(LoadBelief("Event(Retire)")) == 0)
				{
					//send event on record this event being triggered
					var eventString = "PostRace(RetirementTriggered)";
					EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
					var eventRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
					if (eventRpc != null)
					{
						RolePlayCharacter.ActionFinished(eventRpc);
					}
					team.RetireCrew(this);
					dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "RetirementTriggered".ToName()).ToList();
					if (dialogueOptions.Any())
					{
						var selectedNext = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
						replies.Add(selectedNext);
					}
				}
			}
			return replies.OrderBy(o => Guid.NewGuid()).ToArray();
		}

		/// <summary>
		/// Get CrewMember reply to player dialogue during a post-race event
		/// </summary>
		public string SendPostRaceEvent(IntegratedAuthoringToolAsset iat, DialogueStateActionDTO selected, Team team, Boat previousBoat)
		{
			string reply = null;
			var nextState = selected.NextState;
			switch (nextState)
			{
				//adjust nextstate based on if the player has said they would pick them in the past and did not
				case "NotPickedSorry":
					if ((LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) ?? "null").ToLower() == "false")
					{
						nextState = selected.NextState + "Again";
					}
						break;
				//adjust nextstate based on if the player has said they were not the best in any position when they could have been
				case "NotPickedSkill":
					foreach (var position in previousBoat.Positions)
					{
						if (position.GetPositionRating(this) >= previousBoat.PositionScores[position])
						{
							nextState = selected.NextState + "Incorrect";
						}
						break;
					}
					break;
			}
			//get dialogue
			var dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, nextState.ToName()).ToList();
			if (dialogueOptions.Any())
			{
				//select reply
				var selectedNext = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
				//set player state
				iat.SetDialogueState("Player", selectedNext.NextState);
				reply = selectedNext.Utterance;
				//trigger this if the player has no next state
				if (selectedNext.NextState == "-")
				{
					PostRaceFeedback(nextState, team);
				}
			}
			//trigger this if the CrewMember has no dialogue to the current event
			else
			{
				iat.SetDialogueState("Player", "-");
				PostRaceFeedback(nextState, team);
			}
			return reply;
		}

		/// <summary>
		/// Make changes based off of post-race events
		/// </summary>
		private void PostRaceFeedback(string lastEvent, Team team)
		{
			SaveStatus();
			var spacelessName = RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = string.Format("PostRace({0})", lastEvent);
			EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new [] { string.Format(eventBase, eventString, spacelessName) });
			//trigger different changes based off of what dialogue the player last picked
			switch (lastEvent)
			{
				case "NotPickedSorry":
					AddOrUpdateOpinion(team.Manager.Name, 1);
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "true");
					break;
				case "NotPickedSorryAgain":
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "true");
					break;
				case "NotPickedSkillIncorrect":
					AddOrUpdateOpinion(team.Manager.Name, -1);
					break;
				case "NotPickedFiredYes":
					AddOrUpdateOpinion(team.Manager.Name, -10);
					team.RetireCrew(this);
					foreach (var cm in team.CrewMembers.Values)
					{
						cm.AddOrUpdateOpinion(team.Manager.Name, -2);
						cm.SaveStatus();
					}
					break;
				case "NotPickedFiredNo":
					AddOrUpdateOpinion(team.Manager.Name, -10);
					break;
				case "NotPickedSkillTrain":
					AddOrUpdateOpinion(team.Manager.Name, 2);
					for (var i = 0; i < 2; i++)
					{
						var randomStat = Math.Pow(2, StaticRandom.Int(0, Skills.Count));
						Skills[(CrewMemberSkill)randomStat]++;
						if (Skills[(CrewMemberSkill)randomStat] > 10)
						{
							Skills[(CrewMemberSkill)randomStat] = 10;
						}
						UpdateSingleBelief(string.Format(NPCBeliefs.Skill.GetDescription(), (CrewMemberSkill)randomStat), Skills[(CrewMemberSkill)randomStat].ToString());
					}
					break;
				case "NotPickedSkillFriends":
					AddOrUpdateOpinion(team.Manager.Name, 1);
					var allCrew = team.CrewMembers;
					allCrew.Remove(Name);
					for (var i = 0; i < 2; i++)
					{
						var randomCrew = StaticRandom.Int(0, allCrew.Count);
						var cm = allCrew.Values.ToList()[randomCrew];
						AddOrUpdateOpinion(cm.Name, 2);
						cm.AddOrUpdateOpinion(Name, 2);
						cm.SaveStatus();
					}
					break;
				case "NotPickedSkillNothing":
					AddOrUpdateOpinion(team.Manager.Name, -10);
					break;
				case "NotPickedSkillLeave":
					AddOrUpdateOpinion(team.Manager.Name, -10);
					team.RetireCrew(this);
					foreach (var cm in team.CrewMembers)
					{
						cm.Value.AddOrUpdateOpinion(team.Manager.Name, -1);
						cm.Value.SaveStatus();
					}
					break;
			}
			if (eventRpc != null)
			{
				RolePlayCharacter.ActionFinished(eventRpc);
			}
			TickUpdate();
			SaveStatus();
		}

		/// <summary>
		/// Retire this CrewMember
		/// </summary>
		public void Retire()
		{
			UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), "Retired");
			var spacelessName = RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,Status(Retired),{0})";
			EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, spacelessName) });
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, spacelessName) });
			if (eventRpc != null)
			{
				RolePlayCharacter.ActionFinished(eventRpc);
			}
			Avatar = new Avatar(this, false, true);
			SaveStatus();
		}

		public int CompareTo(CrewMember other)
		{
			return String.Compare(Name, other.Name, StringComparison.Ordinal);
		}
	}
}
 
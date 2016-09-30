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
	public class CrewMember : Person
	{
		public Dictionary<CrewMemberSkill, int> Skills { get; set; }
		public Dictionary<CrewMemberSkill, int> RevealedSkills { get; }
		public List<CrewOpinion> CrewOpinions { get; }
		public List<CrewOpinion> RevealedCrewOpinions { get; }
		public event EventHandler OpinionChange = delegate { };
		public int RestCount { get; private set; }
		public Avatar Avatar { get; set; }
		private ConfigStore _config { get; }

		/// <summary>
		/// Constructor for creating a CrewMember with a non-random age/gender/name
		/// </summary>
		public CrewMember(ConfigStore config)
		{
			_config = config;
			RevealedSkills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				RevealedSkills.Add(skill, 0);
			}
			CrewOpinions = new List<CrewOpinion>();
			RevealedCrewOpinions = new List<CrewOpinion>();
		}

		/// <summary>
		/// Constructor for creating a CrewMember from a saved game
		/// </summary>
		public CrewMember(RolePlayCharacterAsset rpc, ConfigStore config) : base(rpc)
		{
			_config = config;
			Skills = new Dictionary<CrewMemberSkill, int>();
			RevealedSkills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				Skills.Add(skill, 0);
				RevealedSkills.Add(skill, 0);
			}
			CrewOpinions = new List<CrewOpinion>();
			RevealedCrewOpinions = new List<CrewOpinion>();
		}

		/// <summary>
		/// Constructor for creating a CrewMember with a random age/gender/name
		/// </summary>
		public CrewMember(Random random, Boat boat, ConfigStore config)
		{
			_config = config;
			Gender = SelectGender(random);
			Age = random.Next(18, 45);
			Name = SelectRandomName(Gender, random);
			RevealedSkills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				RevealedSkills.Add(skill, 0);
			}
			CrewOpinions = new List<CrewOpinion>();
			RevealedCrewOpinions = new List<CrewOpinion>();
			boat.UniqueNameCheck(random, this);
			//select a position that is in need of a new crew member
			Position selectedPerferred = boat.GetWeakPosition(random);
			//set the skills of the new CrewMember according to the required skills for the selected position
			Skills = new Dictionary<CrewMemberSkill, int>();
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (selectedPerferred != null)
				{
					if (selectedPerferred.RequiredSkills.Contains(skill))
					{
						Skills.Add(skill, random.Next((int)_config.ConfigValues[ConfigKeys.GoodPositionRating.ToString()], 11));
					}
					else
					{
						Skills.Add(skill, random.Next(1, (int)_config.ConfigValues[ConfigKeys.BadPositionRating.ToString()] + 1));
					}
				}
				else
				{
					Skills.Add(skill, random.Next((int)_config.ConfigValues[ConfigKeys.RandomSkillLow.ToString()], (int)_config.ConfigValues[ConfigKeys.RandomSkillHigh.ToString()] + 1));
				}
			}
		}

		/// <summary>
		/// Randomly select the gender of the CrewMember
		/// </summary>
		private string SelectGender(Random random)
		{
			return random.Next(0, 1000) % 2 == 0 ? "Male" : "Female";
		}

		/// <summary>
		/// Randomly select a new name for this CrewMember
		/// </summary>
		public string SelectNewName(string gender, Random random)
		{
			return SelectRandomName(gender, random);
		}

		/// <summary>
		/// Randomly select a name for this CrewMember
		/// </summary>
		private string SelectRandomName(string gender, Random random)
		{
			var name = "";
			if (gender == "Male")
			{
				var names = new []{"Oliver", "Jack", "Harry", "Jacob", "Charlie", "Thomas", "George", "Oscar", "James", "William", "Noah", "Alfie", "Joshua", "Muhammad", "Henry", "Leo", "Archie", "Ethan", "Joseph", "Freddie", "Samuel", "Alexander", "Logan", "Daniel", "Isaac", "Max", "Mohammed", "Benjamin", "Mason", "Lucas", "Edward", "Harrison", "Jake", "Dylan", "Riley", "Finley", "Theo", "Sebastian", "Adam", "Zachary", "Arthur", "Toby", "Jayden", "Luke", "Harley", "Lewis", "Tyler", "Harvey", "Matthew", "David", "Reuben", "Michael", "Elijah", "Kian", "Tommy", "Mohammad", "Blake", "Luca", "Theodore", "Stanley", "Jenson", "Nathan", "Charles", "Frankie", "Jude", "Teddy", "Louie", "Louis", "Ryan", "Hugo", "Bobby", "Elliott", "Dexter", "Ollie", "Alex", "Liam", "Kai", "Gabriel", "Connor", "Aaron", "Frederick", "Callum", "Elliot", "Albert", "Leon", "Ronnie", "Rory", "Jamie", "Austin", "Seth", "Ibrahim", "Owen", "Caleb", "Ellis", "Sonny", "Robert", "Joey", "Felix", "Finlay", "Jackson" };
				name += names[random.Next(0, names.Length)];
			}
			else if (gender == "Female")
			{
				var names = new [] {"Amelia", "Olivia", "Isla", "Emily", "Poppy", "Ava", "Isabella", "Jessica", "Lily", "Sophie", "Grace", "Sophia", "Mia", "Evie", "Ruby", "Ella", "Scarlett", "Isabelle", "Chloe", "Sienna", "Freya", "Phoebe", "Charlotte", "Daisy", "Alice", "Florence", "Eva", "Sofia", "Millie", "Lucy", "Evelyn", "Elsie", "Rosie", "Imogen", "Lola", "Matilda", "Elizabeth", "Layla", "Holly", "Lilly", "Molly", "Erin", "Ellie", "Maisie", "Maya", "Abigail", "Eliza", "Georgia", "Jasmine", "Esme", "Willow", "Bella", "Annabelle", "Ivy", "Amber", "Emilia", "Emma", "Summer", "Hannah", "Eleanor", "Harriet", "Rose", "Amelie", "Lexi", "Megan", "Gracie", "Zara", "Lacey", "Martha", "Anna", "Violet", "Darcey", "Maria", "Maryam", "Brooke", "Aisha", "Katie", "Leah", "Thea", "Darcie", "Hollie", "Amy", "Mollie", "Heidi", "Lottie", "Bethany", "Francesca", "Faith", "Harper", "Nancy", "Beatrice", "Isabel", "Darcy", "Lydia", "Sarah", "Sara", "Julia", "Victoria", "Zoe", "Robyn" };
				name += names[random.Next(0, names.Length)];
			}
			name += " ";
			var surnames = new [] {"Smith", "Jones", "Williams", "Taylor", "Brown", "Davies", "Evans", "Thomas", "Wilson", "Johnson", "Roberts", "Robinson", "Thompson", "Wright", "Walker", "White", "Edwards", "Hughes", "Green", "Hall", "Lewis", "Harris", "Clarke", "Patel", "Jackson", "Wood", "Turner", "Martin", "Cooper", "Hill", "Morris", "Ward", "Moore", "Clark", "Baker", "Harrison", "King", "Morgan", "Lee", "Allen", "James", "Phillips", "Scott", "Watson", "Davis", "Parker", "Bennett", "Price", "Griffiths", "Young"};
			name += surnames[random.Next(0, surnames.Length)];
			return name;
		}

		/// <summary>
		/// Update the EA file for this CrewMember with updated stats and opinions
		/// </summary>
		public override void UpdateBeliefs(string position = null)
		{
			base.UpdateBeliefs(position);
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				UpdateSingleBelief(string.Format(NPCBeliefs.Skill.GetDescription(), skill), Skills[skill].ToString());
			}
		}

		public void CreateInitialOpinions(Random random, Boat boat)
		{
			foreach (CrewMember otherMember in boat.GetAllCrewMembers())
			{
				if (this != otherMember)
				{
					CreateInitialOpinion(random, otherMember);
				}
			}
			CreateInitialOpinion(random, boat.Manager);
		}

		public void CreateInitialOpinion(Random random, Person person)
		{
			AddOrUpdateOpinion(person, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
			if (person.GetType() == typeof(CrewMember) && Name.Split(' ').Last() == person.Name.Split(' ').Last())
			{
				AddOrUpdateOpinion(person, (int)_config.ConfigValues[ConfigKeys.LastNameBonusOpinion.ToString()]);
			}
			AddOrUpdateRevealedOpinion(person, 0);
			SaveStatus();
		}

		/// <summary>
		/// Adjust or overwrite an opinion on another Person
		/// </summary>
		public void AddOrUpdateOpinion(Person person, int change, bool replace = false)
		{
			var cw = CrewOpinions.SingleOrDefault(op => op.Person == person);
			if (cw != null)
			{
				if (replace)
				{
					cw.Opinion = change;
				}
				else
				{
					cw.Opinion += change;
				}
			}
			else
			{
				cw = new CrewOpinion
				{
					Person = person,
					Opinion = change
				};
				CrewOpinions.Add(cw);
			}
			if (cw.Opinion < -5)
			{
				cw.Opinion = -5;
			}
			if (cw.Opinion > 5)
			{
				cw.Opinion = 5;
			}
			UpdateSingleBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), cw.Person.Name.Replace(" ", "")), cw.Opinion.ToString());
			OpinionChange(this, new EventArgs());
		}

		public void AddOrUpdateRevealedOpinion(Person person, int change)
		{
			var cw = RevealedCrewOpinions.SingleOrDefault(op => op.Person == person);
			if (cw != null)
			{
				cw.Opinion = change;
			}
			else
			{
				cw = new CrewOpinion
				{
					Person = person,
					Opinion = change
				};
				RevealedCrewOpinions.Add(cw);
			}
			if (cw.Opinion < -5)
			{
				cw.Opinion = -5;
			}
			if (cw.Opinion > 5)
			{
				cw.Opinion = 5;
			}
			UpdateSingleBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), cw.Person.Name.Replace(" ", "")), cw.Opinion.ToString());
		}

		/// <summary>
		/// Get the saved stats and opinions for this CrewMember
		/// </summary>
		public void LoadBeliefs(Boat boat)
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
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				AddOrUpdateOpinion(member, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), member.Name.Replace(" ", "")))), true);
			}
			AddOrUpdateOpinion(boat.Manager, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), boat.Manager.Name.Replace(" ", "")))), true);
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				AddOrUpdateRevealedOpinion(member, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), member.Name.Replace(" ", "")))));
			}
			AddOrUpdateRevealedOpinion(boat.Manager, Convert.ToInt32(LoadBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), boat.Manager.Name.Replace(" ", "")))));
			RestCount = Convert.ToInt32(LoadBelief(NPCBeliefs.Rest.GetDescription()));
		}

		/// <summary>
		/// Get the saved last position for this CrewMember
		/// </summary>
		public void LoadPosition(Boat boat)
		{
			if (LoadBelief(NPCBeliefs.Position.GetDescription()) != "null")
			{
				var boatPosition = boat.BoatPositions.Where(bp => bp.Position.Name == LoadBelief(NPCBeliefs.Position.GetDescription()));
				foreach (BoatPosition bp in boatPosition)
				{
					if (bp.CrewMember == null)
					{
						boat.AssignCrew(bp, this);
					}
				}
			}
		}

		/// <summary>
		/// Get the current mood of this CrewMember
		/// </summary>
		public int GetMood()
		{
			int mood = 0;
			if (RolePlayCharacter != null)
			{
				mood = (int)Math.Round(RolePlayCharacter.Mood);
			}
			return mood;
		}

		/// <summary>
		/// Get the current position on this Boat (if any) for this CrewMember
		/// </summary>
		public BoatPosition GetBoatPosition(Boat boat)
		{
			return boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this);
		}

		/// <summary>
		/// Pass events to see what this CrewMember thinks of the current crew line-up and save these and any other changes
		/// </summary>
		public void DecisionFeedback(Boat boat)
		{
			/*var spacelessName = RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var currentPosition = boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this);
			int positionScore = currentPosition != null ? currentPosition.Position.GetPositionRating(this) : 0;
			var eventString = string.Format("PositionRating({0})", positionScore);
			var positionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			if (positionRpc != null)
			{
				var positionKey = positionRpc.Parameters.FirstOrDefault().GetValue().ToString();
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

			eventString = string.Format("ManagerOpinionCheck({0})", boat.Manager.Name.Replace(" ", ""));
			var managerOpinionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			if (managerOpinionRpc != null)
			{
				RolePlayCharacter.ActionFinished(managerOpinionRpc);
			}
			TickUpdate();

			foreach (BoatPosition boatPosition in boat.BoatPositions.OrderBy(b => b.Position.Name))
			{
				if (boatPosition.CrewMember != null && boatPosition.CrewMember != this)
				{
					int possiblePositionScore = boatPosition.Position.GetPositionRating(this);
					eventString = string.Format("OpinionCheck({0},{1},{2})", boatPosition.CrewMember.Name.Replace(" ", ""), possiblePositionScore, positionScore);
					if (positionScore == 0)
					{
						int theirPositionScore = boatPosition.Position.GetPositionRating(boatPosition.CrewMember);
						eventString = string.Format("OpinionCheck({0},{1},{2})", boatPosition.CrewMember.Name.Replace(" ", ""), possiblePositionScore, theirPositionScore);
					}
					var opinionRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
					EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
					if (opinionRpc != null)
					{
						var opinionKey = opinionRpc.Parameters.FirstOrDefault().GetValue().ToString();
						switch (opinionKey)
						{
							case "DislikedInBetter":
								AddOrUpdateOpinion(boatPosition.CrewMember, -1);
								AddOrUpdateOpinion(boat.Manager, -1);
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
				RestCount = (int)_config.ConfigValues[ConfigKeys.PostRaceRest.ToString()];
			}
			UpdateSingleBelief(NPCBeliefs.Rest.GetDescription(), RestCount.ToString());
		}

		/// <summary>
		/// Send an event to the EA/RPC to get CrewMember information
		/// </summary>
		public string SendMeetingEvent(IntegratedAuthoringToolAsset iat, string style, Boat boat)
		{
			string reply = "";
			Random rand = new Random();
			List<DialogueStateActionDTO> dialogueOptions = new List<DialogueStateActionDTO>();
			switch (style)
			{
				case "StatReveal":
					int randomStat = rand.Next(0, Skills.Count);
					string statName = ((CrewMemberSkill)randomStat).ToString();
					int statValue = Skills[(CrewMemberSkill)randomStat];
					RevealedSkills[(CrewMemberSkill)randomStat] = statValue;
					if (statValue <= (int)_config.ConfigValues[ConfigKeys.BadSkillRating.ToString()])
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Bad").ToName()).ToList();
					}
					else if (statValue >= (int)_config.ConfigValues[ConfigKeys.GoodSkillRating.ToString()])
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Good").ToName()).ToList();
					}
					else
					{
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "Middle").ToName()).ToList();
					}
					if (dialogueOptions.Any())
					{
						reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, statName.ToLower());
					}
					UpdateSingleBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), statName), statValue.ToString());
					break;
				case "RoleReveal":
					Position pos = boat.BoatPositions[rand.Next(0, boat.BoatPositions.Count)].Position;
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
						reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pos.Name);
					}
					break;
				case "OpinionRevealPositive":
					var crewOpinionsPositive = CrewOpinions.Where(c => boat.GetAllCrewMembers().Select(cm => cm.Name).Contains(c.Person.Name)).ToList();
					crewOpinionsPositive.AddRange(CrewOpinions.Where(c => c.Person == boat.Manager));
					var opinionsPositive = crewOpinionsPositive.Where(co => co.Opinion >= (int)_config.ConfigValues[ConfigKeys.OpinionLike.ToString()]);
					var pickedOpinionPositive = opinionsPositive.OrderBy(o => Guid.NewGuid()).FirstOrDefault();
					if (pickedOpinionPositive != null)
					{
						if (pickedOpinionPositive.Opinion >= (int)_config.ConfigValues[ConfigKeys.OpinionStrongLike.ToString()])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "High").ToName()).ToList();
						}
						else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, style.ToName()).ToList();
						}
						if (dialogueOptions.Any())
						{
							reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pickedOpinionPositive.Person.Name);
							if (pickedOpinionPositive.Person == boat.Manager)
							{
								reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, "you");
							}
						}
						AddOrUpdateRevealedOpinion(pickedOpinionPositive.Person, pickedOpinionPositive.Opinion);
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
				case "OpinionRevealNegative":
					var crewOpinionsNegative = CrewOpinions.Where(c => boat.GetAllCrewMembers().Select(cm => cm.Name).Contains(c.Person.Name)).ToList();
					crewOpinionsNegative.AddRange(CrewOpinions.Where(c => c.Person == boat.Manager));
					var opinionsNegative = crewOpinionsNegative.Where(co => co.Opinion <= (int)_config.ConfigValues[ConfigKeys.OpinionDislike.ToString()]);
					var pickedOpinionNegative = opinionsNegative.OrderBy(o => Guid.NewGuid()).FirstOrDefault();
					if (pickedOpinionNegative != null)
					{
						if (pickedOpinionNegative.Opinion <= (int)_config.ConfigValues[ConfigKeys.OpinionStrongDislike.ToString()])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, (style + "High").ToName()).ToList();
						}
						else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, style.ToName()).ToList();
						}
						if (dialogueOptions.Any())
						{
							reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pickedOpinionNegative.Person.Name);
							if (pickedOpinionNegative.Person == boat.Manager)
							{
								reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, "you");
							}
						}
						AddOrUpdateRevealedOpinion(pickedOpinionNegative.Person, pickedOpinionNegative.Opinion);
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
		public DialogueStateActionDTO[] CurrentEventCheck(Boat boat, IntegratedAuthoringToolAsset iat, bool afterRaceSession)
		{
			List<DialogueStateActionDTO> replies = new List<DialogueStateActionDTO>();
			List<DialogueStateActionDTO> dialogueOptions;
			var spacelessName = RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			if (afterRaceSession && (LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) ?? "null").ToLower() == "true")
			{
				if (GetBoatPosition(boat) == null)
				{
					AddOrUpdateOpinion(boat.Manager, -3);
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "false");
					var eventString = "PostRace(NotPickedAfterSorry)";
					EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
					var eventRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
					if (eventRpc != null)
					{
						RolePlayCharacter.ActionFinished(eventRpc);
					}
					dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "NotPickedAfterSorry".ToName()).ToList();
				}
				else
				{
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "not");
					dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "PickedAfterSorry".ToName()).ToList();
				}
				if (dialogueOptions.Any())
				{
					DialogueStateActionDTO selectedNext = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
					replies.Add(selectedNext);
				}
				TickUpdate();
			}
			if (afterRaceSession && !string.IsNullOrEmpty(LoadBelief("Event(Retire)")))
			{
				UpdateSingleBelief("Event(Retire)", (Convert.ToInt32(LoadBelief("Event(Retire)")) - 1).ToString());
				if (Convert.ToInt32(LoadBelief("Event(Retire)")) == 0)
				{
					var eventString = "PostRace(RetirementTriggered)";
					EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
					var eventRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, eventString, spacelessName) });
					if (eventRpc != null)
					{
						RolePlayCharacter.ActionFinished(eventRpc);
					}
					boat.RetireCrew(this);
					dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "RetirementTriggered".ToName()).ToList();
					if (dialogueOptions.Any())
					{
						DialogueStateActionDTO selectedNext = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
						replies.Add(selectedNext);
					}
				}
			}
			return replies.OrderBy(o => Guid.NewGuid()).ToArray();
		}

		/// <summary>
		/// Get CrewMember reply to player dialogue during a post-race event
		/// </summary>
		public string SendPostRaceEvent(IntegratedAuthoringToolAsset iat, DialogueStateActionDTO selected, Boat boat, Boat previousBoat)
		{
			string reply = null;
			string nextState = selected.NextState;
			switch (nextState)
			{
				case "NotPickedSorry":
					if ((LoadBelief(NPCBeliefs.ExpectedSelection.GetDescription()) ?? "null").ToLower() == "false")
					{
						nextState = selected.NextState + "Again";
					}
						break;
				case "NotPickedSkill":
					foreach (BoatPosition bp in previousBoat.BoatPositions)
					{
						if (bp.Position.GetPositionRating(this) >= bp.PositionScore)
						{
							nextState = selected.NextState + "Incorrect";
						}
						break;
					}
					break;
			}
			List<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, nextState.ToName()).ToList();
			if (dialogueOptions.Any())
			{
				DialogueStateActionDTO selectedNext = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
				iat.SetDialogueState("Player", selectedNext.NextState);
				reply = selectedNext.Utterance;
				if (selectedNext.NextState == "-")
				{
					PostRaceFeedback(nextState, boat);
				}
			}
			else
			{
				iat.SetDialogueState("Player", "-");
				PostRaceFeedback(nextState, boat);
			}
			return reply;
		}

		/// <summary>
		/// Make changes based off of post-race events
		/// </summary>
		private void PostRaceFeedback(string lastEvent, Boat boat)
		{
			SaveStatus();
			var spacelessName = RolePlayCharacter.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = string.Format("PostRace({0})", lastEvent);
			EmotionalAppraisal.AppraiseEvents(new[] { string.Format(eventBase, eventString, spacelessName) });
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new [] { string.Format(eventBase, eventString, spacelessName) });
			Random rand = new Random();
			switch (lastEvent)
			{
				case "NotPickedSorry":
					AddOrUpdateOpinion(boat.Manager, 1);
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "true");
					break;
				case "NotPickedSorryAgain":
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "true");
					break;
				case "NotPickedSkillIncorrect":
					AddOrUpdateOpinion(boat.Manager, -1);
					break;
				case "NotPickedFiredYes":
					AddOrUpdateOpinion(boat.Manager, -10);
					boat.RetireCrew(this);
					foreach (CrewMember cm in boat.GetAllCrewMembers())
					{
						cm.AddOrUpdateOpinion(boat.Manager, -2);
						cm.SaveStatus();
					}
					break;
				case "NotPickedFiredNo":
					AddOrUpdateOpinion(boat.Manager, -10);
					break;
				case "NotPickedSkillTrain":
					AddOrUpdateOpinion(boat.Manager, 2);
					for (int i = 0; i < 2; i++)
					{
						int randomStat = rand.Next(0, Skills.Count);
						Skills[(CrewMemberSkill)randomStat]++;
						if (Skills[(CrewMemberSkill)randomStat] > 10)
						{
							Skills[(CrewMemberSkill)randomStat] = 10;
						}
						UpdateSingleBelief(string.Format(NPCBeliefs.Skill.GetDescription(), (CrewMemberSkill)randomStat), Skills[(CrewMemberSkill)randomStat].ToString());
					}
					break;
				case "NotPickedSkillFriends":
					AddOrUpdateOpinion(boat.Manager, 1);
					List<CrewMember> allCrew = boat.GetAllCrewMembers();
					allCrew.Remove(this);
					for (int i = 0; i < 2; i++)
					{
						int randomCrew = rand.Next(0, allCrew.Count);
						CrewMember cm = allCrew[randomCrew];
						AddOrUpdateOpinion(cm, 2);
						cm.AddOrUpdateOpinion(this, 2);
						cm.SaveStatus();
					}
					break;
				case "NotPickedSkillNothing":
					AddOrUpdateOpinion(boat.Manager, -10);
					break;
				case "NotPickedSkillLeave":
					AddOrUpdateOpinion(boat.Manager, -10);
					boat.RetireCrew(this);
					foreach (CrewMember cm in boat.GetAllCrewMembers())
					{
						cm.AddOrUpdateOpinion(boat.Manager, -1);
						cm.SaveStatus();
					}
					break;
			}
			if (eventRpc != null)
			{
				RolePlayCharacter.ActionFinished(eventRpc);
			}
			TickUpdate();
			SaveStatus();
			boat.GetIdealCrew();
			boat.UpdateBoatScore();
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
	}
}
 
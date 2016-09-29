using System;
using System.Collections.Generic;
using System.Linq;
using GAIPS.Rage;
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
		public CrewMember(IStorageProvider savedStorage, RolePlayCharacterAsset rpc, ConfigStore config) : base(savedStorage, rpc)
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
				UpdateSingleBelief(string.Format(NPCBeliefs.Skill.GetDescription(), skill), Skills[skill].ToString(), "SELF");
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
				CreateInitialOpinion(random, boat.Manager);
			}
		}

		public void CreateInitialOpinion(Random random, Person person)
		{
			AddOrUpdateOpinion(person, random.Next((int)_config.ConfigValues[ConfigKeys.DefaultOpinionMin.ToString()], (int)_config.ConfigValues[ConfigKeys.DefaultOpinionMax.ToString()] + 1));
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
			UpdateSingleBelief(string.Format(NPCBeliefs.Opinion.GetDescription(), cw.Person.Name.Replace(" ", "")), cw.Opinion.ToString(), "SELF");
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
			UpdateSingleBelief(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), cw.Person.Name.Replace(" ", "")), cw.Opinion.ToString(), "SELF");
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
					Skills[skill] = int.Parse(EmotionalAppraisal.GetBeliefValue(string.Format(NPCBeliefs.Skill.GetDescription(), skill)));
				} else
				{
					Skills.Add(skill, 0);
				}
				if (RevealedSkills.ContainsKey(skill))
				{
					if (EmotionalAppraisal.BeliefExists(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), skill)))
					{
						RevealedSkills[skill] = int.Parse(EmotionalAppraisal.GetBeliefValue(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), skill)));
					}
				}
				else {
					RevealedSkills.Add(skill, 0);
				}
			}
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				if (EmotionalAppraisal.BeliefExists(string.Format(NPCBeliefs.Opinion.GetDescription(), member.Name.Replace(" ", ""))))
				{
					AddOrUpdateOpinion(member, int.Parse(EmotionalAppraisal.GetBeliefValue(string.Format(NPCBeliefs.Opinion.GetDescription(), member.Name.Replace(" ", "")))), true);
				}
			}
			if (EmotionalAppraisal.BeliefExists(string.Format(NPCBeliefs.Opinion.GetDescription(), boat.Manager.Name.Replace(" ", ""))))
			{
				AddOrUpdateOpinion(boat.Manager, int.Parse(EmotionalAppraisal.GetBeliefValue(string.Format(NPCBeliefs.Opinion.GetDescription(), boat.Manager.Name.Replace(" ", "")))), true);
			}
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				if (EmotionalAppraisal.BeliefExists(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), member.Name.Replace(" ", ""))))
				{
					AddOrUpdateRevealedOpinion(member, int.Parse(EmotionalAppraisal.GetBeliefValue(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), member.Name.Replace(" ", "")))));
				}
			}
			if (EmotionalAppraisal.BeliefExists(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), boat.Manager.Name.Replace(" ", ""))))
			{
				AddOrUpdateRevealedOpinion(boat.Manager, int.Parse(EmotionalAppraisal.GetBeliefValue(string.Format(NPCBeliefs.RevealedOpinion.GetDescription(), boat.Manager.Name.Replace(" ", "")))));
			}
			if (EmotionalAppraisal.BeliefExists(NPCBeliefs.Rest.GetDescription()))
			{
				RestCount = int.Parse(EmotionalAppraisal.GetBeliefValue(NPCBeliefs.Rest.GetDescription()));
			}
		}

		/// <summary>
		/// Get the saved last position for this CrewMember
		/// </summary>
		public void LoadPosition(Boat boat)
		{
			if (EmotionalAppraisal.GetBeliefValue(NPCBeliefs.Position.GetDescription()) != "null")
			{
				var boatPosition = boat.BoatPositions.Where(bp => bp.Position.Name == EmotionalAppraisal.GetBeliefValue(NPCBeliefs.Position.GetDescription()));
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
			if (EmotionalAppraisal != null)
			{
				mood = (int)Math.Round(EmotionalAppraisal.Mood);
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
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			if (EmotionalAppraisal.BeliefExists(NPCBeliefs.ExpectedSelection.GetDescription()))
			{
				if (EmotionalAppraisal.GetBeliefValue(NPCBeliefs.ExpectedSelection.GetDescription()).ToLower() == "true")
				{
					if (GetBoatPosition(boat) == null)
					{
						AddOrUpdateOpinion(boat.Manager, -3);
						UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "false", "SELF");
						var eventString = "PostRace(NotPickedAfterSorry)";
						var eventRpc = RolePlayCharacter.PerceptionActionLoop(new [] { string.Format(eventBase, eventString, spacelessName) });
						EmotionalAppraisal.AppraiseEvents(new [] { string.Format(eventBase, eventString, spacelessName) });
						if (eventRpc != null)
						{
							RolePlayCharacter.ActionFinished(eventRpc);
						}
					}
					else
					{
						EmotionalAppraisal.RemoveBelief(NPCBeliefs.ExpectedSelection.GetDescription(), spacelessName.ToString());
					}
					TickUpdate();
				}
			}
			/*var currentPosition = boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this);
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
			UpdateSingleBelief(NPCBeliefs.Rest.GetDescription(), RestCount.ToString(), "SELF");
		}

		/// <summary>
		/// Send an event to the EA/RPC to get CrewMember information
		/// </summary>
		public string SendMeetingEvent(IntegratedAuthoringToolAsset iat, string style, Boat boat)
		{
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = string.Format("SoloInterview({0})", style);
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new [] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new [] { string.Format(eventBase, eventString, spacelessName) });
			string reply = null;
			if (eventRpc != null)
			{
				var eventKey = eventRpc.ActionName.ToString();
				Random rand = new Random();
				List<DialogueStateActionDTO> dialogueOptions = new List<DialogueStateActionDTO>();
				switch (eventKey)
				{
					case "StatReveal":
						int randomStat = rand.Next(0, Skills.Count);
						string statName = ((CrewMemberSkill)randomStat).ToString();
						int statValue = Skills[(CrewMemberSkill)randomStat];
						RevealedSkills[(CrewMemberSkill)randomStat] = statValue;
						if (statValue <= (int)_config.ConfigValues[ConfigKeys.BadSkillRating.ToString()])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Bad").ToList();
						}
						else if (statValue >= (int)_config.ConfigValues[ConfigKeys.GoodSkillRating.ToString()])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Good").ToList();
						} else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Middle").ToList();
						}
						if (dialogueOptions.Any())
						{
							reply = string.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, statName.ToLower());
						}
						UpdateSingleBelief(string.Format(NPCBeliefs.RevealedSkill.GetDescription(), statName), statValue.ToString(), "SELF");
						break;
					case "RoleReveal":
						Position pos = boat.BoatPositions[rand.Next(0, boat.BoatPositions.Count)].Position;
						if (pos.GetPositionRating(this) <= 5)
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Bad").ToList();
						} else if (pos.GetPositionRating(this) >= 6)
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Good").ToList();
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
						if (pickedOpinionPositive != null) {
							if (pickedOpinionPositive.Opinion >= (int)_config.ConfigValues[ConfigKeys.OpinionStrongLike.ToString()])
							{
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "High").ToList();
							}
							else
							{
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey).ToList();
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
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "None").ToList();
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
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "High").ToList();
							}
							else
							{
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey).ToList();
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
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "None").ToList();
							if (dialogueOptions.Any())
							{
								reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
							}
						}
						break;
				}
				RolePlayCharacter.ActionFinished(eventRpc);
			}
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
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "StrongAgree").ToList();
			}
			else if (Skills[skill] >= 7)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Agree").ToList();
			}
			else if (Skills[skill] >= 5)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Neither").ToList();
			}
			else if (Skills[skill] >= 3)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Disagree").ToList();
			}
			else
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "StrongDisagree").ToList();
			}
			return dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
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
					if (EmotionalAppraisal.BeliefExists(NPCBeliefs.ExpectedSelection.GetDescription()))
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
			List<DialogueStateActionDTO> dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, nextState).ToList();
			if (dialogueOptions.Any())
			{
				DialogueStateActionDTO selectedNext = dialogueOptions.OrderBy(o => Guid.NewGuid()).First();
				iat.SetDialogueState("Player", selectedNext.NextState);
				reply = selectedNext.Utterance;
				if (selectedNext.NextState == "-")
				{
					PostRaceFeedback(nextState, boat);
				}
			} else
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
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = string.Format("PostRace({0})", lastEvent);
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new [] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new [] { string.Format(eventBase, eventString, spacelessName) });
			Random rand = new Random();
			switch (lastEvent)
			{
				case "NotPickedSorry":
					AddOrUpdateOpinion(boat.Manager, 1);
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "true", "SELF");
					break;
				case "NotPickedSorryAgain":
					UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "true", "SELF");
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
						UpdateSingleBelief(string.Format(NPCBeliefs.Skill.GetDescription(), (CrewMemberSkill)randomStat), Skills[(CrewMemberSkill)randomStat].ToString(), "SELF");
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
			UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), "Retired", "SELF");
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,Status(Retired),{0})";
			EmotionalAppraisal.AppraiseEvents(new [] { string.Format(eventBase, spacelessName) });
			Avatar = new Avatar(this, false, true);
			SaveStatus();
		}
	}
}
 
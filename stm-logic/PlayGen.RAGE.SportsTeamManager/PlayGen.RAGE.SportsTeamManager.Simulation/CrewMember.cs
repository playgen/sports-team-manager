using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	//Decision Feedback functionality to adjust opinions/mood based on placement currently commented out
	public class CrewMember : Person
	{
		public Dictionary<CrewMemberSkill, int> Skills { get; set; }
		public Dictionary<CrewMemberSkill, int> RevealedSkills { get; set; }
		public List<CrewOpinion> CrewOpinions { get; set; }
		public List<CrewOpinion> RevealedCrewOpinions { get; set; }
		public event EventHandler OpinionChange = delegate { };
		public int restCount { get; set; }
		private ConfigStore _config { get; set; }

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
		/// Constructor for creating a CrewMember with a random age/gender/name
		/// </summary>
		public CrewMember(Random random, ConfigStore config)
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
		/// Randomly selected the gender of the CrewMember
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
		/// Randomly seect a name for this CrewMember
		/// </summary>
		private string SelectRandomName(string gender, Random random)
		{
			var name = "";
			if (gender == "Male")
			{
				var names = new string[]{"Oliver", "Jack", "Harry", "Jacob", "Charlie", "Thomas", "George", "Oscar", "James", "William", "Noah", "Alfie", "Joshua", "Muhammad", "Henry", "Leo", "Archie", "Ethan", "Joseph", "Freddie", "Samuel", "Alexander", "Logan", "Daniel", "Isaac", "Max", "Mohammed", "Benjamin", "Mason", "Lucas", "Edward", "Harrison", "Jake", "Dylan", "Riley", "Finley", "Theo", "Sebastian", "Adam", "Zachary", "Arthur", "Toby", "Jayden", "Luke", "Harley", "Lewis", "Tyler", "Harvey", "Matthew", "David", "Reuben", "Michael", "Elijah", "Kian", "Tommy", "Mohammad", "Blake", "Luca", "Theodore", "Stanley", "Jenson", "Nathan", "Charles", "Frankie", "Jude", "Teddy", "Louie", "Louis", "Ryan", "Hugo", "Bobby", "Elliott", "Dexter", "Ollie", "Alex", "Liam", "Kai", "Gabriel", "Connor", "Aaron", "Frederick", "Callum", "Elliot", "Albert", "Leon", "Ronnie", "Rory", "Jamie", "Austin", "Seth", "Ibrahim", "Owen", "Caleb", "Ellis", "Sonny", "Robert", "Joey", "Felix", "Finlay", "Jackson" };
				name += names[random.Next(0, names.Length)];
			}
			else if (gender == "Female")
			{
				var names = new string[] {"Amelia", "Olivia", "Isla", "Emily", "Poppy", "Ava", "Isabella", "Jessica", "Lily", "Sophie", "Grace", "Sophia", "Mia", "Evie", "Ruby", "Ella", "Scarlett", "Isabelle", "Chloe", "Sienna", "Freya", "Phoebe", "Charlotte", "Daisy", "Alice", "Florence", "Eva", "Sofia", "Millie", "Lucy", "Evelyn", "Elsie", "Rosie", "Imogen", "Lola", "Matilda", "Elizabeth", "Layla", "Holly", "Lilly", "Molly", "Erin", "Ellie", "Maisie", "Maya", "Abigail", "Eliza", "Georgia", "Jasmine", "Esme", "Willow", "Bella", "Annabelle", "Ivy", "Amber", "Emilia", "Emma", "Summer", "Hannah", "Eleanor", "Harriet", "Rose", "Amelie", "Lexi", "Megan", "Gracie", "Zara", "Lacey", "Martha", "Anna", "Violet", "Darcey", "Maria", "Maryam", "Brooke", "Aisha", "Katie", "Leah", "Thea", "Darcie", "Hollie", "Amy", "Mollie", "Heidi", "Lottie", "Bethany", "Francesca", "Faith", "Harper", "Nancy", "Beatrice", "Isabel", "Darcy", "Lydia", "Sarah", "Sara", "Julia", "Victoria", "Zoe", "Robyn" };
				name += names[random.Next(0, names.Length)];
			}
			name += " ";
			var surnames = new string[] {"Smith", "Jones", "Williams", "Taylor", "Brown", "Davies", "Evans", "Thomas", "Wilson", "Johnson", "Roberts", "Robinson", "Thompson", "Wright", "Walker", "White", "Edwards", "Hughes", "Green", "Hall", "Lewis", "Harris", "Clarke", "Patel", "Jackson", "Wood", "Turner", "Martin", "Cooper", "Hill", "Morris", "Ward", "Moore", "Clark", "Baker", "Harrison", "King", "Morgan", "Lee", "Allen", "James", "Phillips", "Scott", "Watson", "Davis", "Parker", "Bennett", "Price", "Griffiths", "Young"};
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
				UpdateSingleBelief(String.Format(NPCBeliefs.Skill.GetDescription(), skill), Skills[skill].ToString(), "SELF");
			}
			foreach (CrewOpinion co in CrewOpinions)
			{
				UpdateSingleBelief(String.Format(NPCBeliefs.Opinion.GetDescription(), co.Person.Name.Replace(" ", "")), co.Opinion.ToString(), "SELF");
			}
			foreach (CrewOpinion co in RevealedCrewOpinions)
			{
				UpdateSingleBelief(String.Format(NPCBeliefs.RevealedOpinion.GetDescription(), co.Person.Name.Replace(" ", "")), co.Opinion.ToString(), "SELF");
			}
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
			UpdateBeliefs();
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
			UpdateBeliefs();
		}

		/// <summary>
		/// Get the saved stats, position, opinions and retirement status for this CrewMember
		/// </summary>
		public void LoadBeliefs(Boat boat)
		{
			foreach (CrewMemberSkill skill in Enum.GetValues(typeof(CrewMemberSkill)))
			{
				if (Skills.ContainsKey(skill))
				{
					Skills[skill] = int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format(NPCBeliefs.Skill.GetDescription(), skill)));
				} else
				{
					Skills.Add(skill, 0);
				}
				if (RevealedSkills.ContainsKey(skill))
				{
					if (EmotionalAppraisal.BeliefExists(String.Format(NPCBeliefs.RevealedSkill.GetDescription(), skill)))
					{
						RevealedSkills[skill] = int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format(NPCBeliefs.RevealedSkill.GetDescription(), skill)));
					}
				}
				else {
					RevealedSkills.Add(skill, 0);
				}
			}
			if (EmotionalAppraisal.GetBeliefValue(NPCBeliefs.Position.GetDescription()) != "null")
			{
				var boatPosition = boat.BoatPositions.SingleOrDefault(bp => bp.Position.Name == EmotionalAppraisal.GetBeliefValue(NPCBeliefs.Position.GetDescription()));
				if (boatPosition != null)
				{
					boat.AssignCrew(boatPosition, this);
				}
			}
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				if (EmotionalAppraisal.BeliefExists(String.Format(NPCBeliefs.Opinion.GetDescription(), member.Name.Replace(" ", ""))))
				{
					AddOrUpdateOpinion(member, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format(NPCBeliefs.Opinion.GetDescription(), member.Name.Replace(" ", "")))), true);
				}
			}
			if (EmotionalAppraisal.BeliefExists(String.Format(NPCBeliefs.Opinion.GetDescription(), boat.Manager.Name.Replace(" ", ""))))
			{
				AddOrUpdateOpinion(boat.Manager, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format(NPCBeliefs.Opinion.GetDescription(), boat.Manager.Name.Replace(" ", "")))), true);
			}
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				if (EmotionalAppraisal.BeliefExists(String.Format(NPCBeliefs.RevealedOpinion.GetDescription(), member.Name.Replace(" ", ""))))
				{
					AddOrUpdateRevealedOpinion(member, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format(NPCBeliefs.RevealedOpinion.GetDescription(), member.Name.Replace(" ", "")))));
				}
			}
			if (EmotionalAppraisal.BeliefExists(String.Format(NPCBeliefs.RevealedOpinion.GetDescription(), boat.Manager.Name.Replace(" ", ""))))
			{
				AddOrUpdateRevealedOpinion(boat.Manager, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format(NPCBeliefs.RevealedOpinion.GetDescription(), boat.Manager.Name.Replace(" ", "")))));
			}
			if (EmotionalAppraisal.BeliefExists(NPCBeliefs.Rest.GetDescription()))
			{
				restCount = int.Parse(EmotionalAppraisal.GetBeliefValue(NPCBeliefs.Rest.GetDescription()));
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
		public string GetPosition(Boat boat)
		{
			string position = "";
			var currentPosition = boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this);
			if (currentPosition != null)
			{
				position = currentPosition.Position.Name;
			}
			return position;
		}

		/// <summary>
		/// Pass events to see what this CrewMember thinks of the current crew line-up and save these and any other changes
		/// </summary>
		public void DecisionFeedback(Boat boat)
		{
			SaveStatus();
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			if (EmotionalAppraisal.BeliefExists(NPCBeliefs.ExpectedSelection.GetDescription()))
			{
				if (EmotionalAppraisal.GetBeliefValue(NPCBeliefs.ExpectedSelection.GetDescription()).ToLower() == "true")
				{
					if (boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this) == null)
					{
						AddOrUpdateOpinion(boat.Manager, -3);
						UpdateSingleBelief(NPCBeliefs.ExpectedSelection.GetDescription(), "false", "SELF");
						var eventString = "PostRace(NotPickedAfterSorry)";
						var eventRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
						EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
						if (eventRpc != null)
						{
							RolePlayCharacter.ActionFinished(eventRpc);
						}
						EmotionalAppraisal.Update();
						RolePlayCharacter.Update();
					}
					else
					{
						EmotionalAppraisal.RemoveBelief(NPCBeliefs.ExpectedSelection.GetDescription(), spacelessName.ToString());
					}
				}
			}
			/*var currentPosition = boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this);
			int positionScore = currentPosition != null ? currentPosition.Position.GetPositionRating(this) : 0;
			var eventString = String.Format("PositionRating({0})", positionScore);
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
			EmotionalAppraisal.Update();
			RolePlayCharacter.Update();

			eventString = String.Format("ManagerOpinionCheck({0})", boat.Manager.Name.Replace(" ", ""));
			var managerOpinionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			if (managerOpinionRpc != null)
			{
				RolePlayCharacter.ActionFinished(managerOpinionRpc);
			}
			EmotionalAppraisal.Update();
			RolePlayCharacter.Update();

			foreach (BoatPosition boatPosition in boat.BoatPositions.OrderBy(b => b.Position.Name))
			{
				if (boatPosition.CrewMember != null && boatPosition.CrewMember != this)
				{
					int possiblePositionScore = boatPosition.Position.GetPositionRating(this);
					eventString = String.Format("OpinionCheck({0},{1},{2})", boatPosition.CrewMember.Name.Replace(" ", ""), possiblePositionScore, positionScore);
					if (positionScore == 0)
					{
						int theirPositionScore = boatPosition.Position.GetPositionRating(boatPosition.CrewMember);
						eventString = String.Format("OpinionCheck({0},{1},{2})", boatPosition.CrewMember.Name.Replace(" ", ""), possiblePositionScore, theirPositionScore);
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
					EmotionalAppraisal.Update();
					RolePlayCharacter.Update();
				}
			}*/
			SaveStatus();
			LoadBeliefs(boat);
		}

		public void RaceRest(bool assigned)
		{
			restCount--;
			if (assigned)
			{
				restCount = (int)_config.ConfigValues[ConfigKeys.PostRaceRest.ToString()];
			}
			UpdateSingleBelief(NPCBeliefs.Rest.GetDescription(), restCount.ToString(), "SELF");
			SaveStatus();
		}

		/// <summary>
		/// Send an event to the EA/EDM to get the CrewMember's reaction and mood change as a result
		/// </summary>
		public string SendMeetingEvent(IntegratedAuthoringToolAsset iat, string state, string style, Boat boat)
		{
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = String.Format("{0}({1})", state, style);
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			string reply = null;
			if (eventRpc != null)
			{
				var eventKey = eventRpc.ActionName.ToString();
				Random rand = new Random();
				IEnumerable<DialogueStateActionDTO> dialogueOptions = Enumerable.Empty<DialogueStateActionDTO>();
				switch (eventKey)
				{
					case "StatReveal":
						int randomStat = rand.Next(0, Skills.Count);
						string statName = ((CrewMemberSkill)randomStat).ToString();
						int statValue = Skills[(CrewMemberSkill)randomStat];
						RevealedSkills[(CrewMemberSkill)randomStat] = statValue;
						if (statValue <= (int)_config.ConfigValues[ConfigKeys.BadSkillRating.ToString()])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Bad");
						}
						else if (statValue >= (int)_config.ConfigValues[ConfigKeys.GoodSkillRating.ToString()])
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Good");
						} else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Middle");
						}
						if (dialogueOptions != null && dialogueOptions.Count() > 0)
						{
							reply = String.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, statName.ToLower());
						}
						UpdateSingleBelief(String.Format(NPCBeliefs.RevealedSkill.GetDescription(), statName), statValue.ToString(), "SELF");
						break;
					case "RoleReveal":
						Position pos = boat.BoatPositions[rand.Next(0, boat.BoatPositions.Count)].Position;
						if (pos.GetPositionRating(this) <= 5)
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Bad");
						} else if (pos.GetPositionRating(this) >= 6)
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Good");
						}
						if (dialogueOptions != null && dialogueOptions.Count() > 0)
						{
							reply = String.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pos.Name);
						}
						break;
					case "OpinionRevealPositive":
						var opinionsPositive = CrewOpinions.Where(co => co.Opinion >= (int)_config.ConfigValues[ConfigKeys.OpinionLike.ToString()]);
						var pickedOpinionPositive = opinionsPositive.OrderBy(o => Guid.NewGuid()).FirstOrDefault();
						if (pickedOpinionPositive != null) {
							if (pickedOpinionPositive.Opinion >= (int)_config.ConfigValues[ConfigKeys.OpinionStrongLike.ToString()])
							{
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "High");
							}
							else
							{
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey);
							}
							if (dialogueOptions != null && dialogueOptions.Count() > 0)
							{
								reply = String.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, pickedOpinionPositive.Person.Name);
							}
							AddOrUpdateRevealedOpinion(pickedOpinionPositive.Person, pickedOpinionPositive.Opinion);
						}
						else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "None");
							if (dialogueOptions != null && dialogueOptions.Count() > 0)
							{
								reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
							}
						}
						break;
					case "OpinionRevealNegative":
						var opinionsNegative = CrewOpinions.Where(co => co.Opinion <= (int)_config.ConfigValues[ConfigKeys.OpinionDislike.ToString()]);
						var pickedOpinionNegative = opinionsNegative.OrderBy(o => Guid.NewGuid()).FirstOrDefault();
						if (pickedOpinionNegative != null)
						{
							if (pickedOpinionNegative.Opinion <= (int)_config.ConfigValues[ConfigKeys.OpinionStrongDislike.ToString()])
							{
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "High");
							}
							else
							{
								dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey);
							}
							if (dialogueOptions != null && dialogueOptions.Count() > 0)
							{
								reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
							}
							AddOrUpdateRevealedOpinion(pickedOpinionNegative.Person, pickedOpinionNegative.Opinion);
						}
						else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "None");
							if (dialogueOptions != null && dialogueOptions.Count() > 0)
							{
								reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
							}
						}
						break;
				}
				RolePlayCharacter.ActionFinished(eventRpc);
			}
			EmotionalAppraisal.Update();
			RolePlayCharacter.Update();
			SaveStatus();
			LoadBeliefs(boat);
			return reply;
		}

		public string SendRecruitEvent(IntegratedAuthoringToolAsset iat, CrewMemberSkill skill)
		{
			string reply = null;
			IEnumerable<DialogueStateActionDTO> dialogueOptions = Enumerable.Empty<DialogueStateActionDTO>();
			if (Skills[skill] >= 9)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "StrongAgree");
			}
			else if (Skills[skill] >= 7)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Agree");
			}
			else if (Skills[skill] >= 5)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Neither");
			}
			else if (Skills[skill] >= 3)
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "Disagree");
			}
			else
			{
				dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, "StrongDisagree");
			}
			reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
			return reply;
		}

		public string SendPostRaceEvent(IntegratedAuthoringToolAsset iat, DialogueStateActionDTO selected, Boat boat)
		{
			IEnumerable<DialogueStateActionDTO> dialogueOptions = Enumerable.Empty<DialogueStateActionDTO>();
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
					foreach (BoatPosition bp in boat.BoatPositions)
					{
						if (bp.Position.GetPositionRating(this) >= bp.PositionScore)
						{
							nextState = selected.NextState + "Incorrect";
						}
						break;
					}
					break;
			}
			dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, nextState);
			if (dialogueOptions != null && dialogueOptions.Count() > 0)
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

		void PostRaceFeedback(string lastEvent, Boat boat)
		{
			SaveStatus();
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = String.Format("PostRace({0})", lastEvent);
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
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
					}
					UpdateBeliefs();
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
			EmotionalAppraisal.Update();
			RolePlayCharacter.Update();
			SaveStatus();
			LoadBeliefs(boat);
		}

		/// <summary>
		/// Retire this CrewMember
		/// </summary>
		public void Retire()
		{
			UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), "Retired", "SELF");
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,Status(Retired),{0})";
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, spacelessName) });
			EmotionalAppraisal.Update();
			RolePlayCharacter.Update();
			SaveStatus();
		}
	}
}
 
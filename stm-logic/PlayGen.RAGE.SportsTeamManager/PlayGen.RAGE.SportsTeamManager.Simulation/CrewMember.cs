﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class CrewMember : Person
	{
		public int Body { get; set; }
		public int Charisma { get; set; }
		public int Perception { get; set; }
		public int Quickness { get; set; }
		public int Wisdom { get; set; }
		public int Willpower { get; set; }
		public List<CrewOpinion> CrewOpinions { get; set; }

		public int RevealedBody { get; set; }
		public int RevealedCharisma { get; set; }
		public int RevealedPerception { get; set; }
		public int RevealedQuickness { get; set; }
		public int RevealedWisdom { get; set; }
		public int RevealedWillpower { get; set; }
		public List<CrewOpinion> RevealedCrewOpinions { get; set; }

		public event EventHandler OpinionChange = delegate { };
		public int restCount { get; set; }

		/// <summary>
		/// Constructor for creating a CrewMember with a non-random age/gender/name
		/// </summary>
		public CrewMember()
		{
			CrewOpinions = new List<CrewOpinion>();
			RevealedCrewOpinions = new List<CrewOpinion>();
		}

		/// <summary>
		/// Constructor for creating a CrewMember with a random age/gender/name
		/// </summary>
		public CrewMember(Random random)
		{
			Gender = SelectGender(random);
			Age = random.Next(18, 45);
			Name = SelectRandomName(Gender, random);
			CrewOpinions = new List<CrewOpinion>();
			RevealedCrewOpinions = new List<CrewOpinion>();
		}

		/// <summary>
		/// Constructor for creating a CrewMember from a saved game
		/// </summary>
		public CrewMember(IStorageProvider savedStorage, RolePlayCharacterAsset rpc) : base(savedStorage, rpc)
		{
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
			UpdateSingleBelief("Value(Body)", Body.ToString(), "SELF");
			UpdateSingleBelief("Value(Charisma)", Charisma.ToString(), "SELF");
			UpdateSingleBelief("Value(Perception)", Perception.ToString(), "SELF");
			UpdateSingleBelief("Value(Quickness)", Quickness.ToString(), "SELF");
			UpdateSingleBelief("Value(Willpower)", Willpower.ToString(), "SELF");
			UpdateSingleBelief("Value(Wisdom)", Wisdom.ToString(), "SELF");
			foreach (CrewOpinion co in CrewOpinions)
			{
				UpdateSingleBelief(String.Format("Opinion({0})", co.Person.Name.Replace(" ", "")), co.Opinion.ToString(), "SELF");
			}
			foreach (CrewOpinion co in RevealedCrewOpinions)
			{
				UpdateSingleBelief(String.Format("RevealedOpinion({0})", co.Person.Name.Replace(" ", "")), co.Opinion.ToString(), "SELF");
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
			Body = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Body)"));
			Charisma = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Charisma)"));
			Perception = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Perception)"));
			Quickness = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Quickness)"));
			Wisdom = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Wisdom)"));
			Willpower = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Willpower)"));
			RevealedBody = EmotionalAppraisal.BeliefExists("RevealedValue(Body)") ? int.Parse(EmotionalAppraisal.GetBeliefValue("RevealedValue(Body)")) : 0;
			RevealedCharisma = EmotionalAppraisal.BeliefExists("RevealedValue(Charisma)") ? int.Parse(EmotionalAppraisal.GetBeliefValue("RevealedValue(Charisma)")) : 0;
			RevealedPerception = EmotionalAppraisal.BeliefExists("RevealedValue(Perception)") ? int.Parse(EmotionalAppraisal.GetBeliefValue("RevealedValue(Perception)")) : 0;
			RevealedQuickness = EmotionalAppraisal.BeliefExists("RevealedValue(Quickness)") ? int.Parse(EmotionalAppraisal.GetBeliefValue("RevealedValue(Quickness)")) : 0;
			RevealedWisdom = EmotionalAppraisal.BeliefExists("RevealedValue(Wisdom)") ? int.Parse(EmotionalAppraisal.GetBeliefValue("RevealedValue(Wisdom)")) : 0;
			RevealedWillpower = EmotionalAppraisal.BeliefExists("RevealedValue(Willpower)") ? int.Parse(EmotionalAppraisal.GetBeliefValue("RevealedValue(Willpower)")) : 0;
			if (EmotionalAppraisal.GetBeliefValue("Value(Position)") != "null")
			{
				var boatPosition = boat.BoatPositions.SingleOrDefault(bp => bp.Position.Name == EmotionalAppraisal.GetBeliefValue("Value(Position)"));
				if (boatPosition != null)
				{
					boat.AssignCrew(boatPosition, this);
				}
			}
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				if (EmotionalAppraisal.BeliefExists(String.Format("Opinion({0})", member.Name.Replace(" ", ""))))
				{
					AddOrUpdateOpinion(member, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format("Opinion({0})", member.Name.Replace(" ", "")))), true);
				}
			}
			if (EmotionalAppraisal.BeliefExists(String.Format("Opinion({0})", boat.Manager.Name.Replace(" ", ""))))
			{
				AddOrUpdateOpinion(boat.Manager, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format("Opinion({0})", boat.Manager.Name.Replace(" ", "")))), true);
			}
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				if (EmotionalAppraisal.BeliefExists(String.Format("RevealedOpinion({0})", member.Name.Replace(" ", ""))))
				{
					AddOrUpdateRevealedOpinion(member, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format("RevealedOpinion({0})", member.Name.Replace(" ", "")))));
				}
			}
			if (EmotionalAppraisal.BeliefExists(String.Format("RevealedOpinion({0})", boat.Manager.Name.Replace(" ", ""))))
			{
				AddOrUpdateRevealedOpinion(boat.Manager, int.Parse(EmotionalAppraisal.GetBeliefValue(String.Format("RevealedOpinion({0})", boat.Manager.Name.Replace(" ", "")))));
			}
			if (EmotionalAppraisal.BeliefExists(String.Format("Race(Rest)")))
			{
				restCount = int.Parse(EmotionalAppraisal.GetBeliefValue("Race(Rest)"));
			}
			if (EmotionalAppraisal.BeliefExists(String.Format("Status(Retired)")))
			{
				if (EmotionalAppraisal.GetBeliefValue("Status(Retired)").ToLower() == "true")
				{
					boat.RetireCrew(this);
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
			var currentPosition = boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this);
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
			}
			SaveStatus();
			LoadBeliefs(boat);
		}

		public void RaceRest(bool assigned)
		{
			restCount--;
			if (assigned)
			{
				restCount = 1;
			}
			UpdateSingleBelief("Race(Rest)", restCount.ToString(), "SELF");
			SaveStatus();
		}

		/// <summary>
		/// Send an event to the EA/EDM to get the CrewMember's reaction and mood change as a result
		/// </summary>
		public string SendEvent(IntegratedAuthoringToolAsset iat, string state, string style, Boat boat)
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
						int randomStat = rand.Next(0, 6);
						string statName = "";
						int statValue = 0;
						switch (randomStat)
						{
							case 0:
								statName = "Body";
								statValue = Body;
								RevealedBody = Body;
								break;
							case 1:
								statName = "Charisma";
								statValue = Charisma;
								RevealedCharisma = Charisma;
								break;
							case 2:
								statName = "Perception";
								statValue = Perception;
								RevealedPerception = Perception;
								break;
							case 3:
								statName = "Quickness";
								statValue = Quickness;
								RevealedQuickness = Quickness;
								break;
							case 4:
								statName = "Willpower";
								statValue = Willpower;
								RevealedWillpower = Willpower;
								break;
							case 5:
								statName = "Wisdom";
								statValue = Wisdom;
								RevealedWisdom = Wisdom;
								break;
						}
						if (statValue <= 3)
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Bad");
						}
						else if (statValue >= 8)
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Good");
						} else
						{
							dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey + "Middle");
						}
						if (dialogueOptions != null && dialogueOptions.Count() > 0)
						{
							reply = String.Format(dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance, statName);
						}
						UpdateSingleBelief("RevealedValue(" + statName + ")", statValue.ToString(), "SELF");
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
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey);
						if (dialogueOptions != null && dialogueOptions.Count() > 0)
						{
							reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
						}
						break;
					case "OpinionRevealNegative":
						dialogueOptions = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey);
						if (dialogueOptions != null && dialogueOptions.Count() > 0)
						{
							reply = dialogueOptions.OrderBy(o => Guid.NewGuid()).First().Utterance;
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

		/// <summary>
		/// Retire this CrewMember
		/// </summary>
		public void Retire()
		{
			UpdateSingleBelief("Status(Retired)", "True", "SELF");
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,Status(Retired),{0})";
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, spacelessName) });
			SaveStatus();
		}
	}
}
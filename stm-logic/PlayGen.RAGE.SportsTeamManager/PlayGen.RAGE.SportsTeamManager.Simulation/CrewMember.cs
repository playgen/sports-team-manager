using System;
using System.Collections.Generic;
using System.Linq;
using AutobiographicMemory.DTOs;
using GAIPS.Rage;
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
		public List<CrewOpinion> CrewOpinions { get; set; } = new List<CrewOpinion>();
		public event EventHandler OpinionChange = delegate { };

		public CrewMember()
		{

		}

		public CrewMember(IStorageProvider savedStorage, RolePlayCharacterAsset rpc) : base(savedStorage, rpc)
		{

		}

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
				UpdateSingleBelief($"Opinion({co.Person.Name.Replace(" ", "")})", co.Opinion.ToString(), "SELF");
			}
		}

		public void AddOrUpdateOpinion(Person person, int change, bool replace = false)
		{
			var cw = CrewOpinions.SingleOrDefault(op => op.Person == person);
			if (cw != null)
			{
				if (replace)
				{
					cw.Opinion = change;
				} else
				{
					cw.Opinion += change;
				}
			} else
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

		public void LoadBeliefs(Boat boat, IStorageProvider savedStorage)
		{
			Body = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Body)"));
			Charisma = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Charisma)"));
			Perception = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Perception)"));
			Quickness = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Quickness)"));
			Wisdom = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Wisdom)"));
			Willpower = int.Parse(EmotionalAppraisal.GetBeliefValue("Value(Willpower)"));
			if (EmotionalAppraisal.GetBeliefValue("Value(Position)") != "null")
			{
				var boatPosition = boat.BoatPositions.SingleOrDefault(bp => bp.Position.Name == EmotionalAppraisal.GetBeliefValue("Value(Position)"));
				if (boatPosition != null)
				{
					boat.AssignCrew(boatPosition, this);
				}
			}
			foreach (CrewMember otherMember in boat.UnassignedCrew)
			{
				if (EmotionalAppraisal.BeliefExists($"Opinion({otherMember.Name.Replace(" ", "")})"))
				{
					AddOrUpdateOpinion(otherMember, int.Parse(EmotionalAppraisal.GetBeliefValue($"Opinion({otherMember.Name.Replace(" ", "")})")), true);
				}
			}
			foreach (BoatPosition position in boat.BoatPositions)
			{
				if (position.CrewMember != null && EmotionalAppraisal.BeliefExists($"Opinion({position.CrewMember.Name.Replace(" ", "")})"))
				{
					AddOrUpdateOpinion(position.CrewMember, int.Parse(EmotionalAppraisal.GetBeliefValue($"Opinion({position.CrewMember.Name.Replace(" ", "")})")), true);
				}
			}
			if (EmotionalAppraisal.BeliefExists($"Opinion({boat.Manager.Name.Replace(" ", "")})"))
			{
				AddOrUpdateOpinion(boat.Manager, int.Parse(EmotionalAppraisal.GetBeliefValue($"Opinion({boat.Manager.Name.Replace(" ", "")})")), true);
			}
		}

		public int GetMood()
		{
			int mood = 0;
			if (EmotionalAppraisal != null)
			{
				mood = (int)Math.Round(EmotionalAppraisal.Mood);
			}
			return mood;
		}

		public void DecisionFeedback(Boat boat)
		{
			SaveStatus();
			
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var currentPosition = boat.BoatPositions.SingleOrDefault(bp => bp.CrewMember == this);
			int positionScore = currentPosition != null ? currentPosition.Position.GetPositionRating(this) : 0;
			var eventString = $"PositionRating({positionScore})";
			var positionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			RolePlayCharacter.Update();
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
						AddOrUpdateOpinion(boat.Manager, -2);
						break;
				}
				RolePlayCharacter.ActionFinished(positionRpc);
			}
			

			var managerOpinion = CrewOpinions.SingleOrDefault(op => op.Person == boat.Manager);
			var managerOpinionRating = managerOpinion != null ? managerOpinion.Opinion : 0;
			eventString = $"OpinionCheck({managerOpinionRating})";
			var managerOpinionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName)});
			RolePlayCharacter.Update();
			if (managerOpinionRpc != null)
			{
				RolePlayCharacter.ActionFinished(managerOpinionRpc);
			}

			foreach (BoatPosition boatPosition in boat.BoatPositions)
			{
				if (boatPosition.CrewMember != null && boatPosition.CrewMember != this)
				{
					var opinion = CrewOpinions.SingleOrDefault(op => op.Person == boatPosition.CrewMember);
					var opinionRating = opinion != null ? opinion.Opinion : 0;
					int possiblePositionScore = boatPosition.Position.GetPositionRating(this);
					eventString = $"OpinionCheck({opinionRating},{possiblePositionScore},{positionScore})";
					if (positionScore == 0)
					{
						int theirPositionScore = boatPosition.Position.GetPositionRating(boatPosition.CrewMember);
						eventString = $"OpinionCheck({opinionRating},{possiblePositionScore},{theirPositionScore})";
					}
					var opinionRpc = RolePlayCharacter.PerceptionActionLoop(new [] { string.Format(eventBase, eventString, spacelessName)});
					RolePlayCharacter.Update();
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
				}
			}
			//EmotionalAppraisal.Mood = RolePlayCharacter.Mood;
			var events = EmotionalAppraisal.EventRecords;
			foreach (var ev in events)
			{
				EmotionalAppraisal.AddEventRecord(ev);
			}
			
			SaveStatus();
			LoadBeliefs(boat, LocalStorageProvider.Instance);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using AutobiographicMemory.DTOs;

using EmotionalAppraisal;

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

		public void LoadBeliefs(Boat boat)
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
			foreach (CrewMember member in boat.GetAllCrewMembers())
			{
				if (EmotionalAppraisal.BeliefExists($"Opinion({member.Name.Replace(" ", "")})"))
				{
					AddOrUpdateOpinion(member, int.Parse(EmotionalAppraisal.GetBeliefValue($"Opinion({member.Name.Replace(" ", "")})")), true);
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
						AddOrUpdateOpinion(boat.Manager, -2);
						break;
				}
				RolePlayCharacter.ActionFinished(positionRpc);
			}

			eventString = $"ManagerOpinionCheck({boat.Manager.Name.Replace(" ", "")})";
			var managerOpinionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName)});
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			if (managerOpinionRpc != null)
			{
				RolePlayCharacter.ActionFinished(managerOpinionRpc);
			}

			foreach (BoatPosition boatPosition in boat.BoatPositions.OrderBy(b => b.Position.Name))
			{
				if (boatPosition.CrewMember != null && boatPosition.CrewMember != this)
				{
					int possiblePositionScore = boatPosition.Position.GetPositionRating(this);
					eventString = $"OpinionCheck({boatPosition.CrewMember.Name.Replace(" ", "")},{possiblePositionScore},{positionScore})";
					if (positionScore == 0)
					{
						int theirPositionScore = boatPosition.Position.GetPositionRating(boatPosition.CrewMember);
						eventString = $"OpinionCheck({boatPosition.CrewMember.Name.Replace(" ", "")},{possiblePositionScore},{theirPositionScore})";
					}
					var opinionRpc = RolePlayCharacter.PerceptionActionLoop(new [] { string.Format(eventBase, eventString, spacelessName)});
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
				}
			}
			SaveStatus();
			LoadBeliefs(boat);
		}

		public string SendEvent(IntegratedAuthoringToolAsset iat, DialogueStateActionDTO selected, Boat boat, CrewMember crewMember = null)
		{
			var spacelessName = EmotionalAppraisal.Perspective;
			var eventBase = "Event(Action-Start,Player,{0},{1})";
			var eventString = $"{selected.CurrentState}({selected.Style})";
			var eventRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { string.Format(eventBase, eventString, spacelessName) });
			EmotionalAppraisal.AppraiseEvents(new string[] { string.Format(eventBase, eventString, spacelessName) });
			string reply = null;
			if (eventRpc != null)
			{
				var eventKey = eventRpc.ActionName.ToString();
				var options = iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, eventKey);
				if (options != null && options.Count() > 0)
				{
					reply = RolePlayCharacter.CharacterName + ": " + options.OrderBy(o => Guid.NewGuid()).First().Utterance;
				}
				switch (eventKey)
				{
					
				}
				RolePlayCharacter.ActionFinished(eventRpc);
			}
			SaveStatus();
			LoadBeliefs(boat);
			return reply;
		}
	}
}

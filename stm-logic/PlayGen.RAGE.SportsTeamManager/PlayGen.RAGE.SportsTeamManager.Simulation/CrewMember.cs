﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmotionalAppraisal.DTOs;

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

		public void LoadBeliefs(Boat boat, IStorageProvider savedStorage, RolePlayCharacterAsset rpc)
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
			if (RolePlayCharacter != null)
			{
				var rpc = RolePlayCharacterAsset.LoadFromFile(LocalStorageProvider.Instance, RolePlayCharacter.AssetFilePath);
				mood = (int)Math.Round(rpc.Mood);
			}
			return mood;
		}

		public override void SaveStatus(Boat boat)
		{
			base.SaveStatus();
			var actionRpc = RolePlayCharacter.PerceptionActionLoop(new string[] { "Event(Action,Player,UpdateCrew,*)" }).FirstOrDefault();
			RolePlayCharacter.Update();
			if (actionRpc != null)
			{
				var actionKey = actionRpc.ActionName.ToString();
				LoadBeliefs(boat, LocalStorageProvider.Instance,RolePlayCharacter);
			}
		}
	}
}

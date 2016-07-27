using EmotionalAppraisal;
using EmotionalAppraisal.DTOs;
using EmotionalDecisionMaking;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class DataLoader
	{
		public static void NewGame(IStorageProvider storagePorvider, string storageLocation, Boat boat, List<CrewMember> newCrew, Person manager)
		{
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(templateStorage, "template_iat");
			iat.ScenarioName = boat.Name;

			var templateRpc = RolePlayCharacterAsset.LoadFromFile(templateStorage, "template_rpc");
			var ea = EmotionalAppraisalAsset.LoadFromFile(templateStorage, templateRpc.EmotionalAppraisalAssetSource);
			var edm = EmotionalDecisionMakingAsset.LoadFromFile(templateStorage, templateRpc.EmotionalDecisionMakingSource);

			foreach (CrewMember member in newCrew)
			{
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Age)",
					Value = member.Age.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Gender)",
					Value = member.Gender.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Body)",
					Value = member.Body.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Charisma)",
					Value = member.Charisma.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Perception)",
					Value = member.Perception.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Quickness)",
					Value = member.Quickness.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Willpower)",
					Value = member.Willpower.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Wisdom)",
					Value = member.Wisdom.ToString(),
					Perspective = "SELF"
				});
				ea.AddOrUpdateBelief(new BeliefDTO
				{
					Name = "Value(Position)",
					Value = "null",
					Perspective = "SELF"
				});
				templateRpc.CharacterName = member.Name;
				var noSpaceName = templateRpc.CharacterName.Replace(" ", "");
				ea.SetPerspective("NPC" + noSpaceName);
				ea.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceName + ".ea"));
				edm.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceName + ".edm"));
				templateRpc.EmotionalAppraisalAssetSource = Path.Combine(storageLocation, noSpaceName + ".ea");
				templateRpc.EmotionalDecisionMakingSource = Path.Combine(storageLocation, noSpaceName + ".edm");
				templateRpc.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceName + ".rpc"));
				iat.AddCharacter(templateRpc);
				member.RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, noSpaceName + ".rpc"));
				templateRpc = RolePlayCharacterAsset.LoadFromFile(templateStorage, "template_rpc");
				ea = EmotionalAppraisalAsset.LoadFromFile(templateStorage, templateRpc.EmotionalAppraisalAssetSource);
				edm = EmotionalDecisionMakingAsset.LoadFromFile(templateStorage, templateRpc.EmotionalDecisionMakingSource);
			}

			ea.AddOrUpdateBelief(new BeliefDTO
			{
				Name = "Value(Age)",
				Value = manager.Age.ToString(),
				Perspective = "SELF"
			});
			ea.AddOrUpdateBelief(new BeliefDTO
			{
				Name = "Value(Gender)",
				Value = manager.Gender.ToString(),
				Perspective = "SELF"
			});
			ea.AddOrUpdateBelief(new BeliefDTO
			{
				Name = "Value(Position)",
				Value = "Manager",
				Perspective = "SELF"
			});
			ea.AddOrUpdateBelief(new BeliefDTO
			{
				Name = "Value(BoatType)",
				Value = boat.GetType().ToString(),
				Perspective = "SELF"
			});
			templateRpc.CharacterName = manager.Name;
			var noSpaceManagerName = templateRpc.CharacterName.Replace(" ", "");
			ea.SetPerspective("NPC" + noSpaceManagerName);
			ea.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceManagerName + ".ea"));
			edm.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceManagerName + ".edm"));
			templateRpc.EmotionalAppraisalAssetSource = Path.Combine(storageLocation, noSpaceManagerName + ".ea");
			templateRpc.EmotionalDecisionMakingSource = Path.Combine(storageLocation, noSpaceManagerName + ".edm");
			templateRpc.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceManagerName + ".rpc"));
			iat.AddCharacter(templateRpc);
			manager.RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, noSpaceManagerName + ".rpc"));

			var noSpaceBoatName = boat.Name.Replace(" ", "");
			iat.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceBoatName + ".iat"));
		}

		public static Boat LoadBoat(IStorageProvider storagePorvider, string storageLocation, string boatName)
		{
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, boatName.Replace(" ", "") + ".iat"));
			var rpcList = iat.GetAllCharacters();

			Boat boat = new Boat
			{
				Name = iat.ScenarioName
			};

			foreach (RolePlayCharacterAsset rpc in rpcList)
			{
				var ea = EmotionalAppraisalAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, rpc.EmotionalAppraisalAssetSource));
				var edm = EmotionalDecisionMakingAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, rpc.EmotionalDecisionMakingSource));
				int age;
				int.TryParse(ea.GetBeliefValue("Value(Age)"), out age);
				Person person = new Person
				{
					Name = rpc.CharacterName,
					Age = age,
					Gender = ea.GetBeliefValue("Value(Gender)"),
					RolePlayCharacter = rpc
				};
				string position = ea.GetBeliefValue("Value(Position)");
				if (position == "Manager")
				{
					boat.Manager = person;
					//get boat positions
					break;
				}
				else
				{
					CrewMember crewMember = new CrewMember
					{
						Name = person.Name,
						Age = person.Age,
						Gender = person.Gender,
						RolePlayCharacter = person.RolePlayCharacter,
						Body = int.Parse(ea.GetBeliefValue("Value(Body)")),
						Charisma = int.Parse(ea.GetBeliefValue("Value(Charisma)")),
						Perception = int.Parse(ea.GetBeliefValue("Value(Perception)")),
						Quickness = int.Parse(ea.GetBeliefValue("Value(Quickness)")),
						Wisdom = int.Parse(ea.GetBeliefValue("Value(Wisdom)")),
						Willpower = int.Parse(ea.GetBeliefValue("Value(Willpower)"))
					};
					boat.AddCrew(crewMember);
				}
			}

			foreach (CrewMember crewMember in boat.UnassignedCrew)
			{
				var eaSource = crewMember.RolePlayCharacter.EmotionalAppraisalAssetSource;
				var ea = EmotionalAppraisalAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, eaSource));
				if (ea.GetBeliefValue("Value(Position)") != "null")
				{
					var boatPosition = boat.BoatPositions.SingleOrDefault(bp => bp.Position.Name == ea.GetBeliefValue("Value(Position)"));
					if (boatPosition != null)
					{
						boat.AssignCrew(boatPosition, crewMember);
					}
				}
			}

			return boat;
		}
	}
}

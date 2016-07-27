using EmotionalAppraisal;
using EmotionalAppraisal.DTOs;
using EmotionalDecisionMaking;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using System.Collections.Generic;
using System.IO;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class DataLoader
	{
		public static void NewGame(IStorageProvider storagePorvider, string storageLocation, string boatName, List<CrewMember> newCrew, Person manager)
		{
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(templateStorage, "template_iat");
			iat.ScenarioName = boatName;

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
				templateRpc.CharacterName = member.Name;
				var noSpaceName = templateRpc.CharacterName.Replace(" ", "");
				ea.SetPerspective("NPC" + noSpaceName);
				ea.SaveToFile(storagePorvider, Path.Combine(storageLocation, member.Name + ".ea"));
				edm.SaveToFile(storagePorvider, Path.Combine(storageLocation, member.Name + ".edm"));
				templateRpc.EmotionalAppraisalAssetSource = Path.Combine(storageLocation, member.Name + ".ea");
				templateRpc.EmotionalDecisionMakingSource = Path.Combine(storageLocation, member.Name + ".edm");
				templateRpc.SaveToFile(storagePorvider, Path.Combine(storageLocation, member.Name + ".rpc"));
				iat.AddCharacter(templateRpc);
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
			templateRpc.CharacterName = manager.Name;
			var noSpaceManagerName = templateRpc.CharacterName.Replace(" ", "");
			ea.SetPerspective("NPC" + noSpaceManagerName);
			ea.SaveToFile(storagePorvider, Path.Combine(storageLocation, manager.Name + ".ea"));
			edm.SaveToFile(storagePorvider, Path.Combine(storageLocation, manager.Name + ".edm"));
			templateRpc.EmotionalAppraisalAssetSource = Path.Combine(storageLocation, manager.Name + ".ea");
			templateRpc.EmotionalDecisionMakingSource = Path.Combine(storageLocation, manager.Name + ".edm");
			templateRpc.SaveToFile(storagePorvider, Path.Combine(storageLocation, manager.Name + ".rpc"));

			var noSpaceBoatName = boatName.Replace(" ", "");
			iat.SaveToFile(storagePorvider, Path.Combine(storageLocation, boatName + ".iat"));
		}
	}
}

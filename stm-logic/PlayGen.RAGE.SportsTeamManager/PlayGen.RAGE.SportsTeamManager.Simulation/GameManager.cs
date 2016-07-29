using EmotionalAppraisal;
using EmotionalAppraisal.DTOs;
using EmotionalDecisionMaking;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class GameManager
	{
		public void NewGame(IStorageProvider storagePorvider, string storageLocation, Boat boat, List<CrewMember> newCrew, Person manager)
		{
			TemplateStorageProvider templateStorage = new TemplateStorageProvider();
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(templateStorage, "template_iat");
			iat.ScenarioName = boat.Name;

			foreach (CrewMember member in newCrew)
			{
				member.CreateFile(iat, templateStorage, storagePorvider, storageLocation);
				boat.AddCrew(member);
				member.UpdateBeliefs("null");
				member.SaveStatus();
			}
			manager.CreateFile(iat, templateStorage, storagePorvider, storageLocation);
			manager.UpdateBeliefs("Manager");
			manager.UpdateSingleBelief("Value(BoatType)", boat.GetType().ToString(), "SELF");
			boat.Manager = manager;
			manager.SaveStatus();

			var noSpaceBoatName = boat.Name.Replace(" ", "");
			iat.SaveToFile(storagePorvider, Path.Combine(storageLocation, noSpaceBoatName + ".iat"));
			//boat.ConfirmChanges();
		}

		public Boat LoadGame(IStorageProvider storagePorvider, string storageLocation, string boatName)
		{
			var iat = IntegratedAuthoringToolAsset.LoadFromFile(storagePorvider, Path.Combine(storageLocation, boatName.Replace(" ", "") + ".iat"));
			var rpcList = iat.GetAllCharacters();

			Boat boat = new Boat();
			List<CrewMember> crewList = new List<CrewMember>();

			foreach (RolePlayCharacterAsset rpc in rpcList)
			{
				var tempea = EmotionalAppraisalAsset.LoadFromFile(storagePorvider, rpc.EmotionalAppraisalAssetSource);
				string position = tempea.GetBeliefValue("Value(Position)");
				if (position == "Manager")
				{
					Person person = new Person(storagePorvider, rpc);
					boat = (Boat)Activator.CreateInstance(Type.GetType(person.EmotionalAppraisal.GetBeliefValue("Value(BoatType)")));
					boat.Name = iat.ScenarioName;
					boat.Manager = person;
					continue;
				}
				CrewMember crewMember = new CrewMember(storagePorvider, rpc);
				crewList.Add(crewMember);
			}

			crewList.ForEach(cm => boat.AddCrew(cm));
			crewList.ForEach(cm => cm.LoadBeliefs(boat, storagePorvider));

			return boat;
		}
	}
}

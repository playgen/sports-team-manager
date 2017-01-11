using System;
using System.IO;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

using RolePlayCharacter;

using WellFormedNames;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	/// <summary>
	/// Stores base details of a person (aka, the manager) or crew member and functionality to create and update storytelling framework files
	/// </summary>
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public string Nationality { get; set; }
		public RolePlayCharacterAsset RolePlayCharacter { get; private set; }

		/// <summary>
		/// Constructor for creating a Person
		/// </summary>
		public Person(RolePlayCharacterAsset rpc)
		{
			if (rpc != null)
			{
				RolePlayCharacter = rpc;
				RolePlayCharacter.Initialize();
				Name = RolePlayCharacter.BodyName;
				Age = Convert.ToInt32(LoadBelief(NPCBeliefs.Age.GetDescription()));
				Gender = LoadBelief(NPCBeliefs.Gender.GetDescription());
			}
		}

		/// <summary>
		/// Create the required files for this Person
		/// </summary>
		public void CreateFile(IntegratedAuthoringToolAsset iat, string storageLocation, string fileName = "")
		{
			//Get Storytelling Framework files
			var rpc = ConfigStore.RolePlayCharacter;
			var ea = ConfigStore.EmotionalAppraisal;
			var edm = ConfigStore.EmotionalDecisionMaking;
			var si = ConfigStore.SocialImportance;
			//set values
			rpc.BodyName = Name;
			var noSpaceName = rpc.BodyName.Replace(" ", "");
			rpc.CharacterName = (Name)noSpaceName;
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = noSpaceName;
			}
			//save files
			ea.SaveConfigurationToFile(Path.Combine(storageLocation, fileName + ".ea"));
			edm.SaveConfigurationToFile(Path.Combine(storageLocation, fileName + ".edm"));
			si.SaveConfigurationToFile(Path.Combine(storageLocation, fileName + ".si"));
			//add character to iat asset
			rpc.SaveConfigurationToFile(Path.Combine(storageLocation, fileName + ".rpc"));
			//assign asset files to RPC
			rpc.EmotionalAppraisalAssetSource = fileName + ".ea";
			rpc.EmotionalDecisionMakingSource = fileName + ".edm";
			rpc.SocialImportanceAssetSource = fileName + ".si";
			rpc.SaveConfigurationToFile(Path.Combine(storageLocation, fileName + ".rpc"));
			iat.AddNewCharacterSource(new CharacterSourceDTO { Name = rpc.BodyName, Source = fileName + ".rpc" });
			//store RPC locally
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(Path.Combine(storageLocation, fileName + ".rpc"));
			RolePlayCharacter.Initialize();
		}

		/// <summary>
		/// Update the base information for this Person
		/// </summary>
		public virtual void UpdateBeliefs(string position = null)
		{
			if (Age != 0)
			{
				UpdateSingleBelief(NPCBeliefs.Age.GetDescription(), Age.ToString());
			}
			if (Gender != null)
			{
				UpdateSingleBelief(NPCBeliefs.Gender.GetDescription(), Gender);
			}
			UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), position);
		}

		/// <summary>
		/// Update the stored information to match what is passed here or add if it doesn't already exist
		/// </summary>
		public void UpdateSingleBelief(string name, string value)
		{
			RolePlayCharacter.AddBelief(name, value);
		}

		/// <summary>
		/// Loaded stored information if it already exists
		/// </summary>
		public string LoadBelief(string belief)
		{
			return RolePlayCharacter.GetBeliefValue(belief);
		}

		/// <summary>
		/// Save the Person's mood, emotions and events to the EmotionalAppraisal file
		/// </summary>
		public void SaveStatus()
		{
			RolePlayCharacter.SaveConfigurationToFile();
		}

		/// <summary>
		/// Tick RolePlayCharacter asset amount passed through
		/// </summary>
		public void TickUpdate(int amount = 1)
		{
			for (var i = 0; i < amount; i++)
			{
				RolePlayCharacter.Update();
			}
			SaveStatus();
		}
	}
}

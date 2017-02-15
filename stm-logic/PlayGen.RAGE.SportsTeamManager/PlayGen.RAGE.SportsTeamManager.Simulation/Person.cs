using System;
using System.IO;
using System.Linq;

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
		internal RolePlayCharacterAsset RolePlayCharacter { get; private set; }

		/// <summary>
		/// Constructor for creating a Person
		/// </summary>
		internal Person(RolePlayCharacterAsset rpc)
		{
			if (rpc != null)
			{
				RolePlayCharacter = rpc;
				Name = RolePlayCharacter.BodyName;
				Age = Convert.ToInt32(LoadBelief(NPCBeliefs.Age.GetDescription()));
				Gender = LoadBelief(NPCBeliefs.Gender.GetDescription());
			}
		}

		/// <summary>
		/// Create the required files for this Person
		/// </summary>
		internal void CreateFile(IntegratedAuthoringToolAsset iat, string storageLocation, string fileName = "")
		{
			//Get Storytelling Framework files
			var rpc = ConfigStore.RolePlayCharacter;
			var ea = ConfigStore.EmotionalAppraisal;
			var edm = ConfigStore.EmotionalDecisionMaking;
			var si = ConfigStore.SocialImportance;
			//set values
			rpc.BodyName = Name;
			var noSpaceName = rpc.BodyName.Replace(" ", string.Empty);
			rpc.CharacterName = (Name)noSpaceName;
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = noSpaceName;
			}
			//save files
			ea.SaveToFile(Path.Combine(storageLocation, fileName + ".ea"));
			edm.SaveToFile(Path.Combine(storageLocation, fileName + ".edm"));
			si.SaveToFile(Path.Combine(storageLocation, fileName + ".si"));
			//add character to iat asset
			rpc.SaveToFile(Path.Combine(storageLocation, fileName + ".rpc"));
			//assign asset files to RPC
			rpc.EmotionalAppraisalAssetSource = fileName + ".ea";
			rpc.EmotionalDecisionMakingSource = fileName + ".edm";
			rpc.SocialImportanceAssetSource = fileName + ".si";
			rpc.SaveToFile(Path.Combine(storageLocation, fileName + ".rpc"));
			iat.AddNewCharacterSource(new CharacterSourceDTO { Source = rpc.AssetFilePath });
			//store RPC locally
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(rpc.AssetFilePath);
			RolePlayCharacter.LoadAssociatedAssets();
		}

		/// <summary>
		/// Update the base information for this Person
		/// </summary>
		internal virtual void UpdateBeliefs(string position = null)
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
		internal void UpdateSingleBelief(string name, string value)
		{
			var belief = EventHelper.PropertyChanged(name, value, Name.NoSpaces());
			RolePlayCharacter.Perceive(new[] { belief });
			RolePlayCharacter.ForgetEvent(RolePlayCharacter.EventRecords.Last().Id);
		}

		/// <summary>
		/// Loaded stored information if it already exists
		/// </summary>
		internal string LoadBelief(string belief)
		{
			return RolePlayCharacter.GetBeliefValue(belief);
		}

		/// <summary>
		/// Save the Person's mood, emotions and events to the EmotionalAppraisal file
		/// </summary>
		internal void SaveStatus()
		{
			RolePlayCharacter.Save();
		}

		/// <summary>
		/// Tick RolePlayCharacter asset amount passed through
		/// </summary>
		internal void TickUpdate(int amount = 1)
		{
			for (var i = 0; i < amount; i++)
			{
				RolePlayCharacter.Update();
			}
			SaveStatus();
		}
	}
}

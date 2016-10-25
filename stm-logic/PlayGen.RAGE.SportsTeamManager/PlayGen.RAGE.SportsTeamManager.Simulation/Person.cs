using System;
using System.IO;
using AssetManagerPackage;
using IntegratedAuthoringTool;
using RolePlayCharacter;

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
				AssetManager.Instance.Bridge = new BaseBridge();
				RolePlayCharacter = rpc;
				Name = rpc.CharacterName;
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
			AssetManager.Instance.Bridge = new BaseBridge();
			var rpc = ConfigStore.RolePlayCharacter;
			var ea = ConfigStore.EmotionalAppraisal;
			var edm = ConfigStore.EmotionalDecisionMaking;
			var si = ConfigStore.SocialImportance;
			//set values
			rpc.CharacterName = Name;
			var noSpaceName = rpc.CharacterName.Replace(" ", "");
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = noSpaceName;
			}
			ea.SetPerspective(string.Format("NPC{0}", noSpaceName));
			//save files
			ea.SaveToFile(Path.Combine(storageLocation, fileName + ".ea"));
			edm.SaveToFile(Path.Combine(storageLocation, fileName + ".edm"));
			si.SaveToFile(Path.Combine(storageLocation, fileName + ".si"));
			//add character to iat asset
			rpc.SaveToFile(Path.Combine(storageLocation, fileName + ".rpc"));
			iat.AddCharacter(rpc);
			//assign asset files to RPC
			rpc.EmotionalAppraisalAssetSource = fileName + ".ea";
			rpc.EmotionalDecisionMakingSource = fileName + ".edm";
			rpc.SocialImportanceAssetSource = fileName + ".si";
			rpc.SaveToFile(Path.Combine(storageLocation, fileName + ".rpc"));
			//store RPC locally
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(Path.Combine(storageLocation, fileName + ".rpc"));
		}

		/// <summary>
		/// Update the base information for this Person
		/// </summary>
		public virtual void UpdateBeliefs(string position = null)
		{
			UpdateSingleBelief(NPCBeliefs.Age.GetDescription(), Age.ToString());
			UpdateSingleBelief(NPCBeliefs.Gender.GetDescription(), Gender);
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
			RolePlayCharacter.SaveEmotionalAppraisalAsset();
		}

		/// <summary>
		/// Tick EmotionalAppraisal asset amount passed through
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

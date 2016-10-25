using System;
using System.IO;
using AssetManagerPackage;
using EmotionalAppraisal;
using EmotionalAppraisal.DTOs;

using EmotionalDecisionMaking;
using IntegratedAuthoringTool;

using RolePlayCharacter;
using SocialImportance;

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
		public EmotionalAppraisalAsset EmotionalAppraisal { get; private set; }
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
				EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(rpc.EmotionalAppraisalAssetSource);
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
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(Path.Combine(storageLocation, fileName + ".ea"));
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
			/*if (RolePlayCharacter != null && name != null && value != null)
			{
				var eventBase = "Event(Property-Change,{0},{1},{2})";
				var eventRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, RolePlayCharacter.Perspective, name, value) });
				if (eventRpc != null)
				{
					RolePlayCharacter.ActionFinished(eventRpc);
				}
			}*/
			EmotionalAppraisal.AddOrUpdateBelief(new BeliefDTO
			{
				Name = name,
				Value = value,
				Perspective = "SELF"
			});
		}

		/// <summary>
		/// Loaded stored information if it already exists
		/// </summary>
		public string LoadBelief(string belief)
		{
			/*if (ea.BeliefExists(belief))
			{
				return ea.GetBeliefValue(belief);
			}*/
			return EmotionalAppraisal.BeliefExists(belief) ? EmotionalAppraisal.GetBeliefValue(belief) : null;
		}

		/// <summary>
		/// Save the Person's mood, emotions and events to the EmotionalAppraisal file
		/// </summary>
		public void SaveStatus()
		{
			AssetManager.Instance.Bridge = new BaseBridge();
			EmotionalAppraisal.SaveToFile(EmotionalAppraisal.AssetFilePath);
			try
			{
				RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(RolePlayCharacter.AssetFilePath);
			}
			catch
			{
				var filePath = EmotionalAppraisal.AssetFilePath.Replace(Name.Replace(" ", "") + ".ea", "");
				var rpc = ConfigStore.RolePlayCharacter;
				var edm = ConfigStore.EmotionalDecisionMaking;
				var si = ConfigStore.SocialImportance;
				si.BindEmotionalAppraisalAsset(EmotionalAppraisal);
				rpc.CharacterName = Name;
				var noSpaceName = rpc.CharacterName.Replace(" ", "");
				AssetManager.Instance.Bridge = new BaseBridge();
				edm.SaveToFile(Path.Combine(filePath, noSpaceName + ".edm"));
				si.SaveToFile(Path.Combine(filePath, noSpaceName + ".si"));
				rpc.SaveToFile(Path.Combine(filePath, noSpaceName + ".rpc"));
				//assign asset files to RPC
				rpc.EmotionalAppraisalAssetSource = noSpaceName + ".ea";
				rpc.EmotionalDecisionMakingSource = noSpaceName + ".edm";
				rpc.SocialImportanceAssetSource = noSpaceName + ".si";
				rpc.SaveToFile(Path.Combine(filePath, noSpaceName + ".rpc"));
				RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(Path.Combine(filePath, noSpaceName + ".rpc"));
				RolePlayCharacter.SaveToFile(RolePlayCharacter.AssetFilePath);
			}
			//RolePlayCharacter.SaveToFile(RolePlayCharacter.AssetFilePath);
		}

		/// <summary>
		/// Tick EmotionalAppraisal asset amount passed through
		/// </summary>
		public void TickUpdate(int amount = 1)
		{
			for (var i = 0; i < amount; i++)
			{
				EmotionalAppraisal.Update();
				//RolePlayCharacter.Update();
			}
			SaveStatus();
		}
	}
}

﻿using System;
using System.IO;
using AssetManagerPackage;
using EmotionalAppraisal;
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
		public RolePlayCharacterAsset RolePlayCharacter { get; private set; }

		/// <summary>
		/// Constructor for creating a Person
		/// </summary>
		public Person()
		{
			
		}

		/// <summary>
		/// Constructor for creating a Person from a saved game
		/// </summary>
		public Person(RolePlayCharacterAsset rpc)
		{
			AssetManager.Instance.Bridge = new BaseBridge();
			RolePlayCharacter = rpc;
			Name = rpc.CharacterName;
			Age = Convert.ToInt32(LoadBelief(NPCBeliefs.Age.GetDescription()));
			Gender = LoadBelief(NPCBeliefs.Gender.GetDescription());
		}

		/// <summary>
		/// Create the required files for this Person
		/// </summary>
		public void CreateFile(IntegratedAuthoringToolAsset iat, string storageLocation, string fileName = "")
		{
			//Create Storytelling Framework files
			AssetManager.Instance.Bridge = new TemplateBridge();
			var templateRpc = RolePlayCharacterAsset.LoadFromFile("template_rpc");
			var ea = EmotionalAppraisalAsset.LoadFromFile(templateRpc.EmotionalAppraisalAssetSource);
			var edm = EmotionalDecisionMakingAsset.LoadFromFile(templateRpc.EmotionalDecisionMakingSource);
			var si = SocialImportanceAsset.LoadFromFile(templateRpc.SocialImportanceAssetSource);
			//set values
			templateRpc.CharacterName = Name;
			var noSpaceName = templateRpc.CharacterName.Replace(" ", "");
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = noSpaceName;
			}
			ea.SetPerspective("NPC" + noSpaceName);
			//save files
			AssetManager.Instance.Bridge = new BaseBridge();
			ea.SaveToFile(Path.Combine(storageLocation, fileName + ".ea"));
			edm.SaveToFile(Path.Combine(storageLocation, fileName + ".edm"));
			si.SaveToFile(Path.Combine(storageLocation, fileName + ".si"));
			//add character to iat asset
			templateRpc.SaveToFile(Path.Combine(storageLocation, fileName + ".rpc"));
			iat.AddCharacter(templateRpc);
			//assign asset files to RPC
			templateRpc.EmotionalAppraisalAssetSource = fileName + ".ea";
			templateRpc.EmotionalDecisionMakingSource = fileName + ".edm";
			templateRpc.SocialImportanceAssetSource = fileName + ".si";
			templateRpc.SaveToFile(Path.Combine(storageLocation, fileName + ".rpc"));
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
			if (RolePlayCharacter != null && name != null && value != null)
			{
				var eventBase = "Event(Property-Change,{0},{1},{2})";
				var eventRpc = RolePlayCharacter.PerceptionActionLoop(new[] { string.Format(eventBase, RolePlayCharacter.Perspective, name, value) });
				if (eventRpc != null)
				{
					RolePlayCharacter.ActionFinished(eventRpc);
				}
			}
		}

		public string LoadBelief(string belief)
		{
			var ea = EmotionalAppraisalAsset.LoadFromFile(RolePlayCharacter.EmotionalAppraisalAssetSource);
			if (ea.BeliefExists(belief))
			{
				return ea.GetBeliefValue(belief);
			}
			return null;
		}

		/// <summary>
		/// Save the Person's mood, emotions and events to the EmotionalAppraisal file
		/// </summary>
		public void SaveStatus()
		{
			AssetManager.Instance.Bridge = new BaseBridge();
			RolePlayCharacter.SaveToFile(RolePlayCharacter.AssetFilePath);
			RolePlayCharacter.SaveToFile
		}

		/// <summary>
		/// Tick EmotionalAppraisal asset amount passed through
		/// </summary>
		public void TickUpdate(int amount = 1)
		{
			for (int i = 0; i < amount; i++)
			{
				RolePlayCharacter.Update();
			}
			SaveStatus();
		}
	}
}

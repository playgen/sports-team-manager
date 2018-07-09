using System;
using System.IO;

using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;

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
		internal RolePlayCharacterAsset RolePlayCharacter { get; private set; }
		protected SocialImportanceAsset SocialImportance { get; private set; }

		/// <summary>
		/// Constructor for creating a Person
		/// </summary>
		internal Person(RolePlayCharacterAsset rpc)
		{
			if (rpc != null)
			{
				RolePlayCharacter = rpc;
				SetRelations();
				SocialImportance.RegisterKnowledgeBase(RolePlayCharacter.m_kb);
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
			RolePlayCharacter = ConfigStore.RolePlayCharacter.Copy();
			SetRelations();
			RolePlayCharacter.BodyName = Name;
			var noSpaceName = RolePlayCharacter.BodyName.Replace(" ", string.Empty);
			RolePlayCharacter.CharacterName = noSpaceName.ToName();
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = noSpaceName;
			}
			RolePlayCharacter.VoiceName = fileName;
			RolePlayCharacter.SetFutureFilePath(Path.Combine(storageLocation, fileName + ".rpc"));
			RolePlayCharacter.Save();
			//set up SI file
			iat.AddNewCharacterSourceWithoutCheck(new CharacterSourceDTO { Source = RolePlayCharacter.AssetFilePath });
		}

		internal void SetRelations()
		{
			SocialImportance = ConfigStore.SocialImportance.Copy();
			RolePlayCharacter.SetEmotionalAppraisalAsset(ConfigStore.EmotionalAppraisal.Copy());
			RolePlayCharacter.SetEmotionalDecisionMakingAsset(ConfigStore.EmotionalDecisionMaking.Copy());
			RolePlayCharacter.SetSocialImportanceAsset(SocialImportance);
		}

		/// <summary>
		/// Update the stored information to match what is passed here or add if it doesn't already exist
		/// </summary>
		internal void UpdateSingleBelief(string name, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				value = WellFormedNames.Name.NIL_STRING;
			}
			RolePlayCharacter.UpdateBelief(name, value);
		}

		/// <summary>
		/// Loaded stored information
		/// </summary>
		internal string LoadBelief(string belief)
		{
			var value = RolePlayCharacter.GetBeliefValue(belief);
			return value == WellFormedNames.Name.NIL_STRING ? null : value;
		}

		/// <summary>
		/// Save the Person's mood, emotions and events to the RPC file
		/// </summary>
		internal void SaveStatus()
		{
			SocialImportance.RegisterKnowledgeBase(RolePlayCharacter.m_kb);
			RolePlayCharacter.Save();
		}

		/// <summary>
		/// Tick RolePlayCharacter asset amount passed through
		/// </summary>
		internal void TickUpdate(int amount = 1, bool save = true)
		{
			if (amount == 0)
			{
				RolePlayCharacter.UpdateWithoutTick();
			}
			else
			{
				for (var i = 0; i < amount; i++)
				{
					RolePlayCharacter.Update();
				}
			}
			if (save)
			{
				SaveStatus();
			}
		}
	}
}

using System.IO;
using EmotionalAppraisal;
using EmotionalAppraisal.DTOs;
using EmotionalDecisionMaking;
using GAIPS.Rage;
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

		public EmotionalAppraisalAsset EmotionalAppraisal { get; private set; }
		protected RolePlayCharacterAsset RolePlayCharacter { get; private set; }

		/// <summary>
		/// Constructor for creating a Person
		/// </summary>
		public Person()
		{
			
		}

		/// <summary>
		/// Constructor for creating a Person from a saved game
		/// </summary>
		public Person(IStorageProvider savedStorage, RolePlayCharacterAsset rpc)
		{
			var ea = EmotionalAppraisalAsset.LoadFromFile(savedStorage, rpc.EmotionalAppraisalAssetSource);
			int age;
			int.TryParse(ea.GetBeliefValue(NPCBeliefs.Age.GetDescription()), out age);
			Name = rpc.CharacterName;
			Age = age;
			Gender = ea.GetBeliefValue(NPCBeliefs.Gender.GetDescription());
			EmotionalAppraisal = ea;
			RolePlayCharacter = rpc;
		}

		/// <summary>
		/// Create the required files for this Person
		/// </summary>
		public void CreateFile(IntegratedAuthoringToolAsset iat, IStorageProvider templateStorage, IStorageProvider savedStorage, string storageLocation, string fileName = "")
		{
            //Create Storytelling Framework files
			var templateRpc = RolePlayCharacterAsset.LoadFromFile(templateStorage, "template_rpc");
			var ea = EmotionalAppraisalAsset.LoadFromFile(templateStorage, templateRpc.EmotionalAppraisalAssetSource);
			var edm = EmotionalDecisionMakingAsset.LoadFromFile(templateStorage, templateRpc.EmotionalDecisionMakingSource);
			var si = SocialImportanceAsset.LoadFromFile(templateStorage, templateRpc.SocialImportanceAssetSource);
            //set values
			si.BindEmotionalAppraisalAsset(ea);
			templateRpc.CharacterName = Name;
			var noSpaceName = templateRpc.CharacterName.Replace(" ", "");
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = noSpaceName;
			}
			ea.SetPerspective("NPC" + noSpaceName);
            //save files
			ea.SaveToFile(savedStorage, Path.Combine(storageLocation, fileName + ".ea"));
			edm.SaveToFile(savedStorage, Path.Combine(storageLocation, fileName + ".edm"));
			si.SaveToFile(savedStorage, Path.Combine(storageLocation, fileName + ".si"));
            //assign asset files to RPC
			templateRpc.EmotionalAppraisalAssetSource = Path.Combine(storageLocation, fileName + ".ea");
			templateRpc.EmotionalDecisionMakingSource = Path.Combine(storageLocation, fileName + ".edm");
			templateRpc.SocialImportanceAssetSource = Path.Combine(storageLocation, fileName + ".si");
			templateRpc.SaveToFile(savedStorage, Path.Combine(storageLocation, fileName + ".rpc"));
            //add character to iat asset
			iat.AddCharacter(templateRpc);
            //store EA and RPC locally
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(savedStorage, Path.Combine(storageLocation, fileName + ".ea"));
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(savedStorage, Path.Combine(storageLocation, fileName + ".rpc"));
		}

		/// <summary>
		/// Update the base information for this Person
		/// </summary>
		public virtual void UpdateBeliefs(string position = null)
		{
			UpdateSingleBelief(NPCBeliefs.Age.GetDescription(), Age.ToString(), "SELF");
			UpdateSingleBelief(NPCBeliefs.Gender.GetDescription(), Gender, "SELF");
			UpdateSingleBelief(NPCBeliefs.Position.GetDescription(), position, "SELF");
		}

		/// <summary>
		/// Update the stored information to match what is passed here or add if it doesn't already exist
		/// </summary>
		public void UpdateSingleBelief(string name, string value, string perspective)
		{
			if (EmotionalAppraisal != null && name != null && value != null && perspective != null)
			{
				EmotionalAppraisal.AddOrUpdateBelief(new BeliefDTO
				{
					Name = name,
					Value = value,
					Perspective = perspective
				});
			}
		}

		/// <summary>
		/// Save the Person's mood, emotions and events to the EmotionalAppraisal file
		/// </summary>
		public void SaveStatus()
		{
			EmotionalAppraisal.SaveToFile(LocalStorageProvider.Instance, EmotionalAppraisal.AssetFilePath);
            try
            {
                RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(LocalStorageProvider.Instance, RolePlayCharacter.AssetFilePath);
            }
            catch
            {
                var filePath = EmotionalAppraisal.AssetFilePath.Replace(Name.Replace(" ", "") + ".ea", "");
                TemplateStorageProvider templateStorage = new TemplateStorageProvider();
                var templateRpc = RolePlayCharacterAsset.LoadFromFile(templateStorage, "template_rpc");
                var edm = EmotionalDecisionMakingAsset.LoadFromFile(templateStorage, templateRpc.EmotionalDecisionMakingSource);
                var si = SocialImportanceAsset.LoadFromFile(templateStorage, templateRpc.SocialImportanceAssetSource);
                si.BindEmotionalAppraisalAsset(EmotionalAppraisal);
                templateRpc.CharacterName = Name;
                var noSpaceName = templateRpc.CharacterName.Replace(" ", "");
                edm.SaveToFile(LocalStorageProvider.Instance, Path.Combine(filePath, noSpaceName + ".edm"));
                si.SaveToFile(LocalStorageProvider.Instance, Path.Combine(filePath, noSpaceName + ".si"));
                templateRpc.EmotionalAppraisalAssetSource = Path.Combine(filePath, noSpaceName + ".ea");
                templateRpc.EmotionalDecisionMakingSource = Path.Combine(filePath, noSpaceName + ".edm");
                templateRpc.SocialImportanceAssetSource = Path.Combine(filePath, noSpaceName + ".si");
                templateRpc.SaveToFile(LocalStorageProvider.Instance, Path.Combine(filePath, noSpaceName + ".rpc"));
                RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(LocalStorageProvider.Instance, Path.Combine(filePath, noSpaceName + ".rpc"));
            }
		}

        /// <summary>
		/// Tick EmotionalAppraisal asset amount passed through
		/// </summary>
		public void TickUpdate(int amount = 1)
		{
			for (int i = 0; i < amount; i++)
			{
				EmotionalAppraisal.Update();
			}
			SaveStatus();
		}
	}
}

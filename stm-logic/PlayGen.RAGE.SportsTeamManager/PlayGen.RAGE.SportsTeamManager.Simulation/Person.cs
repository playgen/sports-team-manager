using System.IO;

using AssetManagerPackage;

using AssetPackage;

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
		public Person(RolePlayCharacterAsset rpc)
		{
			AssetManager.Instance.Bridge = new BaseBridge();
			var ea = EmotionalAppraisalAsset.LoadFromFile(rpc.EmotionalAppraisalAssetSource);
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
		public void CreateFile(IntegratedAuthoringToolAsset iat, string storageLocation, string fileName = "")
		{
			//Create Storytelling Framework files
			AssetManager.Instance.Bridge = new TemplateBridge();
			var templateRpc = RolePlayCharacterAsset.LoadFromFile("template_rpc");
			var ea = EmotionalAppraisalAsset.LoadFromFile(templateRpc.EmotionalAppraisalAssetSource);
			var edm = EmotionalDecisionMakingAsset.LoadFromFile(templateRpc.EmotionalDecisionMakingSource);
			var si = SocialImportanceAsset.LoadFromFile(templateRpc.SocialImportanceAssetSource);
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
			//store EA and RPC locally
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(Path.Combine(storageLocation, fileName + ".ea"));
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(Path.Combine(storageLocation, fileName + ".rpc"));
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
			AssetManager.Instance.Bridge = new BaseBridge();
			EmotionalAppraisal.SaveToFile(EmotionalAppraisal.AssetFilePath);
			try
			{
				RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(RolePlayCharacter.AssetFilePath);
			}
			catch
			{
				var filePath = EmotionalAppraisal.AssetFilePath.Replace(Name.Replace(" ", "") + ".ea", "");
				AssetManager.Instance.Bridge = new TemplateBridge();
				var templateRpc = RolePlayCharacterAsset.LoadFromFile("template_rpc");
				var edm = EmotionalDecisionMakingAsset.LoadFromFile(templateRpc.EmotionalDecisionMakingSource);
				var si = SocialImportanceAsset.LoadFromFile(templateRpc.SocialImportanceAssetSource);
				si.BindEmotionalAppraisalAsset(EmotionalAppraisal);
				templateRpc.CharacterName = Name;
				var noSpaceName = templateRpc.CharacterName.Replace(" ", "");
				AssetManager.Instance.Bridge = new BaseBridge();
				edm.SaveToFile(Path.Combine(filePath, noSpaceName + ".edm"));
				si.SaveToFile(Path.Combine(filePath, noSpaceName + ".si"));
				templateRpc.EmotionalAppraisalAssetSource = Path.Combine(filePath, noSpaceName + ".ea");
				templateRpc.EmotionalDecisionMakingSource = Path.Combine(filePath, noSpaceName + ".edm");
				templateRpc.SocialImportanceAssetSource = Path.Combine(filePath, noSpaceName + ".si");
				templateRpc.SaveToFile(Path.Combine(filePath, noSpaceName + ".rpc"));
				RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(Path.Combine(filePath, noSpaceName + ".rpc"));
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

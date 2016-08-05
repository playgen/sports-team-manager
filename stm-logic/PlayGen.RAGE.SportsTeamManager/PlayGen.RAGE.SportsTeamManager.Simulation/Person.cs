using System.IO;
using EmotionalAppraisal;
using EmotionalAppraisal.DTOs;
using EmotionalDecisionMaking;
using GAIPS.Rage;
using IntegratedAuthoringTool;
using RolePlayCharacter;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
	public class Person
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }

		public EmotionalAppraisalAsset EmotionalAppraisal { get; set; }
		public RolePlayCharacterAsset RolePlayCharacter { get; set; }

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
			int.TryParse(ea.GetBeliefValue("Value(Age)"), out age);
			Name = rpc.CharacterName;
			Age = age;
			Gender = ea.GetBeliefValue("Value(Gender)");
			EmotionalAppraisal = ea;
			RolePlayCharacter = rpc;
		}

		/// <summary>
		/// Create the required files for this Person
		/// </summary>
		public void CreateFile(IntegratedAuthoringToolAsset iat, IStorageProvider templateStorage, IStorageProvider savedStorage, string storageLocation)
		{
			var templateRpc = RolePlayCharacterAsset.LoadFromFile(templateStorage, "template_rpc");
			var ea = EmotionalAppraisalAsset.LoadFromFile(templateStorage, templateRpc.EmotionalAppraisalAssetSource);
			var edm = EmotionalDecisionMakingAsset.LoadFromFile(templateStorage, templateRpc.EmotionalDecisionMakingSource);
			templateRpc.CharacterName = Name;
			var noSpaceName = templateRpc.CharacterName.Replace(" ", "");
			ea.SetPerspective("NPC" + noSpaceName);
			ea.SaveToFile(savedStorage, Path.Combine(storageLocation, noSpaceName + ".ea"));
			edm.SaveToFile(savedStorage, Path.Combine(storageLocation, noSpaceName + ".edm"));
			templateRpc.EmotionalAppraisalAssetSource = Path.Combine(storageLocation, noSpaceName + ".ea");
			templateRpc.EmotionalDecisionMakingSource = Path.Combine(storageLocation, noSpaceName + ".edm");
			templateRpc.SaveToFile(savedStorage, Path.Combine(storageLocation, noSpaceName + ".rpc"));
			iat.AddCharacter(templateRpc);
			EmotionalAppraisal = EmotionalAppraisalAsset.LoadFromFile(savedStorage, Path.Combine(storageLocation, noSpaceName + ".ea"));
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(savedStorage, Path.Combine(storageLocation, noSpaceName + ".rpc"));
		}

		/// <summary>
		/// Update the base information for this Person
		/// </summary>
		public virtual void UpdateBeliefs(string position = null)
		{
			UpdateSingleBelief("Value(Age)", Age.ToString(), "SELF");
			UpdateSingleBelief("Value(Gender)", Gender, "SELF");
			UpdateSingleBelief("Value(Position)", position, "SELF");
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
			RolePlayCharacter = RolePlayCharacterAsset.LoadFromFile(LocalStorageProvider.Instance, RolePlayCharacter.AssetFilePath);
		}
	}
}

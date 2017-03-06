using System.Linq;
using PlayGen.SUGAR.Unity;
using UnityEngine.UI;
using PlayGen.Unity.Utilities.BestFit;

public class AccountInterface : BaseAccountInterface
{
	/// <summary>
	/// Trigger DoBestFit method and add event listener for when resolution changes to trigger DoBestFit.
	/// </summary>
	private void OnEnable()
	{
		DoBestFit();
		BestFit.ResolutionChange += DoBestFit;
	}

	/// <summary>
	/// Remove event listener on disable.
	/// </summary>
	private void OnDisable()
	{
		BestFit.ResolutionChange -= DoBestFit;
	}

	/// <summary>
	/// Set the text of all the active buttons to be as big as possible and the same size.
	/// </summary>
	private void DoBestFit()
	{
		GetComponentsInChildren<Button>(true).Select(t => t.gameObject).Where(t => t.activeSelf).BestFit();
	}
}

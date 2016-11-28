using UnityEngine;
using PlayGen.SUGAR.Client;

namespace SUGAR.Unity
{
	[RequireComponent(typeof(AccountUnityClient))]
	public class SUGARUnityManager : MonoBehaviour
	{
		[SerializeField] private string _baseAddress;

		[SerializeField] private int _gameId;

		void Awake()
		{
			if (SUGARManager.Register(this))
			{
				DontDestroyOnLoad(this);
			}
			else
			{
				Destroy(gameObject);
			}
			SUGARManager.Client = new SUGARClient(_baseAddress); // hTTPhANDLER ?>?!
			SUGARManager.GameId = _gameId;
		}
	}
}
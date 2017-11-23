using UnityEngine;

[CreateAssetMenu(fileName = "Platform Settings", menuName = "Platform/Settings")]
public class PlatformSettings : ScriptableObject {

	[SerializeField]
	private bool _rage;
	[SerializeField]
	private GameObject[] _rageObjects;
	public bool Rage
	{
		get { return _rage; }
	}
	public GameObject[] RageObjects
	{
		get { return _rageObjects; }
	}
}

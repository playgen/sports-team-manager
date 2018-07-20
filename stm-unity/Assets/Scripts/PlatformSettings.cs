using UnityEngine;

[CreateAssetMenu(fileName = "Platform Settings", menuName = "Platform/Settings")]
public class PlatformSettings : ScriptableObject {

	[SerializeField]
	private bool _rage;
	[SerializeField]
	private bool _demoMode;
	[SerializeField]
	private GameObject[] _rageObjects;
	public bool Rage => _rage;
	public bool DemoMode => _demoMode;
	public GameObject[] RageObjects => _rageObjects;
}

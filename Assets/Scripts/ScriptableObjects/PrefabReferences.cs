using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

/// A scriptable object containing references to prefab assets
[CreateAssetMenu(fileName = "PrefabReferences", menuName = "ScriptableObjects/PrefabReferences", order = 2)]
public class PrefabReferences : ScriptableObjectSingleton<PrefabReferences>
{
	[SerializeField] private GameObject navmeshCutterPrefab = null;
	public static GameObject NavmeshCutterPrefab => Instance.navmeshCutterPrefab;

	[SerializeField] private GameObject manipulableParticleSystemPrefab = null;
	public static GameObject ManipulableParticleSystemPrefab => Instance.manipulableParticleSystemPrefab;
}

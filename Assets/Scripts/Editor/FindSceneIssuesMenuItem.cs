using UnityEditor;
using UnityEngine;

public class FindSceneIssuesMenuItem : MonoBehaviour
{
	/// <summary>
	/// Iterates through the open scene and finds potential issues.
	/// </summary>
	[MenuItem("Tools/Find Scene Issues", false, -9999)]
	public static void FindSceneIssues()
	{
		Manipulable[] manipulables = FindObjectsOfType<Manipulable>();

		foreach (Manipulable manipulable in manipulables)
		{
			PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(manipulable.gameObject);

			switch (prefabType)
			{
				case PrefabAssetType.NotAPrefab:
				{
					Debug.LogWarning($"[NotAPrefab] The game object '{manipulable.name}' has a manipulable component but is not attached to a prefab instance.\n"
									 + "Please make this object a prefab and place it into Assets/Prefabs/Manipulables or remove this component.", manipulable.gameObject);
					break;
				}
				case PrefabAssetType.Model:
				{
					Debug.LogWarning($"[ModelPrefab] The game object '{manipulable.name}' has a manipulable component but is attached to a model prefab instance.\n"
									 + "Please make this object a prefab and place it into Assets/Prefabs/Manipulables or remove this component.", manipulable.gameObject);
					break;
				}
				case PrefabAssetType.MissingAsset:
				{
					Debug.LogWarning($"[MissingAsset] The game object '{manipulable.name}' has a manipulable component but is attached to a broken/missing prefab instance.\n"
									 + "Please make this object a prefab and place it into Assets/Prefabs/Manipulables or remove this component.", manipulable.gameObject);
					break;
				}
				default:
				{
					string prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(manipulable.gameObject);

					if (!prefabAssetPath.Contains("Manipulables"))
					{
						Debug.LogWarning($"[NotAManipulablePrefab] The prefab instance '{manipulable.name}' has a manipulable component but is not a prefab from Assets/Prefabs/Manipulables.\n"
										 + "Please create a prefab variant and place the prefab into Assets/Prefabs/Manipulables, or remove this component.", manipulable.gameObject);
					}
					break;
				}
			}

			if (manipulable.gameObject.GetComponent<MeshRenderer>() == null)
			{
				Debug.LogWarning($"The manipulable object '{manipulable.gameObject.name}' does not have a root-level mesh renderer.\n"
								 + "Manipulable objects require a single root-level mesh renderer, please add one to the same game object as this component.", manipulable.gameObject);
			}
		}
		
		Debug.Log("Finished finding scene issues.");
	}
}

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// A collection of utility functions for Unity objects.
/// </summary>
public static class ObjectUtilities
{
	/// <summary>
	/// Returns true if this game object is the only one instantiated.
	/// </summary>
	public static bool IsUnique<T>() where T : Object
	{
		T[] objects = Object.FindObjectsOfType(typeof(T)) as T[];

		if (objects != null && objects.Length > 1)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Instantiates a singleton prefab and returns the singleton component of the
	/// newly instantiated object. Optionally allows for setting the parent of the
	/// singlton game object.
	/// </summary>
	/// <param name="prefab"> The prefab to instantiate. </param>
	/// <param name="parent"> The parent of the instantiated object. </param>
	/// <returns> The singleton component of the newly instantiated object. </returns>
	public static T InstantiateSingletonPrefab<T>(GameObject prefab, GameObject parent = null)
	{
		T[] components = Object.FindObjectsOfType(typeof(T)) as T[];
		string resourceName = typeof(T).Name;

		// If there is already a singleton, then return the exsisting component
		if (components != null && components.Length > 0)
		{
			Debug.LogWarning($"There was already a {resourceName} singleton. Returned the existing singleton.");
			return components[0];
		}

		if (prefab == null)
		{
			Debug.LogError($"Problem with instantiation, the provided {resourceName} prefab was null.");
		}

		GameObject intantiatedObject = Object.Instantiate(prefab);
		if (intantiatedObject == null)
		{
			Debug.LogError($"Problem with instantiation, the instantiated {resourceName} game object is null.");
		}

		// Custom naming that avoids "(clone)" and adds brackets to indicate a singleton
		intantiatedObject.name = $"[{resourceName}]";

		if (parent != null)
		{
			intantiatedObject.transform.parent = parent.transform;
		}

		T component = intantiatedObject.GetComponent<T>();
		if (component == null)
		{
			Debug.LogError($"Problem getting the {resourceName} component.");
		}

		return component;
	}

	/// <summary>
	/// Instantiates a singleton prefab from "Resources/" and returns the singleton
	/// component of the newly instantiated object. Optionally allows for setting the
	/// parent of the singlton game object.
	/// </summary>
	/// <param name="prefabPath"> The file path to the prefab asset. </param>
	/// <param name="parent"> The parent of the instantiated object. </param>
	/// <returns> The singleton component of the newly instantiated object. </returns>
	public static T InstantiateSingletonPrefab<T>(string prefabPath, GameObject parent = null)
	{
		GameObject loadedPrefab = Resources.Load<GameObject>(prefabPath);
		if (loadedPrefab == null)
		{
		#if UNITY_EDITOR
			Debug.LogWarning($"Problem loading the {typeof(T).Name} prefab resource from the provided path. Trying a refresh and reimport...");
			
			// Try a refresh and reimport
			EditorApplication.ExitPlaymode();
			AssetDatabase.ImportAsset("Assets/Prefabs/Systems/Managers/Resources/[GameManager].prefab");
			loadedPrefab = Resources.Load<GameObject>(prefabPath);

			if (loadedPrefab == null)
			{
				Debug.LogError($"Problem loading the {typeof(T).Name} prefab resource from the provided path.");
			}
			else
			{
				EditorApplication.EnterPlaymode();
			}
		#else
			Debug.LogError($"Problem loading the {typeof(T).Name} prefab resource from the provided path.");
		#endif
		}

		return InstantiateSingletonPrefab<T>(loadedPrefab, parent);
	}
}

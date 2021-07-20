using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public class OrganizeSceneMenuItem : MonoBehaviour
{
	/// <summary>
	/// Organizes the scene hierarchy and puts game objects under "folders" depending on what they are.
	/// </summary>
	[MenuItem("Tools/Organize Scene", false, -9997)]
	public static void OrganizeScene()
	{
		const string manipulablesRootName = "❐ - Manipulables";
		const string spawnPointsRootName = "❐ - Spawn Points";
		const string lightingRootName = "❐ - Lighting & VFX";
		const string environmentDynamicRootName = "❐ - Environment (Dynamic)";
		const string environmentStaticRootName = "❐ - Environment (Static)";
		const string triggersRootName = "❐ - Triggers";

		GameObject manipulablesRoot = null;
		GameObject spawnPointsRoot = null;
		GameObject lightingRoot = null;
		GameObject environmentDynamicRoot = null;
		GameObject environmentStaticRoot = null;
		GameObject triggersRoot = null;

		// Get all the top level game objects in the scene
		
		List<GameObject> rootObjects = new List<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());

		// Destroy folders that don't match naming and collect a list of the children under folder objects
		
		List<GameObject> folderChildren = new List<GameObject>();
		
		foreach (GameObject gameObject in rootObjects)
		{
			if (gameObject.name.Contains('❐'))
			{
				// Cache the reference to existing folders with the correct naming and mark whether or not to keep the object

				bool isKeptFolder;
				
				switch (gameObject.name)
				{
					case manipulablesRootName:
					{
						manipulablesRoot = gameObject;
						isKeptFolder = true;
						break;
					}
					case spawnPointsRootName:
					{
						spawnPointsRoot = gameObject;
						isKeptFolder = true;
						break;
					}
					case lightingRootName:
					{
						lightingRoot = gameObject;
						isKeptFolder = true;
						break;
					}
					case environmentDynamicRootName:
					{
						environmentDynamicRoot = gameObject;
						isKeptFolder = true;
						break;
					}
					case environmentStaticRootName:
					{
						environmentStaticRoot = gameObject;
						isKeptFolder = true;
						break;
					}
					case triggersRootName:
					{
						triggersRoot = gameObject;
						isKeptFolder = true;
						break;
					}
					default:
					{
						isKeptFolder = false;
						break;
					}
				}
				
				// Add the child to the list of folder children and delete the folder if it had bad naming

				if (isKeptFolder)
				{
					foreach (Transform child in gameObject.transform)
					{
						folderChildren.Add(child.gameObject);
					}
				}
				else
				{
					List<Transform> children = new List<Transform>();
				
					foreach (Transform child in gameObject.transform)
					{
						children.Add(child);
					}
				
					// Must do this on a temporary list or reparenting child transforms during iteration will silently break the iteration and skip objects
					foreach (Transform child in children)
					{
						// If the folder has bad naming we place the child in the scene root
						child.parent = null;
						child.SetAsLastSibling();
						folderChildren.Add(child.gameObject);
					}

					DestroyImmediate(gameObject);
				}
			}
		}

		// Get the new list of root objects (after bad folders were deleted) and add the children of folders
		
		List<GameObject> sortObjects = new List<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
		sortObjects.AddRange(folderChildren);

		// Create the "folder" game objects if they don't already exist

		manipulablesRoot = manipulablesRoot == null ? new GameObject(manipulablesRootName) : manipulablesRoot;
		spawnPointsRoot = spawnPointsRoot == null ? new GameObject(spawnPointsRootName): spawnPointsRoot;
		lightingRoot = lightingRoot == null ? new GameObject(lightingRootName) : lightingRoot;
		triggersRoot = triggersRoot == null ? new GameObject(triggersRootName) : triggersRoot; 
		environmentDynamicRoot = environmentDynamicRoot == null ? new GameObject(environmentDynamicRootName) : environmentDynamicRoot;
		environmentStaticRoot = environmentStaticRoot == null ? new GameObject(environmentStaticRootName) : environmentStaticRoot;
		environmentStaticRoot.layer = LayerMask.NameToLayer("Environment");
		GameObject navMesh = null;

		// Sort the scene
		
		foreach (GameObject gameObject in sortObjects)
		{
			// Sort Manipulables
			if (gameObject.GetComponent<Manipulable>() != null)
			{
				gameObject.transform.parent = manipulablesRoot.transform;
				gameObject.layer = manipulablesRoot.layer;
				gameObject.isStatic = false;
			}
			// Sort Spawn Points
			else if (gameObject.GetComponent<SpawnPoint>() || gameObject.GetComponent<CheckPoint>())
			{
				gameObject.transform.parent = spawnPointsRoot.transform;
				gameObject.layer = spawnPointsRoot.layer;
				gameObject.isStatic = false;
			}
			// Sort Lights & VFX
			else if (gameObject.GetComponent<Light>() != null || gameObject.GetComponent<Volume>() != null)
			{
				gameObject.transform.parent = lightingRoot.transform;
				gameObject.layer = lightingRoot.layer;
			}
			// Sort dynamic environment objects
			else if (gameObject.GetComponent<Rigidbody>() != null || gameObject.GetComponent<Animator>() != null)
			{
				gameObject.transform.parent = environmentDynamicRoot.transform;
				gameObject.layer = environmentDynamicRoot.layer;
				gameObject.isStatic = false;
			}
			else if (gameObject.GetComponent<KillTrigger>() != null || gameObject.GetComponent<CutSceneTrigger>() != null)
			{
				gameObject.transform.parent = triggersRoot.transform;
				gameObject.layer = triggersRoot.layer;
				gameObject.isStatic = true;
			}
			// Sort Navmesh
			else if (gameObject.GetComponent<AstarPath>() != null)
			{
				navMesh = gameObject;
			}
			// Sort Folders
			else if (gameObject.name.Contains("❐"))
			{
				// Skip folders and sort them later (this is important or they can be moved out of order as items move during sorting)
			}
			// Sort everything else under "Environment"
			else
			{
				gameObject.transform.parent = environmentStaticRoot.transform;
				gameObject.layer = environmentStaticRoot.layer;
				gameObject.isStatic = true;

				Transform[] children = gameObject.GetComponentsInChildren<Transform>();
				foreach (Transform child in children)
				{
					child.gameObject.layer = environmentStaticRoot.layer;
					child.gameObject.isStatic = true;
				}
			}
		}
		
		// Sort the folders
		lightingRoot.transform.SetSiblingIndex(0);
		environmentStaticRoot.transform.SetSiblingIndex(1);
		environmentDynamicRoot.transform.SetSiblingIndex(2);
		manipulablesRoot.transform.SetSiblingIndex(3);
		spawnPointsRoot.transform.SetSiblingIndex(4);
		triggersRoot.transform.SetSiblingIndex(5);
		if (navMesh != null) navMesh.transform.SetAsLastSibling();

		// Dirty the scene so the changes can be saved
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		
		Debug.Log("Finished organizing the scene.");
	}
}

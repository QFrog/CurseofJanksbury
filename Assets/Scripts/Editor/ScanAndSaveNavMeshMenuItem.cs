using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding;
using Pathfinding.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScanAndSaveNavMeshMenuItem : MonoBehaviour
{
	/// <summary>
	/// Scan and cache the navmesh with an optional scene organization step.
	/// </summary>
	[MenuItem("Tools/Scan and Save NavMesh", false, -9996)]
	public static void ScanAndSaveNavMesh()
	{
		AstarPath navMesh = FindObjectOfType<AstarPath>();

		if (navMesh != null)
		{
			SerializeSettings serializationSettings = new SerializeSettings();
			serializationSettings.nodes = true;

			if (EditorUtility.DisplayDialog("Organize scene before scanning?", "Objects that have not had their layer set to 'Environment' will not be scanned by the NavMesh. "
																			   + "Organizing the scene will ensure that all of the environment objects have this layer set.", "Organize", "Don't organize"))
			{
				OrganizeSceneMenuItem.OrganizeScene();
			}
			
			AstarPathEditor.MenuScan();

			// Save graphs
			byte[] bytes = navMesh.data.SerializeGraphs(serializationSettings);

			// Store it in a file
			navMesh.data.file_cachedStartup = AstarPathEditor.SaveGraphData(bytes, SceneManager.GetActiveScene().name, navMesh.data.file_cachedStartup);
			navMesh.data.cacheStartup = true;
		}
		else
		{
			Debug.LogWarning("Did not find a NavMesh in the scene to scan. Did you forget to add the NavMesh prefab to the scene?");
		}
		
		Debug.Log("Finished scanning and saving the NavMesh.");
	}
}

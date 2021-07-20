using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Replacer
{
	[MenuItem("Tools/Update LOD Materials tagged with 'Replacer' to Match Parent", false, -9994)]
	private static void Replace()
	{
		List<GameObject> taggedObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Replacer"));

		foreach (GameObject taggedObject in taggedObjects)
		{
			MeshRenderer taggedObjectMeshRenderer = taggedObject.GetComponent<MeshRenderer>();
			
			if (taggedObjectMeshRenderer != null) // Single mesh renderer on the root
			{
				Material[] parentMaterials = taggedObjectMeshRenderer.sharedMaterials;
					
				foreach (Transform lod in taggedObject.transform)
				{
					lod.gameObject.GetComponent<MeshRenderer>().sharedMaterials = parentMaterials;
				}
			}
			else // Multiple mesh renderer children
			{
				foreach (Transform transform in taggedObject.transform)
				{
					GameObject child = transform.gameObject;

					SkinnedMeshRenderer childSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
					MeshRenderer childMeshRenderer = child.GetComponent<MeshRenderer>();
					
					// Ignore children that are not renderers
					if (childSkinnedMeshRenderer == null && childMeshRenderer == null) continue;

					if (childSkinnedMeshRenderer != null)
					{
						Material[] parentMaterials = childSkinnedMeshRenderer.sharedMaterials;
					
						foreach (Transform lod in child.transform)
						{
							lod.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMaterials = parentMaterials;
						}
					}
				
					if (childMeshRenderer != null)
					{
						Material[] parentMaterials = childMeshRenderer.sharedMaterials;
					
						foreach (Transform lod in child.transform)
						{
							lod.gameObject.GetComponent<MeshRenderer>().sharedMaterials = parentMaterials;
						}
					}
				}	
			}
		}
	}
}

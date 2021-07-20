using UnityEditor;
using UnityEngine;

/// <summary>
/// Extends the right click menu in the editor hierarchy with new game object options.
/// </summary>
public static class EditorHierarchyMenuAdditions
{
	/// <summary>
	/// Adds an empty game object to be used like a folder to the hierarchy.
	/// </summary>
	[MenuItem("GameObject/Folder", false, 10)]
	private static void CreateFolderGameObject(MenuCommand menuCommand)
	{
		GameObject folderGameObject = new GameObject("❐ - New Folder");
		folderGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		
		GameObjectUtility.SetParentAndAlign(folderGameObject, menuCommand.context as GameObject);

		Undo.RegisterCreatedObjectUndo(folderGameObject, $"Create {folderGameObject.name}");

		Selection.activeObject = folderGameObject;
	}
}


using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixManipulablesMenuItem : MonoBehaviour
{
    /// <summary>
    /// Iterates through the open scene and fixes issues with manipulable objects.
    /// </summary>
    [MenuItem("Tools/Fix Manipulables", false, -9998)]
    public static void FixManipulables()
    {
        Manipulable[] manipulables = FindObjectsOfType<Manipulable>();

        foreach (Manipulable manipulable in manipulables)
        {
            Undo.RecordObject(manipulable, "Reset Manipulable Properties");
            manipulable.ResetManipulableProperties();
        }

        Debug.Log("Finished fixing manipulables.");
    }
}

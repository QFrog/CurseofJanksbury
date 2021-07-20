using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPatrol))]

public class EnemyPatrolEditor : Editor
{
		
	public override void OnInspectorGUI()
	{
		EnemyPatrol patrolScript = (EnemyPatrol)target;

		DrawDefaultInspector();

		if (patrolScript.PatrolType == EnemyPatrol.PatrolTypes.Sequential)
		{
			if (GUILayout.Button("Add Vertex"))
			{
				patrolScript.AddVert();
				EditorUtility.SetDirty(patrolScript);
			}

			if (GUILayout.Button("Remove Vertex"))
			{
				patrolScript.RemoveVert();
				EditorUtility.SetDirty(patrolScript);
			}
		}
		else if (patrolScript.PatrolType == EnemyPatrol.PatrolTypes.Random)
		{
			if (GUILayout.Button("Add Vertex"))
			{
				patrolScript.AddVert();
				EditorUtility.SetDirty(patrolScript);
			}

			if (GUILayout.Button("Remove Vertex"))
			{
				patrolScript.RemoveVert();
				EditorUtility.SetDirty(patrolScript);
			}
		}
		else if (patrolScript.PatrolType == EnemyPatrol.PatrolTypes.idle)
		{
			if (GUILayout.Button("Update Vertices"))
			{
				patrolScript.setIdle();
				EditorUtility.SetDirty(patrolScript);
			}

			//if (GUILayout.Button("Remove Vertex"))
			//{
			//	//patrolScript.RemoveVert();
			//	EditorUtility.SetDirty(patrolScript);
			//}
		}

	}
}

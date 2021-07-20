using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyPatrol : MonoBehaviour
{
	public enum PatrolTypes
	{
		Random,
		Sequential,
		idle
	}

	public PatrolTypes PatrolType => patrolType;

	public enum EnemyTypes
	{
		BaseEnemy,
		BipedYellow,
		BipedOrange,
		BipedRed,
		GuardEnemy,
		FlyEnemy,
		QuadEnemy,
		Civilian
	}

	public EnemyTypes EnemyType => enemyType;
	
	/// Chance of reversing patrol direction 0 being no chance and 1 being everytime. Only applies to sequential patrolType.
	[Range(0.0f, 1.0f)] public float directionChangeFrequency;

#pragma warning disable CS0649
	[SerializeField] private PatrolTypes patrolType;
	[SerializeField] private EnemyTypes enemyType;
#pragma warning restore CS0649

	/// List of vertices that make up a patrol path
	public List<Transform> vertices = new List<Transform>();
	/// Used to hold reference to waypoint before adding to vertices
	private GameObject toAdd;

	/// <summary>
	/// Creates a waypoint gameobject and sets it to be a child of the spawn point.
	/// </summary>
	public void AddVert()
	{
		toAdd = Instantiate(GameObject.Find("waypoint (0)"), gameObject.transform);
		toAdd.name = "waypoint (" + vertices.Count + ")";
		toAdd.transform.position = gameObject.transform.position;
		vertices.Add(toAdd.transform);
	}

	/// <summary>
	/// Removes a waypoint and deletes it from the scene.
	/// </summary>
	public void RemoveVert()
	{
		Transform toDelete = vertices[vertices.Count - 1];
		vertices.Remove(toDelete);
		DestroyImmediate(toDelete.gameObject);
	}

	public void setIdle()
	{
		for (int i = 1; i < vertices.Count; i++)
		{
			Transform toDelete = vertices[i];
			vertices.Remove(toDelete);
			DestroyImmediate(toDelete.gameObject);
		}
		vertices[0].localPosition = Vector3.zero;
	}

	private void OnDrawGizmos()
	{
		if(patrolType == PatrolTypes.Sequential)
		{
			for (int i = 0; i < vertices.Count; i++)
			{
				if ((i + 1) == vertices.Count)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(vertices[i].position, vertices[0].position);
				}
				else
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(vertices[i].position, vertices[i + 1].position);
				}
			}
		}
		
	}
}

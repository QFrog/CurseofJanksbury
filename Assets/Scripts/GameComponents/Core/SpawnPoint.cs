using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component marks a game object as a certain type of spawn point and makes
/// it easy to visualize its position in the editor.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
	[SerializeField] private CharacterCapsuleData characterCapsuleData = null;

	public enum SpawnTypes
	{
		Player,
		Enemy
	}

	public SpawnTypes SpawnType => spawnType;

#pragma warning disable CS0649
	[SerializeField] private SpawnTypes spawnType;
#pragma warning restore CS0649

	// Drawn in the editor only
	void OnDrawGizmos()
	{
		// Draw a debug arrow for the facing direction
		DebugExtension.DrawArrow(transform.position, transform.forward, Color.yellow);

		// Draw a debug capule for the character collision capsule
		Vector3 capsuleTop = transform.position + new Vector3(0, characterCapsuleData.GetRealHeight() / 2f, 0);
		Vector3 capsuleBottom = transform.position + new Vector3(0, -(characterCapsuleData.GetRealHeight() / 2f), 0);
		float capsuleRadius = characterCapsuleData.GetRealRadius();

		Color gizmoColor;
		switch (SpawnType)
		{
			case SpawnTypes.Player:
				{
					gizmoColor = Color.green;
					break;
				}
			case SpawnTypes.Enemy:
				{
					gizmoColor = Color.red;
					break;
				}
			default:
				{
					gizmoColor = Color.white;
					break;
				}
		}

		DebugExtension.DrawCapsule(capsuleTop, capsuleBottom, gizmoColor, capsuleRadius);
	}
}


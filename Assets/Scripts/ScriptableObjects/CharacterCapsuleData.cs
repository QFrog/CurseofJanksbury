using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object for the player character's character controller component public properties.
/// </summary>
[CreateAssetMenu(fileName = "CharacterCapsule", menuName = "ScriptableObjects/CharacterCapsule", order = 1)]
public class CharacterCapsuleData : ScriptableObject
{
	[SerializeField] private float skinWidth = 0.05f;
	[SerializeField] Vector3 center = Vector3.zero;
	[SerializeField] private float radius = 0.5f;
	[SerializeField] private float height = 2f;

	public float SkinWidth => skinWidth;
	public Vector3 Center => center;
	public float Radius => radius;
	public float Height => height;

	/// <summary>
	/// Get the height of the character capsule including the skin width.
	/// </summary>
	public float GetRealHeight()
	{
		return height + (skinWidth * 2f);
	}

	/// <summary>
	/// Get the radius of the character capsule including the skin width.
	/// </summary>
	public float GetRealRadius()
	{
		return radius + skinWidth;
	}

	/// <summary>
	/// Modifies the given character controller to match the data in this scriptable object.
	/// </summary>
	/// <param name="characterController"> The character controller to modify. </param>
	public void SyncCharacterController(CharacterController characterController)
	{
		characterController.skinWidth = skinWidth;
		characterController.center = center;
		characterController.radius = radius;
		characterController.height = height;
	}
}

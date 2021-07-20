using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns cameras and manages transitions between multiple cameras.
/// </summary>
public class CameraManager : MonoBehaviour
{
	[SerializeField] private GameObject playerCameraPrefab = null;
	[SerializeField] private PlayerCameraSettingsData playerCameraSettings = null;

	private PlayerCamera playerCamera;

	public PlayerCamera PlayerCamera => playerCamera;
	public PlayerCameraSettingsData PlayerCameraSettings => playerCameraSettings;

	/// <summary>
	/// Instantiates a player camera near the player character using designer specificed offsets and tilt.
	/// </summary>
	public void CreatePlayerCamera()
	{
		//Destroy(playerCamera);
		Quaternion yawRotation = Quaternion.LookRotation(GameManager.PlayerCharacter.transform.forward, Vector3.up);
		Quaternion pitchRotation = Quaternion.AngleAxis(playerCameraSettings.Tilt, Vector3.right);

		Vector3 positionOffset = yawRotation * new Vector3(0f, playerCameraSettings.StartingHeightOffset, -playerCameraSettings.StartingDepthOffset);
		positionOffset = GameManager.PlayerCharacter.transform.position + positionOffset;

		playerCamera = Object.Instantiate(playerCameraPrefab, positionOffset, yawRotation * pitchRotation).GetComponent<PlayerCamera>();
	}

	private void Awake()
	{
		CreatePlayerCamera();
	}
}

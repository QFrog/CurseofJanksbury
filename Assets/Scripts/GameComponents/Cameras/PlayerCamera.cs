using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine;

/// <summary>
/// Handles the state and movement of the player controlled camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
	private PlayerCameraSettingsData settings;

	private float yaw;
	private float pitch;
	private float maxCameraDistance;

	private Gamepad gamepad;

	private Vector3 FocusPosition => GameManager.PlayerCharacter.transform.position + new Vector3(0f, settings.PivotHeightOffset, 0f);

	// Use this for initialization
	void Start()
	{
		settings = GameManager.CameraManager.PlayerCameraSettings;

		Vector3 cameraToFocusDirection = (FocusPosition - transform.position).normalized;
		Quaternion initialRotation = Quaternion.LookRotation(cameraToFocusDirection);
		yaw = initialRotation.eulerAngles.y;
		pitch = initialRotation.eulerAngles.x;

		maxCameraDistance = Vector3.Distance(transform.position, FocusPosition);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		GameManager.PlayerManager.OnPlayerDestroy += CaughtByEnemy;
	}

	void LateUpdate()
	{
		gamepad = Gamepad.current;
		OrbitCamera();
	}

	/// <summary>
	/// Orbits the camera around a focus point using player input.
	/// </summary>
	private void OrbitCamera()
	{
		if (!GameManager.PlayerCharacter.transform)
		{
			Debug.LogWarning("No camera focus found");
			return;
		}
		
		if (gamepad == null || (Input.GetAxis("Mouse X") != 0.0 || Input.GetAxis("Mouse Y") != 0.0)) {
			yaw += Input.GetAxis("Mouse X") * GameManager.PlayerSettings.MouseSensitivity;
			pitch -= Input.GetAxis("Mouse Y") * GameManager.PlayerSettings.MouseSensitivity;
		} else {
			yaw += gamepad.rightStick.ReadValue().x * GameManager.PlayerSettings.MouseSensitivity * 3;
			pitch -= gamepad.rightStick.ReadValue().y * GameManager.PlayerSettings.MouseSensitivity * 3;
		}

		yaw = MathUtilities.ClampAngle(yaw, settings.YawClamp.x, settings.YawClamp.y);
		pitch = MathUtilities.ClampAngle(pitch, settings.PitchClamp.x, settings.PitchClamp.y);

		Quaternion newRotation = Quaternion.Euler(pitch, yaw, 0f);

		// Calculate the target position (where the camera is positioned when unobstructed
		Vector3 targetOffsetFromFocus = new Vector3(0.0f, 0.0f, -maxCameraDistance);
		Vector3 newTargetPosition = (newRotation * targetOffsetFromFocus) + FocusPosition;

		AdjustCameraDistance(newTargetPosition);
		float distanceToFocus = Vector3.Distance(transform.position, FocusPosition);

		// Calculate the real camera position (after adjusting for any obstructions)
		Vector3 cameraOffsetFromFocus = new Vector3(0.0f, 0.0f, -distanceToFocus);
		Vector3 newCameraPosition = (newRotation * cameraOffsetFromFocus) + FocusPosition;

		Quaternion tiltRotation = Quaternion.AngleAxis(settings.Tilt, Vector3.right);

		transform.rotation = newRotation * tiltRotation;
		transform.position = newCameraPosition;
	}

	/// <summary>
	/// Updates the camera position to avoids becoming obscured by objects in the scene.
	/// The camera will ease out toward the given targetPosition when objects are no longer in the way.
	/// </summary>
	/// <param name="targetPosition"> The position the camera will attempt to reach. </param>
	private void AdjustCameraDistance(Vector3 targetPosition)
	{
		if (Physics.Linecast(FocusPosition, targetPosition, out RaycastHit hit, LayerMaskUtilities.IgnoreAllExceptLayer("Environment")))
		{
			// Move the camera to the trace hit location instantly if we are hitting something closer than the current location of the camera.
			if ((hit.point - FocusPosition).sqrMagnitude <= (transform.position - FocusPosition).sqrMagnitude)
			{
				transform.position = hit.point;
			}
			// Otherwise we are hitting something farther away than the current location of the camera and we lerp to the location of the trace hit.
			else
			{
				transform.position = MathUtilities.EaseOutToTarget(transform.position, hit.point, settings.LerpTime);
			}
		}
		else // If we aren't hitting anything then lerp out to the target distance until reaching the target.
		{
			if (Mathf.Approximately(0f, Vector3.Distance(transform.position, targetPosition)) == false)
			{
				transform.position = MathUtilities.EaseOutToTarget(transform.position, targetPosition, settings.LerpTime);
			}
		}
	}

	private void CaughtByEnemy()
	{
		GameManager.PlayerManager.OnPlayerDestroy -= CaughtByEnemy;
		Destroy(gameObject);
	}
}

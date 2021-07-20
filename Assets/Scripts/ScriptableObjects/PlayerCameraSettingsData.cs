using UnityEngine;

/// <summary>
/// A scriptable object for player camera settings.
/// </summary>
[CreateAssetMenu(fileName = "PlayerCameraSettings", menuName = "ScriptableObjects/PlayerSettingsCamera", order = 1)]
public class PlayerCameraSettingsData : ScriptableObject
{
	[SerializeField] private float pivotHeightOffset = 2.0f;
	[SerializeField] private float startingHeightOffset = 2.5f;
	[SerializeField] private float startingDepthOffset = 4.5f;
	[SerializeField] private float tilt = -15f;
	[SerializeField] private Vector2 yawClamp = new Vector2(-360.0f, 360.0f);
	[SerializeField] private Vector2 pitchClamp = new Vector2(-70.0f, 70.0f);
	[SerializeField] private float lerpTime = 2f;

	/// <summary>
	/// How high above the player character position the camera pivot should rest.
	/// </summary>
	public float PivotHeightOffset { get => pivotHeightOffset; }
	
	/// <summary>
	/// How high above the player character the camera should rest.
	/// </summary>
	public float StartingHeightOffset { get => startingHeightOffset; }

	/// <summary>
	/// How far back from the player the camera should rest.
	/// </summary>
	public float StartingDepthOffset { get => startingDepthOffset; }

	/// <summary>
	/// The amount (in degrees) the camera should be pitched downwards.
	/// </summary>
	public float Tilt { get => tilt; }

	/// <summary>
	/// The minimum and maximum yaw angles allowed by the player camera
	/// </summary>
	public Vector2 YawClamp { get => yawClamp; }

	/// <summary>
	/// The minimum and maximum pitch angles allowed by the player camera
	/// </summary>
	public Vector2 PitchClamp { get => pitchClamp; }

	/// <summary>
	/// How long it takes the camera to return to its default distance from the player.
	/// </summary>
	public float LerpTime { get => lerpTime; }
}

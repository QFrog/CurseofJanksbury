using UnityEngine;

/// <summary>
/// A scriptable object that stores and updates various player settings.
/// </summary>
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings", order = 1)]
public class PlayerSettingsData : ScriptableObject
{
	[SerializeField] private int fixedFrameRate = 200;
	[SerializeField] private bool vSync = true;
	[SerializeField] private float mouseSensitivity = 1.1f;
	[SerializeField] private bool invertPitch = false;
	[SerializeField] private bool sprintToggle = false;

	/// <summary>
	/// The maximum framerate the game will be able to run at when vsync is turned off.
	/// </summary>
	public int FixedFrameRate
	{
		get => fixedFrameRate;
		set
		{
			fixedFrameRate = value;

			// Stop framerate from getting unnecessarily high when vsync is off
			Application.targetFrameRate = fixedFrameRate;
		}
	}

	/// <summary>
	/// Sync the framerate of the game to the monitor's refresh rate to avoid screen tearing.
	/// </summary>
	public bool VSync
	{
		get => vSync;
		set
		{
			vSync = value;

			QualitySettings.vSyncCount = vSync ? 1 : 0;
		}
	}

	/// <summary>
	/// The coefficient applied mouse cursor input while rotating the camera. Higher values increase look speed.
	/// </summary>
	public float MouseSensitivity
	{
		get => mouseSensitivity;
		set => mouseSensitivity = value;
	}

	/// <summary>
	/// Invert the input for looking up and down.
	/// </summary>
	public bool InvertPitch
	{
		get => invertPitch;
		set => invertPitch = value;
	}

	/// <summary>
	/// Allow the sprint input to be pressed once instead of held.
	/// </summary>
	public bool SprintToggle
	{
		get => sprintToggle;
		set => sprintToggle = value;
	}

	/// <summary>
	/// Initialize Unity settings
	/// </summary>
	public void InitalizeSettings()
	{
		FixedFrameRate = fixedFrameRate;
		VSync = vSync;
	}
}

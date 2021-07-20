using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtilities
{
	/// <summary>
	/// Clamps an angle (in degrees) between a minimum and maximum range.
	/// Handles wrapping by normalizing the angle between -360 and 360 degrees.
	/// </summary>
	/// <param name="angle"> The angle (in degrees) to clamp. </param>
	/// <param name="min"> The maximum allowed value of the angle. </param>
	/// <param name="max"> The minimum allowed value of the angle. </param>
	/// <returns> The clamped angle value. </returns>
	public static float ClampAngle(float angle, float min, float max)
	{
		angle = angle % 360;

		if ((angle >= -360f) && (angle <= 360f))
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
		}

		return Mathf.Clamp(angle, min, max);
	}

	/// <summary>
	/// A quick and dirty way of getting a vector location to "ease out" to a target location.
	/// Intended to be called once per frame until the target is reached.
	/// </summary>
	/// <param name="currentLocation"> The vector to move. </param>
	/// <param name="targetLocation"> The target to move towards. </param>
	/// <param name="speed"> How fast (in seconds) it should take to reach the target. </param>
	/// <returns> The new vector location moved closer to the target. </returns>
	///
	public static Vector3 EaseOutToTarget(Vector3 currentLocation, Vector3 targetLocation, float speed)
	{
		// interpolationSpeed must be positive or the ease out is instant
		if (speed <= 0f)
		{
			return targetLocation;
		}

		Vector3 vectorToTarget = targetLocation - currentLocation;

		// Stop interpolating when we get close to zero
		if (Mathf.Approximately(0f, vectorToTarget.sqrMagnitude))
		{
			return targetLocation;
		}

		Vector3 interpolation = vectorToTarget * Mathf.Clamp(Time.deltaTime * speed, 0f, 1f);
		return currentLocation + interpolation;
	}

	/// <summary>
	///     Returns value moved towards target with an exponential ease out. As the damping factor
	///     gets higher the interpolation amount gets closer to 100%.
	/// </summary>
	/// <param name="value"> The value to update. </param>
	/// <param name="target"> The target to move value towards. </param>
	/// <param name="dampingFactor"> The amount of interpolation. Where 0 to infinity maps from 0% to 100%. </param>
	/// <param name="deltaTime"> The delta time for the current frame. </param>
	/// <returns> Value moved towards target. </returns>
	public static Quaternion DampedQuatSlerp(Quaternion value, Quaternion target, float dampingFactor, float deltaTime)
	{
		// e^(-infinity) = 0
		// As the damping factor gets higher the lerp input for 't'
		// gets closer to 1 (i.e. the lerp will set value = target).
		return Quaternion.Slerp(value, target, 1f - Mathf.Exp(-dampingFactor * deltaTime));
	}
}

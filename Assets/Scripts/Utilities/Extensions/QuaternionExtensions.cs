using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtensions
{
	/// <summary>
	///     Returns the Quaternion spherically interpolated towards the target rotation. When applied
	///     across multiple frames this acts as an exponential ease out. As the slerp speed increases the
	///     interpolation applied approaches 100%.
	/// </summary>
	/// <param name="value"> The value to update. </param>
	/// <param name="target"> The target to move value towards. </param>
	/// <param name="slerpSpeed">
	///     The speed of interpolation. Where a speed of 0 to infinity maps to an
	///     interpolation amount of 0% to 100%.
	/// </param>
	/// <returns> Value moved towards target. </returns>
	public static Quaternion EaseOutSlerp(this Quaternion value, Quaternion target, float slerpSpeed)
	{
		// e^(0) = 1
		// e^(-infinity) = 0
		// As the slerp speed gets higher the lerp input for 't' gets closer to 1 (i.e. the lerp will set value = target),
		// and as the slerp speed gets lower the lerp input for 't' gets closer to 0.
		return Quaternion.Slerp(value, target, 1f - Mathf.Exp(-slerpSpeed * Time.deltaTime));
	}
}

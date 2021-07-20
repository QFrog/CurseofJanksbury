using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoundsExtensions
{
	/// <summary>
	/// Returns the largest extent of the given bounds.
	/// </summary>
	/// <returns> The largest extent of the given bounds. </returns>
	public static float GetLargestBoundsExtent(this Bounds bounds)
	{
		float largestExtent = bounds.extents.x;

		if (bounds.extents.y > largestExtent)
		{
			largestExtent = bounds.extents.y;
		}

		if (bounds.extents.z > largestExtent)
		{
			largestExtent = bounds.extents.z;
		}

		return largestExtent;
	}
}

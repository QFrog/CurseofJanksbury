using UnityEngine;

/// <summary>
///     Extension methods for Unity's Vector3 struct which solve the issue of not being able to
///     directly modify components of vectors returned from properties (e.g. transform.position).
///
///		Before:
///			transform.position = new Vector3(1.0f, transform.position.y, transform.position.z);
///
///		After:
///			transform.position.SetX(1.0f);
/// </summary>
public static class Vector3Extensions
{
	/// <summary>
	/// Sets the vector's x component.
	/// </summary>
	/// <param name="x"> The new value of the vector's x component. </param>
	public static void SetX(this Vector3 vector, float x)
	{
		vector.x = x;
	}

	/// <summary>
	/// Sets the vector's y component.
	/// </summary>
	/// <param name="y"> The new value of the vector's y component. </param>
	public static void SetY(this Vector3 vector, float y)
	{
		vector.y = y;
	}

	/// <summary>
	/// Sets the vector's z component.
	/// </summary>
	/// <param name="z"> The new value of the vector's z component. </param>
	public static void SetZ(this Vector3 vector, float z)
	{
		vector.z = z;
	}

	/// <summary>
	/// Adds to the vector's x component.
	/// </summary>
	/// <param name="x"> The value to add to the vector's x component. </param>
	public static void AddToX(this Vector3 vector, float x)
	{
		vector.x += x;
	}

	/// <summary>
	/// Adds to the vector's y component.
	/// </summary>
	/// <param name="y"> The value to add to the vector's y component. </param>
	public static void AddToY(this Vector3 vector, float y)
	{
		vector.y += y;
	}

	/// <summary>
	/// Adds to the vector's z component.
	/// </summary>
	/// <param name="z"> The value to add to the vector's z component. </param>
	public static void AddToZ(this Vector3 vector, float z)
	{
		vector.z += z;
	}

	public static bool Approximately(this Vector3 vector, Vector3 other)
	{
		return (Mathf.Approximately(vector.x, other.x) && Mathf.Approximately(vector.y, other.y) && Mathf.Approximately(vector.z, other.z));
	}
}

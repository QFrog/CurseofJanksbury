using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Sets the scale of the transform globally instead of locally.
    /// </summary>
    /// <param name="globalScale"> The scale to set for this tranform globally. </param>
    public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        transform.localScale = new Vector3 (globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
}

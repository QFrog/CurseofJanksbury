using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMaskUtilities
{
	public static int IgnoreAllExceptLayer(string name)
	{
		return 1 << LayerMask.NameToLayer(name);
	}
	
	public static int IncludeAllExceptLayer(string name)
	{
		return ~(1 << LayerMask.NameToLayer(name));
	}
}

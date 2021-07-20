using System.Threading;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Responsible for starting the game and instancing critical subsystems.
/// Allows each scene to function as a playable standalone scene.
/// </summary>
public static class BootLoader
{
	private const string pathToGameSceneManagerPrefab = "[GameManager]";

	/// <summary>
	/// This method is called only once before the game begins.
	/// </summary>
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void BeforeSceneLoad()
	{
		ObjectUtilities.InstantiateSingletonPrefab<GameManager>(pathToGameSceneManagerPrefab);
	}
}

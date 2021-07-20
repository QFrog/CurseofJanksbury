using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;
using Object = UnityEngine.Object;
using UnityEditor;

/// <summary>
/// Manages state of currently loaded scenes and the transition between scenes.
/// Additionally, it is responsible for instancing and setting up dependency
/// injection for all transient subsystems, such as the gameplay and GUI managers.
/// </summary>
public class GameManager : MonoBehaviour
{
	[SerializeField] private PlayerSettingsData playerSettings = null;
	
	// Subsystem prefab assets
	[SerializeField] private GameObject hudCanvasPrefab = null;
	[SerializeField] private GameObject eventSystemPrefab = null;
	[SerializeField] private GameObject playerControllerPrefab = null;
	[SerializeField] private GameObject cameraControllerPrefab = null;
	[SerializeField] private GameObject aiControllerPrefab = null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	[SerializeField] private GameObject debugCanvasPrefab = null;
#endif
	
	#pragma warning disable CS0414
	// This reference is not actually accessed by any scripts. We need to keep a reference to a PrefabReferences asset or it wont exist at runtime.
	[SerializeField] private PrefabReferences prefabReferences = null;
	#pragma warning restore CS0414

	// Instantiated subsystem components
	private HUDCanvas hudCanvas;
	private PlayerManager playerManager;
	private CameraManager cameraManager;
	private EnemyManager enemyManager;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	private DebugCanvas debugCanvas;
#endif

	// Top level hierarchy "folders"
	private GameObject subsystems;
	private GameObject guiCanvasGroup;

	// The singleton instance of this class.
	private static GameManager instance;

	// Statically accessible subsystem instances
	public static HUDCanvas HudCanvas => instance.hudCanvas;
	public static PlayerManager PlayerManager => instance.playerManager;
	public static CameraManager CameraManager => instance.cameraManager;
	public static EnemyManager EnemyManager => instance.enemyManager;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	public static DebugCanvas DebugCanvas => instance.debugCanvas;
#endif
	public static ActionMap ActionMap;

	// Statically accessible gameobject shortcuts
	public static PlayerCharacter PlayerCharacter => instance.playerManager.PlayerCharacter;
	public static PlayerCamera PlayerCamera => instance.cameraManager.PlayerCamera;
	
	/// Various editable player settings.
	public static PlayerSettingsData PlayerSettings => instance.playerSettings;


	/// <summary>
	/// Reloads the current scene
	/// </summary>
	public static void ResetLevel()
	{
		instance._ResetLevel();
	}
	
	/// <summary>
	/// Get the transform to use for the next player respawn.
	/// </summary>
	/// <returns> The transform to use for the player spawn point. </returns>
	public static Transform GetPlayerSpawnPoint()
	{
		return instance._GetPlayerSpawnPoint();
	}
	
	/// <summary>
	/// Search for enemy spawn points in the scene and get the list of enemy patrol components attached to them.
	/// </summary>
	/// <returns> The list of enemy patrol components found in the scene. </returns>
	public static List<EnemyPatrol> GetEnemyPatrols()
	{
		return instance._GetEnemyPatrols();
	}
	
	private void Awake()
	{
		ActionMap = new ActionMap();
		// Make this class a singleton
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(instance);
		}
		else
		{
			DestroyImmediate(gameObject);
			return;
		}
	}

	private void Start()
	{
		SceneManager.sceneLoaded += SceneLoaded;
		
		playerSettings.InitalizeSettings();
		CreateSubsystems();
	}

	/// <summary>
	/// Instantiates the subsystems handled by the GameManager.
	/// </summary>
	private void CreateSubsystems()
	{
		// Don't add gameplay stuff to cutscene scene
		if (SceneManager.GetActiveScene().buildIndex > 0) //checks if scene is not mainmenu 
		{
			// Create subsystems
			
			subsystems = new GameObject("[Subsystems]");
			
			playerManager = ObjectUtilities.InstantiateSingletonPrefab<PlayerManager>(playerControllerPrefab);
			playerManager.transform.parent = subsystems.transform;
		
			cameraManager = ObjectUtilities.InstantiateSingletonPrefab<CameraManager>(cameraControllerPrefab);
			cameraManager.transform.parent = subsystems.transform;
		
			enemyManager = ObjectUtilities.InstantiateSingletonPrefab<EnemyManager>(aiControllerPrefab);
			enemyManager.transform.parent = subsystems.transform;
			
			// Create GUI canvases
			
			guiCanvasGroup = new GameObject("[GUI]");
			guiCanvasGroup.AddComponent<CanvasGroup>();
			Instantiate(eventSystemPrefab).transform.parent = guiCanvasGroup.transform;
			
			hudCanvas = ObjectUtilities.InstantiateSingletonPrefab<HUDCanvas>(hudCanvasPrefab);
			hudCanvas.transform.SetParent(guiCanvasGroup.transform, false);
			
		#if UNITY_EDITOR || DEVELOPMENT_BUILD
			debugCanvas = ObjectUtilities.InstantiateSingletonPrefab<DebugCanvas>(debugCanvasPrefab);
			debugCanvas.transform.SetParent(guiCanvasGroup.transform, false);
		#endif
		}
	}
	
	private void _ResetLevel()
	{
		Debug.Log("Resetting level");

		DOTween.KillAll();
		
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
	}
	// loads main menu
	public static void loadMainMenu()
    {
        Debug.Log("Loading Main Menu");
        SceneManager.LoadScene(0);
    }

    // loads Seq 1
    public static void loadSeq1()
    {
        Debug.Log("Loading Seq 1");
		SceneManager.LoadScene(1);
	}

    // loads Seq 1
    public static void loadSeq2()
    {
        Debug.Log("Loading Seq 2");
		SceneManager.LoadScene(2);
	}

    /// <summary>
    /// A callback fired when a scene is loaded. Recreates the subsystems and resets
    /// the service locator.
    /// </summary>
    /// <param name="scene"> The scene that was just loaded. </param>
    /// <param name="loadSceneMode"> The load scene mode that was used for the loaded scene. </param>
    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		Destroy(subsystems);
		subsystems = null;
		Destroy(guiCanvasGroup);
		guiCanvasGroup = null;

		CreateSubsystems();
	}

	/// <summary>
	/// A callback fired when a scene is loaded. Recreates the subsystems and resets
	/// the service locator.
	/// </summary>
	public void spawnOnCheckPoint()
	{
		playerManager.RespawnPlayer();
	}
	
	private Transform _GetPlayerSpawnPoint()
	{
		SpawnPoint[] spawnPoints = Object.FindObjectsOfType<SpawnPoint>();

		if (spawnPoints.Length == 0)
		{
			throw new NoSpawnsException("No spawn points found in the scene!");
		}

		SpawnPoint[] playerSpawnPoints = spawnPoints.Where(spawnPoint => spawnPoint.SpawnType == SpawnPoint.SpawnTypes.Player).ToArray();

		if (playerSpawnPoints.Length == 0)
		{
			throw new NoSpawnsException("No player spawn points found in the scene!");
		}

		// For now simply return the first player spawn found. TODO: Handle multiple spawn points?
		Transform firstSpawnFound = playerSpawnPoints[0].transform;

		return firstSpawnFound;
	}

	private List<EnemyPatrol> _GetEnemyPatrols()
	{
		SpawnPoint[] spawnPoints = Object.FindObjectsOfType<SpawnPoint>();

		if (spawnPoints.Length == 0)
		{
			throw new NoSpawnsException("No spawn points found in the scene!");
		}

		// Filter out any spawn points that are not of the enemy spawn point type
		SpawnPoint[] enemySpawnPoints = spawnPoints.Where(spawnPoint => spawnPoint.SpawnType == SpawnPoint.SpawnTypes.Enemy).ToArray();

		if (enemySpawnPoints.Length == 0)
		{
			throw new NoSpawnsException("No Enemy spawn points found in the scene!");
		}

		List<EnemyPatrol> enemyPatrols = new List<EnemyPatrol>();
		
		// Grab the EnemyPatrol component attached to each SpawnPoint.
		// (An enemy spawn point gameobject should have an EnemyPatrol component)
		foreach (SpawnPoint enemySpawnPoint in enemySpawnPoints)
		{
			EnemyPatrol enemyPatrol = enemySpawnPoint.GetComponent<EnemyPatrol>();

			if (enemyPatrol == null)
			{
				Debug.LogWarning($"Enemy spawn point on '{enemySpawnPoint.gameObject.name}' did not have an enemy patrol component attached!");
			}
			else
			{
				enemyPatrols.Add(enemyPatrol);
			}
		}

		return enemyPatrols;
	}
}

/// <summary>
/// A special expection for when the expected spawn points are not found in the scene.
/// </summary>
public class NoSpawnsException : System.Exception
{
	public NoSpawnsException(string message) : base(message) { }
}

using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Possess and controls a particular instance of the player character. Also maintains
/// state about the player that must persist between respawns of the character.
/// </summary>
public class PlayerManager : MonoBehaviour
{
	[SerializeField] private CharacterCapsuleData characterCapsuleData = null;
	[SerializeField] private GameObject playerCharacterPrefab = null;

	public event Action OnSpawnPlayer;

	private PlayerCharacter playerCharacter;
	private int playerHealth;

	public PlayerCharacter PlayerCharacter => playerCharacter;
	public int PlayerHealth => playerHealth;

	public event Action OnPlayerCaught;
	public event Action OnPlayerDestroy;

	public CharacterCapsuleData GetInitialCharacterCapsuleData()
	{
		return characterCapsuleData;
	}

	/// <summary>
	/// Destroys the current player character object and spawns a new one.
	/// </summary>
	public void RespawnPlayer()
	{
		try
		{
			Transform spawnPointTransform = GameManager.GetPlayerSpawnPoint();
			Vector3 spawnLocation = spawnPointTransform.position;

			// The distance from the center of the character capsule to either of the two spheres that define its ends
			float offsetToCapsuleSphere = (characterCapsuleData.GetRealHeight() / 2.0f) - characterCapsuleData.GetRealRadius();

			// Stop the character from starting clipped into the ground if the spawn point was placed slightly underground
			if (Physics.SphereCast(spawnLocation, characterCapsuleData.GetRealRadius(), Vector3.down, out RaycastHit hit, offsetToCapsuleSphere))
			{
				spawnLocation = spawnLocation + (Vector3.down * hit.distance); // Location of SphereCast sphere center after collsion
				spawnLocation.y += offsetToCapsuleSphere; // Offset back to the capsule center
			}

			playerCharacter = Object.Instantiate(playerCharacterPrefab, spawnLocation, Quaternion.LookRotation(spawnPointTransform.forward, Vector3.up)).GetComponent<PlayerCharacter>();

			if (playerCharacter != null)
			{
				OnSpawnPlayer?.Invoke();
			}
			else
			{
				Debug.LogError("Failed to spawn the player!");
			}
		}
		catch (NoSpawnsException e)
		{
			Debug.LogError(e);
		}
	}

	/// <summary>
	/// Call this method to put the character into the caught state and effectively "end" the level as a game over.
	/// </summary>
	public void BecomeCaughtByEnemy(EnemyPatrol.EnemyTypes enemyType = EnemyPatrol.EnemyTypes.BaseEnemy)
	{
		if(playerCharacter.IsDead) return;  // Skip if already dead
		
		if (enemyType != EnemyPatrol.EnemyTypes.GuardEnemy)
		{
			playerHealth -= 1;
		}
		OnPlayerCaught?.Invoke();
		
		StartCoroutine(DelayedBecomeCaught(enemyType));
	}

	private IEnumerator DelayedBecomeCaught(EnemyPatrol.EnemyTypes enemyType)
	{
		// Wait for death animation to finish
		yield return new WaitForSeconds(4.4f);
		
		Destroy(playerCharacter.gameObject);

		if (playerHealth > 0)
		{
			RespawnPlayer();
			OnPlayerDestroy?.Invoke();
			GameManager.CameraManager.CreatePlayerCamera();
		}
	}

	void Awake()
	{
		// TODO: Something more intelligent than immediately spawning the player
		playerHealth = 5;
		RespawnPlayer();
	}
}

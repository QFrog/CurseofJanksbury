using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
	// Add different enemy types here
    [SerializeField] private GameObject baseEnemyCharacterPrefab = null;
	[SerializeField] private GameObject bipedYellowCharacterPrefab = null;
	[SerializeField] private GameObject bipedOrangeCharacterPrefab = null;
	[SerializeField] private GameObject bipedRedCharacterPrefab = null;
	[SerializeField] private GameObject guardEnemyCharacterPrefab = null;
	[SerializeField] private GameObject FlyEnemyCharacterPrefab = null;
	[SerializeField] private GameObject QuadEnemyCharacterPrefab = null;
	[SerializeField] private GameObject CivilianCharacterPrefab = null;

	private List<IEnemy> enemyCharacters = new List<IEnemy>();
    private List<EnemyPatrol> enemyPatrols = new List<EnemyPatrol>();

    private void Start()
    {
        enemyPatrols = GameManager.GetEnemyPatrols();
        
        SpawnEnemies();
    }

    /// <summary>
    /// Iterates through all of the EnemyPatrol components in the scene and spawns an enemy character
    /// at a random point in the patrol path.
    /// </summary>
    private void SpawnEnemies()
    {
        foreach (EnemyPatrol enemyPatrol in enemyPatrols)
        {
            int randWayPoint = Random.Range(0, enemyPatrol.vertices.Count);
			GameObject enemyCharacter;
			
			// Create if statement for the enemy types here
			if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.BaseEnemy)
			{
				enemyCharacter = Instantiate(baseEnemyCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}
			else if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.BipedYellow)
			{
				enemyCharacter = Instantiate(bipedYellowCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}
			else if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.BipedOrange)
			{
				enemyCharacter = Instantiate(bipedOrangeCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}
			else if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.BipedRed)
			{
				enemyCharacter = Instantiate(bipedRedCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}
			else if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.GuardEnemy)
			{
				enemyCharacter = Instantiate(guardEnemyCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}
			else if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.FlyEnemy)
			{
				enemyCharacter = Instantiate(FlyEnemyCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}
			else if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.QuadEnemy)
			{
				enemyCharacter = Instantiate(QuadEnemyCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}
			else if (enemyPatrol.EnemyType == EnemyPatrol.EnemyTypes.Civilian)
			{
				enemyCharacter = Instantiate(CivilianCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
					.transform.rotation);
			}
			else
			{
				Debug.LogError(enemyPatrol.gameObject.name + " failed to have a spawn a vaild enemytype BaseEnemy was spawned instead");
				enemyCharacter = Instantiate(baseEnemyCharacterPrefab, enemyPatrol.vertices[randWayPoint].transform.position, enemyPatrol.vertices[randWayPoint]
				.transform.rotation);
			}

            IEnemy enemyCharacterComponent = enemyCharacter.GetComponent<IEnemy>();
            enemyCharacterComponent.SetPatrol(enemyPatrol, randWayPoint);

            if (enemyCharacter != null && enemyCharacterComponent != null)
            {
                enemyCharacters.Add(enemyCharacterComponent);
            }
            else
            {
                Debug.LogWarning($"Enemy character was not able to be instantiated at EnemyPatrol on '{enemyPatrol.gameObject.name}'");
            }
        }
    }
}

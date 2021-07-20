using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CheckPoint : MonoBehaviour
{
    public GameObject playerSpawnPoint;

    /// <summary>
	/// Player must collide with a check point
    /// If the player collides with an enemy then the player will spawn at the check point
    /// Once the player collides with the enemy, player's health should subtract by 1
	/// </summary>

    void Start()
    {
        playerSpawnPoint = GameObject.Find("PlayerSpawnPoint");
		//GameManager.PlayerManager.OnPlayerCaught += CaughtByEnemy;
    } 

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            //UnityEngine.Debug.Log("Works!");

            playerSpawnPoint.transform.position = this.transform.position;
        }
    }
}


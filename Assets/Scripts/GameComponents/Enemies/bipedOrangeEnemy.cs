using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Pathfinding;

public class bipedOrangeEnemy : EnemyCharacter, IEnemy
{
	/* varable comment format: 
	 * <data type> <name>
	 *
	 * AIPath ai;
	 *
	 * EnemyPatrol homePatrolPath; // patrol path the enemy starts on
	 * int homePatrolIndex; // index for the patrol path
	 * EnemyPatrol patrolPath; // current patrol path
	 * int patrolIndex; // current index of patroling
	 *
	 * float directionChangeFrequency; // frequency of direction changes when patroling
	 * bool reverse = false; // changed to make the enemy patrol in opposite direction whe sequntially patroling
	 *
	 * bool playerInSight = false; //flag for if the player is in sight of the enemy
	 * bool movedObjectInSight = false; //flag for if a moved object is in sight
	 *
	 * float pauseTime; // time the enemy pauses when he see the player or moved object in sight
	 * float time; // used for puase time
	 * bool reactionWait = true; // used for reaction wait time
	 *
	 * float fov = 0; // total fov (field of veiw) in degrees
	 *
	 * SphereCollider detectionSphereCollider = null; // shpere collider used for detection
	 * Vector3 lastKnownPosition; //last know postion of player or moved object
	 */

	/* overridable functions list:
	 * 
	 * void Start() // intialzes several varibles
	 * void Update() // handles when to call next patrol point or if somthing is sighted to chase them
	 * 
	 * void SetPatrol(EnemyPatrol patrol, int startingPatrolIndex) // initalizes the home patrol on startup
	 * void NextPatrolPoint() //does all the logic for selecting the next patol point
	 * 
	 * void OnTriggerStay(Collider other) // handles all the visual detection for the enemies
	 * void OnTriggerExit(Collider other) // handles losing sight of the player
	 * void OnCollisionEnter(Collision collision) //handles the hit of the player
	 * 
	 * void HitWithJankifyGun() //what to do if hit with jankify gun
	 * 
	 * void OnDrawGizmos() // draws gizmos
	 * */

	
}

using System;
using Pathfinding;
using UnityEngine;
using System.Collections;
using Thread = System.Threading.Thread;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class flyEnemy : EnemyCharacter, IEnemy
{
	[SerializeField]
	protected float chaseTime;
	[SerializeField]
	private float diveDelay = 3f;
	protected bool chasingPlayer;
	public GameObject flyBody;
	[SerializeField]
	[Range (0f, 0.03f)]
	private float lerpSpeed = 0.03f;
	private float lerpPct = 0;
	[SerializeField]
	private bool diving = false;
	[SerializeField]
	private bool returning = false;
	private Vector3 playerPos;
	private Vector3 originalPos;
	[SerializeField]
	private bool wet = false;
	private bool playerCaught = false;

	protected override void Start()
	{
		base.Start();
		GameManager.PlayerManager.OnPlayerCaught += gotThePlayer;
	}

	protected override void Update() {
		if (diving) {
			//Debug.Log("diving");
			lerpPct += lerpSpeed;
			ai.canMove = false;
			playerPos = GameManager.PlayerCharacter.transform.position;
			flyBody.transform.position = Vector3.Lerp(originalPos, playerPos, lerpPct);
		}
		if (returning)
		{
			lerpPct += lerpSpeed;
			//Debug.Log("returning");
			Vector3 newPos = flyBody.transform.position;
			flyBody.transform.position = Vector3.Lerp(newPos, originalPos, lerpPct);
		}
		if (returning == true && VectCompare(flyBody.transform.position, originalPos, 0.1f))
		{
			Debug.Log("stopped returning");
			returning = false;
			ai.canMove = true;
			lerpPct = 0;
		}
		if (wet)
		{
			if (lerpPct < 1)
			{
				lerpPct += lerpSpeed;
			}
			flyBody.transform.position = Vector3.Lerp(originalPos, transform.position, lerpPct);
		}

		base.Update();

		// Upon reaching a destination selects another
		if (ai.reachedDestination && !movedObjectInSight && !playerInSight)
		{
			//Debug.Log("not chasing player A");
			chasingPlayer = false;
		}
		else if (playerInSight && diving == false) // If player is in sight select player as destination
		{
			//Debug.Log(flyBody.transform.position);
			//Debug.Log(GameManager.PlayerCharacter.transform.position);
			if (!playerCaught)
			{
				chaseTime += Time.deltaTime;
			}
			chasingPlayer = true;
		}
		else if (movedObjectInSight)  // If player is in sight select player as destination
		{
			//Debug.Log("not chasing player");
			chasingPlayer = false;
		}

		if (chasingPlayer && chaseTime > diveDelay) {
			chaseTime = 0;
			Debug.Log("Dive Dive Dive!");
			diving = true;
			returning = false;
			lerpPct = 0;
			originalPos = flyBody.transform.position;
			StartCoroutine(StopDive());
		}
	}

	protected override void distanceChecker(Collider other)
	{
		Vector3 direct = other.transform.position - flyBody.transform.position;
		if (Physics.Raycast(flyBody.transform.position, direct, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers,
						QueryTriggerInteraction.Ignore))
		{
			Debug.DrawRay(flyBody.transform.position, direct, Color.green);
			DebugExtension.DebugPoint(hit.point, Color.green);

			if (hit.collider.tag == "Player" && hit.distance < playerKillDistance && diving == true)
			{
				GameManager.PlayerManager.BecomeCaughtByEnemy(homePatrolPath.EnemyType);
			}
		}
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponent<ProjectileShot>())
		{
			if (wet)
			{
				HitWithJankifyGun();
			}
		}
	}
	
	protected virtual void OnTriggerEnter(Collider other)
	{
		// Checks if the player leaves volume of detection
		if (other.name == "WaterZone")
		{
			NextPatrolPoint();
			StartCoroutine(FacePlant());
		}
	}

	private bool VectCompare(Vector3 me, Vector3 other, float allowedDifference)
	{
		float dx = me.x - other.x;
		float dy = me.y - other.y;
		float dz = me.z - other.z;
		if (Mathf.Abs(dx) > allowedDifference)
		{
			Debug.Log("x fail");
			return false;
		}
		else if (Mathf.Abs(dy) > allowedDifference)
		{
			Debug.Log("y fail");
			return false;
		}
		else if (Mathf.Abs(dz) > allowedDifference)
		{
			Debug.Log("z fail");
			return false;
		}
		else
		{
			Debug.Log("pass");
			return true;
		}
	}

	IEnumerator StopDive() {
		Debug.Log("stop dive started");
		yield return new WaitForSeconds(5);
		diving = false;
		returning = true;
		lerpPct = 0;
		Debug.Log("stop dive complete");
	}

	IEnumerator FacePlant () {
		yield return new WaitForSeconds(1.5f);
		ai.canMove = false;
		originalPos = flyBody.transform.position;
		lerpPct = 0;
		wet = true;
	}

	private void gotThePlayer()
	{
		diving = false;
		lerpPct = 0;
		playerCaught = true;
	}

	protected override void playerGone()
	{
		base.playerGone();
		chaseTime = 0;
		lerpPct = 0;
		diving = false;
		playerCaught = false;
	}

	// protected override void OnCollisionEnter(Collision collision)
	// {
	// 	if (collision.gameObject.GetComponent<PlayerCharacter>())
	// 	{
	// 		playerInSight = false;
	// 		GameManager.PlayerCharacter.BecomeCaughtByEnemy();
	// 	}
	// 	else if (collision.gameObject.GetComponent<ProjectileShot>())
	// 	{
	// 		HitWithJankifyGun();
	// 		Debug.Log(gameObject.name);
	// 	}
	// 	else
	// 	{

	// 	}
	// }

	// if collide with something - stop reducing height and bounce back up

	// create fire hydrant that stuns and drops enemy


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

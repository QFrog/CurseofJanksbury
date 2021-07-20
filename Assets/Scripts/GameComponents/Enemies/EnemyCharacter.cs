using System;
using Pathfinding;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class EnemyCharacter : MonoBehaviour, IEnemy
{
	[SerializeField] protected int patrolIndex;
	/// Flag for if the player is in sight of the enemy
	public bool playerInSight = false;
	public bool movedObjectInSight = false;
	public float pauseTime = 0.8f;
	[SerializeField] protected float playerKillDistance;
	[SerializeField] protected bool reactionWait = true;
	/// Total fov (field of view) in degrees
	[SerializeField] protected float fov = 0;
	public Vector3 position = new Vector3(1f, 1f, 1f);

	protected AIPath ai;
	protected EnemyPatrol homePatrolPath;
	protected int homePatrolIndex;
	protected EnemyPatrol patrolPath;

	// Patrol direction and change variables
	protected float directionChangeFrequency;
	/// Changed to make the enemy patrol in opposite direction whe sequentially patrolling
	protected bool reverse = false;
	protected float time;

	protected SphereCollider detectionSphereCollider = null;
	/// Last know position of player or moved object
	protected Vector3 lastKnownPosition;

	public Light detectionLight;
	protected float destinationHeightOffset = 1f;

	private Rigidbody rigidbody = null;
	protected Animator anim = null;

	protected ParticleSystem.EmissionModule deathEmission;
	[SerializeField] protected ParticleSystem deathParticles;
	
	private float timeToNextProcess = 0f;

	public bool isDying { get; private set; } = false;

	public virtual void SetPatrol(EnemyPatrol patrol, int startingPatrolIndex)
	{
		homePatrolPath = patrol;
		homePatrolIndex = startingPatrolIndex;
	}

	public virtual void HitWithJankifyGun()
	{
		detectionLight.color = Color.white;
		FindObjectOfType<PlayerCharacter>().CheckEnemyMusic();
		FindObjectOfType<PlayerCharacter>().PlayEnemyDeath();
		anim.SetTrigger("isHit");
		isDying = true;
		deathParticles.Play();
		deathEmission.enabled = true;
		GameManager.PlayerManager.OnPlayerDestroy -= playerGone;
		Destroy(gameObject, 1.3f);
	}

	public virtual EnemyPatrol GetEnemyPatrol()
	{
		return homePatrolPath;
	}

	public void PlayAttack()
	{
		anim.SetTrigger("isAttacking");
	}

	protected virtual void Awake()
	{
		// Needs to be in awake or we get nullrefs
		detectionLight = GetComponentInChildren<Light>();
	}

	protected virtual void Start()
	{
		position = gameObject.transform.position;
		ai = gameObject.GetComponent<AIPath>();
		if (ai == null)
		{
			Debug.LogError("Gameobject: \"" + gameObject.name + "\" is missing an AIPath component.");
		}

		time = pauseTime;
		playerInSight = false;

		patrolPath = homePatrolPath;
		patrolIndex = homePatrolIndex;
		ai.destination = transform.position;

		rigidbody = GetComponent<Rigidbody>();
		anim = GetComponentInChildren<Animator>();
		
		deathEmission = deathParticles.emission;
		deathEmission.enabled = false;

		GameManager.PlayerManager.OnPlayerDestroy += playerGone;

		if (TryGetComponent(out SphereCollider sphereCollider))
		{
			detectionSphereCollider = sphereCollider;
		}
	}
	
	protected virtual void Update()
	{
		if (isDying)
		{
			ai.isStopped = true;
			return;
		}
		
		position = gameObject.transform.position;
		// Updates direction change frequency value
		directionChangeFrequency = patrolPath.directionChangeFrequency;
		
		anim.SetFloat("Speed", ai.velocity.magnitude);

		// Upon reaching a destination selects another
		if (ai.reachedDestination && !movedObjectInSight && !playerInSight)
		{
			NextPatrolPoint();
			reactionWait = true;
		}
		else if (playerInSight) // If player is in sight select player as destination
		{
			ai.isStopped = false;
			if (reactionWait)
			{
				time = 0;
				reactionWait = false;
			}
			Vector3 nearestPos = (Vector3)AstarPath.active.GetNearest(lastKnownPosition).node.position;
			nearestPos.y += destinationHeightOffset;
			ai.destination = nearestPos;
		}
		else if (movedObjectInSight)  // If player is in sight select player as destination
		{
			ai.isStopped = false;
			if (reactionWait)
			{
				time = 0;
				reactionWait = false;
				NextPatrolPoint();
			}

			if (ai.reachedDestination)
			{
				NextPatrolPoint();
			}
		}

		time += Time.deltaTime;

		if (time <= pauseTime)
		{
			ai.isStopped = true;
		}
		else
		{
			ai.isStopped = false;
		}

	#if UNITY_EDITOR
		if (detectionSphereCollider != null)
		{
			DebugExtension.DebugWireSphere(transform.position, Color.yellow, detectionSphereCollider.radius);

			Vector3 leftLineDirection = Quaternion.AngleAxis(fov / 2f, Vector3.up) * transform.forward;
			Vector3 leftLineEnd = transform.position + (leftLineDirection * detectionSphereCollider.radius);
			Debug.DrawLine(transform.position, leftLineEnd, Color.green);
			Vector3 rightLineDirection = Quaternion.AngleAxis(-(fov / 2f), Vector3.up) * transform.forward;
			Vector3 rightLineEnd = transform.position + (rightLineDirection * detectionSphereCollider.radius);
			Debug.DrawLine(transform.position, rightLineEnd, Color.green);
		}
	#endif
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (isDying) return;

		if (collision.gameObject.GetComponent<ProjectileShot>())
		{
			HitWithJankifyGun();
			Debug.Log(gameObject.name);
		}
	}

	/// Draws arrow in direction of facing
	protected virtual void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		//Gizmos.DrawSphere(lastKnownPosition, 0.5f);
		try
		{
			Gizmos.DrawSphere(ai.destination, 0.5f);
		}
		catch (NullReferenceException)
		{
			// Do nothing
		}

		DebugExtension.DebugArrow(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.forward, Color.yellow);
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (isDying) return;
		
		// Checks if the player leaves volume of detection
		if (other.GetComponent<PlayerCharacter>())
		{
			playerInSight = false;

			//change the spotlight to white
			detectionLight.color = Color.white;
			
			EnemyCharacter[] allEnemies = FindObjectsOfType(typeof(EnemyCharacter)) as EnemyCharacter[];
			bool EnemiesChasing = false;
			foreach(EnemyCharacter enemy in allEnemies) {
				if (enemy.detectionLight.color == Color.red) {
					EnemiesChasing = true;
				}
			}

			if (!EnemiesChasing) {
				IEnumerator coroutine;
				AudioSource audio = GameObject.Find("Level_2").GetComponent<AudioSource>();
				coroutine = StartFade(audio, 2, 1);
				StartCoroutine(coroutine);
				
				audio = GameObject.Find("Level_3").GetComponent<AudioSource>();
				coroutine = StartFade(audio, 2, 1);
				StartCoroutine(coroutine);
			}
		}
	}

	public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (isDying) return;
		
		if (other.GetComponent<PlayerCharacter>()) {
			EnemyCharacter[] allEnemies = FindObjectsOfType(typeof(EnemyCharacter)) as EnemyCharacter[];
			bool EnemiesChasing = false;
			foreach(EnemyCharacter enemy in allEnemies) {
				if (enemy.detectionLight.color == Color.red) {
					EnemiesChasing = true;
				}
			}

			if (!EnemiesChasing) {
				IEnumerator coroutine;
				AudioSource audio = GameObject.Find("Level_2").GetComponent<AudioSource>();
				coroutine = StartFade(audio, 2, 1);
				StartCoroutine(coroutine);
				
				audio = GameObject.Find("Level_3").GetComponent<AudioSource>();
				coroutine = StartFade(audio, 2, 1);
				StartCoroutine(coroutine);
			}
		}
	}

	/// Handles enemy sight calculations within the detection volume (sphere collider)
	protected virtual void OnTriggerStay(Collider other)
	{
		if (isDying) return;
		
		if (timeToNextProcess > 0f)
		{
			timeToNextProcess -= Time.deltaTime;
			return;
		}
		else
		{
			timeToNextProcess = Random.Range(0.1f, 0.2f);
		}

		if (other.gameObject.layer != LayerMask.NameToLayer("Environment"))
		{
			// Calculates the angle of the object from the direction the enemy is facing for FOV checks
			Vector3 direct = other.transform.position - transform.position;
			float angle = Vector3.Angle(transform.forward, direct);

			// Checks if the object is close enough and in the FOV of the enemy
			if (angle < fov * 0.5f)
			{
				if (other.GetComponent<PlayerCharacter>() != null || other.GetComponent<Manipulable>() != null)
				{
					// Raycasts for line of sight
					if (Physics.Raycast(transform.position, direct, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers,
						QueryTriggerInteraction.Ignore))
					{
						Debug.DrawRay(transform.position, direct, Color.red);
						//Debug.Log(hit.transform.tag);
						DebugExtension.DebugPoint(hit.point, Color.red);
						// Checks if the enemy has a clear line of sight on the player
						if (hit.transform.GetComponent<PlayerCharacter>() != null)
						{
							// Follows the player and flags the player as seen
							playerInSight = true;
							lastKnownPosition = GameManager.PlayerCharacter.transform.position;

							// Change the spotlight to red
							detectionLight.color = Color.red;
						}

						if (hit.transform.TryGetComponent(out Manipulable manip))
						{
							if (manip.isChanged)
							{
								movedObjectInSight = true;
								manip.isChanged = false;
								// Change the spotlight to yellow
								detectionLight.color = Color.yellow;

								if (playerInSight == false)
								{
									patrolPath = hit.transform.GetComponentInChildren<EnemyPatrol>();
									homePatrolIndex = patrolIndex;
									reverse = false;
									patrolIndex = -1;

									// Change the spotlight to yellow
									detectionLight.color = Color.yellow;
								}
							}
							else
							{
								// Change the spotlight to white
								detectionLight.color = Color.white;
							}
						}
					}
				}
			}
			else if (other.GetComponent<PlayerCharacter>() != null || other.GetComponent<Manipulable>() != null)
			{
				if (other.GetComponent<PlayerCharacter>() != null)
				{
					playerInSight = false;
				}

			}
			if (other.GetComponent<PlayerCharacter>() != null)
			{
				distanceChecker(other);
			}

		}
	}

	protected virtual void distanceChecker(Collider other)
	{
		Vector3 direct = other.transform.position - transform.position;
		if (Physics.Raycast(transform.position, direct, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers,
						QueryTriggerInteraction.Ignore))
		{
			Debug.DrawRay(transform.position, direct, Color.green);
			DebugExtension.DebugPoint(hit.point, Color.green);

			if (hit.collider.tag == "Player" && hit.distance < playerKillDistance)
			{
				GameManager.PlayerManager.BecomeCaughtByEnemy(homePatrolPath.EnemyType);
			}
		}
	}
	
	/// Sets the next patrol point to the pathfinding target
	protected virtual void NextPatrolPoint()
	{
		if (patrolPath.PatrolType == EnemyPatrol.PatrolTypes.Sequential)
		{
			float rand = Random.value;
			if (rand < directionChangeFrequency)
			{
				reverse = !reverse;
			}

			if (reverse && !movedObjectInSight)
			{
				patrolIndex--;
				if (patrolIndex < 0)
				{
					patrolIndex = patrolPath.vertices.Count - 1;
				}
			}
			else
			{
				patrolIndex++;
				if (patrolIndex >= patrolPath.vertices.Count)
				{
					if (movedObjectInSight)
					{
						patrolPath = homePatrolPath;
						patrolIndex = homePatrolIndex;
						movedObjectInSight = false;
						if (reactionWait)
						{
							time = 0;
							reactionWait = false;
						}
					}
					else
					{
						patrolIndex = 0; // Causes issues when returning to home path
					}
				}
			}
		}
		else if (patrolPath.PatrolType == EnemyPatrol.PatrolTypes.Random)
		{
			patrolIndex = (int) Random.Range(0.0f, (patrolPath.vertices.Count - 0.1f));
		}
		else if (patrolPath.PatrolType == EnemyPatrol.PatrolTypes.idle)
		{
			patrolIndex = 0;
			ai.isStopped = true;
			Vector3 lookDirct = (homePatrolPath.gameObject.transform.position + homePatrolPath.gameObject.transform.forward);
			lookDirct.y = transform.position.y;
			gameObject.transform.LookAt(lookDirct);
		}
		Vector3 nearestPos = (Vector3)AstarPath.active.GetNearest(patrolPath.vertices[patrolIndex].position).node.position;
		nearestPos.y += destinationHeightOffset;
		ai.destination = nearestPos;
	}

	protected virtual void playerGone()
	{
		playerInSight = false;
		detectionLight.color = Color.white;
	}
}

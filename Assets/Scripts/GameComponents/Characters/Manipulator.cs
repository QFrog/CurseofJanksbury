using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Manipulator : MonoBehaviour
{
	[FormerlySerializedAs("objectTarget")] [SerializeField] private Transform manipTargetIcon = null;
	[SerializeField] private float followAcceleration = 40f;
	[SerializeField] private float reorientSpeed = 6f;
	[SerializeField] private float maxObjectVelocity = 50f;
	// The the minimum padding space allowed between the player and the bounding box of a manipulable
	[SerializeField] private float objectBoundsPadding = 2f;
	[SerializeField] private float grabDistance = 7f;
	[SerializeField] private float grabLineRadius = 0.2f;
	[SerializeField] private float manipulableTargetDepth = 10f;
	// The range outside of the manipulable target depth the object can be in and not be dropped, outside of this it will be
	private float manipulableDropDepthBuffer;
	[SerializeField] private float beamExtendSpeed = 10f;

	private readonly int materialID_IsGlowing = Shader.PropertyToID("_IsGlowing");
	
	// The maximum amount of collisionNeighbours visualised
	private const int MAX_NEIGHBOURS = 16;
	private Collider[] collisionNeighbours;

	private Manipulable grabbedManipulable = null; // used to flag the object as moved
	private Vector3 originalTargetLocalPosition;

	[SerializeField] private Transform projectileSpawnTransform = null;
	[SerializeField] private Transform projectileSpawnTransform_Recoilless = null;
	[SerializeField] private float maxJankEnergy = 100f;
	[Tooltip("rate at which the jank energy refills")]
	[SerializeField] private float refillRate = 30f;
	public float MaxJankEnergy => maxJankEnergy;
	public float ShootEnergyCost => shootJankEnergy;
	public float JankEnergy { get; private set; }
	[SerializeField] private float shootJankEnergy = 25f;
	//[SerializeField] private float manipJankEnergy;
	[SerializeField] ProjectileShot _jankShotPrefab = null;
	//[SerializeField] private float aimOffset = 1f;
	private Vector3 oldTargetPosition;
	private Vector3 beamEndOffset;
	private bool isBeamExtending = false;

	public Transform ProjectileSpawnTransform => projectileSpawnTransform;
	public Transform ProjectileSpawnTransform_Recoilless => projectileSpawnTransform_Recoilless;

	public ParticleSystem beam;
	private ParticleSystem.EmissionModule em;
	private ParticleSystemRenderer psRenderer;
	
	private void OnEnable()
	{
		manipulableDropDepthBuffer = (grabDistance - manipulableTargetDepth) + 2;
	}

	/// <summary>
	/// For now this does a simple raycast from the player camera and toggles the jankify state of a manipulable object.
	/// </summary>
	public void ShootJankifyProjectile()
	{
		JankEnergy -= shootJankEnergy;
			
		// maxFocalDistance is different from manipulableTargetDepth (and must be larger) because it starts from the camera instead of from the player
		float maxFocalDistance = 9999999f;
		Vector3 pointInDistanceFromCamera = GameManager.PlayerCamera.transform.position + GameManager.PlayerCamera.transform.forward * maxFocalDistance;
		Vector3 fromPlayerToFocalPoint;

		// Raycast from the camera forward up to the maxFocalDistance
		if (Physics.Raycast(GameManager.PlayerCamera.transform.position, GameManager.PlayerCamera.transform.forward, out RaycastHit hit, maxFocalDistance,
			~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Projectile")), QueryTriggerInteraction.Ignore))
		{
			fromPlayerToFocalPoint = hit.point - projectileSpawnTransform.position;
			Debug.DrawRay(projectileSpawnTransform.position, fromPlayerToFocalPoint, Color.cyan, 3f);
		}
		else
		{
			fromPlayerToFocalPoint = pointInDistanceFromCamera - projectileSpawnTransform.position;
		}

		ProjectileShot shot = Instantiate(_jankShotPrefab, projectileSpawnTransform.position, projectileSpawnTransform.rotation);

		if (GameManager.PlayerCharacter.IsTorsoTwistedBeyondMax())
		{
			// If we aren't twisting the torso forward then shoot in the direction of the gun
			shot.Launch(projectileSpawnTransform.forward, GameManager.PlayerCharacter.GetVelocity());
		}
		else
		{
			shot.Launch(fromPlayerToFocalPoint, GameManager.PlayerCharacter.GetVelocity());	
		}
		
		// Cut off manip beam immediately
		StopAllCoroutines();
		em.enabled = false;
		beam.gameObject.transform.LookAt(projectileSpawnTransform);
		isBeamExtending = false;
		psRenderer.enabled = false;
	}
	
	/// <summary>
	/// Attempts to grab the closest grabbable manipulable object and returns true if successful.
	/// </summary>
	public void ShootManipulateBeam()
	{
		if (!isBeamExtending && !IsManipulating())
		{
			beamEndOffset = Vector3.zero;

			StartCoroutine(ShootBeam());
		}
	}

	private IEnumerator ShootBeam()
	{
		while (true)
		{
			psRenderer.enabled = true;
			em.enabled = true;
			isBeamExtending = true;

			float maxFocalDistance = 9999999f;
			Vector3 pointInDistanceFromCamera = GameManager.PlayerCamera.transform.position + GameManager.PlayerCamera.transform.forward * maxFocalDistance;
			Vector3 fromGunToFocalPoint;

			// Raycast from the camera forward up to the maxFocalDistance
			if (Physics.Raycast(GameManager.PlayerCamera.transform.position, GameManager.PlayerCamera.transform.forward, out RaycastHit hitPoint, maxFocalDistance,
				~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Projectile")), QueryTriggerInteraction.Ignore))
			{
				fromGunToFocalPoint = hitPoint.point - projectileSpawnTransform.position;
			}
			else
			{
				fromGunToFocalPoint = pointInDistanceFromCamera - projectileSpawnTransform.position;
			}

			Vector3 beamDirection = fromGunToFocalPoint.normalized;
			
			// Reproject onto new forward
			beamEndOffset = beamDirection * beamEndOffset.magnitude;
			
			beamEndOffset = MathUtilities.EaseOutToTarget(beamEndOffset, beamDirection * grabDistance, beamExtendSpeed);

			float beamLength = beamEndOffset.magnitude;

			if (beamLength > (grabDistance - 1f))
			{
				em.enabled = false;
				beam.gameObject.transform.LookAt(projectileSpawnTransform);
				isBeamExtending = false;
				yield break;
			}
			
			if (Physics.SphereCast(projectileSpawnTransform.position, grabLineRadius, beamDirection, out RaycastHit hit, beamLength,
				~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Projectile")), QueryTriggerInteraction.Ignore))
			{
				Manipulable manipulable = hit.collider.gameObject.GetComponent<Manipulable>();
				if (manipulable != null)
				{
					grabbedManipulable = manipulable;
					FindObjectOfType<PlayerCharacter>().ManipulatingNoise();
					manipulable.isChanged = true;
					manipulable.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
				
					isBeamExtending = false;
					yield break;
				}
			}

			yield return null;
		}
	}
	
	/// <summary>
	/// Drops the currently held manipulable if one exists.
	/// </summary>
	public void DropManipulable()
	{
		if (IsManipulating())
		{
			// If object is held, let it go
			grabbedManipulable.Rigidbody.constraints = RigidbodyConstraints.None;
			grabbedManipulable.NavmeshCutter.ForceUpdate(); //updates the navmesh cutting
			grabbedManipulable = null;
			FindObjectOfType<PlayerCharacter>().StopManipulatingNoise();
		}
	}

	public bool IsManipulating()
	{
		return grabbedManipulable != null;
	}

	private void Start()
	{

		originalTargetLocalPosition = new Vector3(0f, 0f, manipulableTargetDepth);
		
		JankEnergy = maxJankEnergy;

		collisionNeighbours = new Collider[MAX_NEIGHBOURS];

		beam.Play();
		em = beam.emission;
		beam.gameObject.transform.LookAt(projectileSpawnTransform);
		em.enabled = false;
		psRenderer = beam.gameObject.GetComponent<ParticleSystemRenderer>();
		psRenderer.enabled = false;

		if (projectileSpawnTransform == null)
		{
			Debug.LogError("The Manipulator component must have a projectile spawn position set!");
		}
	}
	
	private void Update()
	{
		if (SceneManager.GetActiveScene().buildIndex < 2) return;
		
		if (IsManipulating())
		{
			if (IsPastDropDistance(grabbedManipulable.transform.position) || !IsObjectInLOS(grabbedManipulable.gameObject))
			{
				DropManipulable();
			}
			else // Manipulating as normal
			{
				em.enabled = true;
				beam.gameObject.transform.LookAt(grabbedManipulable.transform.position);
			}
		}
		else if (!isBeamExtending)
		{
			em.enabled = false;
			beam.gameObject.transform.LookAt(projectileSpawnTransform);
		}
		
		if (JankEnergy < maxJankEnergy)
		{
			JankEnergy += refillRate * Time.deltaTime;
			JankEnergy = Mathf.Min(JankEnergy, 100f);
		}
	}

	private void FixedUpdate()
	{
		if (SceneManager.GetActiveScene().buildIndex < 2) return;
		
		if (IsManipulating())
		{
				//energy.useJankEnergy(manipJankEnergy); //(Might be tied to the fram rate)
				if (grabbedManipulable.inEnemyRange)
				{
					grabbedManipulable.inEnemyRange = false;
					DropManipulable();
				}
				else // Manipulating as normal
				{
					// Reorient the manipulable while it's being translated
					if (grabbedManipulable.isJankified)
					{
						grabbedManipulable.transform.rotation = MathUtilities.DampedQuatSlerp(
							grabbedManipulable.transform.rotation,
							Quaternion.Euler(grabbedManipulable.jankifiedRotation),
							reorientSpeed,
							Time.fixedDeltaTime);
					}
					else
					{
						grabbedManipulable.transform.rotation = MathUtilities.DampedQuatSlerp(
							grabbedManipulable.transform.rotation,
							Quaternion.Euler(grabbedManipulable.normalRotation),
							reorientSpeed,
							Time.fixedDeltaTime);
					}

					MoveManipulable();
				}
		}
		else
		{
			// Reset follow icon when no object is being manipulated (debug only)
			manipTargetIcon.localPosition = Vector3.zero;
			beam.gameObject.transform.LookAt(projectileSpawnTransform);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Manipulable>() != null)
		{
			// Turn on manipulable object glow

			Material manipulableMaterial = other.GetComponent<MeshRenderer>().material;
			if (manipulableMaterial.HasProperty(materialID_IsGlowing))
			{
				manipulableMaterial.SetInt(materialID_IsGlowing, 1);
			}
			else
			{
				Debug.LogWarning($"Manipulable object '{other.gameObject.name}' does not have the correct material shader!", other.gameObject);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<Manipulable>() != null)
		{
			// Turn off manipulable object glow

			Material manipulableMaterial = other.GetComponent<MeshRenderer>().material;
			if (manipulableMaterial.HasProperty(materialID_IsGlowing))
			{
				manipulableMaterial.SetInt(materialID_IsGlowing, 0);
			}
			else
			{
				Debug.LogWarning($"Manipulable object '{other.gameObject.name}' does not have the correct material shader!", other.gameObject);
			}
		}
	}

	private void MoveManipulable()
	{
		// maxFocalDistance is different from manipulableTargetDepth (and must be larger) because it starts from the camera instead of from the player
		float maxFocalDistance = 10f;
		Vector3 pointInDistanceFromCamera = GameManager.PlayerCamera.transform.position + GameManager.PlayerCamera.transform.forward * maxFocalDistance;
		Vector3 newTargetPosition;
		float radius = 0.3f;
		float largestExtent = grabbedManipulable.Collider.bounds.GetLargestBoundsExtent() + objectBoundsPadding;
		float targetDepth = Mathf.Max(manipulableTargetDepth, largestExtent);
		
		// Spherecast from the camera forward up to the maxFocalDistance
		if (Physics.SphereCast(GameManager.PlayerCamera.transform.position, radius, GameManager.PlayerCamera.transform.forward, out RaycastHit hit, maxFocalDistance, 
			LayerMaskUtilities.IgnoreAllExceptLayer("Environment")))
		{
			Vector3 hitSphereCenter = hit.point + (hit.normal * radius);
			DebugExtension.DebugWireSphere(hitSphereCenter, Color.magenta, radius);

			// Use the focal point as the position if there was something in the way
			
			newTargetPosition = hitSphereCenter;
		}
		else
		{
			Vector3 fromPlayerToFocalPoint = pointInDistanceFromCamera - transform.position;
			
			// Make sure extending the target depth doesn't make the object clip into geometry or the player (if possible)

			// Spherecast from the player towards the focal point up to targetDepth in distance
			if (Physics.SphereCast(transform.position, radius, fromPlayerToFocalPoint.normalized, out hit, targetDepth, 
				LayerMaskUtilities.IgnoreAllExceptLayer("Environment")))
			{
				// Use the center of the hit sphere for the target point if there was something in the way
				
				Vector3 hitSphereCenter = hit.point + (hit.normal * radius);
				
				newTargetPosition = hitSphereCenter;
			}
			else
			{
				// When nothing is in the way extend the target depth fully
				
				newTargetPosition = transform.position + (fromPlayerToFocalPoint.normalized * targetDepth);
			}
		}

		// float distanceFromPlayerToTarget = Vector3.Distance(transform.position, newTargetPosition);
		// if (distanceFromPlayerToTarget < targetDepth)
		// {
		// 	// Don't allow the target to get closer than the targetDepth
		// 	
		// 	newTargetPosition = oldTargetPosition;
		// }
		// else // Depenetrate the new target
		// {
		// 	newTargetPosition += GetDepenetrationVector(newTargetPosition);
		// }
		
		//newTargetPosition += GetDepenetrationVector(newTargetPosition);

		// Find the velocity for the manipulable so it can move towards the target position

		Vector3 offsetFromTarget = newTargetPosition - grabbedManipulable.transform.position;
		// Arbitrarily boost the offset magnitude to reach the target faster.
		Vector3 newVelocity = offsetFromTarget * followAcceleration;

		// Clamp the manipulable's velocity by some reasonable amount
		
		if (newVelocity.magnitude > maxObjectVelocity)
		{
			newVelocity = newVelocity.normalized * maxObjectVelocity;
		}

		// Don't allow the velocity to overshoot the target (helps at low framerates)
		
		float distanceToTarget = Vector3.Distance(grabbedManipulable.transform.position, newTargetPosition);
		if (newVelocity.magnitude * Time.fixedDeltaTime > distanceToTarget)
		{
			newVelocity = newVelocity.normalized * distanceToTarget;
		}

		// Vector3 positionAfterAddedVelocity = grabbedManipulable.transform.position + newVelocity;
		// Vector3 depenetrationVector = GetDepenetrationVector(positionAfterAddedVelocity);
		//
		// if (depenetrationVector != Vector3.zero)
		// {
		// 	// Adjust the manipulable's velocity to ensure it doesn't clip into objects
		// 	positionAfterAddedVelocity += depenetrationVector;
		// 	Vector3 adjustedVelocity = grabbedManipulable.transform.position - positionAfterAddedVelocity;
		// 	grabbedManipulable.Rigidbody.velocity = adjustedVelocity;
		// }
		// else
		// {
		// 	// Set the manipulable's velocity
		// 	grabbedManipulable.Rigidbody.velocity = newVelocity;
		// }
		
		// Set the manipulable's velocity
		grabbedManipulable.Rigidbody.velocity = newVelocity;

		// Set new position for follow icon (debug only)
		
		manipTargetIcon.position = newTargetPosition;

		oldTargetPosition = newTargetPosition;
	}

	// private bool IsObjectInGrabArc(GameObject gameObject)
	// {
	// 	Vector3 directionVector = Vector3.ProjectOnPlane(gameObject.transform.position - transform.position, Vector3.up).normalized;
	// 	Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
	//
	// 	float angleFromForward = Vector3.Angle(forward, directionVector);
	//
	// 	return (angleFromForward < grabArc * 0.5f);
	// }
	
	/*private Vector3 GetDepenetrationVector(Vector3 startingPosition)
	{
		if (!IsManipulating())
		{
			Debug.LogWarning("GetDepenetrationVector() should not be used unless an object is being manipulated.");
			return Vector3.zero;
		}

		// Use the AABB to find and cache all the colliders we might be overlapping
		Array.Clear(collisionNeighbours, 0, MAX_NEIGHBOURS);
		int collisionNeighboursCount = Physics.OverlapBoxNonAlloc(startingPosition, grabbedManipulable.PaddedAABBCollider.bounds.extents, collisionNeighbours);
		DebugExtension.DebugBounds(grabbedManipulable.PaddedAABBCollider.bounds, Color.yellow);

		Vector3 depenetrationVector = Vector3.zero;
		
		for (int i = 0; i < collisionNeighboursCount; i++)
		{
			Collider collisionNeighbour = collisionNeighbours[i];
			
			// Don't compute penetration against the manipulable itself or triggers
			if (collisionNeighbour.gameObject == grabbedManipulable.gameObject ||
				collisionNeighbour.isTrigger)
			{
				// Don't compute penetration against the manipulable itself
				continue;
			}

			//Debug.Log(collisionNeighbour.gameObject.name);

			if (Physics.ComputePenetration(
				grabbedManipulable.PaddedAABBCollider, 
				startingPosition, 
				grabbedManipulable.transform.rotation,
				collisionNeighbour, 
				collisionNeighbour.gameObject.transform.position, 
				collisionNeighbour.gameObject.transform.rotation,
				out Vector3 direction, 
				out float distance
			))
			{
				// Draw a line showing the depenetration direction if overlapped
				Debug.DrawRay(startingPosition, direction * distance, Color.red);
				// Accumulate all depenetration vectors
				depenetrationVector += (direction * distance);
			}
		}
		
		Debug.DrawLine(startingPosition, startingPosition + depenetrationVector, Color.blue);
		return depenetrationVector;
	}*/

	private bool IsObjectInLOS(GameObject gameObject)
	{
		Vector3 direction = gameObject.transform.position - transform.position;
		float magnitude = direction.magnitude;
		direction = direction.normalized;
		return !Physics.Raycast(transform.position, direction, out RaycastHit hit, magnitude, LayerMaskUtilities.IgnoreAllExceptLayer("Environment"));
	}

	private bool IsPastDropDistance(Vector3 position)
	{
		return Vector3.Distance(position, transform.position) > (manipulableTargetDepth + manipulableDropDepthBuffer);
	}
}

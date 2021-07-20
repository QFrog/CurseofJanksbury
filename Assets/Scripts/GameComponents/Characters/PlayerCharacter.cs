using System;
using System.Collections;
using DG.Tweening;
using StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
///     The kinematic physics object representing the character which is controlled directly by
///     the player.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{
	[Header("Ground Movement Settings")] [Tooltip("The maximum speed that the character can move laterally while walking.")] [SerializeField]
	private float maxWalkSpeed = 7f;

	[Tooltip("The maximum speed that the character can move laterally while sprinting.")] [SerializeField]
	private float maxSprintSpeed = 10f;
	
	[Tooltip("The maximum amount of time the character can sprint for.")] [SerializeField] 
	private float maxSprintEnergy = 5f;
	
	[Tooltip("The multiplier by which sprint energy recharging is faster than real time.")] [SerializeField] 
	private float sprintRechargeMultiplier = 2f;
	
	[Tooltip("The multiplier by which sprint energy draining is faster than real time.")] [SerializeField] 
	private float sprintDrainMultiplier = 1.25f;
	
	[Tooltip("The maximum speed at which the character will rotate towards their movement direction.")] [SerializeField]
	private float turningRate = 6f;

	[Tooltip("How long in seconds it takes the character to rotate towards the shooting direction.")] [SerializeField]
	private float turnToShootTime = 0.3f;

	[Tooltip("The character's acceleration speed is equal to max walk speed multiplied by this value")] [SerializeField]
	private float acceleration = 5f;

	[Tooltip("The deceleration applied when the player is inputing movement. The deceleration caused by "
			 + "friction is equal to the character's velocity multiplied by this value")]
	[SerializeField]
	private float movingFriction = 2.6f;

	[Tooltip("The deceleration applied when the player isn't inputing any movement. The deceleration caused "
			 + "by friction is equal to the character's velocity multiplied by this value")]
	[SerializeField]
	private float brakingFriction = 6f;

	[Tooltip("Cutoff speed is the speed at which friction is no longer proportional to the character's "
			 + "velocity and speed decreases linearly. The higher the cutoff speed the less smoothly the character "
			 + "will come to a stop.")]
	[SerializeField]
	private float cutoffSpeed = 1.2f;

	[Tooltip("The amount of instantaneous downward force to add each frame while grounded to help keep the character "
			 + "on the ground.")]
	[SerializeField]
	private float stickingForce = 0.1f;

	[Header("Air Movement Settings")]
	[Tooltip("The maximum speed that the character can move vertically (i.e. how fast can the character fall)")]
	[SerializeField]
	private float terminalVelocity = 40.0f;

	[Tooltip("The constant downward force on the character")] [SerializeField]
	public float gravity = 16.0f;

	[Tooltip("The instantaneous upwards force applied when the jump button is pressed")] [SerializeField]
	private float jumpingForce = 5.0f;

	[Tooltip("The percentage of the friction force used while in the air. A value of zero means that there will "
			 + "be no air friction")]
	[SerializeField]
	private float frictionScaleInAir = 0.2f;

	[Tooltip("The character's air control is the percentage of movement acceleration allowed while not touching the "
			 + "ground (only affects lateral movement acceleration)")]
	[SerializeField]
	private float airControl = 0.4f;

	[Header("General Settings")]
	[SerializeField] private float maxTorsoTwistAngle = 120f;
	[SerializeField] private float maxUpwardsTorsoAngle = 335f;
	[SerializeField] private float focalDistance = 15f;
	[SerializeField] private Animator anim = null;
	[SerializeField] private Transform pivotBone = null;
	[SerializeField] private GameObject jankGunObject = null;
	
	public float SprintEnergy { get; private set; }
	public float MaxJankEnergy => manipulator.MaxJankEnergy;
	public float JankEnergy => manipulator.JankEnergy;
	public float MaxSprintEnergy => maxSprintEnergy;
	public HUDCanvas hudCanvas;
	public bool IsDead { get; private set; } = false;
	public float TorsoPivotAngle { get; private set; } = 0f;
	public ActionMap.PlayerActions PlayerInput => playerInput;

	private bool airborne = false;
	
	private Manipulator manipulator = null;
	private CharacterController characterCapsule;
	private Vector3 addedVelocity;
	private bool isJumpRedirected = false;
	private ControllerColliderHit controllerHitInfo;
	private float startingMaxWalkingSpeed;
	private Quaternion lastPivotBoneRotation;
	private Quaternion lastRotation;

	private Vector2 inputDirection = Vector2.zero;
	private ActionMap.PlayerActions playerInput;

	private PlayerCamera playerCamera;
	private Gamepad gamepad;

	private StateMachine<MovementState> fsm;
	private InputAction jump;

	[Header("Audio Settings")]
	public AudioSource audio;
	public AudioClip[] audioClips;
	private int gunNoise = 0;

	public enum MovementState
	{
		Airborne,
		Walking,
		// Sliding, //When character is walking on a surface over the slope limit
		Caught
	}

	public event Action<MovementState> OnChangeState;

	public MovementState GetCurrentState()
	{
		return fsm.State;
	}

	public Vector3 GetVelocity()
	{
		return characterCapsule.velocity;
	}

	public bool IsManipulating()
	{
		return manipulator.IsManipulating();
	}

	public bool IsTorsoTwistedBeyondMax()
	{
		return (TorsoPivotAngle > maxTorsoTwistAngle);
	}

	public void DisableControl()
	{
		fsm.ChangeState(MovementState.Caught, StateTransition.Overwrite);
	}
	
	private void Awake()
	{
		fsm = StateMachine<MovementState>.Initialize(this);
		fsm.Changed += Fsm_Changed;
	}

	// Use this for initialization
	private void Start()
	{
		audio = GetComponent<AudioSource>();
		playerInput = GameManager.ActionMap.Player;
		playerInput.Enable();
		manipulator = GetComponent<Manipulator>();
		characterCapsule = GetComponent<CharacterController>();
		GameManager.PlayerManager.GetInitialCharacterCapsuleData().SyncCharacterController(characterCapsule);
		playerCamera = GameManager.PlayerCamera;

		hudCanvas = FindObjectOfType<HUDCanvas>();
		// Start character in airborne state to account for the spawn point hovering above ground
		fsm.ChangeState(MovementState.Walking); // Important so that fsm.LastState is not invalid
		fsm.ChangeState(MovementState.Airborne);

		gamepad = Gamepad.current;

		startingMaxWalkingSpeed = maxWalkSpeed;
		SprintEnergy = maxSprintEnergy;
		
		manipulator.ProjectileSpawnTransform_Recoilless.position = manipulator.ProjectileSpawnTransform.position;
		manipulator.ProjectileSpawnTransform_Recoilless.rotation = manipulator.ProjectileSpawnTransform.rotation;

		// Remove the gun and related animations when we are in sequence 1
		if (SceneManager.GetActiveScene().buildIndex == 1)
		{
			anim.SetLayerWeight(1, 0f);
			anim.SetLayerWeight(2, 0f);
			Destroy(jankGunObject);
		}

		GameManager.PlayerManager.OnPlayerCaught += CaughtByEnemy;
	}
	
	private void LateUpdate()
	{
		if (!IsDead)
		{
			// Don't handle the jank gun or twist the torso in sequence 1
			if (SceneManager.GetActiveScene().buildIndex > 1)
			{
				if (!manipulator.IsManipulating())
				{
					// The pivot bone rotation gets cleared every frame so we save that untwisted rotation and reapply the old one
					Quaternion untwistedPivotRotation = pivotBone.rotation;
					pivotBone.rotation = lastPivotBoneRotation;
					
					// Calculate what angle away from the camera forward that the character controller is facing
					Vector3 lateralCharacterForward = transform.forward;
					lateralCharacterForward.y = 0f;
					Vector3 lateralCameraForward = GameManager.PlayerCamera.transform.forward;
					lateralCameraForward.y = 0f;
					TorsoPivotAngle = Vector3.Angle(lateralCharacterForward, lateralCameraForward);
					
					// Only rotate the pivot bone when the character is facing relatively forward
					if (!IsTorsoTwistedBeyondMax())
					{
						Vector3 gunForward;
						Vector3 gunPosition;
						
						// The recoilless transform follows along until we are shooting and then stays put
						if (anim.GetCurrentAnimatorStateInfo(2).IsName("Gun_Shoot") == false)
						{
							gunForward = manipulator.ProjectileSpawnTransform.forward;
							gunPosition = manipulator.ProjectileSpawnTransform.position;
							Debug.DrawRay(manipulator.ProjectileSpawnTransform.position, gunForward, Color.yellow);
						}
						else
						{
							gunForward = manipulator.ProjectileSpawnTransform_Recoilless.forward;
							gunPosition = manipulator.ProjectileSpawnTransform_Recoilless.position;
							Debug.DrawRay(manipulator.ProjectileSpawnTransform_Recoilless.position, gunForward, Color.yellow);
						}

						// Create an arbitrary target point in the distance to aim at
						Vector3 targetPosition = GameManager.PlayerCamera.transform.position + GameManager.PlayerCamera.transform.forward * focalDistance;
						DebugExtension.DebugWireSphere(targetPosition, Color.magenta, 0.2f);
						
						// Target direction is relative to the gun position
						Vector3 targetDirection = targetPosition - gunPosition;
						
						Quaternion aimOffsetFromTarget = Quaternion.FromToRotation(gunForward, targetDirection);

						 //Slerp the character's spine rotation from the current aim direction towards the target direction
						pivotBone.rotation = pivotBone.rotation.EaseOutSlerp((aimOffsetFromTarget * pivotBone.rotation), turningRate);
						
						// When looking up we start at 360 and decrease in angle, when looking down we start at 0 and increase in angle
						// We clamp the rotation so that Marcus doesn't break his back looking up
						if (pivotBone.localEulerAngles.x > 90f && pivotBone.localEulerAngles.x < maxUpwardsTorsoAngle)
						{
							pivotBone.localRotation = Quaternion.Euler(maxUpwardsTorsoAngle, pivotBone.localEulerAngles.y, 0f);
						}
						else
						{
							// We want yaw and pitch but remove roll from pivot bone rotation
							pivotBone.localRotation = Quaternion.Euler(pivotBone.localEulerAngles.x, pivotBone.localEulerAngles.y, 0f);
						}
					
						lastPivotBoneRotation = pivotBone.rotation;
					}
					else // Slerp back towards normal orientation when facing backwards (past maxTorsoTwistAngle)
					{
						pivotBone.rotation = pivotBone.rotation.EaseOutSlerp(untwistedPivotRotation, turningRate);
						lastPivotBoneRotation = pivotBone.rotation;
					}
				}

				// JANK GUN - SHOOT ---------------------------

				int animationLayer = 2;
				if (playerInput.Jankify.WasPressedThisFrame()
					&& JankEnergy > manipulator.ShootEnergyCost
					&& !playerInput.Manipulate.IsPressed() 
					&& anim.GetCurrentAnimatorStateInfo(animationLayer).IsName("Gun_Idle") 
					&& !anim.GetBool("isShooting"))
				{
					manipulator.ShootJankifyProjectile();
					gunNoise = gunNoise == 2 ? 0 : gunNoise + 1;
					audio.PlayOneShot(audioClips[gunNoise]);
					anim.SetBool("isShooting", true);
					manipulator.ProjectileSpawnTransform_Recoilless.position = manipulator.ProjectileSpawnTransform.position;
					manipulator.ProjectileSpawnTransform_Recoilless.rotation = manipulator.ProjectileSpawnTransform.rotation;
					
				}
				else if (anim.GetCurrentAnimatorStateInfo(animationLayer).IsName("Gun_Shoot"))
				{
					anim.SetBool("isShooting", false);
				}

				// JANK GUN - MANIPULATE ----------------------
				
				if (playerInput.Manipulate.WasPressedThisFrame())
				{
					manipulator.ShootManipulateBeam();
				}
				
				if (playerInput.Manipulate.IsPressed() && manipulator.IsManipulating())
				{
					anim.SetBool("manipulating", true);
					
					// Override the standard rotation to lock to the camera forward
					Vector3 cameraLateralDirection = Vector3.ProjectOnPlane(GameManager.PlayerCamera.transform.forward, Vector3.up);
					transform.rotation = lastRotation.EaseOutSlerp(Quaternion.LookRotation(cameraLateralDirection), turningRate);
				}
				else if (playerInput.Manipulate.WasReleasedThisFrame() || !manipulator.IsManipulating())
				{
					if (airborne)
					{
						fsm.ChangeState(MovementState.Airborne);
					}
					else
					{
						fsm.ChangeState(MovementState.Walking);
						
						// if (IsGroundWithinSlopeLimit())
						// {
						// 	fsm.ChangeState(MovementState.Walking);
						// }
						// else
						// {
						// 	fsm.ChangeState(MovementState.Sliding);
						// }
					}

					manipulator.DropManipulable();
					anim.SetBool("manipulating", false);
				}
			}

			// Sprint recharge
			if (!playerInput.Run.IsPressed() && SprintEnergy < maxSprintEnergy)
			{
				// update sprint bar to go up over time
				SprintEnergy += Time.deltaTime * sprintRechargeMultiplier;
			
				// Don't let it go over maxSprintEnergy
				SprintEnergy = Mathf.Min(SprintEnergy, maxSprintEnergy);
			}

			lastRotation = transform.rotation;
		}

		// Debugging gizmos
		Debug.DrawRay(transform.position, DesiredMovementDirection(), Color.red);
		Debug.DrawRay(transform.position, addedVelocity, Color.yellow);
		Debug.DrawRay(transform.position, characterCapsule.velocity, Color.black);
	}

	public void PlayJankifyNoise() {
		audio.PlayOneShot(audioClips[4]);
	}

	public void PlayUnJankifyNoise() {
		audio.PlayOneShot(audioClips[5]);
	}

	public void PlayJumpNoise() {
		int x_random = UnityEngine.Random.Range(7, 9);
		audio.PlayOneShot(audioClips[x_random]);
	}


	public void ManipulatingNoise() {
		audio.loop = true;
		audio.clip = audioClips[6];
		audio.Play();
	}

	public void StopManipulatingNoise() {
		audio.loop = false;
		audio.Stop();
	}

	private void OnDestroy()
	{
		fsm.DestroyFSM();
		fsm = null;
	}

	public void CheckEnemyMusic() {
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
			coroutine = StartFade(audio, 2, 0);
			StartCoroutine(coroutine);
			
			audio = GameObject.Find("Level_3").GetComponent<AudioSource>();
			coroutine = StartFade(audio, 2, 0);
			StartCoroutine(coroutine);
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

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		controllerHitInfo = hit;
	}

	/// <summary>
	///     Call this method to put the character into the caught state and effectively "end" the
	///     level as a game over.
	/// </summary>
	private void CaughtByEnemy()
	{
		if (IsDead) return; // Skip if already dead

		jankGunObject.transform.parent = null;
		jankGunObject.GetComponent<Rigidbody>().isKinematic = false;
		jankGunObject.GetComponent<Collider>().enabled = true;
		StartCoroutine(CleanUpJankGun(jankGunObject));
		
		// Turn off upper body layers so the death animation can work properly
		anim.SetLayerWeight(1, 0f);
		anim.SetLayerWeight(2, 0f);

		IsDead = true;
		fsm.ChangeState(MovementState.Caught, StateTransition.Overwrite);
		anim.SetBool("isDead", true);

		int x_random = UnityEngine.Random.Range(10, 12);
		audio.PlayOneShot(audioClips[x_random]);
		
		manipulator.DropManipulable();

		GameManager.PlayerManager.OnPlayerCaught -= CaughtByEnemy;
	}

	private IEnumerator CleanUpJankGun(GameObject gameObject)
	{
		// Wait for death animation to finish
		yield return new WaitForSeconds(4.4f);
		
		Destroy(gameObject);
	}

	private void Fsm_Changed(MovementState obj)
	{
		//Debug.Log(obj); // Log out state changes

		// Broadcast the state change out to anyone whose subscribed
		OnChangeState?.Invoke(obj);
	}

	private void Airborne_Enter()
	{
		airborne = true;
		anim.SetBool("isAirborne", true);
	}

	private void Airborne_Update()
	{
		// Redirects the velocity of the character to be parallel to the surface it is touching when colliding with something while airborne.
		// This stops the character from sticking to sloped ceilings when jumping into them.
		if (!isJumpRedirected && characterCapsule.collisionFlags != CollisionFlags.None)
		{
			addedVelocity = Vector3.ProjectOnPlane(addedVelocity, controllerHitInfo.normal);

			// Only redirect the jump once per jump, otherwise the character will stick to the ceiling when jumping and moving forward
			// towards the slope. This is because the character will continue to collide with the ceiling as it follows the slope down,
			// which constantly re-projects the character's velocity parallel to the ceiling.
			isJumpRedirected = true;
		}

		Vector3 movementForce = MovementForce();
		// Scale the amount of movement force applied while airborne.
		movementForce *= airControl;

		Vector3 frictionForce = FrictionForce();
		// If we are in the air then we don't want friction to slow our fall.
		frictionForce.y = 0.0f;
		// Scale the amount of friction force applied while airborne.
		frictionForce *= frictionScaleInAir;

		// Acceleration is the sum of all forces acting on the character.
		Vector3 acceleration = movementForce + frictionForce + GravityForce();

		UpdateCharacterPosition(acceleration);
		UpdateRotation(GetLateralVelocity());

		if (characterCapsule.isGrounded)
		{
			fsm.ChangeState(MovementState.Walking);
			
			// // Transition out of airborne state when the character hits the ground
			// if (IsGroundWithinSlopeLimit())
			// {
			// 	fsm.ChangeState(MovementState.Walking);
			// }
			// else
			// {
			// 	fsm.ChangeState(MovementState.Sliding);
			// }
		}
	}

	private void Airborne_Exit()
	{
		isJumpRedirected = false;
		airborne = false;

		anim.SetBool("isAirborne", false);
	}

	private void Walking_Update()
	{
		// Acceleration is the sum of all forces acting on the character.
		Vector3 acceleration = MovementForce() + GravityForce() + FrictionForce();

		if (playerInput.Jump.IsPressed())
		{
			acceleration += JumpForce();
			PlayJumpNoise();
			anim.SetBool("jumped", true);
			StartCoroutine(FrameDelayedJumpDisable());
			fsm.ChangeState(MovementState.Airborne);
		}
		else if (IsGroundWithinSlopeLimit())
		{
			// Make the character stick to the ground while moving down slopes.
			// This also helps ensure that isGrounded is true when it should be.
			addedVelocity.y -= stickingForce / Time.deltaTime;
		}

		// SPRINTING
		if (playerInput.Run.IsPressed() && SprintEnergy > 0f)
		{
			DOTween.Kill(1);
			DOTween.To(()=> maxWalkSpeed, x=> maxWalkSpeed = x, maxSprintSpeed, 0.5f).SetId(0);
			SprintEnergy -= Time.deltaTime * sprintDrainMultiplier; // update sprint bar to go down over time

			// Stop sprinting if out of energy
			if (SprintEnergy <= 0f)
			{
				DOTween.Kill(0);
				DOTween.To(()=> maxWalkSpeed, x=> maxWalkSpeed = x, startingMaxWalkingSpeed, 0.5f).SetId(1);
				// Set sprint energy lower than zero so we have a buffer before the player can start sprinting again
				SprintEnergy = -1f;
			}
		}
		else
		{
			DOTween.Kill(0);
			DOTween.To(()=> maxWalkSpeed, x=> maxWalkSpeed = x, startingMaxWalkingSpeed, 0.5f).SetId(1);
		}

		UpdateCharacterPosition(acceleration);
		UpdateRotation(GetLateralVelocity());

		// Match up y velocity when possible to prevent the buildup of downward velocity while grounded
		// Excluding upward movement outside the slope limit prevents super bounce behaviour
		// Forcing all downward velocity to match prevents overly accelerated falls from ledges
		if (IsGroundWithinSlopeLimit() || characterCapsule.velocity.y <= 0f)
		{
			// Side effect: This effectively projects the addedVelocity onto the surface the character is walking on
			addedVelocity.y = characterCapsule.velocity.y;
		}

		if (characterCapsule.isGrounded)
		{
			// if (IsGroundWithinSlopeLimit() == false)
			// {
			// 	fsm.ChangeState(MovementState.Sliding);
			// }
		}
		else
		{
			fsm.ChangeState(MovementState.Airborne);
		}

		anim.SetFloat("Speed", characterCapsule.velocity.magnitude);
	}

	// Do this so the jump flag is toggled off in a frame regardless of transition
	private IEnumerator FrameDelayedJumpDisable()
	{
		yield return 0; // Wait a frame
		
		anim.SetBool("jumped", false);
	}

	// private void Sliding_Update()
	// {
	// 	Vector3 acceleration = MovementForce()*0.5f + GravityForce();
	//
	// 	if (playerInput.Jump.IsPressed())
	// 	{
	// 		acceleration += JumpForce();
	// 		PlayJumpNoise();
	// 		anim.SetBool("jumped", true);
	// 		StartCoroutine(FrameDelayedJumpDisable());
	// 		fsm.ChangeState(MovementState.Airborne);
	// 	}
	//
	// 	Vector3 hitNormal = controllerHitInfo.normal;
	// 	Vector3 slidingDirection = Vector3.Cross(transform.right, hitNormal).normalized;
	// 	if (slidingDirection.y > 0)
	// 	{
	// 		slidingDirection = -slidingDirection;
	// 	}
	// 	slidingDirection *= 20f; //slide speed multiple
	// 	
	// 	acceleration += slidingDirection;
	//
	// 	UpdateCharacterPosition(acceleration);
	// 	UpdateRotation(GetLateralVelocity());
	// 	
	// 	if (IsGroundWithinSlopeLimit())
	// 	{
	// 		fsm.ChangeState(MovementState.Walking);
	// 	}
	// 	else if (!characterCapsule.isGrounded)
	// 	{
	// 		fsm.ChangeState(MovementState.Airborne);
	// 	}
	// }

	private void Caught_Update()
	{
		Vector3 acceleration = GravityForce() + FrictionForce();
		
		UpdateCharacterPosition(acceleration);
		
		if (playerInput.Continue.IsPressed())
		{
			GameManager.ResetLevel();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "EnemyKillZone")
		{
			EnemyCharacter enemyCharacter = other.transform.parent.gameObject.GetComponent<EnemyCharacter>();
			if (!enemyCharacter.isDying)
			{
				enemyCharacter.PlayAttack();
				GameManager.PlayerManager.BecomeCaughtByEnemy();
			}
		}
	}

	/// <summary>
	///     Updates the position and rotation of the character using the given acceleration vector.
	///     The character will move and slide against level geometry using CharacterController.Move().
	/// </summary>
	/// <param name="acceleration"> The acceleration to be applied to the character. </param>
	private void UpdateCharacterPosition(Vector3 acceleration)
	{
		// Add the acceleration, the sum of all forces acting on the character, (per second) to the character's velocity (vel += acc * dt).
		addedVelocity += acceleration * Time.deltaTime;

		// Limit the maximum lateral and vertical movement speed of the character.
		addedVelocity = LimitedCharacterVelocity(addedVelocity);

		// Add the character's velocity (per second) to the character's positon (pos += vel * dt).
		characterCapsule.Move(addedVelocity * Time.deltaTime);
	}

	// Update rotation to match the velocity direction.
	private void UpdateRotation(Vector3 targetDirection)
	{
		// Force to be lateral
		targetDirection.y = 0f;
		
		if (targetDirection != Vector3.zero)
		{
			transform.rotation = transform.rotation.EaseOutSlerp(Quaternion.LookRotation(targetDirection), turningRate);
		}
	}

	private Vector3 GetLateralVelocity()
	{
		Vector3 lateralVelocity = characterCapsule.velocity;
		lateralVelocity.y = 0f;

		return lateralVelocity;
	}

	/// <summary> The lateral force generated by player input meant to represent locomotion. </summary>
	/// <returns> The lateral movement force vector. </returns>
	private Vector3 MovementForce()
	{
		return maxWalkSpeed * acceleration * DesiredMovementDirection();
	}

	/// <summary> The constant downward force which is always applied the character. </summary>
	/// <returns> The gravity force vector. </returns>
	private Vector3 GravityForce()
	{
		return Vector3.down * gravity;
	}

	/// <summary>
	///     The force which counteracts the current character velocity and will eventually bring it
	///     to a stop.
	/// </summary>
	/// <returns> The friction force vector. </returns>
	private Vector3 FrictionForce()
	{
		// If there is no movement input from the player this frame
		if (DesiredMovementDirection() == Vector3.zero)
		{
			// Cut off the end of the velocity curve when decelerating to a stop by decreasing character velocity linearly
			// so that the character comes to a stop more quickly. Without the linear deceleration the character will spend a
			// noticeable amount of time coming to a stop at extremely low speeds (i.e. stopping completely takes too long).
			if (characterCapsule.velocity.magnitude < cutoffSpeed)
			{
				// Instead of the friction force scaling with character's speed keep the friction force constant
				Vector3 frictionForce = -((addedVelocity.normalized * cutoffSpeed) * brakingFriction);

				// frictionForce.magnitude is multiplied by Time.deltaTime to convert it from the per second value it is currently in
				// to a per frame value. This conversion doesn't happen until acceleration is added to character velocity, so we have
				// to do it ahead of time here for this check to work properly, otherwise the frictionForce will be too large.
				if (addedVelocity.magnitude - (frictionForce.magnitude * Time.deltaTime) < 0f)
				{
					// If the current friction force would cause the character velocity to change direction (i.e. go negative)
					// then add the exact amount of friction force needed to bring the character velocity to zero.
					return -addedVelocity / Time.deltaTime;
				}
				else
				{
					return frictionForce;
				}
			}
			else // If we are not below the cutoff speed then apply friction normally
			{
				return -(addedVelocity * brakingFriction);
			}
		}
		else // If there IS movement input from the player
		{
			return -(addedVelocity * movingFriction);
		}
	}

	/// <summary> The amount of instantaneous force to add when the character jumps. </summary>
	/// <returns> The instantaneous upwards force vector for the jump. </returns>
	private Vector3 JumpForce()
	{
		// Division by deltaTime ensures all of jumpForce is added this frame, effectively canceling out the
		// multiplication by Time.deltaTime that happens when acceleration is added to character velocity.
		return Vector3.up * jumpingForce / Time.deltaTime;
	}

	/// <summary>
	///     Should be called once per Update() to limit the maximum lateral and vertical movement
	///     speed of the character.
	/// </summary>
	/// <param name="velocity"> The character's current velocity. </param>
	/// <returns> Returns the character velocity limited by max walk speed and terminal velocity. </returns>
	private Vector3 LimitedCharacterVelocity(Vector3 velocity)
	{
		Vector3 lateralVelocity = velocity;
		lateralVelocity.y = 0.0f;
		lateralVelocity = lateralVelocity.normalized * Mathf.Clamp(lateralVelocity.magnitude, 0.0f, maxWalkSpeed);

		Vector3 verticalVelocity = velocity;
		verticalVelocity.x = 0.0f;
		verticalVelocity.z = 0.0f;
		verticalVelocity = verticalVelocity.normalized * Mathf.Clamp(verticalVelocity.magnitude, 0.0f, terminalVelocity);

		Vector3 limitedCharacterVelocity = lateralVelocity + verticalVelocity;

		return limitedCharacterVelocity;
	}

	/// <summary>
	///     Used to give a direction to the movement force added to the character as well as to check
	///     if the player is currently inputting any movement (i.e. they are trying to move).
	/// </summary>
	/// <returns>
	///     Returns the desired movement direction vector created by the input of the player in this
	///     frame.
	/// </returns>
	private Vector3 DesiredMovementDirection()
	{
		// Vector2 inputDirection = Vector2.zero;
		// Vector3 actualInputDirection = Vector3.zero;
		// Reset input vector to zero each frame so that the added vector inputs don't accumulate
		// playerActions.Move.performed += context => inputDirection = context.ReadValue<Vector2>();
		// playerActions.Player.Move.performed += context => inputDirection = context.ReadValue<Vector2>();
		// actualInputDirection = inputDirection;
		// Debug.Log(inputDirection);
		// Debug.Log(actualInputDirection);

		Vector3 inputDirection = new Vector3(playerInput.Move.ReadValue<Vector2>().x, 0f, playerInput.Move.ReadValue<Vector2>().y);

		// Transform the entered input direction into a useful world direction vector orieted to the player camera's forward vector
		Vector3 desiredMovementDirection = playerCamera.transform.TransformDirection(inputDirection);
		// Transform into lateral only input
		desiredMovementDirection = Vector3.ProjectOnPlane(desiredMovementDirection, Vector3.up);
		// Prevent diagonal movement from being faster.
		desiredMovementDirection.Normalize();

		return desiredMovementDirection;
	}

	/// <summary>
	///     Gets the angle in degrees the plane the character is standing on is from the horizontal
	///     plane. Note: This angle will never be negative because the character can not stand on walls or
	///     ceilings.
	/// </summary>
	/// <returns> The slope angle in degrees or -1f as a sentinal value if there is no surface to walk on. </returns>
	private float GetWalkAngle()
	{
		if (characterCapsule.isGrounded
			&& controllerHitInfo != null)
		{
			Vector3 controllerBottomWithoutHemisphere = transform.position;
			controllerBottomWithoutHemisphere.y -= ((characterCapsule.height / 2.0f) - characterCapsule.radius);

			Vector3 capsuleToCollsion = controllerHitInfo.point - controllerBottomWithoutHemisphere;

			float walkAngle = Vector3.Angle(Vector3.down, capsuleToCollsion);

			return walkAngle;
		}

		return -1f;
	}

	/// <summary> Returns whether or not the character is standing on a surface within the slope limit. </summary>
	/// <returns> True if the surface is within the slope limit and false if not. </returns>
	private bool IsGroundWithinSlopeLimit()
	{
		float angle = GetWalkAngle();
		return (angle >= 0f && angle <= characterCapsule.slopeLimit);
	}

	public void PlayEnemyDeath() {
		int random_voice = UnityEngine.Random.Range(0, 2);
		if (random_voice == 0) {
			int x_random = UnityEngine.Random.Range(13, 15);
			audio.PlayOneShot(audioClips[x_random]);
		} else {
			int[] nums = new int[] {29, 34};
			FindObjectOfType<Seq_2_Dialogue>().PlayDialogue(nums);
		}
	}
}

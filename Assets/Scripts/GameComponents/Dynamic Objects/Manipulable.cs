using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Pathfinding;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(Rigidbody))]
public class Manipulable : MonoBehaviour, ISerializationCallbackReceiver
{
	// Set a min and max variable on each interactable object
	[ReadOnly] [BoxGroup("JankifiedState")] [ValidateInput("IsJankified", "This object is currently in the jankified state!")]
	public bool isJankified = false;

	[BoxGroup("Scaling")] [ReadOnly] [FormerlySerializedAs("minScale")] public Vector3 normalScale = new Vector3(1f, 1f, 1f);
	[BoxGroup("Scaling")] [ReadOnly] [FormerlySerializedAs("maxScale")] public Vector3 jankifiedScale = new Vector3(1f, 1f, 1f);
	[BoxGroup("Scaling")] public float jankifyScalingSpeed = 1f;
	
	[BoxGroup("Rotation")] [ReadOnly] public Vector3 normalRotation = new Vector3(0f, 0f, 0f);
	[BoxGroup("Rotation")] [ReadOnly] public Vector3 jankifiedRotation = new Vector3(0f, 0f, 0f);
	[BoxGroup("Rotation")] public float jankifyRotationSpeed = 1f;
	
	[NonSerialized] public Vector3 respawnPoint;
	[NonSerialized] public bool isChanged;// marker to know the object has been moved
	[NonSerialized] public bool inEnemyRange = false;

	public Rigidbody Rigidbody { get; private set; }
	public NavmeshCut NavmeshCutter { get; private set; }
	public Collider Collider { get; private set; }
	//public BoxCollider PaddedAABBCollider { get; private set; }

	private const float COLLIDER_PADDING_RATIO = 1.2f;
	
	public Vector3 OriginalScale { get; private set; }
	public Vector3 OriginalRotation { get; private set; }
	public bool OriginalJankifyState { get; private set; }

	// BEGIN ISerializationCallbackReceiver IMPLEMENTATION
	
	public void OnBeforeSerialize()
	{
	#if UNITY_EDITOR
		// Record changes to transform for a better Undo/Redo experience
		Undo.RecordObject(transform, $"{gameObject.name}.transform");
	#endif
	}

	public void OnAfterDeserialize() { }

	// END ISerializationCallbackReceiver IMPLEMENTATION

	private void Start()
	{
		respawnPoint = gameObject.transform.position;

		OriginalJankifyState = isJankified;
		if (isJankified)
		{
			OriginalScale = jankifiedScale;
			OriginalRotation = jankifiedRotation;
		}
		else
		{
			OriginalScale = normalScale;
			OriginalRotation = normalRotation;
		}

		// if (IsScaleGreaterThanMax())
		// {
		// 	Debug.LogWarning($"The manipulable object '{gameObject.name}' has a scale greater than the set maximum.", gameObject);
		// }
		// if (IsScaleLessThanMin())
		// {
		// 	Debug.LogWarning($"The manipulable object '{gameObject.name}' has a scale less than the set minimum.", gameObject);
		// }
		
		Rigidbody = GetComponent<Rigidbody>();
		Collider = GetComponent<Collider>();
		NavmeshCutter = GetComponentInChildren<NavmeshCut>();

	// #if UNITY_EDITOR
	// 	if (EditorApplication.isPlaying) // Need this or extra components get permentantly added each play thanks to ExecuteInEditMode
	// 	{
	// 		PaddedAABBCollider = gameObject.AddComponent<BoxCollider>();
	// 		// Must be doubled because extents is half of size
	// 		PaddedAABBCollider.size = (Collider.bounds.extents * 2f) * COLLIDER_PADDING_RATIO;
	// 		// We use this for depenetration not real collision
	// 		PaddedAABBCollider.isTrigger = true;
	// 	}
	// #else
	// 	PaddedAABBCollider = gameObject.AddComponent<BoxCollider>();
	// 	// Must be doubled because extents is half of size
	// 	PaddedAABBCollider.size = (Collider.bounds.extents * 2f) * COLLIDER_PADDING_RATIO;
	// 	// We use this for depenetration not real collision
	// 	PaddedAABBCollider.isTrigger = true;
	// #endif
	}
	
#if UNITY_EDITOR

	private void OnEnable()
	{
		EditorApplication.update += EditorUpdate;
	}

	private void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
	}
	
	// Draw a red bounds around the object to make it clear that it is in a jankified state
	void OnDrawGizmos()
	{
		if (Collider != null && isJankified)
		{
			DebugExtension.DrawBounds(Collider.bounds, Color.red);
		}
	}
	
	/// <summary>
	/// Called when this component is added to a game object.
	/// </summary>
	private void Reset()
	{
		ResetManipulableProperties();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.name == "DropObjectZone")
		{
			inEnemyRange = true;
		}
	}

	/// <summary>
	/// Resets the manipulable object's component properties to fix the required settings and avoid issues.
	/// </summary>
	public void ResetManipulableProperties()
	{
		if (gameObject.TryGetComponent(out MeshRenderer meshRenderer))
		{
			if (gameObject.GetComponent<Collider>() == null)
			{
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.convex = true;
				Debug.LogWarning($"The manipulable object '{gameObject.name}' had no collider. Adding a convex mesh collider.", gameObject);
			}

			if (meshRenderer.sharedMaterial == null)
			{
				meshRenderer.sharedMaterial = Resources.Load<Material>("ManipulableObject");
				Debug.LogWarning($"The manipulable object '{gameObject.name}' did not have a material set. Adding the default ManipulableObject material.", gameObject);
			}
			else
			{
				Shader manipulableShader = Shader.Find("Shader Graphs/ManipulableObject");
				if (meshRenderer.sharedMaterial.shader != manipulableShader)
				{
					meshRenderer.sharedMaterial = Resources.Load<Material>("ManipulableObject");
					Debug.LogWarning($"The manipulable object '{gameObject.name}' was not using the correct shader. Adding the default ManipulableObject material.", gameObject);
				}	
			}
		}

		if (gameObject.TryGetComponent(out Rigidbody existingRigidbody))
		{
			existingRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			existingRigidbody.useGravity = true;
			existingRigidbody.isKinematic = false;
			existingRigidbody.constraints = RigidbodyConstraints.None;
		}
		else
		{
			Rigidbody rb = gameObject.AddComponent<Rigidbody>();
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			rb.useGravity = true;
			rb.isKinematic = false;
			rb.constraints = RigidbodyConstraints.None;
			Debug.LogWarning($"The manipulable object '{gameObject.name}' did not have a rigidbody. Adding a rigidbody.", gameObject);
		}

		if (gameObject.GetComponentInChildren<NavmeshCut>() == null)
		{

			GameObject cutter = (GameObject)PrefabUtility.InstantiatePrefab(PrefabReferences.NavmeshCutterPrefab);
			cutter.transform.parent = gameObject.transform;
			cutter.transform.localPosition = new Vector3(0, 0, 0);
			cutter.transform.localEulerAngles = new Vector3(0, 0, 0);
			Debug.LogWarning($"The manipulable object '{gameObject.name}' did not have a navmesh cutter. Adding a NavmeshCutter prefab as a child object.", gameObject);
		}

		if (gameObject.GetComponentInChildren<ParticleSystem>() == null)
		{
			
			GameObject particles = (GameObject)PrefabUtility.InstantiatePrefab(PrefabReferences.ManipulableParticleSystemPrefab);
			
			Quaternion orignalTransform = gameObject.transform.rotation;
			gameObject.transform.rotation = Quaternion.identity;

			particles.transform.parent = gameObject.transform;
			//particles.transform.SetGlobalScale( Collider.bounds.extents);
			var shape = particles.GetComponent<ParticleSystem>().shape;
			shape.scale = Collider.bounds.extents;
			
			gameObject.transform.rotation = orignalTransform;
			particles.transform.localPosition = new Vector3(0, 0, 0);
			particles.transform.localEulerAngles = gameObject.transform.localEulerAngles;
			//particles.transform.SetGlobalScale(Vector3.one);
			//particles.transform.localEulerAngles = new Vector3(0, 0, 0);


			//particles.transform.localScale.SetX(Collider.bounds.extents.x/gameObject.transform.localScale.x);
			//particles.transform.localScale.SetY(Collider.bounds.extents.y / gameObject.transform.localScale.y);
			//particles.transform.localScale.SetZ(Collider.bounds.extents.z / gameObject.transform.localScale.z);

			//particles.transform.localScale


			Debug.LogWarning($"The manipulable object '{gameObject.name}' did not have a partile system. Adding a Particle system prefab as a child object.", gameObject);
		}

		// if (IsScaleGreaterThanMax())
		// {
		// 	if (transform.localScale.x > jankifiedScale.x)
		// 	{
		// 		jankifiedScale.x = transform.localScale.x;
		// 	}
		// 	if (transform.localScale.y > jankifiedScale.y)
		// 	{
		// 		jankifiedScale.y = transform.localScale.y;
		// 	}
		// 	if (transform.localScale.z > jankifiedScale.z)
		// 	{
		// 		jankifiedScale.z = transform.localScale.z;
		// 	}
		// 	Debug.LogWarning($"The manipulable object '{gameObject.name}' had a scale greater than the set maximum. Changing max scale to the object scale.", gameObject);
		// }
		// if (IsScaleLessThanMin())
		// {
		// 	if (transform.localScale.x < normalScale.x)
		// 	{
		// 		normalScale.x = transform.localScale.x;
		// 	}
		// 	if (transform.localScale.y < normalScale.y)
		// 	{
		// 		normalScale.y = transform.localScale.y;
		// 	}
		// 	if (transform.localScale.z < normalScale.z)
		// 	{
		// 		normalScale.z = transform.localScale.z;
		// 	}
		// 	Debug.LogWarning($"The manipulable object '{gameObject.name}' had a scale less than the set minimum. Changing min scale to the object scale.", gameObject);
		// }
	}

	private void EditorUpdate()
	{
		if (EditorApplication.isPlaying) return;

		if (transform.hasChanged)
		{
			if (isJankified)
			{
				Undo.RecordObject(this, "Set Jankified As Current Transform State");
		
				jankifiedScale = transform.localScale;
				jankifiedRotation = transform.rotation.eulerAngles;
			}
			else
			{
				Undo.RecordObject(this, "Set Normal As Current Transform State");
		
				normalScale = transform.localScale;
				normalRotation = transform.rotation.eulerAngles;
			}
			
			transform.hasChanged = false;
		}
	}
	
	[Button]
	private void ToggleJankify()
	{
		Undo.RecordObject(this, "Toggled Jankify");
		
		// Toggle jankified scale/rotation in editor
		if (transform.localScale.Approximately(jankifiedScale) && transform.rotation.eulerAngles.Approximately(jankifiedRotation))
		{
			transform.localScale = normalScale;
			transform.rotation = Quaternion.Euler(normalRotation);
		}
		else
		{
			transform.localScale = jankifiedScale;
			transform.rotation = Quaternion.Euler(jankifiedRotation);
		}

		// Toggle jankified state
		isJankified = !isJankified;
	}

	/// Used for the isJankified field validation
	private bool IsJankified(bool value) => !value;

#endif
	
	// private bool IsScaleGreaterThanMax()
	// {
	// 	return (transform.localScale.x > jankifiedScale.x || transform.localScale.y > jankifiedScale.y || transform.localScale.z > jankifiedScale.z);
	// }
	//
	// private bool IsScaleLessThanMin()
	// {
	// 	return (transform.localScale.x < normalScale.x || transform.localScale.y < normalScale.y || transform.localScale.z < normalScale.z);
	// }
}

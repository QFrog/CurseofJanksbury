using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class KillTrigger : MonoBehaviour
{
	public Collider TriggerCollider { get; private set; }
	
	private void Start()
	{
		TriggerCollider = GetComponent<Collider>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerCharacter>() != null)
		{
			GameManager.PlayerManager.BecomeCaughtByEnemy();
		}
		else if (other.GetComponent<Manipulable>() != null)
		{
			Debug.Log($"{gameObject.name} starting respawn for {other.gameObject.name}", gameObject);
			other.transform.position = other.GetComponent<Manipulable>().respawnPoint;
			Manipulable manipulable = other.GetComponent<Manipulable>();
			if (manipulable != null)
			{
				manipulable.transform.localScale = manipulable.OriginalScale;
				manipulable.transform.eulerAngles = manipulable.OriginalRotation;
				manipulable.isJankified = manipulable.OriginalJankifyState;
			}
		}
	}
	
	private IEnumerator startRespawn(Collider other)
	{
		other.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
		yield return new WaitForSeconds(1);

		other.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		other.transform.position = other.GetComponent<Manipulable>().respawnPoint;
		Manipulable manipulable = other.GetComponent<Manipulable>();
		if (manipulable != null)
		{
			manipulable.transform.localScale = manipulable.OriginalScale;
			manipulable.transform.eulerAngles = manipulable.OriginalRotation;
			manipulable.isJankified = manipulable.OriginalJankifyState;
		}
	}

#if UNITY_EDITOR
	// Draw a red bounds around the object to make it clear that it is in a jankified state
	private void OnDrawGizmos()
	{
		if (TriggerCollider != null)
		{
			DebugExtension.DrawBounds(TriggerCollider.bounds, Color.red);
		}
	}
#endif
}

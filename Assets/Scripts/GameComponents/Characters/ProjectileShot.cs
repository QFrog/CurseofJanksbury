using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileShot : MonoBehaviour
{
    [SerializeField] float _speed = 15f;

    public void Launch(Vector3 direction, Vector3 initialVelocity)
    {
        direction.Normalize();
        transform.up = direction;
        GetComponent<Rigidbody>().velocity = (direction * _speed) + initialVelocity;
	}

    private void OnCollisionEnter(Collision collision)
    {
		Destroy(gameObject);
		
		Manipulable manipulable = collision.gameObject.GetComponent<Manipulable>();
		if (manipulable != null)
		{
			manipulable.transform.DOKill();

			if (manipulable.isJankified)
			{
				manipulable.transform.DOScale(manipulable.normalScale, manipulable.jankifyScalingSpeed).SetUpdate(UpdateType.Fixed); // Not sure if the fixed update is strictly required here
				manipulable.transform.DORotate(manipulable.normalRotation, manipulable.jankifyRotationSpeed).SetUpdate(UpdateType.Fixed);
				manipulable.isJankified = false;
				FindObjectOfType<PlayerCharacter>().PlayUnJankifyNoise();
			}
			else
			{
				manipulable.transform.DOScale(manipulable.jankifiedScale, manipulable.jankifyScalingSpeed).SetUpdate(UpdateType.Fixed);
				manipulable.transform.DORotate(manipulable.jankifiedRotation, manipulable.jankifyRotationSpeed).SetUpdate(UpdateType.Fixed);
				manipulable.isJankified = true;
				FindObjectOfType<PlayerCharacter>().PlayJankifyNoise();
			}
		}
	}

    void Start()
    {
        Destroy(gameObject, 5f);
    }
}

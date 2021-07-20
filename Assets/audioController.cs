using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioPlayer : MonoBehaviour
{
	private AudioSource audio;
	public AudioClip[] audioClips;
	private int index = 0;

	// Start is called before the first frame update
	void Start()
    {
        audio = gameObject.GetComponent<AudioSource>();
    }

	private void OnCollisionEnter(Collision collision)
	{
		//Debug.Log("collision entered");
		audio.PlayOneShot(audioClips[index]);

		index++;
		if (index >= audioClips.Length)
		{
			index = 0;
		}
	}

}

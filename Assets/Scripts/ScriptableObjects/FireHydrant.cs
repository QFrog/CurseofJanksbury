using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireHydrant : MonoBehaviour
{
    public ParticleSystem water;
    private bool waterStarted;
    public GameObject waterZone;

    // Start is called before the first frame update
    void Start()
    {
        waterStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision) {
        if (!waterStarted && collision.gameObject.GetComponent<ProjectileShot>())
		{
            Debug.Log("water time");
            waterStarted = true;
            waterZone.SetActive(true);
            water.Play();
		}
    }
}

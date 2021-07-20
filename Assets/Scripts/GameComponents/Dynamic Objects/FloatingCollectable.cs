using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCollectable : MonoBehaviour
{
    public float speed = 5f;
    public float floatHeight = 0.2f;
    
    private Vector3 originalPosition;
    
    private void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        transform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y + (speed * Time.deltaTime), Vector3.up);
        transform.position = new Vector3(transform.position.x, originalPosition.y + Mathf.Sin(Time.time) * floatHeight, transform.position.z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterParentRotation : MonoBehaviour
{
	//[SerializeField]
	private Transform parent;

	[SerializeField]
	private Vector3 parentRoation;

	[SerializeField]
	private Vector3 offset = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
		parent = transform.parent.GetComponent<Transform>();
		parentRoation = new Vector3(parent.localEulerAngles.x, parent.localEulerAngles.y, parent.localEulerAngles.z);
    }

    // Update is called once per frame
    void Update()
    {
		parentRoation = new Vector3(parent.localEulerAngles.x, parent.localEulerAngles.y, parent.localEulerAngles.z);
		transform.localEulerAngles = new Vector3(-parentRoation.x + offset.x, 0f + offset.y, -parentRoation.z + offset.z);
    }
}

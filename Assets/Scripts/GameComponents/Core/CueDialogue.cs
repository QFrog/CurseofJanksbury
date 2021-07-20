using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CueDialogue : MonoBehaviour
{
    public int[] index;
    public void OnTriggerEnter(Collider other) {
        FindObjectOfType<Seq_1_Dialogue>().PlayDialogue(index);
        Destroy(this.gameObject.GetComponent<BoxCollider>());
    }
}

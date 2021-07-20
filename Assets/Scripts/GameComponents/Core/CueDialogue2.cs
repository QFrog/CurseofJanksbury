using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CueDialogue2 : MonoBehaviour
{
    public int[] index;
    public void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            FindObjectOfType<Seq_2_Dialogue>().PlayDialogue(index);
            Destroy(this.gameObject.GetComponent<BoxCollider>());
        }
    }
}

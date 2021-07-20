using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seq_1_Dialogue : MonoBehaviour
{
	public AudioClip[] seq1Dialogue;
	public AudioSource audio;
    private Queue<int[]> queue;

    private void Start() {
        queue = new Queue<int[]>();
    }

    private void Update() {
        if (!audio.isPlaying && queue.Count > 0) {
            var x = queue.Dequeue();
            
            int x_random = x[0];
            if (x.Length > 1) {
                x_random = Random.Range(x[0], x[1] + 1);
            }
            StartCoroutine("PlaySound", x_random);
        }

    }
    public void PlayDialogue(int[] x) {
        queue.Enqueue(x);
    }

    public IEnumerator PlaySound(int x_random) {
        yield return new WaitForSeconds(1);
        audio.PlayOneShot(seq1Dialogue[x_random]);
    }
}

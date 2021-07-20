using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneTrigger : MonoBehaviour
{
    [SerializeField]
    private string sceneName = null;
    public void OnTriggerEnter(Collider other) {
        if (other.tag == "Player")
        {
            if (GameManager.PlayerCharacter != null && GameManager.PlayerCamera != null)
            {
                Destroy(GameManager.PlayerCamera);
                Destroy(GameManager.PlayerCharacter);
            }
            
            CutSceneLoader loader = FindObjectOfType<CutSceneLoader>();
            loader.gameObject.GetComponent<Canvas>().enabled = true;
            if (loader != null)
            {
                loader.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("Couldn't find a CutSceneLoader in the scene.");
            }
        }
    }
}
 

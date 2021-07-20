using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutSceneLoader : MonoBehaviour
{
	private ActionMap.UIActions UI_Input;
    [SerializeField]
    private GameObject videoScreen;
    [SerializeField]
    private VideoPlayer cutscene;
    [SerializeField]
    private RawImage fader;
    [SerializeField]
    private GameObject previousCanvasPanel = null;
    // private AsyncOperation operation;
    //private bool videoDone = false;
    private bool beginLoading = false;
    private string scene;
    //private bool paused = false;
    public Animator animator;

    public void Start() {
        fader.gameObject.SetActive(true);
        Resume();
        UI_Input = GameManager.ActionMap.UI;
		UI_Input.Enable();
        cutscene.frame = 0;
    }
    
    public void Update() {
        if (beginLoading && (!cutscene.isPlaying || UI_Input.Submit.WasPerformedThisFrame())) {
            Resume();
            animator.SetTrigger("FadeOut");
            beginLoading = false;
        }
    }

    public void Pause() {
        Time.timeScale = 0;
        //paused = true;
    }

    public void Resume() {
        Time.timeScale = 1;
        //paused = false;
    }

    public void LoadScene(string sceneName)
    {
        scene = sceneName;
        animator.SetTrigger("VideoFadeOut");
    }

    public void OnFadeComplete() {
        SceneManager.LoadScene(scene);
    }

    public void VideoFadedOut() {
        // if Main Menu, else if IN-SCENE
        if (previousCanvasPanel != null) {
            previousCanvasPanel.SetActive(false);
        } else {
            AudioSource[] allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
            foreach(AudioSource audioS in allAudioSources) {
                audioS.Stop();
            }

            EnemyCharacter[] allEnemies = FindObjectsOfType(typeof(EnemyCharacter)) as EnemyCharacter[];
            foreach(EnemyCharacter enemy in allEnemies) {
                Destroy(enemy.gameObject, 0.0f);
            }

            Debug.Log("why do you not play the video? or fade?");
            // MeshRenderer[] allMeshes = FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
            // foreach(MeshRenderer mesh in allMeshes) {
            //     Destroy(mesh.gameObject, 0.0f);
            // }
            
            FindObjectOfType<Camera>().backgroundColor= Color.black;
            //FindObjectOfType<HUDCanvas>().sceneIsPlaying = true;
        }
        videoScreen.SetActive(true);
        StartCoroutine(WaitForVideo());
    }
    
    public IEnumerator WaitForVideo() {
        yield return new WaitForSeconds(0.25f);
        
        animator.SetTrigger("VideoFadeIn");
        beginLoading = true;
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			LoadScene(scene);
		}
	}
}

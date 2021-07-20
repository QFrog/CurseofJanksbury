// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.Video;

// public class SceneController : MonoBehaviour
// {
//     public VideoClip clip;

//     [SerializeField] private GameObject cutScenceCanvas = null;
//     private GameObject videoFrame;
//     private GameObject videoContent;
//     private VideoPlayer video;
//     private AudioSource audioSource;

//     // find next cutscene
//     void Start()
//     {
//         cutScenceCanvas.SetActive(true);
//         // video frame - raw image
//         videoFrame = GameObject.FindGameObjectWithTag("Video Frame");
//         videoFrame.SetActive(false);

//         // video content - video player and audio source
//         videoContent = GameObject.FindGameObjectWithTag("Video Content");
//         video = videoContent.GetComponent<VideoPlayer>();
//         video.clip = clip; // set to correct video clip
//     }
//     //start next cutscene
//     void OnTriggerEnter(Collider other)
//     {
//         if (other.gameObject.tag == "Player")
//         {
//             Debug.Log("entered cutscence zone");
//             videoFrame.SetActive(true);
//             StartCoroutine(playVideo());
//             video.loopPointReached += loadScene;
//         }
//     }
//     /*IMPORTANT: No audio seems to work?*/

//     // setup so audio works as direct video doesn't work 
//     IEnumerator playVideo()
//     {
//         //Add AudioSource to the GameObject
//         audioSource = gameObject.AddComponent<AudioSource>();

//         //Disable Play on Awake for both Video and Audio
//         video.playOnAwake = false;
//         audioSource.playOnAwake = false;

//         //Prepare Audio to prevent Buffering
//         video.Prepare();

//         //Wait until video is prepared
//         while (!video.isPrepared)
//         {
//             Debug.Log("Preparing Video");
//             yield return null;
//         }

//         Debug.Log("Done Preparing Video");

//         //Set Audio Output to AudioSource
//         video.audioOutputMode = VideoAudioOutputMode.AudioSource;

//         //Assign the Audio from Video to AudioSource to be played
//         video.EnableAudioTrack(0, true);
//         video.SetTargetAudioSource(0, audioSource);
//         video.controlledAudioTrackCount = 1;
//         audioSource.volume = 1.0f;

//         //Play Video
//         video.Play();

//         Debug.Log("Playing Video");
//         while (video.isPlaying)
//         {
//             yield return null;
//         }

//         Debug.Log("Done Playing Video");
//     }
//     // load next scence after cutscene
//     void loadScene(UnityEngine.Video.VideoPlayer vp)
//     {
//         videoFrame.SetActive(false);
//         if (SceneManager.GetActiveScene().name == "MainMenu")
//         {
//             GameManager.loadSeq1();
//         }
//         else if (SceneManager.GetActiveScene().name == "Seq_01")
//         {
//             GameManager.loadSeq2();
//         }
//         else if (SceneManager.GetActiveScene().name == "seq_02")
//         {
//             GameManager.loadMainMenu();
//         }
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCanvas : MonoBehaviour
{
    [SerializeField] private GameObject optionsImage = null;
    [SerializeField] private GameObject creditsImage = null;
    [SerializeField] private GameObject blurredImage = null;
	private ActionMap.UIActions UI_Input;

    public GameObject Point;
  
    public int SelectedButton = 1;
    [SerializeField]
    private int NumberOfButtons = 4;

    public Transform ButtonPosition1;
    public Transform ButtonPosition2;
    public Transform ButtonPosition3;
    public Transform ButtonPosition4;
    [SerializeField]
    public string sceneName;

	[Header("Sounds")]
    public AudioClip menuUp;
    public AudioClip menuDown;

    private AudioSource audioSoruce;

    void Start()
    {
        audioSoruce = GetComponent<AudioSource>();
        UI_Input = GameManager.ActionMap.UI;
		UI_Input.Enable();
        creditsImage.SetActive(false);
        blurredImage.SetActive(false);
        optionsImage.SetActive(false);
    }

    void Update() 
    {
		if (UI_Input.Navigate.WasPerformedThisFrame()) {
            Vector2 direction = UI_Input.Navigate.ReadValue<Vector2>();
            
            if (direction.x == 0.0 && direction.y == 1.0) {
                if (SelectedButton > 1)
                {
                    audioSoruce.clip = menuDown;
                    SelectedButton -= 1;
                }
            } else if (direction.x == 0.0 && direction.y == -1.0) {
                if (SelectedButton < NumberOfButtons)
                {
                    audioSoruce.clip = menuUp;
                    SelectedButton += 1;
                }
            }

            audioSoruce.Play();

            MoveThePointer();
        }
    
        if (UI_Input.Submit.WasPerformedThisFrame()) {
            if (SelectedButton == 1)
            {
                NewGameButton();
            }
            else if (SelectedButton == 2)
            {
                OptionsButton();
            }
            else if (SelectedButton == 3)
            {
                CreditsButton();
            }
            else if (SelectedButton == 4)
            {
                ExitButton();
            }
        }
    }

     private void MoveThePointer()
    {
        // Moves the pointer
        if (SelectedButton == 1)
        {
            Point.transform.position = ButtonPosition1.position;
            optionsImage.SetActive(false);
            blurredImage.SetActive(false);
        }
        else if (SelectedButton == 2)
        {
            Point.transform.position = ButtonPosition2.position;
            blurredImage.SetActive(true);
            creditsImage.SetActive(false);
            optionsImage.SetActive(true);
        }
        else if (SelectedButton == 3)
        {
            Point.transform.position = ButtonPosition3.position;
            blurredImage.SetActive(true);
            creditsImage.SetActive(true);
            optionsImage.SetActive(false);
        }
        else if (SelectedButton == 4)
        {
            Point.transform.position = ButtonPosition4.position;
            creditsImage.SetActive(false);
            blurredImage.SetActive(false);
        }
    }

    public void NewGameButton()
    {
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
    
    public void OptionsButton()
    {
        // optionsImage.SetActive(true);
        // creditsImage.SetActive(false);
    }
    public void CreditsButton()
    {
        // optionsImage.SetActive(false);
        // creditsImage.SetActive(true);
    }

    public void ExitButton()
    {
        Debug.Log("Exiting game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

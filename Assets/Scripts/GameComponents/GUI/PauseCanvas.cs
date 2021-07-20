using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseCanvas : MonoBehaviour
{
    [SerializeField] private GameObject controlsImage = null;
    [SerializeField] private GameObject creditsImage = null;
    [SerializeField] private GameObject optionsImage = null;
    [SerializeField] private GameObject exit = null;
    [SerializeField] private GameObject restart = null;
    [SerializeField] private GameObject controls = null;
    [SerializeField] private GameObject credits = null;
    [SerializeField] private GameObject back = null;
    private ActionMap.UIActions UI_Input;

    public GameObject Point;

    public int SelectedButton = 1;
    public bool secondaryMenu = false;
    public bool volumeControl = false;
    [SerializeField]
    private int NumberOfButtons = 5;

    public Transform ButtonPosition1;
    public Transform ButtonPosition2;
    public Transform ButtonPosition3;
    public Transform ButtonPosition4;
    public Transform ButtonPosition5; // volume slider
    public Transform ButtonPosition6; // back button

    [Header("Sounds")]
    public AudioClip menuUp;
    public AudioClip menuDown;
    public AudioClip click;
    public float masterVolume = 1.0f;
    public GameObject slider;
    private Slider volumeSlider; 
    private AudioSource audioSource;

    public GameObject highlight;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UI_Input = GameManager.ActionMap.UI;
        UI_Input.Enable();
        creditsImage.SetActive(false);
        controlsImage.SetActive(false);
        volumeSlider = slider.GetComponent<Slider>();
    }

    void Update()
    {
        AudioListener.volume = volumeSlider.value;
        Vector2 direction = UI_Input.Navigate.ReadValue<Vector2>();
        if (UI_Input.Navigate.WasPerformedThisFrame() && !secondaryMenu)
        {

            if (direction.x == 1.0 && direction.y == 0.0)
            {
                if (SelectedButton < NumberOfButtons)
                {
                    audioSource.clip = menuUp;
                    SelectedButton += 1;
                }
            }
            else if (direction.x == -1.0 && direction.y == 0.0)
            {
                if (SelectedButton > 1)
                {
                    audioSource.clip = menuDown;
                    SelectedButton -= 1;
                }
            }
            else if (direction.x == 0.0 && direction.y == 1.0)
            {
                if (SelectedButton > 1)
                {
                    audioSource.clip = menuDown;
                    SelectedButton -= 1;
                }
            }
            else if (direction.x == 0.0 && direction.y == -1.0)
            {
                if (SelectedButton < NumberOfButtons)
                {
                    audioSource.clip = menuUp;
                    SelectedButton += 1;
                }
            }

            audioSource.Play();

            MoveThePointer();
        }

        if (UI_Input.Submit.WasPerformedThisFrame())
        {
            if (SelectedButton == 1) // exit
            {
                ExitButton();
            }
            else if (SelectedButton == 2) // restart
            {
                GameManager.ResetLevel();
            }
            else if (SelectedButton == 3) // control
            {
                SelectedButton = 6;
                secondaryMenu = true;
                MoveThePointer();
                ControlsButton();
            }
            else if (SelectedButton == 4) // credits
            {
                SelectedButton = 6;
                secondaryMenu = true;
                MoveThePointer();
                CreditsButton();
            }
            else if (SelectedButton == 5) // volume
            {
                volumeControl = true;
                highlight.SetActive(true);
            }
            else if (SelectedButton == 6) // back 
            {
                BackButton();
            }

            audioSource.clip = click;
            audioSource.Play();
        }
        // volume control
        if (volumeControl == true && UI_Input.Navigate.WasPerformedThisFrame())
        {
            secondaryMenu = true;
            Point.transform.position = ButtonPosition5.position;
            if (direction.x == 1.0 && volumeSlider.value < 1)
                {
                volumeSlider.value += 0.05f;
            }
                else if (direction.x == -1.0 && volumeSlider.value > 0.0001)
            {
                volumeSlider.value -= 0.05f;
            }
            else if (direction.y == 1.0 || direction.y == -1.0 || UI_Input.Submit.WasPerformedThisFrame())
            {
                highlight.SetActive(false);
                volumeControl = false;
                secondaryMenu = false;
                audioSource.Play();
            }
        }
    }

    private void MoveThePointer()
    {
        // Moves the pointer
        if (SelectedButton == 1)
        {
            Point.transform.position = ButtonPosition1.position;
        }
        else if (SelectedButton == 2)
        {
            Point.transform.position = ButtonPosition2.position;
        }
        else if (SelectedButton == 3)
        {
            Point.transform.position = ButtonPosition3.position;
        }
        else if (SelectedButton == 4)
        {
            Point.transform.position = ButtonPosition4.position;
        }
        else if (SelectedButton == 5) // volume slider
        {
            Point.transform.position = ButtonPosition5.position;
        }
        else if (SelectedButton == 6) // back button
        {
            Point.transform.position = ButtonPosition6.position;
        }
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

    public void RestartButton()
    {
        GameManager.ResetLevel();
    }

    public void ControlsButton()
    {
        // old
        optionsImage.SetActive(false);
        exit.SetActive(false);
        controls.SetActive(false);
        restart.SetActive(false);
        credits.SetActive(false);

        // new
        controlsImage.SetActive(true);
        back.SetActive(true);
        slider.SetActive(false);

    }

    public void CreditsButton()
    {
        // old
        optionsImage.SetActive(false);
        exit.SetActive(false);
        controls.SetActive(false);
        restart.SetActive(false);
        credits.SetActive(false);

        // new
        creditsImage.SetActive(true);
        back.SetActive(true);
        slider.SetActive(false);
    }

    public void BackButton()
    {
        secondaryMenu = false;
        SelectedButton = 1;
        MoveThePointer();
        slider.SetActive(true);
        // old
        creditsImage.SetActive(false);
        controlsImage.SetActive(false);
        back.SetActive(false);

        // old
        optionsImage.SetActive(true);
        exit.SetActive(true);
        controls.SetActive(true);
        restart.SetActive(true);
        credits.SetActive(true);
    }
    
}

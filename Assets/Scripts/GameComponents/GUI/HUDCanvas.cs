
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Maintains HUD state and handles the transitions between states.
/// </summary>
public class HUDCanvas : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas = null;
    [SerializeField] private GameObject pauseCanvas = null;
    [SerializeField] private GameObject cutScenceCanvas = null;
    [SerializeField] private GameObject crosshair = null;

    private bool gameOver = false;
    private bool gameIsPaused = false;
    private ActionMap.UIActions UI_Input;

    public Slider sprintBar;
    public Slider energyBar;
    public Slider healthBar;

	[Header("Sounds")]
    public AudioClip pause;
    public AudioClip lostGame;

    private AudioSource audioSource;
    public bool sceneIsPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UI_Input = GameManager.ActionMap.UI;
        UI_Input.Enable();
        GameManager.PlayerManager.OnPlayerCaught += PlayerCaught;

        //sprinting
        sprintBar.minValue = 0f;
        sprintBar.maxValue = GameManager.PlayerCharacter.MaxSprintEnergy;
        
        // energy
        energyBar.minValue = 0f;
        energyBar.maxValue = GameManager.PlayerCharacter.MaxJankEnergy;

        //heatlh
        healthBar.minValue = 0f;
        healthBar.maxValue = GameManager.PlayerManager.PlayerHealth;
        healthBar.value = healthBar.maxValue;
        if (GameObject.FindGameObjectsWithTag("CutsceneTrigger").Length == 0)
        {
            Debug.Log("No Cutscene trigger setup, disabling cutscenes.");
            cutScenceCanvas.SetActive(false);
        }
        // disable jank energy and crosshair in seq 1
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            energyBar.gameObject.SetActive(false);
            crosshair.SetActive(false);
        }
        else
        {
            energyBar.gameObject.SetActive(true);
            crosshair.SetActive(true);
        }
    }

    void Update()
    {
        // Sprint Bar
        sprintBar.value = Mathf.Max(GameManager.PlayerCharacter.SprintEnergy, 0f);
        
        // Energy Bar
        energyBar.value = Mathf.Max(GameManager.PlayerCharacter.JankEnergy, 0f);

        if (UI_Input.Pause.WasPerformedThisFrame() && !sceneIsPlaying)
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }

            audioSource.clip = pause;
            audioSource.Play();
        }
    }
    /// <summary>
    /// The callback for when the player signals that it has been caught by an enemy.
    /// Turns on the game over canvas.
    /// </summary>
    private void PlayerCaught()
    {
        healthBar.value = GameManager.PlayerManager.PlayerHealth;

        if (GameManager.PlayerManager.PlayerHealth <= 0)
        {
            gameOverCanvas.SetActive(!gameOver);
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseCanvas.SetActive(false);
        crosshair.SetActive(true);
        gameIsPaused = false;
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        pauseCanvas.SetActive(true);
        crosshair.SetActive(false);
        gameIsPaused = true;
    }

}


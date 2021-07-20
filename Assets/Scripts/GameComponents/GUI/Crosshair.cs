using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    private Image crosshairImage;

    private void Start()
    {
        crosshairImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 1) {
            if (crosshairImage.enabled && GameManager.PlayerCharacter.IsManipulating())
            {
                crosshairImage.enabled = false;
            }
            else if (!crosshairImage.enabled && !GameManager.PlayerCharacter.IsManipulating())
            {
                crosshairImage.enabled = true;
            }

            if (GameManager.PlayerCharacter.IsDead)
            {
                crosshairImage.enabled = false;
            }
        } else {
            crosshairImage.enabled = false;
        }
    }
}

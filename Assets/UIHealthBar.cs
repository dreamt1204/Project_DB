using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealthBar : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    Character ownerCharacter;
    UIProgressBar progressBar;
    float healthPercentage = 1.0f;

    //===========================
    //      Functions
    //===========================
    public void Init(Character character)
    {
        ownerCharacter = character;
        progressBar = GetComponent<UIProgressBar>();

        UpdateHealthBarColor();
    }

    void Update()
    {
        healthPercentage = ownerCharacter.CurrentHealth / ownerCharacter.health;
        progressBar.value = healthPercentage;
    }

    void UpdateHealthBarColor()
    {
        UISprite barSprite = transform.Find("Sprite_Bar").GetComponent<UISprite>();

        barSprite.spriteName = "Progress-Bar-Green";

        barSprite.spriteName = "Progress-Bar-Blue";

        barSprite.spriteName = "Progress-Bar-PinkDark";
    }
}

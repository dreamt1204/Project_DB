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

		gameObject.name = "Widget_HealthBar (" + ownerCharacter.gameObject.name + ")";
		GetComponent<UIFollowTarget>().target = ownerCharacter.transforms.Find("HealthBarPosition");

		progressBar = GetComponent<UIProgressBar>();
        UpdateHealthBarColor();
    }

    void Update()
    {
        healthPercentage = ownerCharacter.CurrentHealth / ownerCharacter.health;
        progressBar.value = healthPercentage;
    }

	// Set health bar color based on character owner and team condition
    void UpdateHealthBarColor()
    {
        UISprite barSprite = transform.Find("Sprite_Bar").GetComponent<UISprite>();

		if (ownerCharacter.isControllable)
		{
			barSprite.spriteName = "Progress-Bar-Green";
		}
		else
		{
			if (TeamUTL.IsCharacterMyTeamMate(ownerCharacter))
				barSprite.spriteName = "Progress-Bar-Blue";
			else
				barSprite.spriteName = "Progress-Bar-PinkDark";
		}
    }
}

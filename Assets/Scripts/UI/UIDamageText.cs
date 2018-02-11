using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDamageText : MonoBehaviour 
{
	//===========================
	//      Variables
	//===========================
	Character ownerCharacter;
	HUDText hudText;

	//===========================
	//      Functions
	//===========================
	public void Init(Character character)
	{
		ownerCharacter = character;

		gameObject.name = "Widget_DamageText (" + ownerCharacter.gameObject.name + ")";
		GetComponent<UIFollowTarget>().target = ownerCharacter.transforms.Find("DamageTextPosition");

		hudText = GetComponent<HUDText>();
	}

	public void ShowDamage(float damage)
	{
		hudText.AddUnsigned(damage, Color.white, 0f);
	}
}

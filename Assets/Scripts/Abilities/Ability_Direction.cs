using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Direction : Ability
{
    //===========================
    //      Variables
    //===========================
    // Prefabs
    public GameObject RangeIndicatorPrefab;

    // Variables
    protected GameObject RangeIndicator;
    protected Vector3 AimingVector;

    // UI elements
    protected UIJoyStick joystick;
	

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    public override void Init(Character character)
	{
        ownerCharacter = character;

        // Init UI button
        InitJoystick();

		// Init range indicator
		RangeIndicator = Instantiate(RangeIndicatorPrefab, ownerCharacter.transform.position, Quaternion.Euler(0, 0, 0));
		RangeIndicator.transform.parent = ownerCharacter.transform;
		RangeIndicator.SetActive(false);
    }

	public virtual void InitJoystick()
	{
		// ??
        //joystick.ability = this;
	}

    //---------------------------
    //      Update Functions
    //---------------------------
    void Update()
	{
        if (!Character.CheckAuthority(ownerCharacter))
            return;

        UpdateAiming();
	}

    public virtual void DisplayRangeIndicator(bool active)
    {
        RangeIndicator.SetActive(active);
    }

    public virtual void UpdateAiming()
	{
		float rotation = Mathf.Atan2(joystick.joyStickPosX, joystick.joyStickPosY) * 180 / Mathf.PI;
		RangeIndicator.transform.rotation = Quaternion.Euler(0, rotation, 0);
		AimingVector = new Vector3(joystick.joyStickPosX, 0, joystick.joyStickPosY).normalized;
	}

    //---------------------------
    //      Ability action
    //---------------------------
	public virtual void ShowAbility(bool show)
	{
		DisplayRangeIndicator(show);
        /*
		if (show)
			ownerCharacter.castingAbility = this;
		else
			ownerCharacter.castingAbility = null;
        */
	}

    public virtual void ActivateAbility()
	{
		
	}

	public virtual void BallHitAction(Collision col, Ball ball, Character hitCharacter)
	{

	}
}

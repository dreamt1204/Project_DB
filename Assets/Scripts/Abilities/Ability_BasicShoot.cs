using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_BasicShoot : Ability
{
    //===========================
    //      Variables
    //===========================
    // Prefabs
    public GameObject RangeIndicatorPrefab;

    // Variables
    GameObject RangeIndicator;
    Vector3 AimingVector;

    // UI elements
    UIJoyStick joystick;
    bool AimingJoystick = false;

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
        InitRangeIndicator();
    }

    void InitJoystick()
    {
        joystick = UIManager.GetJoystick("Widget_Joystick_BasicShoot");
        joystick.listenerObject = this.gameObject;
    }

    void InitRangeIndicator()
    {
        RangeIndicator = Instantiate(RangeIndicatorPrefab, ownerCharacter.transform.position, Quaternion.identity);
        RangeIndicator.transform.parent = this.transform;
    }

    //---------------------------
    //      Update Functions
    //---------------------------
    void Update()
    {
        UpdateAiming();
    }

    void UpdateAiming()
    {
        if (RangeIndicator.GetActive() != AimingJoystick)
            RangeIndicator.SetActive(AimingJoystick);

        if (AimingJoystick)
        {
            float rotation = Mathf.Atan2(joystick.joyStickPosX, joystick.joyStickPosY) * 180 / Mathf.PI;
            RangeIndicator.transform.rotation = Quaternion.Euler(0, rotation, 0);
            AimingVector = new Vector3(joystick.joyStickPosX, 0, joystick.joyStickPosY).normalized;
        }
    }

    //---------------------------
    //      Joystick Callbacks
    //---------------------------
    void OnJoystickDragged()
    {
        AimingShot(true);
    }

    void OnJoystickReleased()
    {
        AimingShot(false);
    }

    //---------------------------
    //      Ability Action
    //---------------------------
    void AimingShot(bool isAiming)
    {
        AimingJoystick = isAiming;

        if (!isAiming)
        {
            joystick.ResetJoystick();
        }
    }
}

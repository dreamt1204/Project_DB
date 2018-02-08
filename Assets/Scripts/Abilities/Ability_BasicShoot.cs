using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_BasicShoot : Ability
{
    //===========================
    //      Variables
    //===========================
    [Header("Prefabs")]
    public Ball ballPrefab;
    public GameObject rangeIndicatorPrefab;

    // Variables
    GameObject rangeIndicator;
    Vector3 aimingVector;

    // UI elements
    UIJoyStick joystick;
    bool isAimingJoystick = false;

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    public override void Init(Character character)
    {
        base.Init(character);

        // Initialization done by owner player
        if (ownerCharacter.isControllable)
        {
            // Init UI button
            InitJoystick();

            // Init range indicator
            InitRangeIndicator();
        }
    }

    void InitJoystick()
    {
        joystick = UIManager.GetJoystick("Widget_Joystick_BasicShoot");
        joystick.listenerObject = this.gameObject;
    }

    void InitRangeIndicator()
    {
        rangeIndicator = Instantiate(rangeIndicatorPrefab, ownerCharacter.transform.position, Quaternion.identity);
        rangeIndicator.transform.parent = this.transform;
    }

    //---------------------------
    //      Update Functions
    //---------------------------
    void Update()
    {
        if (ownerCharacter.isControllable)
        {
            if ((joystick != null) && (rangeIndicator != null))
            {
                UpdateAiming();
            }
        }
    }

    void UpdateAiming()
    {
        if (rangeIndicator.GetActive() != isAimingJoystick)
            rangeIndicator.SetActive(isAimingJoystick);

        if (isAimingJoystick)
        {
            float rotation = Mathf.Atan2(joystick.joyStickPosX, joystick.joyStickPosY) * 180 / Mathf.PI;
            rangeIndicator.transform.rotation = Quaternion.Euler(0, rotation, 0);
            aimingVector = new Vector3(joystick.joyStickPosX, 0, joystick.joyStickPosY).normalized;
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
        ActivateAbility();
    }

    //---------------------------
    //      Ability Action
    //---------------------------
    void AimingShot(bool isAiming)
    {
        isAimingJoystick = isAiming;

        if (isAiming)
        {
            ownerCharacter.State = PlayerState.AimingBall;
        }
        else
        {
            ownerCharacter.State = PlayerState.None;
            joystick.ResetJoystick();
        }
    }

    public override void ActivateAbility()
    {
        Vector3 shootingPos = ownerCharacter.shootingPositionTransform.position;
        Ball.SpawnShootingBall(ballPrefab, shootingPos, ownerCharacter, aimingVector);
    }

    //---------------------------
    //      Calculation
    //---------------------------
    public virtual float GetBallStartSpeed()
    {
        float ballSpeedMultiplier = 0.35f;
        return ownerCharacter.power * ballSpeedMultiplier;
    }

    public virtual float GetBallDamage(Ball ball)
    {
        return Mathf.Floor(ball.body.velocity.magnitude);
    }
}

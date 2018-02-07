using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShoot : Ability_Direction
{
    //===========================
    //      Variables
    //===========================
    // Transform Part
    Transform BallPosition_Shoot;

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    public override void Init(Character character)
	{
        base.Init(character);

        BallPosition_Shoot = RangeIndicator.transform.Find("BallPosition_Shoot").transform;
    }

	public override void InitJoystick()
	{
		joystick = UIManager.GetJoystick("Widget_Joystick_BasicShoot");
		joystick.ability = this;
	}

    //---------------------------
    //      Ability action
    //---------------------------
    public override void ActivateAbility()
	{
        // ??
        /*
        LevelManager.Ball.transform.position = BallPosition_Shoot.position;

        // Update ball status
        LevelManager.Ball.State = BallState.Shooting;
        LevelManager.Ball.ownerCharacter = null;
        LevelManager.Ball.transform.parent = null;

        // Give ball a force to fly
        float startSpeed = GetBallStartSpeed();
        LevelManager.Ball.GetComponent<Rigidbody>().velocity = new Vector3(AimingVector.x * startSpeed, 0, AimingVector.z * startSpeed);

        ownerCharacter.State = PlayerState.None;
        */
    }

    public override void BallHitAction(Collision col, Ball ball, Character hitCharacter)
	{
		hitCharacter.RecieveDamage(GetBallDamage(ball));
	}

	//---------------------------
	//      Other
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

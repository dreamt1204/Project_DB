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
    //      Update Functions
    //---------------------------
    public override void DisplayRangeIndicator(bool active)
	{
		base.DisplayRangeIndicator(active);

		if (active)
		{
            ownerCharacter.Ball.transform.position = BallPosition_Shoot.position;
            ownerCharacter.Ball.transform.parent = BallPosition_Shoot;
		}
	}

    //---------------------------
    //      Ability action
    //---------------------------
    public override void ActivateAbility()
	{
        // Grab ball
        DodgeBall ball = ownerCharacter.Ball;
		if (ball == null)
			return;

        // Update ball status
        ownerCharacter.Ball = null;
        ball.Shot(this);

        // Give ball a force to fly
		float startSpeed = GetBallStartSpeed();
		ball.GetComponent<Rigidbody>().velocity = new Vector3(AimingVector.x * startSpeed, 0, AimingVector.z * startSpeed);
	}

	public override void BallHitAction(Collision col, DodgeBall ball, Character hitCharacter)
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

	public virtual float GetBallDamage(DodgeBall ball)
	{
		return Mathf.Floor(ball.rb.velocity.magnitude);
	}
}

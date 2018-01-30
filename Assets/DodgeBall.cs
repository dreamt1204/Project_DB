using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallStatus
{
	Unpicked,
	Picked,
	Shooting
}

public class DodgeBall : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    // Prefabs
    [Header("Materials")]
    public Material MaterialSafe;
    public Material MaterialDamage;

    // Variables
    [HideInInspector] public Character ownerCharacter;
	[HideInInspector] public Rigidbody rb;
    BallStatus status;
    [HideInInspector] public Ability_Direction ActionAbility;
    [HideInInspector] public int AttackerTeam = -1;

    // Const Variables
	const float safeSpeed = 5f;

    //---------------------------
    //      Properties
    //---------------------------
    public BallStatus Status
	{
		get
		{ 
			return status;
		}

		set
		{
			status = value;
			UpdateBallAttackData();
			UpdatePhysics();
			UpdateMaterial();
		}
	}

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    void Start()
	{
		rb = GetComponent<Rigidbody>();
		Status = BallStatus.Unpicked;
	}

    //---------------------------
    //      Update Functions
    //---------------------------
    void Update()
    {
        UpdateShootingBallStatus();
    }

	// Reset ball status to Unpicked if ball's velocity is lower than safe speed
    void UpdateShootingBallStatus()
	{
		if (Status != BallStatus.Shooting)
			return;

		float currentBallSpeed = gameObject.GetComponent<Rigidbody>().velocity.magnitude;
		if (Mathf.Floor(currentBallSpeed) <= safeSpeed)
		{
			Status = BallStatus.Unpicked;
		}
	}

	// Update ball's action ability and attacker team based on status
	void UpdateBallAttackData()
	{
		if (Status == BallStatus.Unpicked)
		{
			ActionAbility = null;
			AttackerTeam = -1;
		}
	}

	// Update ball's material based on status
    void UpdateMaterial()
    {
        MeshRenderer render = gameObject.GetComponent<MeshRenderer>();

        if (status == BallStatus.Shooting)
			render.material = MaterialDamage;
        else
			render.material = MaterialSafe;
    }

	// Update ball's physics based on status
    void UpdatePhysics()
    {
		if (status == BallStatus.Picked)
			EnablePhysics(false);
		else
			EnablePhysics(true);
    }

	// Enable / Disable ball rigidbody
	void EnablePhysics(bool enabled)
	{
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		rb.detectCollisions = enabled;
		rb.isKinematic = !enabled;
	}

    //---------------------------
    //      Ball status action
    //---------------------------
    public void PickedUp(Character character)
    {
        Status = BallStatus.Picked;

        ownerCharacter = character;
        AttackerTeam = character.ownerPlayer.team;
        transform.position = character.BallPosition_Hold.position;
        transform.parent = character.BallPosition_Hold;
    }

    public void Shot(Ability_Direction shootAbility)
    {
        Status = BallStatus.Shooting;

        ownerCharacter = null;
        ActionAbility = shootAbility;
        transform.parent = null;
    }

    //---------------------------
    //      Collision events
    //---------------------------
    void OnCollisionEnter (Collision col)
	{
		OnCharacterHit(col);
    }

	void OnCharacterHit(Collision col)
    {
		Character hitCharacter = col.gameObject.GetComponent<Character>();
		if (hitCharacter == null)
			return;

		if (Status == BallStatus.Unpicked)
            hitCharacter.TryPickUpBall(this);
        else if (Status == BallStatus.Shooting)
			ActionAbility.BallHitAction(col, this, hitCharacter);
    }    
}

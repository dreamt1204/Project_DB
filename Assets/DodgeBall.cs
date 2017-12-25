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
    BallStatus status;
    [HideInInspector] public Ability_Direction ActionAbility;
    [HideInInspector] public int AttackerTeam = -1;

    // Const Var
	const float safeSpeed = 3f;

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
		UpdateMaterial();
	}

    //---------------------------
    //      Update Functions
    //---------------------------
    void Update()
    {
        UpdateShootingBallStatus();
    }

    void UpdateShootingBallStatus()
	{
		if (Status != BallStatus.Shooting)
			return;

		float currentBallSpeed = gameObject.GetComponent<Rigidbody>().velocity.magnitude;
		if (currentBallSpeed <= safeSpeed)
		{
			Status = BallStatus.Unpicked;
            ActionAbility = null;
            AttackerTeam = -1;
		}
	}

    void UpdateMaterial()
    {
        MeshRenderer render = gameObject.GetComponent<MeshRenderer>();

        if (status == BallStatus.Shooting)
        {
            render.material = MaterialDamage;
        }
        else
        {
            render.material = MaterialSafe;
        }
    }

    void UpdatePhysics()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if (status == BallStatus.Picked)
        {
            if (rb != null)
                Destroy(rb);
        }
        else
        {
            if (rb == null)
                gameObject.AddComponent<Rigidbody>();
        }
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
        Character hitCharacter = col.gameObject.GetComponent<Character>();
        if (hitCharacter != null)
            OnCharacterHit(hitCharacter);
    }

    void OnCharacterHit(Character hitCharacter)
    {
        if (Status == BallStatus.Unpicked)
            hitCharacter.TryPickUpBall(this);
        else if (Status == BallStatus.Shooting)
            ActionAbility.BallHitAction(this, hitCharacter);
    }    
}

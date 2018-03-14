using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState : byte
{
    Unpicked,
    Shooting
}

[RequireComponent(typeof(PhotonView))]
public class Ball : Photon.MonoBehaviour, IPunObservable
{
    //===========================
    //      Variables
    //===========================
    // Prefabs
    [Header("Materials")]
    public Material MaterialSafe;
    public Material MaterialDamage;

    // Flags
    [HideInInspector] public bool sentPickup;

    // Variables
	[HideInInspector] public Rigidbody body;
    BallState state;
    Character attacker;    // The character shoots this ball
    Vector3 startingVector;    // The starting vector direction of the shooting ball

    // Const Variables
	const float safeSpeed = 5f;

    //---------------------------
    //      Properties
    //---------------------------
    public BallState State
	{
		get
		{ 
			return this.state;
		}

		set
		{
            this.state = value;

            if (this.photonView.isMine)
                this.photonView.RPC("UpdateBallState", PhotonTargets.Others, this.state);

            UpdateMaterial();
        }
	}

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    void Awake()
	{
        this.body = GetComponent<Rigidbody>();
    }

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = this.photonView.instantiationData;

        if (data != null)
        {
            for (int i = 0; i < data.Length; i++)
            {
                // Get starting state
                if (i == 0)
                {
                    this.State = (BallState)data[0];
                    UpdateMaterial();
                }
                // Get attacker
                else if (i == 1)
                {
                    this.attacker = (Character)data[1];
                }
                // Get direction
                else if (i == 2)
                {
                    this.startingVector = (Vector3)data[2];
                }
            }
        }

        // Start the ball as projectile if it meets the following requirements
        if ((State == BallState.Shooting) && (attacker != null))
        {
            Shoot((int)data[3]);
        }
    }

    //---------------------------
    //      Update Functions
    //---------------------------
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			stream.SendNext(this.State);
		}
		else
		{
			this.State = (BallState)stream.ReceiveNext();
		}
	}

	void Update()
    {
        // Let only the master client to update the shooting state
        if (this.photonView.isMine)
            UpdateShootingBallState();
    }

    // Reset ball status to Unpicked if ball's velocity is lower than safe speed
    void UpdateShootingBallState()
	{
		if (this.State != BallState.Shooting)
			return;

		float currentBallSpeed = this.body.velocity.magnitude;
		if (Mathf.Floor(currentBallSpeed) <= safeSpeed)
		{
            this.State = BallState.Unpicked;
		}
	}

    [PunRPC]
    void UpdateBallState(BallState newState)
    {
        if (State != newState)
            State = newState;
    }

    // Update ball's material based on status
    void UpdateMaterial()
    {
        MeshRenderer render = gameObject.GetComponent<MeshRenderer>();

        if (this.State == BallState.Shooting)
			render.material = MaterialDamage;
        else
			render.material = MaterialSafe;
    }

    //---------------------------
    //      Collision events
    //---------------------------
    void OnCollisionEnter(Collision col)
    {
        // Player character collision event is handled by character class because of CharacterController class.
        if (col.gameObject.tag == "Player")
        {
            OnHitCharacter(col.gameObject.GetComponent<Character>());
        }
    }

    public void OnHitCharacter(Character target)
    {
        if (this.State == BallState.Unpicked)
            PickUp(target);
        else if (this.State == BallState.Shooting)
            Hit(target);
    }

    //---------------------------
    //      Pick Up
    //---------------------------
	// Called on the colliding picker
    void PickUp(Character character)
    {
        if (character.State != PlayerState.None)
            return;

        // Set state for the ball holder character
        character.State = PlayerState.HoldingBall;

        // Hide the gameObject and call the onwer client to destroy it
        this.gameObject.SetActive(false);
        this.photonView.RPC("PickUp_RPC_owner", this.photonView.owner);
    }

	// Called on the original ball owner/attacker
    [PunRPC]
    void PickUp_RPC_owner()
    {
        // Since the ball is picked, let's destroy it.
        PhotonNetwork.Destroy(this.gameObject);
    }

	//---------------------------
	//      Shoot
	//---------------------------
    void Shoot(int spawnTime)
    {
        int deltaTime = PhotonNetwork.ServerTimestamp - spawnTime;
        float startingSpeed = GetBallStartSpeed();

        this.transform.position += new Vector3(this.body.velocity.x * deltaTime, 0, this.body.velocity.z * deltaTime);
        this.body.velocity = new Vector3(this.startingVector.x * startingSpeed, 0, this.startingVector.z * startingSpeed);
    }

	//---------------------------
	//      Hit
	//---------------------------
	// Called on the colliding character
	public virtual void Hit(Character character)
	{
		character.RecieveDamage(GetBallDamage());
	}

    //---------------------------
    //      Calculation
    //---------------------------
    public virtual float GetBallStartSpeed()
    {
        float ballSpeedMultiplier = 0.35f;

        return attacker.power * ballSpeedMultiplier;
    }

	public virtual float GetBallDamage()
	{
		float ballDamageMultiplier = 0.5f;

		return attacker.power * ballDamageMultiplier;
	}


    //===========================
    //      Static Functions
    //===========================
    //---------------------------
    //      Spawn
    //---------------------------
    public static Ball SpawnBall(Ball ballPrefab, Vector3 spawnPos)
    {
        object[] data = new object[1];
        data[0] = BallState.Unpicked;

        return PhotonNetwork.Instantiate(ballPrefab.name, spawnPos, Quaternion.identity, 0, data).GetComponent<Ball>();
    }

    public static Ball SpawnBall(Ball ballPrefab, Vector3 spawnPos, BallState startingState)
    {
        object[] data = new object[1];
        data[0] = startingState;

        return PhotonNetwork.Instantiate(ballPrefab.name, spawnPos, Quaternion.identity, 0, data).GetComponent<Ball>();
    }

    public static Ball SpawnShootingBall(Ball ballPrefab, Vector3 spawnPos, Character attacker, Vector3 aimingVector, int spawnTime)
    {
        object[] data = new object[4];
        data[0] = BallState.Shooting;
        data[1] = attacker;
        data[2] = aimingVector;
        data[3] = spawnTime;

        return PhotonNetwork.Instantiate(ballPrefab.name, spawnPos, Quaternion.identity, 0, data).GetComponent<Ball>();
    }
}

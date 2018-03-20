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
    public PhysicMaterial PhysicMaterialSafe;
    public Material MaterialSafe;
    public Material MaterialDamage;

    // Flags
    [HideInInspector] public bool sentPickup;

    // Variables
	[HideInInspector] public Rigidbody body;
    BallState state;
    Character attacker;     // The character shoots this ball
    Vector3 direction = Vector3.zero;            // The starting vector direction of the shooting ball
    float speed = 0;
    int bounceTimes;      // Number of times this ball bounces

    int maxBounceTimes = 1;
    float bounciness = 0.75f;
    float safeSpeed = 10f;

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
        if (PhotonNetwork.isMasterClient)
            this.body.isKinematic = false;
        else
            this.body.isKinematic = true;

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
                    this.direction = (Vector3)data[2];
                }
            }
        }

        // Start the ball as projectile if it meets the following requirements
        if (State == BallState.Shooting)
        {
            if (PhotonNetwork.isMasterClient)
                Shoot(this.transform.position, (int)data[3], this.direction, GetBallStartSpeed());
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

	void FixedUpdate()
    {
        UpdateMovement();

        if (PhotonNetwork.isMasterClient)
            TryTransformSlowBallToSafeBall();
    }

    void UpdateMovement()
    {
        if (this.State != BallState.Shooting)
            return;

        if (this.direction == Vector3.zero)
            return;

        if (this.speed == 0)
            return;

        this.transform.Translate(this.direction * this.speed * Time.fixedDeltaTime);
    }

    void TryTransformSlowBallToSafeBall()
	{
        if (!PhotonNetwork.isMasterClient)
            return;

        if (this.State != BallState.Shooting)
			return;

        if (this.speed > this.safeSpeed)
            return;

        TransformToSafeBall(this.direction);
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
        if (PhotonNetwork.isMasterClient)
            OnHitBounce(col);

        /*
        // Player character collision event is handled by character class because of CharacterController class.
        if (col.gameObject.tag == "Player")
        {
            //OnHitCharacter(col.gameObject.GetComponent<Character>());
        }
        */
    }

    void OnHitBounce(Collision col)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        this.bounceTimes += 1;
        
        Vector3 reflectDir = new Vector3(col.contacts[0].normal.x, 0, col.contacts[0].normal.z);
        this.direction = Vector3.Reflect(this.direction, reflectDir).normalized;

        if (this.maxBounceTimes >= 0)
        {
            if (this.bounceTimes >= this.maxBounceTimes)
            {
                TransformToSafeBall(this.direction);
                return;
            }
        }

        Shoot(this.transform.position, PhotonNetwork.ServerTimestamp, this.direction, GetBallPostBounceSpeed());
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
    //      Transform To Safe Ball
    //---------------------------
    void TransformToSafeBall(Vector3 droppingDir)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        this.photonView.RPC("TransformToSafeBall_RPC", PhotonTargets.All, this.transform.position, droppingDir);
    }

    [PunRPC]
    void TransformToSafeBall_RPC(Vector3 droppingPos,Vector3 droppingDir)
    {
        this.State = BallState.Unpicked;
        this.direction = Vector3.zero;
        float droppingSpeed = this.speed * 5;
        this.speed = 0;
        this.attacker = null;

        this.transform.position = droppingPos;

        GetComponent<SphereCollider>().material = PhysicMaterialSafe;
        this.body.isKinematic = false;
        this.body.useGravity = true;
        this.body.drag = 1;
        this.body.angularDrag = 1;
        this.body.AddForce(droppingDir * this.safeSpeed * 10);
    }

    //---------------------------
    //      Shoot
    //---------------------------
    void Shoot(Vector3 shootPos, int shootTime, Vector3 shootDir, float shootSpeed)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        this.photonView.RPC("Shoot_RPC", PhotonTargets.All, shootPos, shootTime, shootDir, shootSpeed);
    }
    
    [PunRPC]
    void Shoot_RPC(Vector3 shootPos, int shootTime, Vector3 shootDir, float shootSpeed)
    {
        float timeOffset = (PhotonNetwork.ServerTimestamp - shootTime) / 1000;
        this.transform.position = shootPos + new Vector3(shootDir.x * shootSpeed * timeOffset, 0, shootDir.z * shootSpeed * timeOffset);

        this.direction = shootDir;
        this.speed = shootSpeed;
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

    public virtual float GetBallPostBounceSpeed()
    {
        return this.speed * Mathf.Pow(bounciness, this.bounceTimes);
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

    public static Ball SpawnShootingBall(Ball ballPrefab, Vector3 spawnPos, Character attacker, Vector3 dir, int spawnTime)
    {
        object[] data = new object[4];
        data[0] = BallState.Shooting;
        data[1] = attacker;
        data[2] = dir;
        data[3] = spawnTime;

        return PhotonNetwork.Instantiate(ballPrefab.name, spawnPos, Quaternion.identity, 0, data).GetComponent<Ball>();
    }
}

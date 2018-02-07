using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState : byte
{
    Unpicked,
	Picked,
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
    [HideInInspector] public Character ownerCharacter;
	[HideInInspector] public Rigidbody body;
    BallState state;

    // ??
    [HideInInspector] public Ability_Direction ActionAbility;
    [HideInInspector] public Team AttackerTeam = Team.None;

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
        // Init Ball State
        object[] data = this.photonView.instantiationData;

        if (data != null && data.Length == 1)
        {
            this.State = (BallState)data[0];
            UpdateMaterial();
        }
    }

    //---------------------------
    //      Update Functions
    //---------------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext((byte)this.State);

            // ??
            //stream.SendNext(Character.GetPhotonViewIDFromCharacter(this.ownerCharacter));
        }
        else
        {
            this.State = (BallState)stream.ReceiveNext();

            // ??
            //this.ownerCharacter = Character.GetCharacterFromViewID((int)stream.ReceiveNext());
        }
    }

    void Update()
    {
        // Let only the master client to update the shooting state
        if (PhotonNetwork.isMasterClient)
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
        Character collider_char = col.gameObject.GetComponent<Character>();
        PhotonView collider_pv = col.gameObject.GetComponent<PhotonView>();

        if (collider_char != null && collider_pv != null && collider_pv.isMine)
        {
            if (this.State == BallState.Unpicked)
                PickUp(collider_char);

            // ??
            /*
            else if (this.State == BallState.Shooting)
                ActionAbility.BallHitAction(col, this, collider_char);
            */
        }
    }

    //---------------------------
    //      Ball status action
    //---------------------------
    void PickUp(Character target)
    {
        if (target.State != PlayerState.None)
            return;

        // Set state for the ball holder character
        target.State = PlayerState.HoldingBall;

        // Hide the gameObject and call the onwer client to destroy it
        this.gameObject.SetActive(false);
        this.photonView.RPC("PickUp_RPC_owner", this.photonView.owner);
    }

    [PunRPC]
    void PickUp_RPC_owner()
    {
        this.State = BallState.Picked;
        PhotonNetwork.Destroy(this.gameObject);
    }


    // ??
    /*
    public void TryShot()
    {
        this.photonView.RPC("Shot", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void Shot()
    {
        this.State = BallState.Shooting;

        this.ownerCharacter = null;
        //ActionAbility = shootAbility;
        transform.parent = null;
    }
    */

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
}

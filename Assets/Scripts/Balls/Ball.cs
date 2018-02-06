using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState
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
			return state;
		}

		set
		{
            state = value;

            UpdateBallVisibility();
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
		body = GetComponent<Rigidbody>();
        state = BallState.Unpicked;
	}

    //---------------------------
    //      Update Functions
    //---------------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(State);
            stream.SendNext(Character.GetPhotonViewIDFromCharacter(ownerCharacter));
        }
        else
        {
            State = (BallState)stream.ReceiveNext();
            ownerCharacter = Character.GetCharacterFromViewID((int)stream.ReceiveNext());
        }
    }

    void Update()
    {
        UpdateShootingBallStatus();
    }

    // Reset ball status to Unpicked if ball's velocity is lower than safe speed
    void UpdateShootingBallStatus()
	{
		if (State != BallState.Shooting)
			return;

		float currentBallSpeed = gameObject.GetComponent<Rigidbody>().velocity.magnitude;
		if (Mathf.Floor(currentBallSpeed) <= safeSpeed)
		{
            State = BallState.Unpicked;
		}
	}

    // Update ball visiblity based on ball state
    void UpdateBallVisibility()
    {
        this.gameObject.SetActive(State != BallState.Picked);
    }

    // Update ball's action ability and attacker team based on status
    void UpdateBallAttackData()
	{
		if (State == BallState.Unpicked)
		{
			ActionAbility = null;
			AttackerTeam = Team.None;
		}
	}

	// Update ball's material based on status
    void UpdateMaterial()
    {
        MeshRenderer render = gameObject.GetComponent<MeshRenderer>();

        if (State == BallState.Shooting)
			render.material = MaterialDamage;
        else
			render.material = MaterialSafe;
    }

	// Update ball's physics based on status
    void UpdatePhysics()
    {
		if (State == BallState.Picked)
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
    public void TryPickUp(Character target)
    {
        if (target.State != PlayerState.None)
            return;

        if (sentPickup)
            return;

        sentPickup = true;

        this.photonView.RPC("PickUp", PhotonTargets.AllViaServer, Character.GetPhotonViewIDFromCharacter(target));
    }

    [PunRPC]
    void PickUp(int BallHolderViewID)
    {
        State = BallState.Picked;
        ownerCharacter = Character.GetCharacterFromViewID(BallHolderViewID);

        ownerCharacter.State = PlayerState.HoldingBall;
        AttackerTeam = ownerCharacter.ownerPlayer.GetTeam();

        sentPickup = false;
    }
    
    public void TryShot()
    {
        this.photonView.RPC("Shot", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void Shot()
    {
        State = BallState.Shooting;

        ownerCharacter = null;
        //ActionAbility = shootAbility;
        transform.parent = null;
    }

    //---------------------------
    //      Collision events
    //---------------------------
    void OnCollisionEnter (Collision col)
	{
        Character collider_char = col.gameObject.GetComponent<Character>();
        PhotonView collider_pv = col.gameObject.GetComponent<PhotonView>();
        
        if (collider_char != null && collider_pv != null && collider_pv.isMine)
        {
            if (State == BallState.Unpicked)
                TryPickUp(collider_char);
            else if (State == BallState.Shooting)
                ActionAbility.BallHitAction(col, this, collider_char);
        }
    }
}

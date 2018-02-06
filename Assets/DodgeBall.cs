using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallStatus
{
	Unpicked,
    Picking,
	Picked,
	Shooting
}

[RequireComponent(typeof(PhotonView))]
public class DodgeBall : Photon.MonoBehaviour, IPunObservable
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
	[HideInInspector] public Rigidbody rb;
    BallStatus status;
    [HideInInspector] public Ability_Direction ActionAbility;
    [HideInInspector] public Team AttackerTeam = Team.None;

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

            if (status == BallStatus.Picking)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                this.gameObject.SetActive(true);
                UpdateBallAttackData();
                UpdatePhysics();
                UpdateMaterial();
            }
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(Status);
            stream.SendNext(Character.GetPhotonViewIDFromCharacter(ownerCharacter));
        }
        else
        {
            Status = (BallStatus)stream.ReceiveNext();
            ownerCharacter = Character.GetCharacterFromViewID((int)stream.ReceiveNext());
        }
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
			AttackerTeam = Team.None;
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
    public void TryPickUp(Character target)
    {
        if (target.Status != PlayerStatus.None)
            return;

        if (sentPickup)
            return;

        sentPickup = true;

        Status = BallStatus.Picking;
        this.photonView.RPC("PickUp", PhotonTargets.AllViaServer, Character.GetPhotonViewIDFromCharacter(target));
    }

    [PunRPC]
    void PickUp(int BallHolderViewID)
    {
        Status = BallStatus.Picked;
        ownerCharacter = Character.GetCharacterFromViewID(BallHolderViewID);

        ownerCharacter.Status = PlayerStatus.HoldingBall;
        AttackerTeam = ownerCharacter.ownerPlayer.GetTeam();
        transform.position = ownerCharacter.BallPosition_Hold.position;
        //transform.parent = ownerCharacter.BallPosition_Hold;

        sentPickup = false;
    }
    
    public void TryShot()
    {
        this.photonView.RPC("Shot", PhotonTargets.AllViaServer);
    }

    [PunRPC]
    void Shot()
    {
        Status = BallStatus.Shooting;

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
            if (Status == BallStatus.Unpicked)
                TryPickUp(collider_char);
            else if (Status == BallStatus.Shooting)
                ActionAbility.BallHitAction(col, this, collider_char);
        }
    }
}

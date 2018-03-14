using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState : byte
{
	None,
	HoldingBall,
    AimingBall,
    ShootingBall,
    CatchingBall
}

[RequireComponent(typeof(PhotonView))]
public class Character : Photon.MonoBehaviour, IPunObservable
{
    //===========================
    //      Variables
    //===========================
    public CharacterData characterData;

    // Flags
    bool isInited = false;
    public bool isControllable = false;
    
    // Variables
    [HideInInspector] public PhotonPlayer ownerPlayer;
    PlayerState state;
    CharacterController characterController;
    [HideInInspector] public Transform transforms;
	[HideInInspector] public Transform shootingParentTransform;
    [HideInInspector] public Transform shootingPositionTransform;

    // Information
    [Header("Information")]
    public string characterName;

	// Attributes
	[Header("Attributes")]
    [Range(1, 200)] public float health;
    [Range(1, 200)] public float speed;
    [Range(1, 200)] public float power;
    [Range(1, 200)] public float defend;

    float currentHealth = 0;
  
	// Abilities
	[Header("Abilities")]
    [HideInInspector] public List<Ability> abilityPrefabs = new List<Ability>();
	[HideInInspector] public List<Ability> abilities = new List<Ability>();

	// UI elements
	UIManager uiManager;
	UIJoyStick joystickMovement;
	UIHealthBar healthBar;
	UIDamageText damageText;

    //---------------------------
    //      Properties
    //---------------------------
    public float CurrentHealth
	{
		get
		{
			return currentHealth;
		}
		set
		{
			currentHealth = Mathf.Clamp(value, 0, health);
		}
	}

	public PlayerState State
    {
		get
		{
			return state;
		}
		set
		{
            state = value;

            if (this.photonView.isMine)
				this.uiManager.UpdateBasicAbilityUI(this.state);

            UpdatePlayerAnimation();                
        }
    }

    const float moveSpeedMultiplier = 0.05f;
    public float moveSpeed { get { return speed * moveSpeedMultiplier; } }
    
    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        this.isControllable = info.sender.IsLocal;
        Init(info.sender);
    }

    void Init(PhotonPlayer player)
    {
        this.ownerPlayer = player;

        // Init instances
        InitInstances();

        // Init character name and object name
        this.characterName = this.characterData.characterName;
        this.gameObject.name = characterName + " (P" + player.ID + ")";

        // Init attributes
        InitAttributes(this.characterData);

        // Init abilities
        InitAbilities(this.characterData);

        // Init UI elements
        InitUI();

        // Set finished init flag
        this.isInited = true;
    }

    void InitInstances()
    {
        this.characterController = this.transform.GetComponent<CharacterController>();
        this.transforms = this.transform.Find("Transforms");
		this.shootingParentTransform = this.transforms.Find("ShootingParent");
		this.shootingPositionTransform = this.shootingParentTransform.Find("ShootingPosition");
    }

    void InitAttributes(CharacterData data)
    {
        this.health = data.health;
        this.speed = data.speed;
        this.power = data.power;
        this.defend = data.defend;

        this.CurrentHealth = health;
    }

    void InitAbilities(CharacterData data)
    {
        // Add basic abilities
        this.abilityPrefabs.Add(PrefabManager.instance.BasicShootPrefab);

        // Add character specific abilities
        foreach (Ability ability in data.abilityPrefabs)
        {
            if (ability != null)
                this.abilityPrefabs.Add(ability);
        }

        // Instantiate abilities
        for (int i = 0; i < this.abilityPrefabs.Count; i++)
        {
            Ability newAbility = Instantiate(this.abilityPrefabs[i], this.transform.position, Quaternion.identity).GetComponent<Ability>();
            this.abilities.Add(newAbility);
            newAbility.transform.parent = this.transform;
            newAbility.Init(this);
        }
    }

    void InitUI()
	{
        if (this.isControllable)
            InitUI_LocalPlayer();

        // Health Bar
		healthBar = Instantiate(PrefabManager.instance.HealthBarPrefab.gameObject).GetComponent<UIHealthBar>();
		healthBar.Init(this);

		// Damage Text
		damageText = Instantiate(PrefabManager.instance.DamageTextPrefab.gameObject).GetComponent<UIDamageText>();
		damageText.Init(this);
    }

    void InitUI_LocalPlayer()
    {
        this.uiManager = GameObject.Find("GameManagers").GetComponent<UIManager>();

        this.joystickMovement = UIManager.GetJoystick("Widget_Joystick_Movement");
        this.uiManager.UpdateBasicAbilityUI(State);
    }

    //---------------------------
    //      Update Functions
    //---------------------------
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
            stream.SendNext(this.State);
			stream.SendNext(this.CurrentHealth);
		}
		else
		{
            this.State = (PlayerState)stream.ReceiveNext();
			this.CurrentHealth = (float)stream.ReceiveNext();
        }
	}

    void FixedUpdate()
	{
        if (!this.isInited)
            return;

        if (isControllable)
        {
            UpdateMovement();
        }
    }

    void UpdatePlayerAnimation()
    {
        // tmp: placeholder art/animation
        GameObject Model_HoldingBall = null;
        GameObject Model_AimingBall = null;
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "Model_HoldingBall")
                Model_HoldingBall = child.gameObject;

            if (child.gameObject.name == "Model_AimingBall")
                Model_AimingBall = child.gameObject;
        }

        if (State == PlayerState.HoldingBall)
        {
            Model_HoldingBall.SetActive(true);
            Model_AimingBall.SetActive(false);
        }
        else if (State == PlayerState.AimingBall)
        {
            Model_HoldingBall.SetActive(false);
            Model_AimingBall.SetActive(true);
        }
        else
        {
            Model_HoldingBall.SetActive(false);
            Model_AimingBall.SetActive(false);
        }
    }

    //---------------------------
    //      Collision events
    //---------------------------
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!isControllable)
            return;

        if (hit.gameObject.tag == "Ball")
        {
            hit.gameObject.GetComponent<Ball>().OnHitCharacter(this);
        }
    }

    //---------------------------
    //      Movement
    //---------------------------
    void UpdateMovement()
    {
        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            return;

        Vector2 moveDir = new Vector2(this.joystickMovement.joyStickPosX != 0 ? this.joystickMovement.joyStickPosX : Input.GetAxis("Horizontal"),
                                      this.joystickMovement.joyStickPosY != 0 ? this.joystickMovement.joyStickPosY : Input.GetAxis("Vertical"));

        float moveDirectionz = -0.05f;

        Vector3 moveVelocity = new Vector3(moveDir.x, moveDirectionz, moveDir.y) * moveSpeed;
        this.characterController.Move(moveVelocity * Time.fixedDeltaTime);
    }

	//---------------------------
	//      Actions
	//---------------------------
	public void RecieveDamage(float damage)
	{
		CurrentHealth -= damage;
		this.photonView.RPC("RecieveDamage_RPC", PhotonTargets.AllViaServer, damage);
	}

	[PunRPC] void RecieveDamage_RPC(float damage)
	{
		damageText.ShowDamage(damage);
	}


    //===========================
    //      Static Functions
    //===========================
    //---------------------------
    //      Spawn
    //---------------------------
    public static void SpawnCharacter()
	{
        PhotonPlayer localPlayer = PhotonNetwork.player;
        Vector3 spawnPos = GameObject.Find("SpawnPoint_" + localPlayer.GetTeamString() + "_" + localPlayer.GetTeamPos().ToString()).transform.position;
        string prefabName = PrefabManager.GetCharacterPrefab(localPlayer).name;

        PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity, 0).GetComponent<Character>();
    }
}

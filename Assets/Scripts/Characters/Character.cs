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
public class Character : Photon.MonoBehaviour
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
    Rigidbody body;
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

    // Attribute multipliers
    const float speedMultiplier = 0.05f;

	// Abilities
	[Header("Abilities")]
    [HideInInspector] public List<Ability> abilityPrefabs = new List<Ability>();
	[HideInInspector] public List<Ability> abilities = new List<Ability>();

	// UI elements
	UIManager uiManager;
	UIJoyStick joystickMovement;


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
            {
                // Synce State over network
                this.photonView.RPC("UpdatePlayerState", PhotonTargets.Others, this.state);

                // Update basic ability UI
                this.uiManager.UpdateBasicAbilityUI(this.state);
            }

            UpdatePlayerAnimation();                
        }
    }
    
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
        this.body = transform.GetComponent<Rigidbody>();
        this.shootingPositionTransform = this.transform.Find("ShootingPosition");
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
        UIWidget Widget_HealthBar = Instantiate(PrefabManager.instance.HealthBarPrefab.gameObject).GetComponent<UIWidget>();
        Widget_HealthBar.SetAnchor(this.transform.Find("HealthBarPosition"));
        Widget_HealthBar.GetComponent<UIHealthBar>().Init(this);
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
    void Update()
	{
        if (!this.isInited)
            return;

        if (this.isControllable)
        {
            UpdateMovement();
            UpdateKeyboardMovement();
        }
    }

    [PunRPC]
    void UpdatePlayerState(PlayerState newState)
    {
        if (State != newState)
            State = newState;
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
    //      Movement
    //---------------------------
    void UpdateMovement()
	{
		float speedFactor = speed * speedMultiplier;

        this.body.velocity = new Vector3(this.joystickMovement.joyStickPosX * speedFactor, 0, this.joystickMovement.joyStickPosY * speedFactor);
	}

	void UpdateKeyboardMovement()
	{
		if (this.joystickMovement.joyStickPosX != 0)
			return;
		if (this.joystickMovement.joyStickPosY != 0)
			return;

		float speedFactor = speed * speedMultiplier;
        this.body.velocity = new Vector3(Input.GetAxis("Horizontal") * speedFactor, 0, Input.GetAxis("Vertical") * speedFactor);
	}

	//---------------------------
	//      Other
	//---------------------------
	public void RecieveDamage(float damage)
	{
		CurrentHealth -= damage;
		Debug.Log("Heath: " + CurrentHealth);
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

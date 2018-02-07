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
public class Character : MonoBehaviour, IPunObservable
{
    //===========================
    //      Variables
    //===========================
    public CharacterData characterData;

    // Flags
    bool isInitialized = false;
    bool isControllable = false;

    // Variables
    [HideInInspector] public PhotonPlayer ownerPlayer;
    PlayerState state;
    Rigidbody body;

    // ??
    [HideInInspector] public Ability castingAbility;

    // Information
    [Header("Information")]
    [HideInInspector] public string characterName;

	// Attributes
	[Header("Attributes")]
    [HideInInspector] [Range(1, 200)] public float health;
    [HideInInspector] [Range(1, 200)] public float speed;
    [HideInInspector] [Range(1, 200)] public float power;
    [HideInInspector] [Range(1, 200)] public float defend;

    float currentHealth = 0;

    // Attribute multipliers
    const float speedMultiplier = 0.05f;

	// Abilities
	[Header("Abilities")]
    [HideInInspector] public List<Ability> abilityPrefabs = new List<Ability>();
	[HideInInspector] public List<Ability> abilities = new List<Ability>();

	// UI elements
	UIManager uiManager;
	UIJoyStick_Movement movementJoystick;


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

            UpdatePlayerAnimation();


            // ??
            /*
            if (this.isControllable)
                this.uiManager.UpdateBasicAbilityUI(status == PlayerState.HoldingBall);
            */
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
        //InitAbilities(this.characterData);

        // Init local character stuff
        if (this.isControllable)
        {
            InitUI();
        }

        // Set finished init flag
        this.isInitialized = true;
    }

    void InitInstances()
    {
        this.body = transform.GetComponent<Rigidbody>();
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
        this.abilityPrefabs.Insert(0, PrefabManager.instance.BasicShootPrefab);

        for (int i = 0; i < this.abilityPrefabs.Count; i++)
        {
            Ability newAbility = Instantiate(this.abilityPrefabs[i]).GetComponent<Ability>();
            newAbility.transform.parent = transform;
            this.abilities.Add(newAbility);
            newAbility.StartInit(this);
        }
    }

    void InitUI()
	{
        // ??
        this.uiManager = GameObject.Find("GameManagers").GetComponent<UIManager>();
        this.movementJoystick = UIManager.GetJoystickObject("Widget_Joystick_Movement").GetComponentInChildren<UIJoyStick_Movement>();

        this.uiManager.UpdateBasicAbilityUI(false);
	}

    //---------------------------
    //      Update Functions
    //---------------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext((byte)State);
        }
        else
        {
            State = (PlayerState)stream.ReceiveNext();
        }
    }

    void Update()
	{
        if (!this.isInitialized)
            return;

        if (this.isControllable)
        {
            UpdateMovement();
            UpdateKeyboardMovement();
        }
    }

    void UpdatePlayerAnimation()
    {
        // tmp: placeholder art
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

        this.body.velocity = new Vector3(this.movementJoystick.joyStickPosX * speedFactor, 0, this.movementJoystick.joyStickPosY * speedFactor);
	}

	void UpdateKeyboardMovement()
	{
		if (this.movementJoystick.joyStickPosX != 0)
			return;
		if (this.movementJoystick.joyStickPosY != 0)
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
    
    //---------------------------
    //      Check Functions
    //---------------------------
    public static bool CheckAuthority(Character character)
    {
        if (character == null)
            return false;

        if (!character.isControllable)
            return false;

        return true;
    }

    //---------------------------
    //      Get Functions
    //---------------------------

    // ??
    public static int GetPhotonViewIDFromCharacter(Character character)
    {
        UTL.TryCatchError(character.gameObject.GetPhotonView() == null, "Cannot find PhotonView for the input character.");

        return character.gameObject.GetPhotonView().viewID;
    }

    public static Character GetCharacterFromViewID(int ViewID)
    {
        UTL.TryCatchError(PhotonView.Find(ViewID) == null, "Cannot find the character with PhotonViewID.");
        UTL.TryCatchError(PhotonView.Find(ViewID).gameObject.GetComponent<Character>() == null, "Cannot find the character with PhotonViewID.");

        return PhotonView.Find(ViewID).gameObject.GetComponent<Character>();
    }
}

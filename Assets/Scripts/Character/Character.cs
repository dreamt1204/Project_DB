using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatus
{
	None,
	HoldingBall,
    AimingBall,
    ShootingBall,
    CatchingBall
}

[RequireComponent(typeof(PhotonView))]
public class Character : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    public CharacterData characterData;

    // Flags
    bool isInited = false;
    [HideInInspector] public bool isControllable = false;

    // Variables
    [HideInInspector] public PhotonPlayer ownerPlayer;
    PlayerStatus status;

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

    // Transform
    [HideInInspector] public Transform BallPosition_Hold;
	[HideInInspector] public Transform BallPosition_Shoot;

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

	public PlayerStatus Status
    {
		get
		{
			return status;
		}
		set
		{
            status = value;

            if (isControllable)
                uiManager.UpdateBasicAbilityJoytick(status == PlayerStatus.HoldingBall);
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
        isControllable = info.sender.IsLocal;
        Init(info.sender);
    }

    void Init(PhotonPlayer player)
    {
        ownerPlayer = player;

        characterName = characterData.characterName;
        this.gameObject.name = characterName + " (" + player.ID + ")";

        // Init attributes
        InitAttributes(characterData);

        // Init abilities
        InitAbilities(characterData);

        // Init Transform
        InitVisualTransform(characterData);

        // Init local character stuff
        if (isControllable)
            InitMyCharacter();

        // Set finished init flag
        isInited = true;
    }

    void InitAttributes(CharacterData data)
    {
        health = data.health;
        speed = data.speed;
        power = data.power;
        defend = data.defend;
        CurrentHealth = health;
    }

    void InitAbilities(CharacterData data)
    {
        abilityPrefabs.Insert(0, PrefabManager.instance.BasicShootPrefab);

        for (int i = 0; i < abilityPrefabs.Count; i++)
        {
            Ability newAbility = Instantiate(abilityPrefabs[i]).GetComponent<Ability>();
            newAbility.transform.parent = transform;
            abilities.Add(newAbility);
            newAbility.StartInit(this);
        }
    }

    void InitVisualTransform(CharacterData data)
    {
        BallPosition_Hold = gameObject.transform.Find("BallPosition_Hold").transform;
    }

    void InitMyCharacter()
	{
		uiManager = GameObject.Find("GameManagers").GetComponent<UIManager>();
		movementJoystick = UIManager.GetJoystickObject("Widget_Joystick_Movement").GetComponentInChildren<UIJoyStick_Movement>();

		uiManager.UpdateBasicAbilityJoytick(false);
	}

	//---------------------------
	//      Update Functions
	//---------------------------
	void Update()
	{
        if (!isInited)
            return;

        if (isControllable)
        {
            UpdateMovement();
            UpdateKeyboardMovement();
        }
    }

    //---------------------------
    //      Movement
    //---------------------------
    void UpdateMovement()
	{
		float speedFactor = speed * speedMultiplier;

		transform.GetComponent<Rigidbody>().velocity = new Vector3(movementJoystick.joyStickPosX * speedFactor, 0, movementJoystick.joyStickPosY * speedFactor);
	}

	void UpdateKeyboardMovement()
	{
		if (movementJoystick.joyStickPosX != 0)
			return;
		if (movementJoystick.joyStickPosY != 0)
			return;

		float speedFactor = speed * speedMultiplier;
		transform.GetComponent<Rigidbody>().velocity = new Vector3(Input.GetAxis("Horizontal") * speedFactor, 0, Input.GetAxis("Vertical") * speedFactor);
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

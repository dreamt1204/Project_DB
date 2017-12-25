using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStatus
{
	Idle,
	HoldingBall
}

public class Character : MonoBehaviour
{
	//===========================
	//      Variables
	//===========================
	// Variables
	LevelManager level = LevelManager.instance;
	[HideInInspector] public PlayerController ownerPlayer;
	[HideInInspector] public CharacterStatus status;
	float healthMax;
	float health;
	DodgeBall ball;
	[HideInInspector] public Ability castingAbility;

	// Attributes
	[Header("Attributes")]
	public float stamina = 100;
	public float speed = 100;
    
	// Attribute multipliers
	const float speedMultiplier = 0.05f;

	// Abilities
	[Header("Abilities")]
	public Ability BasicShootPrefab;
	public Ability BasicCatchPrefab;
	public List<Ability> abilityPrefabs = new List<Ability>();
	[HideInInspector] public List<Ability> abilities = new List<Ability>();

	// UI elements
	UIManager uiManager;
	UIJoyStick_Movement movementJoystick;

	// Transform Part
	[HideInInspector] public Transform BallPosition_Hold;
	[HideInInspector] public Transform BallPosition_Shoot;

	//---------------------------
	//      Properties
	//---------------------------
	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = Mathf.Clamp(value, 0, healthMax);
		}
	}

	public DodgeBall Ball
	{
		get
		{
			return ball;
		}
		set
		{
			ball = value;

			if (ownerPlayer == level.myPlayer)
				uiManager.UpdateBasicAbilityJoytick(ball != null);
		}
	}

	//===========================
	//      Functions
	//===========================
	//---------------------------
	//      Init Functions
	//---------------------------
	void Init(PlayerController player)
	{
		// Init variable intances
		ownerPlayer = player;
		BallPosition_Hold = gameObject.transform.Find("BallPosition_Hold").transform;

		// Init attributes 
		healthMax = CalculateMaxHealth(stamina);
		Health = healthMax;

		// Init abilities
		abilityPrefabs.Insert(0, BasicShootPrefab);
		abilityPrefabs.Insert(1, BasicCatchPrefab);

		for (int i = 0; i < abilityPrefabs.Count; i++)
		{
			Ability newAbility = Instantiate(abilityPrefabs[i]).GetComponent<Ability>();
			newAbility.transform.parent = transform;
			abilities.Add(newAbility);
			newAbility.Init(this);
		}

		// My character specicific init
		if (ownerPlayer == level.myPlayer)
			InitMyCharacter();
	}

	void InitMyCharacter()
	{
		uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
		movementJoystick = UIManager.GetJoystickObject("Widget_Joystick_Movement").GetComponentInChildren<UIJoyStick_Movement>();
	}

	//---------------------------
	//      Update Functions
	//---------------------------
	void Update()
	{
		if (ownerPlayer == level.myPlayer)
		{
			UpdateMovement();
			UpdateKeyboardMovement();
		}
	}

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
	//      Ball
	//---------------------------
	public void TryPickUpBall(DodgeBall targetBall)
	{
		if (status != CharacterStatus.Idle)
			return;

		if (targetBall.ownerCharacter == this)
			return;
		
		PickUpBall(targetBall);
	}

	public void PickUpBall(DodgeBall targetBall)
	{
		Ball = targetBall;
		targetBall.PickedUp(this);
	}

	//---------------------------
	//      Other
	//---------------------------
	public void RecieveDamage(float damage)
	{
		Health = Mathf.Clamp((Health - damage), 0, healthMax);
		Debug.Log("Heath: " + Health);
	}

	//===========================
	//      Static Functions
	//===========================
	public static float CalculateMaxHealth(float stamina)
	{
		float healthMax = stamina;
		return Mathf.Clamp(healthMax, 0, healthMax);
	}

	public static void SpawnCharacter(PlayerController player)
	{
		UTL.TryCatchError((player.characterPrefab == null), "This player doesn't have character prefab to spawn.");

		Vector3 spawnPos = GameObject.Find("Team" + player.team + "_SpawnPoint_" + player.teamPos).transform.position;
		player.currentCharacter = Instantiate(player.characterPrefab, spawnPos, Quaternion.Euler(0, 0, 0)).GetComponent<Character>();
		player.currentCharacter.Init(player);
	}
}

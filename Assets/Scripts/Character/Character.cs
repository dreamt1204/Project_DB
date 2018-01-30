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
	float currentHealth = 0;
	DodgeBall ball;
	[HideInInspector] public Ability castingAbility;

	// Attributes
	[Header("Attributes")]
	[Range(0, 200)] public float health = 100;
	[Range(0, 200)] public float power = 100;
	[Range(0, 200)] public float speed = 100;
	[Range(0, 200)] public float defend = 100;
    
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

	// Transform parts
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
		CurrentHealth = health;

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

		uiManager.UpdateBasicAbilityJoytick(false);
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
		if (ball !=null)
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
		CurrentHealth -= damage;
		Debug.Log("Heath: " + CurrentHealth);
	}

	//===========================
	//      Static Functions
	//===========================
	public static void SpawnCharacter(PlayerController player)
	{
		UTL.TryCatchError((player.characterPrefab == null), "This player doesn't have character prefab to spawn.");

		Vector3 spawnPos = GameObject.Find("Team" + player.team + "_SpawnPoint_" + player.teamPos).transform.position;
		player.currentCharacter = Instantiate(player.characterPrefab, spawnPos, Quaternion.Euler(0, 0, 0)).GetComponent<Character>();
		player.currentCharacter.Init(player);
	}
}

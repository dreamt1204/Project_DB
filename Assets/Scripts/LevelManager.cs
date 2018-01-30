using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    public static LevelManager instance;

	[Header("Player")]
	public PlayerController myPlayer;
	[HideInInspector] public PlayerController[,] player = new PlayerController[2, 3];

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    // This init all the variable instances before Level Initialization
    void Awake()
	{
		if (instance == null)
			instance = this;
	}

    // Level Initialization
	void Start ()
	{
        InitPlayers(); // (tmp) Temporary team assign function until match making is done.
        InitCharacters();
    }

    // (tmp) Temporary team assign function until match making is done.
    void InitPlayers()
	{
		player[0, 0] = myPlayer;
		myPlayer.team = 0;
		myPlayer.teamPos = 0;
	}

	// Spawn character for each player
	void InitCharacters()
	{
		for (int i = 0; i < player.GetLength(0); i++)
		{
			for (int j = 0; j < player.GetLength(1); j++)
			{
				if (player[i, j] == null)
					continue;

				Character.SpawnCharacter(player[i, j]);
			}
		}
	}
}

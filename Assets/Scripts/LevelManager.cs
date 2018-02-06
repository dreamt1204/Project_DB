using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    public static LevelManager instance;

    public static DodgeBall Ball;

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    void Awake()
	{
		if (instance == null)
			instance = this;

        Ball = GameObject.Find("Ball").GetComponent<DodgeBall>();
	}

    // 
    void OnJoinedRoom()
    {
        // Assign Team
        PhotonNetwork.player.AutoAssignTeam();

        // Spawn character
        Character.SpawnCharacter();
    }
}

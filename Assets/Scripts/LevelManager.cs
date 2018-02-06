using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    // Inspector
    public Ball startingBall;

    // Global variables
    public static Ball Ball;

    //===========================
    //      Functions
    //===========================
    void OnJoinedRoom()
    {
        Init();
    }

    void Init()
    {
        // Assign Team
        PhotonNetwork.player.AutoAssignTeam();

        // Spawn character
        Character.SpawnCharacter();

        // Master Client Initialize
        if (PhotonNetwork.isMasterClient)
            MasterClientInit();
    }

    void MasterClientInit()
    {
        // Spawn the starting ball
        SpawnStartingBall();
    }

    void SpawnStartingBall()
    {
        Vector3 spawnPos = GameObject.Find("SpawnPoint_Ball").transform.position;
        PhotonNetwork.Instantiate(startingBall.name, spawnPos, Quaternion.identity, 0);
    }
}

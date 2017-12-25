using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    [HideInInspector] public int team;
	[HideInInspector] public int teamPos;

	public GameObject characterPrefab;
	[HideInInspector] public Character currentCharacter;
}

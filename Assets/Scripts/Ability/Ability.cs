using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    [HideInInspector] public Character ownerCharacter;

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    public virtual void Init(Character character)
    {
        ownerCharacter = character;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    [HideInInspector] public Character ownerCharacter;

    public string abilityName = "No Name";

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

    //---------------------------
    //      Ability Action
    //---------------------------
    public virtual void ActivateAbility()
    {

    }
}

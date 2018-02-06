using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType
{
	Shoot,
	Catch
}

public class Ability : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    [HideInInspector] public Character ownerCharacter;
	[HideInInspector] public AbilityType type;

    //===========================
    //      Functions
    //===========================
    //---------------------------
    //      Init Functions
    //---------------------------
    public void StartInit(Character character)
    {
        if (!Character.CheckAuthority(character))
            return;

        Init(character);
    }

    public virtual void Init(Character character)
    {
        ownerCharacter = character;
    }
}

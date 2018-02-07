using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Ability : Photon.MonoBehaviour
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
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = this.photonView.instantiationData;

        if (data != null && data.Length == 1)
        {
            ownerCharacter = (Character)data[0];
            Init();
        }
    }

    public virtual void Init()
    {
        this.transform.parent = ownerCharacter.transform;
        ownerCharacter.abilities.Add(this);
    }

    //---------------------------
    //      Ability Action
    //---------------------------
    public virtual void ActivateAbility()
    {

    }
}

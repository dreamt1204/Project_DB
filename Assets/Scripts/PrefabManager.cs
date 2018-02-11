using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    //===========================
    //      Variables
    //===========================
    public static PrefabManager instance;

    // Prefabs
    [Header("Characters")]
    public List<Character> CharacterPrefabs;

    [Header("UI Elements")]
    public UIWidget HealthBarPrefab;
	public UIWidget DamageTextPrefab;

    [Header("Basic Abilities")]
    public Ability BasicShootPrefab;


    //===========================
    //      Functions
    //===========================
    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    //---------------------------
    //      Static Functions
    //---------------------------
    public static Character GetCharacterPrefab(int characterID)
    {
        return instance.CharacterPrefabs[characterID];
    }

    public static Character GetCharacterPrefab(PhotonPlayer player)
    {
        int characterID = 0;
        object IDObj;

        if (player.CustomProperties.TryGetValue("selectedCharacterID", out IDObj))
            characterID = (int)IDObj;

        return GetCharacterPrefab(characterID);
    }
}

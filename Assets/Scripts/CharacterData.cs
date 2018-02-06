using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Information")]
    public string characterName = "Empty Character";

    [Header("Art")]
    public GameObject model;

    [Header("Attributes")]
    [Range(1, 200)] public float health = 100;
    [Range(1, 200)] public float speed = 100;
    [Range(1, 200)] public float power = 100;
    [Range(1, 200)] public float defend = 100;

    [Header("Abilities")] public List<Ability> abilityPrefabs = new List<Ability>();
}

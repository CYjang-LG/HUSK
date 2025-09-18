using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Game/Character Profile")]

public class CharacterProfile : ScriptableObject
{
    [Header("multipliers")]
    [Min(0f)] public float moveSpeedMul = 1f;
    [Min(0f)] public float weaponSpeedMul = 1f;
    [Min(0f)] public float weaponRateMul = 1f;
    [Min(0f)] public float damageMul = 1f;
    [Min(0f)] public float maxHealth = 100f;
    public int extraProjectileCount = 0;
}
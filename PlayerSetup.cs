// PlayerSetup.cs
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
public class PlayerSetup : MonoBehaviour
{
    public CharacterProfile[] profiles;

    //component exm
    public PlayerMovement movement;
    public PlayerCombat combat;
    public Health health;
    public CharacterProfile SelectedProfile { get; private set; }

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        // GameManager의 playerId 기반 선택
        int id = GameManager.instance.playerId;
        id = Mathf.Clamp(id, 0, profiles.Length - 1);
        SelectedProfile = profiles[id];

        // 이동/공격 배수 적용
        movement.SetSpeedMultiplier(SelectedProfile.moveSpeedMul);
        combat.SetDamageMultiplier(SelectedProfile.damageMul);
        combat.SetOrbitSpeedMultiplier(SelectedProfile.weaponSpeedMul);
        combat.SetFireIntervalMultiplier(SelectedProfile.weaponRateMul);

        //player health reset
        health.SetMaxHealth(SelectedProfile.maxHealth);
        health.ResetHealth();
    }
}


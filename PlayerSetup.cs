// PlayerSetup.cs
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
public class PlayerSetup : MonoBehaviour
{
    public CharacterProfile[] profiles;
    public SpriteRenderer playerSprite;
    public PlayerMovement movement;
    public PlayerCombat combat;

    public CharacterProfile SelectedProfile { get; private set; }

    void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
    }

    void OnEnable()
    {
        // GameManager의 playerId 기반 선택
        int id = GameManager.Instance.playerId;
        id = Mathf.Clamp(id, 0, profiles.Length - 1);
        SelectedProfile = profiles[id];

        // 이동/공격 배수 적용
        movement.SetSpeedMultiplier(SelectedProfile.moveSpeedMul);
        combat.SetDamageMultiplier(SelectedProfile.damageMul);
        combat.SetOrbitSpeedMultiplier(SelectedProfile.weaponSpeedMul);
        combat.SetFireIntervalMultiplier(SelectedProfile.weaponRateMul);

        // 애니메이터/스킨 교체가 있다면 여기서 처리
        // ex) GetComponent<Animator>().runtimeAnimatorController = ...
    }
}

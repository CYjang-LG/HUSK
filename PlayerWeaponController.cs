using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("무기 슬롯")]
    public Transform[] weaponSlots = new Transform[3]; // 최대 3개 무기

    [Header("손 컨트롤러")]
    public HandController handController;

    private List<WeaponBase> equippedWeapons = new List<WeaponBase>();
    private Scanner scanner;

    void Awake()
    {
        scanner = GetComponent<Scanner>();

        // 무기 슬롯 생성
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                GameObject slot = new GameObject($"WeaponSlot_{i}");
                slot.transform.SetParent(transform);
                slot.transform.localPosition = Vector3.zero;
                weaponSlots[i] = slot.transform;
            }
        }
    }

    public bool EquipWeapon(ItemData weaponData)
    {
        // 빈 슬롯 찾기
        int emptySlot = FindEmptySlot();
        if (emptySlot == -1)
        {
            Debug.Log("무기 슬롯이 가득 참!");
            return false;
        }

        // 무기 생성
        WeaponBase weapon = WeaponFactory.CreateWeapon(
            weaponData,
            GameManager.instance.pool,
            scanner,
            weaponSlots[emptySlot]
        );

        if (weapon != null)
        {
            equippedWeapons.Add(weapon);

            // 손에 무기 스프라이트 설정
            if (handController != null && weaponData.handSprite != null)
            {
                handController.SetHandSprite(emptySlot, weaponData.handSprite, true);
            }

            Debug.Log($"무기 장착: {weaponData.itemName}");
            return true;
        }

        return false;
    }

    public void UnequipWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedWeapons.Count) return;

        WeaponBase weapon = equippedWeapons[slotIndex];
        if (weapon != null)
        {
            equippedWeapons.RemoveAt(slotIndex);
            Destroy(weapon.gameObject);

            // 손에서 무기 제거
            if (handController != null)
            {
                handController.SetHandSprite(slotIndex, null, false);
            }
        }
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (i >= equippedWeapons.Count || equippedWeapons[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    public List<WeaponBase> GetEquippedWeapons()
    {
        return new List<WeaponBase>(equippedWeapons);
    }
}

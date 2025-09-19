using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("���� ����")]
    public Transform[] weaponSlots = new Transform[3]; // �ִ� 3�� ����

    [Header("�� ��Ʈ�ѷ�")]
    public HandController handController;

    private List<WeaponBase> equippedWeapons = new List<WeaponBase>();
    private Scanner scanner;

    void Awake()
    {
        scanner = GetComponent<Scanner>();

        // ���� ���� ����
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
        // �� ���� ã��
        int emptySlot = FindEmptySlot();
        if (emptySlot == -1)
        {
            Debug.Log("���� ������ ���� ��!");
            return false;
        }

        // ���� ����
        WeaponBase weapon = WeaponFactory.CreateWeapon(
            weaponData,
            GameManager.instance.pool,
            scanner,
            weaponSlots[emptySlot]
        );

        if (weapon != null)
        {
            equippedWeapons.Add(weapon);

            // �տ� ���� ��������Ʈ ����
            if (handController != null && weaponData.handSprite != null)
            {
                handController.SetHandSprite(emptySlot, weaponData.handSprite, true);
            }

            Debug.Log($"���� ����: {weaponData.itemName}");
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

            // �տ��� ���� ����
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

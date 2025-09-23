using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Item, Gear, Hand, HandController 기능을 통합한 장비 관리 시스템
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    [Header("=== 손 컨트롤러 ===")]
    public Transform leftHand;
    public Transform rightHand;
    public SpriteRenderer leftHandRenderer;
    public SpriteRenderer rightHandRenderer;
    
    [Header("=== 장비 슬롯 ===")]
    public int maxGearSlots = 6;
    public Transform gearParent;
    
    [Header("=== 아이템 설정 ===")]
    public ItemData[] availableItems;
    
    // 장비된 아이템들
    private List<ItemData> equippedGears = new List<ItemData>();
    private Dictionary<int, Sprite> handSprites = new Dictionary<int, Sprite>();
    private Dictionary<int, bool> handVisibility = new Dictionary<int, bool>();
    
    // 아이템 레벨 및 효과
    private Dictionary<int, int> itemLevels = new Dictionary<int, int>();
    private Dictionary<int, float> itemEffects = new Dictionary<int, float>();
    
    void Awake()
    {
        InitializeEquipment();
    }
    
    #region 초기화
    private void InitializeEquipment()
    {
        // 손 컨트롤러 초기화
        SetupHandControllers();
        
        // 장비 슬롯 초기화
        SetupGearSlots();
        
        // 아이템 레벨 초기화
        InitializeItemLevels();
    }
    
    private void SetupHandControllers()
    {
        // 손 오브젝트가 없으면 생성
        if (leftHand == null)
        {
            GameObject leftHandObj = new GameObject("LeftHand");
            leftHandObj.transform.SetParent(transform);
            leftHand = leftHandObj.transform;
            leftHandRenderer = leftHandObj.AddComponent<SpriteRenderer>();
        }
        
        if (rightHand == null)
        {
            GameObject rightHandObj = new GameObject("RightHand");
            rightHandObj.transform.SetParent(transform);
            rightHand = rightHandObj.transform;
            rightHandRenderer = rightHandObj.AddComponent<SpriteRenderer>();
        }
        
        // 초기 손 위치 설정
        leftHand.localPosition = new Vector3(-0.3f, -0.2f, 0);
        rightHand.localPosition = new Vector3(0.3f, -0.2f, 0);
    }
    
    private void SetupGearSlots()
    {
        if (gearParent == null)
        {
            GameObject gearParentObj = new GameObject("GearParent");
            gearParentObj.transform.SetParent(transform);
            gearParent = gearParentObj.transform;
        }
    }
    
    private void InitializeItemLevels()
    {
        if (availableItems == null) return;
        
        foreach (var item in availableItems)
        {
            if (item != null)
            {
                itemLevels[item.itemId] = 0;
                itemEffects[item.itemId] = 0f;
            }
        }
    }
    #endregion
    
    #region 아이템 관리
    public bool EquipItem(ItemData itemData)
    {
        if (itemData == null) return false;
        
        switch (itemData.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                return EquipWeapon(itemData);
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                return EquipGear(itemData);
            case ItemData.ItemType.Heal:
                return UseHealItem(itemData);
            default:
                return false;
        }
    }
    
    private bool EquipWeapon(ItemData weaponData)
    {
        // PlayerController의 무기 장착 시스템 사용
        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            return playerController.EquipWeapon(weaponData);
        }
        return false;
    }
    
    private bool EquipGear(ItemData gearData)
    {
        if (equippedGears.Count >= maxGearSlots)
        {
            Debug.Log("장비 슬롯이 가득 참!");
            return false;
        }
        
        // 같은 타입의 장비가 이미 있는지 확인
        foreach (var gear in equippedGears)
        {
            if (gear.itemType == gearData.itemType)
            {
                // 기존 장비 업그레이드
                UpgradeGear(gear.itemId);
                return true;
            }
        }
        
        // 새로운 장비 추가
        equippedGears.Add(gearData);
        ApplyGearEffect(gearData);
        
        Debug.Log($"장비 장착: {gearData.itemName}");
        return true;
    }
    
    private bool UseHealItem(ItemData healData)
    {
        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            float healAmount = healData.baseDamage; // 힐 아이템의 경우 baseDamage를 힐량으로 사용
            playerController.Heal(healAmount);
            
            Debug.Log($"체력 회복: {healAmount}");
            return true;
        }
        return false;
    }
    #endregion
    
    #region 장비 효과 적용
    private void ApplyGearEffect(ItemData gearData)
    {
        var playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        
        switch (gearData.itemType)
        {
            case ItemData.ItemType.Shoe:
                // 신발: 이동속도 증가
                float speedBonus = gearData.baseDamage * 0.1f; // baseDamage를 속도 보너스로 활용
                playerController.SetSpeedMultiplier(1f + speedBonus);
                break;
                
            case ItemData.ItemType.Glove:
                // 장갑: 공격속도나 데미지 증가 (무기 시스템과 연동)
                ApplyWeaponBonus(gearData.baseDamage);
                break;
        }
    }
    
    private void ApplyWeaponBonus(float bonusAmount)
    {
        var playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        
        var equippedWeapons = playerController.GetEquippedWeapons();
        foreach (var weapon in equippedWeapons)
        {
            if (weapon != null)
            {
                // 무기에 보너스 적용 (WeaponBase에 보너스 시스템이 있다면)
                weapon.ApplyRateMultipliers(1.1f, 0.9f); // 속도 증가, 쿨다운 감소
            }
        }
    }
    
    private void UpgradeGear(int gearId)
    {
        if (itemLevels.ContainsKey(gearId))
        {
            itemLevels[gearId]++;
            
            // 레벨에 따른 추가 효과 적용
            var gearData = GetItemDataById(gearId);
            if (gearData != null)
            {
                ApplyGearEffect(gearData);
            }
            
            Debug.Log($"장비 업그레이드: ID {gearId}, 레벨 {itemLevels[gearId]}");
        }
    }
    #endregion
    
    #region 손 컨트롤러
    public void SetHandSprite(int handIndex, Sprite sprite, bool visible)
    {
        handSprites[handIndex] = sprite;
        handVisibility[handIndex] = visible;
        
        UpdateHandDisplay(handIndex);
    }
    
    private void UpdateHandDisplay(int handIndex)
    {
        SpriteRenderer targetRenderer = handIndex == 0 ? leftHandRenderer : rightHandRenderer;
        
        if (targetRenderer != null)
        {
            if (handSprites.ContainsKey(handIndex))
            {
                targetRenderer.sprite = handSprites[handIndex];
            }
            
            if (handVisibility.ContainsKey(handIndex))
            {
                targetRenderer.enabled = handVisibility[handIndex];
            }
        }
    }
    
    public void UpdateHandPositions(Vector3 leftPos, Vector3 rightPos)
    {
        if (leftHand != null)
            leftHand.localPosition = leftPos;
            
        if (rightHand != null)
            rightHand.localPosition = rightPos;
    }
    
    public void SetHandVisibility(bool visible)
    {
        if (leftHandRenderer != null)
            leftHandRenderer.enabled = visible;
            
        if (rightHandRenderer != null)
            rightHandRenderer.enabled = visible;
    }
    #endregion
    
    #region 레벨업 아이템 선택
    public void SetupRandomItem()
    {
        // UIManager에서 호출되는 랜덤 아이템 설정
        if (availableItems == null || availableItems.Length == 0) return;
        
        ItemData randomItem = availableItems[Random.Range(0, availableItems.Length)];
        // 현재 아이템 데이터 설정 로직
    }
    
    public void OnSelect()
    {
        // 아이템 선택 시 호출되는 메서드
        // UIManager에서 호출됨
    }
    #endregion
    
    #region 유틸리티 메서드
    private ItemData GetItemDataById(int itemId)
    {
        if (availableItems == null) return null;
        
        foreach (var item in availableItems)
        {
            if (item != null && item.itemId == itemId)
            {
                return item;
            }
        }
        return null;
    }
    
    public List<ItemData> GetEquippedGears()
    {
        return new List<ItemData>(equippedGears);
    }
    
    public int GetItemLevel(int itemId)
    {
        return itemLevels.ContainsKey(itemId) ? itemLevels[itemId] : 0;
    }
    
    public float GetItemEffect(int itemId)
    {
        return itemEffects.ContainsKey(itemId) ? itemEffects[itemId] : 0f;
    }
    
    public bool HasGearType(ItemData.ItemType gearType)
    {
        foreach (var gear in equippedGears)
        {
            if (gear.itemType == gearType)
                return true;
        }
        return false;
    }
    
    public void RemoveGear(int itemId)
    {
        for (int i = equippedGears.Count - 1; i >= 0; i--)
        {
            if (equippedGears[i].itemId == itemId)
            {
                equippedGears.RemoveAt(i);
                break;
            }
        }
    }
    
    public void ClearAllGear()
    {
        equippedGears.Clear();
        handSprites.Clear();
        handVisibility.Clear();
        itemLevels.Clear();
        itemEffects.Clear();
    }
    #endregion
}

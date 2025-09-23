using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 모든 기능을 통합 관리하는 컨트롤러
/// PlayerMovement, PlayerCombat, PlayerEquipment, PlayerSetup, PlayerWeaponController, PlayerContactDamageReceiver 통합
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("=== 기본 설정 ===")]
    public CharacterProfile characterProfile;
    
    [Header("=== 이동 설정 ===")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public LayerMask groundLayerMask = 1;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    
    [Header("=== 체력 시스템 ===")]
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }
    
    [Header("=== 무기 시스템 ===")]
    public Transform[] weaponSlots = new Transform;
    public HandController handController;
    
    [Header("=== 전투 설정 ===")]
    public float contactDamage = 10f;
    public float invulnerabilityTime = 1f;
    
    // 컴포넌트 참조
    private Rigidbody2D rb;
    private VirtualJoystick joystick;
    private Scanner scanner;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    // 상태 변수
    private bool isGrounded;
    private bool isInvulnerable;
    private float speedMultiplier = 1f;
    private float lastInvulnerabilityTime;
    
    // 무기 관리
    private List<WeaponBase> equippedWeapons = new List<WeaponBase>();
    
    void Awake()
    {
        InitializeComponents();
        InitializeWeaponSlots();
        InitializeHealth();
    }
    
    void Start()
    {
        SetupCharacter();
    }
    
    void Update()
    {
        if (!GameManager.instance.isLive) return;
        
        HandleMovement();
        HandleJump();
        CheckInvulnerability();
    }
    
    void FixedUpdate()
    {
        CheckGrounded();
    }
    
    #region 초기화
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        scanner = GetComponent<Scanner>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        joystick = Object.FindFirstObjectByType<VirtualJoystick>();
        
        // Ground Check 오브젝트가 없으면 생성
        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = go.transform;
        }
    }
    
    private void InitializeWeaponSlots()
    {
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
    
    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        if (GameManager.instance != null)
        {
            GameManager.instance.health = currentHealth;
            GameManager.instance.maxHealth = maxHealth;
        }
    }
    
    private void SetupCharacter()
    {
        if (characterProfile != null)
        {
            maxHealth = characterProfile.maxHealth;
            moveSpeed = characterProfile.moveSpeed;
            currentHealth = maxHealth;
        }
    }
    #endregion
    
    #region 이동 및 점프
    private void HandleMovement()
    {
        float horizontal = GetHorizontalInput();
        rb.linearVelocity = new Vector2(horizontal * moveSpeed * speedMultiplier, rb.linearVelocity.y);
        
        // 스프라이트 방향 설정
        if (horizontal != 0)
        {
            spriteRenderer.flipX = horizontal < 0;
        }
    }
    
    private void HandleJump()
    {
        if (GetJumpInput() && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
    
    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
    }
    
    private float GetHorizontalInput()
    {
#if UNITY_EDITOR
        try { return Input.GetAxis("Horizontal"); }
        catch { return 0f; }
#else
        return joystick != null ? joystick.Horizontal : 0f;
#endif
    }
    
    private bool GetJumpInput()
    {
#if UNITY_EDITOR
        try { return Input.GetButtonDown("Jump"); }
        catch { return false; }
#else
        return joystick != null && joystick.IsJumping;
#endif
    }
    #endregion
    
    #region 체력 시스템
    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;
        
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        
        // GameManager 동기화
        if (GameManager.instance != null)
            GameManager.instance.health = currentHealth;
        
        // 무적 시간 설정
        SetInvulnerable();
        
        // 애니메이션 트리거
        if (animator != null)
            animator.SetTrigger("Hit");
        
        // 체력이 0이면 게임 오버
        if (currentHealth <= 0)
        {
            GameManager.instance.GameOver();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        if (GameManager.instance != null)
            GameManager.instance.health = currentHealth;
    }
    
    private void SetInvulnerable()
    {
        isInvulnerable = true;
        lastInvulnerabilityTime = Time.time;
    }
    
    private void CheckInvulnerability()
    {
        if (isInvulnerable && Time.time - lastInvulnerabilityTime >= invulnerabilityTime)
        {
            isInvulnerable = false;
        }
    }
    #endregion
    
    #region 무기 시스템
    public bool EquipWeapon(ItemData weaponData)
    {
        int emptySlot = FindEmptyWeaponSlot();
        if (emptySlot == -1)
        {
            Debug.Log("무기 슬롯이 가득 참!");
            return false;
        }
        
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
            
            if (handController != null)
            {
                handController.SetHandSprite(slotIndex, null, false);
            }
        }
    }
    
    private int FindEmptyWeaponSlot()
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
    #endregion
    
    #region 접촉 데미지
    void OnTriggerEnter2D(Collider2D other)
    {
        // 적과의 접촉 데미지 처리
        if (other.CompareTag("Enemy") && !isInvulnerable)
        {
            TakeDamage(contactDamage);
        }
    }
    #endregion
    
    #region 공용 메서드
    public void SetSpeedMultiplier(float multiplier) => speedMultiplier = multiplier;
    public void SetBaseMoveSpeed(float speed) => moveSpeed = speed;
    public void SetMaxHealth(float health) 
    { 
        maxHealth = health; 
        if (currentHealth > maxHealth) 
            currentHealth = maxHealth; 
    }
    public void ResetHealth() => currentHealth = maxHealth;
    public List<WeaponBase> GetEquippedWeapons() => new List<WeaponBase>(equippedWeapons);
    #endregion
    
    #region 디버그
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    #endregion
}

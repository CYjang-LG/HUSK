using System.Collections;
using UnityEngine;

/// <summary>
/// 적의 AI, 체력, 애니메이션을 모두 관리하는 통합 컨트롤러
/// Enemy.cs + Follow.cs 기능 통합
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("=== 적 스탯 ===")]
    public float speed;
    public float health;
    public float maxHealth;
    public float jumpForce = 5f;
    
    [Header("=== AI 설정 ===")]
    public float attackRange = 1.5f;
    public float jumpTriggerDistance = 3f;
    public float detectionRange = 10f;
    private float jumpCooldown = 2f;
    private float lastJumpTime;
    
    [Header("=== 애니메이션 ===")]
    public RuntimeAnimatorController[] animControllers;
    
    [Header("=== 지면 체크 ===")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;
    
    // 타겟 및 상태
    public Rigidbody2D target;
    private bool isLive;
    private bool isFollowing;
    
    // 컴포넌트 참조
    private Rigidbody2D rigid;
    private Collider2D coll;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private WaitForFixedUpdate wait;
    
    void Awake()
    {
        InitializeComponents();
    }
    
    void Update()
    {
        if (!GameManager.instance.isLive || !isLive) return;
        
        CheckGrounded();
        HandleAI();
    }
    
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive || !isLive || target == null) return;
        
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;
            
        HandleMovement();
    }
    
    void LateUpdate()
    {
        if (!GameManager.instance.isLive || target == null) return;
        
        UpdateVisuals();
    }
    
    #region 초기화
    private void InitializeComponents()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
        
        // 물리 설정
        rigid.gravityScale = 2f;
        rigid.freezeRotation = true;
        
        // Ground Check 생성
        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = go.transform;
        }
    }
    #endregion
    
    #region AI 시스템
    private void HandleAI()
    {
        if (target == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        
        // 감지 범위 체크
        isFollowing = distanceToPlayer <= detectionRange;
        
        // 점프 로직
        if (isFollowing && ShouldJump(distanceToPlayer))
        {
            Jump();
        }
    }
    
    private bool ShouldJump(float distanceToPlayer)
    {
        if (!isGrounded || Time.time - lastJumpTime <= jumpCooldown)
            return false;
            
        float heightDifference = target.position.y - transform.position.y;
        return heightDifference > 1f && distanceToPlayer < jumpTriggerDistance;
    }
    
    private void Jump()
    {
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
        lastJumpTime = Time.time;
    }
    #endregion
    
    #region 이동 시스템
    private void HandleMovement()
    {
        if (!isFollowing) return;
        
        Vector2 direction = GetDirectionToTarget();
        float distance = Mathf.Abs(target.position.x - rigid.position.x);
        
        // 공격 범위 밖이면 이동
        if (distance > attackRange)
        {
            Vector2 nextPosition = direction * speed * Time.fixedDeltaTime;
            rigid.MovePosition(new Vector2(rigid.position.x + nextPosition.x, rigid.position.y));
        }
    }
    
    private Vector2 GetDirectionToTarget()
    {
        return new Vector2(target.position.x - rigid.position.x, 0).normalized;
    }
    
    private void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }
    #endregion
    
    #region 시각적 업데이트
    private void UpdateVisuals()
    {
        // 스프라이트 방향 설정
        spriteRenderer.flipX = target.position.x < rigid.position.x;
    }
    #endregion
    
    #region 생명주기 관리
    private void OnEnable()
    {
        SetTarget();
        ResetState();
    }
    
    private void SetTarget()
    {
        if (GameManager.instance?.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }
    }
    
    private void ResetState()
    {
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriteRenderer.sortingOrder = 2;
        
        if (anim != null)
            anim.SetBool("Dead", false);
            
        health = maxHealth;
    }
    
    public void Initialize(SpawnData data)
    {
        // 애니메이터 설정
        if (anim != null && animControllers != null && data.spriteType < animControllers.Length)
        {
            anim.runtimeAnimatorController = animControllers[data.spriteType];
        }
        
        // 스탯 설정
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
        
        Debug.Log($"적 초기화: Type={data.spriteType}, HP={health}, Speed={speed}");
    }
    #endregion
    
    #region 전투 시스템
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet")) return;
        
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            TakeDamage(bullet.damage);
        }
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        StartCoroutine(KnockBack());
        
        if (health > 0)
        {
            if (anim != null)
                anim.SetTrigger("Hit");
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            Die();
        }
    }
    
    private void Die()
    {
        isLive = false;
        coll.enabled = false;
        rigid.simulated = false;
        spriteRenderer.sortingOrder = 1;
        
        if (anim != null)
            anim.SetBool("Dead", true);
            
        // GameManager 업데이트
        GameManager.instance.kill++;
        GameManager.instance.GetExp();
        
        if (GameManager.instance.isLive)
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.Dead);
    }
    
    private IEnumerator KnockBack()
    {
        yield return wait;
        
        if (GameManager.instance.player != null)
        {
            Vector3 playerPos = GameManager.instance.player.transform.position;
            Vector3 directionFromPlayer = transform.position - playerPos;
            rigid.AddForce(directionFromPlayer.normalized * 3, ForceMode2D.Impulse);
        }
    }
    
    // 애니메이션 이벤트에서 호출
    private void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
    }
    #endregion
    
    #region 디버그
    void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 점프 트리거 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpTriggerDistance);
        
        // Ground Check
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    #endregion
}

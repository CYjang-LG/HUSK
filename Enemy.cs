using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("적 스탯")]
    public float speed;
    public float health;
    public float maxHealth;
    public float jumpForce = 5f;

    [Header("AI 설정")]
    public float attackRange = 1.5f;
    public float jumpTriggerDistance = 3f;
    private float jumpCooldown = 2f;
    private float lastJumpTime;

    [Header("애니메이션")]
    public RuntimeAnimatorController[] animCon;

    [Header("지면 체크")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;

    public Rigidbody2D target;

    // 컴포넌트 참조
    bool isLive;
    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();

        rigid.gravityScale = 2f;
        rigid.freezeRotation = true;
    }

    void Update()
    {
        if (!GameManager.instance.isLive || !isLive)
            return;

        // 지면 체크
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // 점프 로직
        if (target != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            float heightDifference = target.position.y - transform.position.y;

            if (isGrounded && heightDifference > 1f && distanceToPlayer < jumpTriggerDistance
                && Time.time - lastJumpTime > jumpCooldown)
            {
                rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
                lastJumpTime = Time.time;
            }
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive || !isLive || target == null)
            return;

        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        // 이동 로직
        Vector2 dirVec = new Vector2(target.position.x - rigid.position.x, 0).normalized;

        float distance = Mathf.Abs(target.position.x - rigid.position.x);
        if (distance > attackRange)
        {
            Vector2 nextVec = dirVec * speed * Time.fixedDeltaTime;
            rigid.MovePosition(new Vector2(rigid.position.x + nextVec.x, rigid.position.y));
        }
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive || target == null)
            return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    private void OnEnable()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }

        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;

        if (anim != null)
            anim.SetBool("Dead", false);

        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        // 애니메이터 설정
        if (anim != null && animCon != null && data.spriteType < animCon.Length)
        {
            anim.runtimeAnimatorController = animCon[data.spriteType];
        }

        // 스탯 설정
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;

        Debug.Log($"적 초기화: Type={data.spriteType}, HP={health}, Speed={speed}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
            return;

        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            health -= bullet.damage;
        }

        StartCoroutine(KnockBack());

        if (health > 0)
        {
            if (anim != null)
                anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        isLive = false;
        coll.enabled = false;
        rigid.simulated = false;
        spriter.sortingOrder = 1;

        if (anim != null)
            anim.SetBool("Dead", true);

        GameManager.instance.kill++;
        GameManager.instance.GetExp();

        if (GameManager.instance.isLive)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
    }

    IEnumerator KnockBack()
    {
        yield return wait;

        if (GameManager.instance.player != null)
        {
            Vector3 playerPos = GameManager.instance.player.transform.position;
            Vector3 dirVec = transform.position - playerPos;
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
        }
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
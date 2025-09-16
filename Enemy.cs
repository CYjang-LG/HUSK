using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public float jumpForce = 5f; // 적 점프력
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;
    
    // 지면 체크
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;
    
    // AI 변수
    public float attackRange = 1.5f;
    public float jumpTriggerDistance = 3f;
    private float jumpCooldown = 2f;
    private float lastJumpTime;

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
        
        // 중력 설정
        rigid.gravityScale = 2f;
        rigid.freezeRotation = true;
    }

    void Update()
    {
        if (!GameManager.instance.isLive || !isLive)
            return;
            
        // 지면 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        float heightDifference = target.position.y - transform.position.y;
        
        // 점프 로직: 플레이어가 위에 있고 일정 거리 내에 있을 때
        if (isGrounded && heightDifference > 1f && distanceToPlayer < jumpTriggerDistance 
            && Time.time - lastJumpTime > jumpCooldown)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
            lastJumpTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive || !isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        // 횡스크롤 적 AI: 주로 X축으로만 이동
        Vector2 dirVec = new Vector2(target.position.x - rigid.position.x, 0).normalized;
        
        // 공격 범위 밖에 있을 때만 이동
        float distance = Mathf.Abs(target.position.x - rigid.position.x);
        if (distance > attackRange)
        {
            Vector2 nextVec = dirVec * speed * Time.fixedDeltaTime;
            rigid.MovePosition(new Vector2(rigid.position.x + nextVec.x, rigid.position.y));
        }
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
            
        // X축 기준으로 스프라이트 뒤집기
        spriter.flipX = target.position.x < rigid.position.x;
    }

    private void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
            return;
        health -= collision.GetComponent<Bullet>().damage;

        //StartCoroutine("KnockBack");
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            anim.SetBool("Dead",true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();

            if(GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    IEnumerator KnockBack()
    {
        //yield return null;  // 1프레임 쉬기
        //yield return new WaitForSeconds(2f);    // 2초 쉬기
        yield return wait;//하나의 물리 프레임을 딜레이 주기
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3,ForceMode2D.Impulse);

    }

    void Dead()
    {
        gameObject.SetActive(false);
    }

}


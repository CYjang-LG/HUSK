using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public float jumpForce = 10f; // 점프력 추가
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;
    
    // 지면 체크 변수 추가
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
        
        // 횡스크롤을 위한 중력 설정
        rigid.gravityScale = 2f;
        rigid.freezeRotation = true;
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
            
        // 지면 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 점프 입력 처리 (Space 키)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
        }
        
        // 낙하 애니메이션
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("VerticalVelocity", rigid.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
            
        // 횡스크롤: X축 이동만 처리, Y축은 중력에 맡김
        Vector2 moveVec = new Vector2(inputVec.x * speed * Time.fixedDeltaTime, 0);
        rigid.MovePosition(new Vector2(rigid.position.x + moveVec.x, rigid.position.y));
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        // 횡스크롤에서는 X축 입력만 사용
        inputVec = new Vector2(input.x, 0);
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        anim.SetFloat("Speed", Mathf.Abs(inputVec.x));

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(!GameManager.instance.isLive)
            return;

        if(collision.gameObject.CompareTag("Enemy"))
        {
            GameManager.instance.health -= Time.deltaTime * 10;

            if(GameManager.instance.health < 0)
            {
                for(int index = 2; index < transform.childCount; index++)
                {
                    transform.GetChild(index).gameObject.SetActive(false);
                }
                anim.SetTrigger("Dead");
                GameManager.instance.GameOver();
            }
        }
    }
}

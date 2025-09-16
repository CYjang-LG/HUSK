using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;


    // 필요한 컴포넌트 선언
    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {  // 필요컴포넌트 초기화
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
    }

    void Shoot()
    {
        if (!GameManager.instance.isLive) return;
        if (scanner == null || scanner.nearestTarget == null) return;

        Vector3 targetPos = scanner.nearestTarget.position;
        foreach (Hand hand in hands)
        {
            if (!hand.isLeft) // 오른손만 총 조준
            {
                hand.AimAt(targetPos); // 발사 시점에서 조준
            }
        }

    }
        void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime; //대각선 이동에서도 거리에 따라 비슷하게 이동하도록


        //3. 위치이동
        rigid.MovePosition(rigid.position + nextVec);
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0){
            spriter.flipX = inputVec.x < 0;  //because left key is (-)
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(!GameManager.instance.isLive)
            return;

        GameManager.instance.health -= Time.deltaTime * 10;

        if(GameManager.instance.health <0)
        {
            for(int index = 2; index<transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }
            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }
}

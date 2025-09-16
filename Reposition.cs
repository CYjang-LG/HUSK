using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    Collider2D coll;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;
        Vector3 PlayerPos=GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;


        switch (transform.tag){
            case "Ground":
                float diffX = PlayerPos.x - myPos.x;
                float diffY = PlayerPos.y - myPos.y;
                float dirX = diffX < 0 ? -1 : 1;
                float dirY = diffY < 0 ? -1 : 1;
                diffX=Mathf.Abs(diffX);
                diffY=Mathf.Abs(diffY);
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 40);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 40);
                }
                else
                {
                    transform.Translate(dirX * 40, dirY * 40, 0);
                }
                    break;
            case "Enemy":
                if (coll.enabled)
                {
                    Vector3 dist = PlayerPos - myPos;
                    Vector3 ran = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);
                    transform.Translate(ran + dist*2);  // 카메라가 안보이는 곳에서 발생되게 하기 위해 맵 범위 20으로 설정
                }
                break;
        }
    }
}

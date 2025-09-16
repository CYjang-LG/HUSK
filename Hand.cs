using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;
    public SpriteRenderer spriter;

    // Player 관련 컴포넌트 참조
    SpriteRenderer playerSprite;
    Scanner scanner;

    // 오른손 위치 값
    Vector3 rightPos = new Vector3(0.35f, -0.15f, 0);
    Vector3 rightPosReverse = new Vector3(-0.15f, -0.15f, 0);
    // 왼손 기본 각도 값
    Quaternion leftRot = Quaternion.Euler(0, 0, -35);
    Quaternion leftRotReverse = Quaternion.Euler(0, 0, 35);

    void Awake()
    {
        // 부모에 있는 Player 컴포넌트를 먼저 찾습니다.
        Player player = GetComponentInParent<Player>();
        if (player != null)
        {
            // Player 컴포넌트를 통해 다른 컴포넌트들을 안전하게 가져옵니다.
            playerSprite = player.GetComponent<SpriteRenderer>();
            scanner = player.scanner;
        }
        else
        {
            // Player를 찾지 못했을 경우, 에러 로그를 남겨서 문제를 쉽게 파악하도록 합니다.
            Debug.LogError("Hand 스크립트가 부모에서 Player 컴포넌트를 찾지 못했습니다!", gameObject);
        }
    }

    public void AimAt(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        spriter.flipY = Mathf.Abs(angle) > 90f;
        spriter.flipX = false;
        spriter.sortingOrder = playerSprite.flipX ? 6 : 6;
    }

    void LateUpdate()
    {
        // playerSprite가 할당되지 않았다면 로직을 실행하지 않습니다.
        if (playerSprite == null) return;

        bool isReverse = playerSprite.flipX;

        // isLeft가 true일 때 (총을 들지 않은 왼손)
        if (isLeft)
        {
            // 왼손은 기존처럼 기본 자세를 유지합니다.
            transform.localRotation = isReverse ? leftRotReverse : leftRot;
            spriter.flipX = isReverse;
            spriter.flipY = false;
            spriter.sortingOrder = isReverse ? 4 : 6;
        }
        // 오른손 로직 (총을 든 오른손)
        else
        {
            // 스캐너가 있고, 가장 가까운 타겟이 감지되었을 때
            if (scanner != null && scanner.nearestTarget != null)
            {
                // === 조준 로직 시작 ===
                Vector3 targetPos = scanner.nearestTarget.position;
                Vector3 dir = targetPos - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                // === 조준 로직 끝 ===

                // 총의 각도에 따라 스프라이트의 Y축을 뒤집어줍니다.
                spriter.flipY = Mathf.Abs(angle) > 90f;
                // 회전으로 방향을 제어하므로 X축 뒤집기는 비활성화합니다.
                spriter.flipX = false;

                // 플레이어 방향에 따라 렌더링 순서를 조절합니다.
                spriter.sortingOrder = isReverse ? 6 : 6;
            }
            else
            {
                transform.localPosition = isReverse ? rightPosReverse : rightPos;
                spriter.flipX = isReverse;
                spriter.flipY = false;
                spriter.sortingOrder = isReverse ? 6 : 6;
            }
        }
    }
}

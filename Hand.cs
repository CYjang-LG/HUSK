using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;
    public SpriteRenderer spriter;

    // Player ���� ������Ʈ ����
    SpriteRenderer playerSprite;
    Scanner scanner;

    // ������ ��ġ ��
    Vector3 rightPos = new Vector3(0.35f, -0.15f, 0);
    Vector3 rightPosReverse = new Vector3(-0.15f, -0.15f, 0);
    // �޼� �⺻ ���� ��
    Quaternion leftRot = Quaternion.Euler(0, 0, -35);
    Quaternion leftRotReverse = Quaternion.Euler(0, 0, 35);

    void Awake()
    {
        // �θ� �ִ� Player ������Ʈ�� ���� ã���ϴ�.
        Player player = GetComponentInParent<Player>();
        if (player != null)
        {
            // Player ������Ʈ�� ���� �ٸ� ������Ʈ���� �����ϰ� �����ɴϴ�.
            playerSprite = player.GetComponent<SpriteRenderer>();
            scanner = player.scanner;
        }
        else
        {
            // Player�� ã�� ������ ���, ���� �α׸� ���ܼ� ������ ���� �ľ��ϵ��� �մϴ�.
            Debug.LogError("Hand ��ũ��Ʈ�� �θ𿡼� Player ������Ʈ�� ã�� ���߽��ϴ�!", gameObject);
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
        // playerSprite�� �Ҵ���� �ʾҴٸ� ������ �������� �ʽ��ϴ�.
        if (playerSprite == null) return;

        bool isReverse = playerSprite.flipX;

        // isLeft�� true�� �� (���� ���� ���� �޼�)
        if (isLeft)
        {
            // �޼��� ����ó�� �⺻ �ڼ��� �����մϴ�.
            transform.localRotation = isReverse ? leftRotReverse : leftRot;
            spriter.flipX = isReverse;
            spriter.flipY = false;
            spriter.sortingOrder = isReverse ? 4 : 6;
        }
        // ������ ���� (���� �� ������)
        else
        {
            // ��ĳ�ʰ� �ְ�, ���� ����� Ÿ���� �����Ǿ��� ��
            if (scanner != null && scanner.nearestTarget != null)
            {
                // === ���� ���� ���� ===
                Vector3 targetPos = scanner.nearestTarget.position;
                Vector3 dir = targetPos - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                // === ���� ���� �� ===

                // ���� ������ ���� ��������Ʈ�� Y���� �������ݴϴ�.
                spriter.flipY = Mathf.Abs(angle) > 90f;
                // ȸ������ ������ �����ϹǷ� X�� ������� ��Ȱ��ȭ�մϴ�.
                spriter.flipX = false;

                // �÷��̾� ���⿡ ���� ������ ������ �����մϴ�.
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
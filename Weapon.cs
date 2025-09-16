using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;

    float timer;
    Player player;

    void Awake()
    {
        player = GameManager.Instance.player;
    }

    void Update()
    {
        if (!GameManager.Instance.isLive)
            return;

        switch (id)
        {
            case 0:
                // ȸ���� ����
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;

            default:
                // �߻��� ����
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.count += count;
        this.damage = damage * Character.Damage;

        if (id == 0)
            Batch();

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void Init(ItemData data)
    {
        // �⺻ ����
        name = "Weapon" + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        // �Ӽ� ����
        id = data.itemId;
        damage = data.baseDamage * Character.Damage;
        count = data.baseCount + Character.Count;

        // Ǯ���� ������ ID ã��
        for (int index = 0; index < GameManager.Instance.pool.prefabs.Length; index++)
        {
            if (data.projectile == GameManager.Instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }

        // ���� Ÿ�Ժ� �ӵ� ����
        switch (id)
        {
            case 0:
                speed = 150 * Character.Speed;
                Batch();
                break;
            default:
                speed = 0.5f * Character.WeaponRate;
                break;
        }

        // �տ� ���� ��������Ʈ ����
        Hand hand = player.hands[(int)data.itemType];
        hand.spriter.sprite = data.handSprite;
        hand.gameObject.SetActive(true);

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    void Batch()
    {
        for (int index = 0; index < count; index++)
        {
            Transform bullet;

            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
            }
            else
            {
                bullet = GameManager.Instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * index / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);

            // speed ���� �߰�
            bullet.GetComponent<Bullet>().Init(damage, -100, Vector3.zero, speed);
        }
    }

    void Fire()
    {
        if (!player.scanner.nearestTarget)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        Transform bullet = GameManager.Instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // speed ���� �߰�
        bullet.GetComponent<Bullet>().Init(damage, count, dir, speed);

        AudioManager.Instance.PlaySfx(AudioManager.Sfx.Range);
    }
}

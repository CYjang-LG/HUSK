// HandController.cs
using UnityEngine;

public class HandController : MonoBehaviour
{
    [System.Serializable]
    public class HandSlot
    {
        public bool isLeft;
        public SpriteRenderer renderer;
        public Transform pivot;
    }

    public HandSlot[] hands;
    public SpriteRenderer playerSprite;

    public void SetHandSprite(int slotIndex, Sprite sprite, bool active)
    {
        if (slotIndex < 0 || slotIndex >= hands.Length) return;
        var h = hands[slotIndex];
        if (h?.renderer == null) return;

        h.renderer.sprite = sprite;
        h.renderer.gameObject.SetActive(active);
    }

    public void AimRightHandAt(Vector3 worldPos)
    {
        // 오른손이 스캔 타겟을 조준하도록 사용(원거리 무기)
        foreach (var h in hands)
        {
            if (h.isLeft || h.pivot == null) continue;

            Vector3 dir = worldPos - h.pivot.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            h.pivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            if (h.renderer)
            {
                h.renderer.flipY = Mathf.Abs(angle) > 90f;
                h.renderer.flipX = false;
                h.renderer.sortingOrder = 6;
            }
        }
    }

    void LateUpdate()
    {
        if (playerSprite == null) return;

        bool isReverse = playerSprite.flipX;
        foreach (var h in hands)
        {
            if (h?.renderer == null) continue;
            if (h.isLeft)
            {
                h.renderer.flipX = isReverse;
                h.renderer.flipY = false;
                h.renderer.sortingOrder = isReverse ? 4 : 6;
            }
            else
            {
                h.renderer.flipX = false;
                h.renderer.sortingOrder = 6;
            }
        }
    }
}

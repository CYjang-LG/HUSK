// PlayerMovement.cs
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Min(0f)] public float baseSpeed = 3f;
    private float speedMul = 1f;

    private Vector2 inputVec;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void SetSpeedMultiplier(float mul) => speedMul = Mathf.Max(0f, mul);

    void FixedUpdate()
    {
        if (!GameManager.Instance.isLive) return;
        rb.MovePosition(rb.position + inputVec * baseSpeed * speedMul * Time.fixedDeltaTime);
    }

    void OnMove(InputValue value) => inputVec = value.Get<Vector2>();
}

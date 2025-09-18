using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private VirtualJoystick joystick;

    [Header("이동 설정")]
    public float moveSpeed = 5f;
    private float speedMultiplier = 1f;

    [Header("점프 설정")]
    public float jumpForce = 5f;
    public LayerMask groundLayerMask = 1;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        joystick = Object.FindFirstObjectByType<VirtualJoystick>();

        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = go.transform;
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayerMask);

        float h = GetHorizontalInput();
        rb.linearVelocity = new Vector2(h* moveSpeed * speedMultiplier, rb.linearVelocity.y);

        if (GetJumpInput() && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public void SetSpeedMultiplier(float m) => speedMultiplier = m;
    public void SetBaseMoveSpeed(float s) => moveSpeed = s;

    private float GetHorizontalInput()
    {
#if UNITY_EDITOR
        try { return Input.GetAxis("Horizontal"); }
        catch { return 0f; }
#else
        return joystick != null ? joystick.Horizontal : 0f;
#endif
    }

    private bool GetJumpInput()
    {
#if UNITY_EDITOR
        try { return Input.GetButtonDown("Jump"); }
        catch { return false; }
#else
        return joystick != null && joystick.IsJumping;
#endif
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

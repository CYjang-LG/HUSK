using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private VirtualJoystick joystick;

    [Header("이동 설정")]
    public float moveSpeed = 5f;

    [Header("점프 설정")]
    public float jumpForce = 12f;
    public LayerMask groundLayerMask = 1; // Ground 레이어 설정
    public Transform groundCheck; // 발밑 체크 포인트
    public float groundCheckRadius = 0.2f;

    private bool isGrounded;
    private float speedMultiplier = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        joystick = Object.FindFirstObjectByType<VirtualJoystick>();

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Update()
    {
        // 바닥 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        // 이동
        float moveInput = GetHorizontalInput();
        rb.linearVelocity = new Vector2(moveInput * GetMoveSpeed(), rb.linearVelocity.y);

        // 점프
        if (GetJumpInput() && isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public float GetMoveSpeed()
    {
        return moveSpeed * speedMultiplier;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    private float GetHorizontalInput()
    {
#if UNITY_EDITOR
        try
        {
            return Input.GetAxis("Horizontal");
        }
        catch (System.Exception)
        {
            return 0f; // Input System 충돌 시 임시 처리
        }
#else
    return joystick != null ? joystick.Horizontal : 0f;
#endif
    }

    private bool GetJumpInput()
    {
#if UNITY_EDITOR
        return Input.GetButtonDown("Jump");
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

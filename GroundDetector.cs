using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Ground Detection")]
    public LayerMask groundLayer = -1;
    public float checkDistance = 0.1f;

    private bool wasGrounded;
    public bool IsGrounded { get; private set; }

    public System.Action onLanded;
    public System.Action onLeftGrounded;

    void FixedUpdate()
    {
        CheckGround();
    }

    void CheckGround()
    {
        wasGrounded = IsGrounded;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, checkDistance, groundLayer);
        IsGrounded = hit.collider != null && hit.collider.CompareTag("Ground");

        //drop to Character Event
        if(IsGrounded && !wasGrounded)
        {
            onLanded?.Invoke();
        }
        else if (!IsGrounded && wasGrounded)
        {
            onLeftGrounded?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector2.down * checkDistance);
    }
}

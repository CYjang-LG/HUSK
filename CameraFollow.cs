using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Player Transform
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 2, -10);
    
    // 카메라 경계 설정
    public bool useBounds = true;
    public float minX = -20f;
    public float maxX = 20f;
    public float minY = -5f;
    public float maxY = 10f;
    
    // 데드존 설정 (플레이어가 이 영역을 벗어날 때만 카메라 이동)
    public float deadZoneWidth = 2f;
    public float deadZoneHeight = 1f;
    
    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        if (target == null && GameManager.instance != null)
        {
            target = GameManager.instance.player.transform;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        
        // 데드존 적용
        Vector3 currentPos = transform.position;
        
        // X축 데드존
        if (Mathf.Abs(desiredPosition.x - currentPos.x) > deadZoneWidth)
        {
            currentPos.x = Mathf.Lerp(currentPos.x, desiredPosition.x, smoothSpeed);
        }
        
        // Y축 데드존
        if (Mathf.Abs(desiredPosition.y - currentPos.y) > deadZoneHeight)
        {
            currentPos.y = Mathf.Lerp(currentPos.y, desiredPosition.y, smoothSpeed);
        }
        
        // 카메라 경계 적용
        if (useBounds)
        {
            currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
            currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);
        }
        
        currentPos.z = desiredPosition.z; // Z축은 항상 유지
        
        transform.position = currentPos;
    }
    
    // 카메라 흔들기 효과 (피격 시 등)
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.position = new Vector3(
                transform.position.x + x, 
                transform.position.y + y, 
                originalPos.z
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPos;
    }
    
    // 에디터에서 카메라 경계 시각화
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
        Gizmos.DrawWireCube(center, size);
        
        // 데드존 시각화
        if (Application.isPlaying && target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                target.position, 
                new Vector3(deadZoneWidth * 2, deadZoneHeight * 2, 0)
            );
        }
    }
}

using UnityEngine;

/// <summary>
/// Scanner, Health, Reposition, GroundDetector, CameraFollow, ParallaxBackground 기능을 통합한 유틸리티 매니저
/// </summary>
public class UtilityManager : MonoBehaviour
{
    [Header("=== 스캐너 설정 ===")]
    public float scanRange = 5f;
    public LayerMask targetLayer = 1;
    
    [Header("=== 카메라 설정 ===")]
    public Transform followTarget;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothSpeed = 0.125f;
    
    [Header("=== 패럴랙스 설정 ===")]
    public Transform[] backgroundLayers;
    public float[] parallaxSpeeds = { 0.1f, 0.3f, 0.5f };
    
    [Header("=== 리포지션 설정 ===")]
    public float repositionDistance = 20f;
    
    // 스캐너 관련
    private RaycastHit2D[] scanTargets;
    public Transform nearestTarget { get; private set; }
    
    // 카메라 관련
    private Camera mainCamera;
    private Vector3 lastCameraPosition;
    
    // 컴포넌트 참조
    private Collider2D myCollider;
    
    void Awake()
    {
        InitializeComponents();
    }
    
    void Start()
    {
        SetupCamera();
        SetupParallax();
    }
    
    void Update()
    {
        if (GameManager.instance?.isLive == true)
        {
            UpdateCameraFollow();
        }
    }
    
    void FixedUpdate()
    {
        if (GameManager.instance?.isLive == true)
        {
            UpdateScanner();
            UpdateParallax();
        }
    }
    
    #region 초기화
    private void InitializeComponents()
    {
        myCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        
        if (followTarget == null && GameManager.instance?.player != null)
        {
            followTarget = GameManager.instance.player.transform;
        }
    }
    
    private void SetupCamera()
    {
        if (mainCamera != null && followTarget != null)
        {
            lastCameraPosition = mainCamera.transform.position;
        }
    }
    
    private void SetupParallax()
    {
        // 패럴랙스 레이어 자동 검색
        if (backgroundLayers == null || backgroundLayers.Length == 0)
        {
            FindParallaxLayers();
        }
    }
    
    private void FindParallaxLayers()
    {
        GameObject[] backgrounds = GameObject.FindGameObjectsWithTag("Background");
        if (backgrounds.Length > 0)
        {
            backgroundLayers = new Transform[backgrounds.Length];
            for (int i = 0; i < backgrounds.Length; i++)
            {
                backgroundLayers[i] = backgrounds[i].transform;
            }
        }
    }
    #endregion
    
    #region 스캐너 시스템
    private void UpdateScanner()
    {
        scanTargets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        nearestTarget = GetNearestTarget();
    }
    
    private Transform GetNearestTarget()
    {
        Transform result = null;
        float shortestDistance = Mathf.Infinity;
        
        foreach (RaycastHit2D target in scanTargets)
        {
            if (target.transform == null) continue;
            
            float distance = Vector3.Distance(transform.position, target.transform.position);
            
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                result = target.transform;
            }
        }
        
        return result;
    }
    
    // 외부에서 스캔 결과를 가져올 수 있는 메서드
    public Transform GetNearestEnemy() => nearestTarget;
    public RaycastHit2D[] GetAllTargetsInRange() => scanTargets;
    #endregion
    
    #region 카메라 시스템
    private void UpdateCameraFollow()
    {
        if (mainCamera == null || followTarget == null) return;
        
        Vector3 desiredPosition = followTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(mainCamera.transform.position, desiredPosition, smoothSpeed);
        mainCamera.transform.position = smoothedPosition;
        
        lastCameraPosition = mainCamera.transform.position;
    }
    
    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }
    
    public void SetCameraOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    #endregion
    
    #region 패럴랙스 시스템
    private void UpdateParallax()
    {
        if (mainCamera == null || backgroundLayers == null) return;
        
        Vector3 cameraMovement = mainCamera.transform.position - lastCameraPosition;
        
        for (int i = 0; i < backgroundLayers.Length && i < parallaxSpeeds.Length; i++)
        {
            if (backgroundLayers[i] != null)
            {
                Vector3 parallaxMovement = cameraMovement * parallaxSpeeds[i];
                backgroundLayers[i].position += parallaxMovement;
            }
        }
    }
    #endregion
    
    #region 리포지션 시스템
    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area")) return;
        
        HandleReposition(collision);
    }
    
    private void HandleReposition(Collider2D collision)
    {
        if (GameManager.instance?.player == null) return;
        
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;
        
        switch (transform.tag)
        {
            case "Ground":
                RepositionGround(playerPos, myPos);
                break;
            case "Enemy":
                RepositionEnemy(playerPos, myPos);
                break;
            case "Background":
                RepositionBackground(playerPos, myPos);
                break;
        }
    }
    
    private void RepositionGround(Vector3 playerPos, Vector3 myPos)
    {
        float diffX = playerPos.x - myPos.x;
        float diffY = playerPos.y - myPos.y;
        float dirX = diffX < 0 ? -1 : 1;
        float dirY = diffY < 0 ? -1 : 1;
        
        diffX = Mathf.Abs(diffX);
        diffY = Mathf.Abs(diffY);
        
        if (diffX > diffY)
        {
            transform.Translate(Vector3.right * dirX * repositionDistance * 2);
        }
        else if (diffX < diffY)
        {
            transform.Translate(Vector3.up * dirY * repositionDistance * 2);
        }
        else
        {
            transform.Translate(dirX * repositionDistance * 2, dirY * repositionDistance * 2, 0);
        }
    }
    
    private void RepositionEnemy(Vector3 playerPos, Vector3 myPos)
    {
        if (myCollider != null && myCollider.enabled)
        {
            Vector3 distance = playerPos - myPos;
            Vector3 randomOffset = new Vector3(
                Random.Range(-3, 3), 
                Random.Range(-3, 3), 
                0
            );
            transform.Translate(randomOffset + distance * 2);
        }
    }
    
    private void RepositionBackground(Vector3 playerPos, Vector3 myPos)
    {
        Vector3 distance = playerPos - myPos;
        if (distance.magnitude > repositionDistance)
        {
            transform.position = playerPos + distance.normalized * repositionDistance * 0.8f;
        }
    }
    #endregion

    #region 지면 감지 (Ground Detector)
    public static bool CheckGroundAtPosition(Vector3 position, float radius = 0.2f, LayerMask groundLayer = default)
    {
        if (groundLayer == default)
            groundLayer = LayerMask.GetMask("Ground"); // "Ground" 레이어만 검사

        return Physics2D.OverlapCircle(position, radius, groundLayer);
    }
    public static RaycastHit2D GetGroundInfo(Vector3 position, float distance = 1f, LayerMask groundLayer = default)
    {
        if (groundLayer == default)
            groundLayer = LayerMask.GetMask("Ground"); // "Ground" 레이어만 사용

        return Physics2D.Raycast(position, Vector2.down, distance, groundLayer);
    }
    #endregion

    #region 헬스 관리 (정적 메서드로 제공)
    public static void ApplyDamage(GameObject target, float damage)
    {
        var healthComponent = target.GetComponent<PlayerController>() ?? 
                            target.GetComponent<EnemyController>() as MonoBehaviour;
        
        if (healthComponent is PlayerController player)
        {
            player.TakeDamage(damage);
        }
        else if (healthComponent is EnemyController enemy)
        {
            enemy.TakeDamage(damage);
        }
    }
    
    public static void ApplyHealing(GameObject target, float healAmount)
    {
        var playerController = target.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Heal(healAmount);
        }
    }
    #endregion
    
    #region 디버그
    void OnDrawGizmosSelected()
    {
        // 스캔 범위 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, scanRange);
        
        // 리포지션 거리 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, repositionDistance);
        
        // 가장 가까운 타겟 표시
        if (nearestTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, nearestTarget.position);
        }
    }
    #endregion
}

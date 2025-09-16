using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxEffectMultiplier = 0.5f; // 0 = 움직임 없음, 1 = 카메라와 동일하게 움직임
    public bool infiniteHorizontal = true; // 무한 반복 배경
    public float textureUnitSizeX; // 배경 텍스처의 가로 크기
    
    private Vector3 lastCameraPosition;
    private float startPositionX;
    private float startPositionY;
    
    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
            
        lastCameraPosition = cameraTransform.position;
        startPositionX = transform.position.x;
        startPositionY = transform.position.y;
        
        // 스프라이트 크기 자동 계산
        if (infiniteHorizontal && textureUnitSizeX <= 0)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Texture2D texture = sr.sprite.texture;
                textureUnitSizeX = texture.width / sr.sprite.pixelsPerUnit * transform.localScale.x;
            }
        }
    }
    
    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // Parallax 효과 적용
        transform.position += new Vector3(
            deltaMovement.x * parallaxEffectMultiplier,
            deltaMovement.y * parallaxEffectMultiplier,
            0
        );
        
        lastCameraPosition = cameraTransform.position;
        
        // 무한 스크롤 처리
        if (infiniteHorizontal && textureUnitSizeX > 0)
        {
            float distanceFromStart = cameraTransform.position.x - startPositionX;
            float offsetPositionX = distanceFromStart * (1 - parallaxEffectMultiplier);
            
            if (offsetPositionX > startPositionX + textureUnitSizeX)
            {
                startPositionX += textureUnitSizeX;
                transform.position = new Vector3(startPositionX, transform.position.y, transform.position.z);
            }
            else if (offsetPositionX < startPositionX - textureUnitSizeX)
            {
                startPositionX -= textureUnitSizeX;
                transform.position = new Vector3(startPositionX, transform.position.y, transform.position.z);
            }
        }
    }
    
    // 배경 레이어 복제 (무한 스크롤용)
    public void CreateParallaxClones()
    {
        if (!infiniteHorizontal) return;
        
        // 왼쪽 클론
        GameObject leftClone = Instantiate(gameObject, transform.parent);
        leftClone.name = gameObject.name + "_LeftClone";
        leftClone.transform.position = new Vector3(
            transform.position.x - textureUnitSizeX,
            transform.position.y,
            transform.position.z
        );
        
        // 오른쪽 클론
        GameObject rightClone = Instantiate(gameObject, transform.parent);
        rightClone.name = gameObject.name + "_RightClone";
        rightClone.transform.position = new Vector3(
            transform.position.x + textureUnitSizeX,
            transform.position.y,
            transform.position.z
        );
    }
}

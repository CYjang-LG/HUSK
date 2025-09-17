using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform transform;
        public float parallaxFactor;
    }

    public Layer[] layers;
    private Transform cameraTransform;
    private Vector3 lastCameraPos;

    void Start()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
            Debug.LogWarning("ParallaxBackground: Camera.main이 없습니다!");
        lastCameraPos = cameraTransform != null ? cameraTransform.position : Vector3.zero;
    }

    void Update()
    {
        if (cameraTransform == null) return;

        Vector3 delta = cameraTransform.position - lastCameraPos;
        foreach (var layer in layers)
        {
            Vector3 newPos = layer.transform.position + new Vector3(delta.x * layer.parallaxFactor, 0, 0);
            layer.transform.position = newPos;
        }
        lastCameraPos = cameraTransform.position;
    }
}

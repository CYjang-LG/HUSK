using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform transform;
        public float parallaxFactor;
        public float width;
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
            if (layer.transform == null) continue;

            Vector3 newPos = layer.transform.position + new Vector3(delta.x * layer.parallaxFactor, 0, 0);
            layer.transform.position = newPos;

            if (layer.width>0)
            {
                float cameraX = cameraTransform.position.x;
                float layerX = layer.transform.position.x; 
                if(layerX <cameraX - layer.width)
                {
                    layer.transform.position += new Vector3(layer.width * 2, 0, 0);
                }
                else if (layerX >cameraX + layer.width)
                {
                    layer.transform.position -= new Vector3(layer.width * 2, 0, 0);
                }
            }
        }
        lastCameraPos = cameraTransform.position;
    }
}

using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastCameraPositionX;
    private float cameraHalfWidth;

    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        FindCamera();
        InitializeLayers();
    }

    private void Start()
    {
        if (mainCamera == null)
        {
            FindCamera();
            InitializeLayers();
        }
    }

    private void FindCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
            lastCameraPositionX = mainCamera.transform.position.x;
        }
    }

    private void FixedUpdate()
    {
        if (mainCamera == null)
        {
            FindCamera();
            if (mainCamera == null) return;
        }

        float currentCameraPositionX = mainCamera.transform.position.x;
        float distanceToMove = currentCameraPositionX - lastCameraPositionX;
        lastCameraPositionX = currentCameraPositionX;

        float cameraLeftEdge = currentCameraPositionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPositionX + cameraHalfWidth;

        if (backgroundLayers == null) return;
        foreach (ParallaxLayer layer in backgroundLayers)
        {
            if (layer != null)
            {
                layer.Move(distanceToMove);
                layer.LoopBackground(cameraLeftEdge, cameraRightEdge);
            }
        }
    }

    public void InitializeLayers()
    {
        if (backgroundLayers == null) return;
        foreach (ParallaxLayer layer in backgroundLayers)
        {
            if (layer != null)
                layer.CalculateImageWidth();
        }
    }

    public void SetLayers(ParallaxLayer[] layers)
    {
        backgroundLayers = layers;
    }
}

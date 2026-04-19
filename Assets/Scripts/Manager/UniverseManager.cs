using UnityEngine;

public class UniverseManager : MonoBehaviour
{
    public static UniverseManager Instance { get; private set; }

    [Header("Environment References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform upWall;

    [Header("Expansion Settings (Per Level)")]
    [SerializeField] private float widthExpansion = 1.0f;
    [SerializeField] private float heightExpansion = 1.0f;
    [SerializeField] private float cameraSizeExpansion = 1.5f;

    private Vector3 initialLeftPos;
    private Vector3 initialRightPos;
    private Vector3 initialUpPos;
    private float initialCameraSize;

    private BoxCollider2D safeZoneCollider;
    private Vector2 initialSafeZoneSize;
    private Vector2 initialSafeZoneOffset;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCamera == null) mainCamera = Camera.main;

        safeZoneCollider = GetComponent<BoxCollider2D>();

        initialLeftPos = leftWall.position;
        initialRightPos = rightWall.position;
        initialUpPos = upWall.position;
        initialCameraSize = mainCamera.orthographicSize;

        if (safeZoneCollider != null)
        {
            initialSafeZoneSize = safeZoneCollider.size;
            initialSafeZoneOffset = safeZoneCollider.offset;
        }
    }

    private void Start()
    {
        ApplyUniverseSize();
    }

    public void ApplyUniverseSize()
    {
        int level = UpgradeManager.Instance.UniverseSizeLevel;
        int expansionMultiplier = level - 1; 

        leftWall.position = initialLeftPos + (Vector3.left * widthExpansion * expansionMultiplier);
        rightWall.position = initialRightPos + (Vector3.right * widthExpansion * expansionMultiplier);
        upWall.position = initialUpPos + (Vector3.up * heightExpansion * expansionMultiplier);

        mainCamera.orthographicSize = initialCameraSize + (cameraSizeExpansion * expansionMultiplier);

        if (safeZoneCollider != null)
        {
            float newWidth = initialSafeZoneSize.x + (widthExpansion * expansionMultiplier * 2f);
            
            float newHeight = initialSafeZoneSize.y + (heightExpansion * expansionMultiplier);

            safeZoneCollider.size = new Vector2(newWidth, newHeight);

            float newOffsetY = initialSafeZoneOffset.y + (heightExpansion * expansionMultiplier * 0.5f);
            safeZoneCollider.offset = new Vector2(initialSafeZoneOffset.x, newOffsetY);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class UniverseManager : MonoBehaviour
{
    public static UniverseManager Instance { get; private set; }

    public HashSet<ShipController> InsideShips { get; private set; } = new HashSet<ShipController>();

    [Header("Environment References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform wallsParent;
    [SerializeField] private Transform player;

    [Header("Expansion Settings (Per Level)")]
    [SerializeField] private float cameraSizeExpansion = 1.5f;

    private float initialCameraSize;
    private Vector3 initialPlayerPos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCamera == null) mainCamera = Camera.main;

        InsideShips = new HashSet<ShipController>(); // HashSet √ ±‚»≠

        initialCameraSize = mainCamera.orthographicSize;
        initialPlayerPos  = player.position;
    }

    private void Start()
    {
        ApplyUniverseSize();
    }

    public void ApplyUniverseSize()
    {
        int level = UpgradeManager.Instance.UniverseSizeLevel;
        float scale = (initialCameraSize + cameraSizeExpansion * (level - 1)) / initialCameraSize;

        mainCamera.orthographicSize = initialCameraSize * scale;
        wallsParent.localScale      = Vector3.one * scale;
        player.position             = initialPlayerPos * scale;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ShipController ship))
        {
            InsideShips.Add(ship);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ShipController ship))
        {
            InsideShips.Remove(ship);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpgradeManager.Instance.UpgradePassive("Universe");
            ApplyUniverseSize();
            Debug.Log($"[TEST] UniverseSize Lv.{UpgradeManager.Instance.UniverseSizeLevel}");
        }
    }
#endif
}

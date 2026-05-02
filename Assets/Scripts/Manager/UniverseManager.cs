using System.Collections;
using UnityEngine;

public class UniverseManager : MonoBehaviour
{
    public static UniverseManager Instance { get; private set; }

    [Header("Environment References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform wallsParent;
    [SerializeField] private Transform player;

    [Header("Expansion Settings (Per Level)")]
    [SerializeField] private float cameraSizeExpansion = 1.5f;
    [SerializeField] private float expandDuration = 0.8f;

    private float initialCameraSize;
    private Vector3 initialPlayerPos;
    private bool pendingExpansion = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCamera == null) mainCamera = Camera.main;

        initialCameraSize = mainCamera.orthographicSize;
        initialPlayerPos  = player.position;
    }

    private void Start()
    {
        GameManager.Instance.OnNewGameStarted += ApplyUniverseSizeImmediate;
        GameManager.Instance.OnStateChanged   += OnStateChanged;
        UpgradeManager.Instance.OnUniverseLevelChanged += OnUniverseLevelChanged;
        ApplyUniverseSizeImmediate();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewGameStarted -= ApplyUniverseSizeImmediate;
            GameManager.Instance.OnStateChanged   -= OnStateChanged;
        }
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUniverseLevelChanged -= OnUniverseLevelChanged;
    }

    private void OnUniverseLevelChanged()
    {
        pendingExpansion = true;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing && pendingExpansion)
        {
            pendingExpansion = false;
            StartCoroutine(SmoothExpandRoutine());
        }
    }

    private IEnumerator SmoothExpandRoutine()
    {
        Time.timeScale = 0f;

        float targetScale = GetTargetScale();

        float fromCamSize  = mainCamera.orthographicSize;
        float toCamSize    = initialCameraSize * targetScale;
        Vector3 fromWall   = wallsParent.localScale;
        Vector3 toWall     = Vector3.one * targetScale;
        Vector3 fromPlayer = player.position;
        Vector3 toPlayer   = initialPlayerPos * targetScale;

        float t = 0f;
        while (t < expandDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / expandDuration));

            mainCamera.orthographicSize = Mathf.Lerp(fromCamSize, toCamSize, p);
            wallsParent.localScale      = Vector3.Lerp(fromWall, toWall, p);
            player.position             = Vector3.Lerp(fromPlayer, toPlayer, p);

            yield return null;
        }

        mainCamera.orthographicSize = toCamSize;
        wallsParent.localScale      = toWall;
        player.position             = toPlayer;

        Time.timeScale = 1f;
    }

    private void ApplyUniverseSizeImmediate()
    {
        float scale = GetTargetScale();
        mainCamera.orthographicSize = initialCameraSize * scale;
        wallsParent.localScale      = Vector3.one * scale;
        player.position             = initialPlayerPos * scale;
    }

    public float CurrentScale => GetTargetScale();

    private float GetTargetScale()
    {
        int level = UpgradeManager.Instance.UniverseSizeLevel;
        return (initialCameraSize + cameraSizeExpansion * (level - 1)) / initialCameraSize;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpgradeManager.Instance.UpgradePassive(PassiveSkillId.Universe);
            Debug.Log($"[TEST] UniverseSize Lv.{UpgradeManager.Instance.UniverseSizeLevel}");
        }
    }
#endif
}

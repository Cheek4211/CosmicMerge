using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance;
    private readonly HashSet<int> discoveredLevelsThisSession = new();

    [Header("System")]
    [SerializeField] private GameObject baseShipPrefab;
    [SerializeField] private MergeUpgradeUI mergeUpgradeUI;

    [Header("Ship Database")]
    [SerializeField] private ShipData[] shipDatabase;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionRadiusPerLevel = 0.1f;
    [SerializeField] private float explosionForce = 4f;
    [SerializeField] private float explosionForcePerLevel = 0.5f;
    
    private readonly List<Collider2D> activeShipColliders = new();
    private readonly HashSet<ShipController> activeShips = new();
    private static readonly WaitForSeconds waitSettleDelay = new(0.5f);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameManager.Instance.OnNewGameStarted += discoveredLevelsThisSession.Clear;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnNewGameStarted -= discoveredLevelsThisSession.Clear;
    }

    public void RegisterShipCollider(Collider2D newCol)
    {
        activeShipColliders.RemoveAll(c => c == null);

        foreach (var existing in activeShipColliders)
            Physics2D.IgnoreCollision(newCol, existing, true);

        activeShipColliders.Add(newCol);
    }

    public int GetRandomSpawnLevelIndex()
    {
        int techLevel = UpgradeManager.Instance.TechLevel;
        
        int maxIndex = Mathf.Min(techLevel, shipDatabase.Length);

        return Random.Range(0, maxIndex);
    }

    public ShipController SpawnShip(Vector2 position, int levelIndex)
    {
        
        if (levelIndex >= shipDatabase.Length) return null;

        GameObject newShipObj = Instantiate(baseShipPrefab, position, Quaternion.identity);
        ShipController shipController = newShipObj.GetComponent<ShipController>();

        shipController.Initialize(shipDatabase[levelIndex]);

        activeShips.Add(shipController);
        return shipController;
    }

    public void MergeShips(Vector2 position, int currentLevel)
    {
        if (currentLevel >= shipDatabase.Length) return;

        int earnedScore = shipDatabase[currentLevel - 1].scoreOnMerge;
        ScoreManager.Instance.AddScore(earnedScore);

        float leveledRadius = explosionRadius + (currentLevel - 1) * explosionRadiusPerLevel;
        float leveledForce = explosionForce + (currentLevel - 1) * explosionForcePerLevel;
        ApplyExplosionForce2D(position, leveledRadius, leveledForce);

        SpawnShip(position, currentLevel);

        int newLevel = currentLevel + 1;
        if (newLevel >= 3 && !discoveredLevelsThisSession.Contains(newLevel))
        {
            discoveredLevelsThisSession.Add(newLevel);
            if (mergeUpgradeUI != null) StartCoroutine(ShowUpgradeWhenSettled());
        }
    }

    private IEnumerator ShowUpgradeWhenSettled()
    {
        const float velocityThreshold = 0.08f;
        const float maxWait = 4f;
        float elapsed = 0f;

        GameManager.Instance.ChangeState(GameState.MergeSettling);

        yield return waitSettleDelay;

        while (elapsed < maxWait)
        {
            bool allSettled = true;
            foreach (var col in activeShipColliders)
            {
                if (col == null) continue;
                var rb = col.GetComponent<Rigidbody2D>();
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic && rb.linearVelocity.sqrMagnitude > velocityThreshold * velocityThreshold)
                {
                    allSettled = false;
                    break;
                }
            }
            if (allSettled) break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        mergeUpgradeUI.Show();
    }

    public void RemoveShip(ShipController ship)
    {
        activeShips.Remove(ship);
    }

    private void ApplyExplosionForce2D(Vector2 explosionCenter, float radius, float force)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionCenter, radius);
        foreach (Collider2D col in colliders)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic) continue;

            Vector2 direction = (Vector2)col.transform.position - explosionCenter;
            float distance = direction.magnitude;
            if (distance <= 0) continue;

            float forceMultiplier = Mathf.Max(0f, 1f - (distance / radius));
            rb.AddForce(direction.normalized * force * forceMultiplier, ForceMode2D.Impulse);
        }
    }
}

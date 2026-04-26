using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance;

    [Header("System")]
    [SerializeField] private GameObject baseShipPrefab;

    [Header("Ship Database")]
    [SerializeField] private ShipData[] shipDatabase;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionRadiusPerLevel = 0.1f;
    [SerializeField] private float explosionForce = 4f;
    [SerializeField] private float explosionForcePerLevel = 0.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        return shipController;
    }

    public void MergeShips(Vector2 position, int currentLevel)
    {
        if (currentLevel >= shipDatabase.Length) 
        {
            // 최종 우주선
            return; 
        }

        int earnedScore = shipDatabase[currentLevel - 1].scoreOnMerge;
        ScoreManager.Instance.AddScore(earnedScore);

        float leveledRadius = explosionRadius + (currentLevel - 1) * explosionRadiusPerLevel;
        float leveledForce = explosionForce + (currentLevel - 1) * explosionForcePerLevel;
        ApplyExplosionForce2D(position, leveledRadius, leveledForce);

        SpawnShip(position, currentLevel);
    }

    // 2D 전용 폭발 물리 공식 직접 구현
    private void ApplyExplosionForce2D(Vector2 explosionCenter, float radius, float force)
    {
        // 1. 중심점에서 radius 반경 안에 있는 모든 콜라이더(우주선)를 찾아냅니다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionCenter, radius);

        foreach (Collider2D col in colliders)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();

            // Rigidbody가 없거나, 발사대에서 대기 중인(isKinematic) 우주선은 밀어내지 않습니다.
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                // 2. 방향 벡터 계산: 목표 위치에서 폭발 중심점을 빼면 밀려날 방향이 나옵니다.
                Vector2 direction = (Vector2)col.transform.position - explosionCenter;
                float distance = direction.magnitude;

                // 중심점과 완전히 겹쳐있을 때의 오류(0으로 나누기) 방지
                if (distance <= 0) continue;

                // 3. 거리에 따른 힘 감쇠(Attenuation) 계산
                // 폭발 중심에 가까울수록(distance가 0에 가까울수록) 1에 가까운 힘을 받고,
                // 가장자리에 있을수록 0에 가까운 힘을 받도록 선형 보간합니다.
                float forceMultiplier = Mathf.Max(0f, 1f - (distance / radius));

                // 4. 정규화된 방향(방향만 남김) * 설정한 폭발력 * 거리에 따른 비율
                rb.AddForce(direction.normalized * force * forceMultiplier, ForceMode2D.Impulse);
            }
        }
    }
}

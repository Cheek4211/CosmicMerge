using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class ShipController : MonoBehaviour
{
    private const float STOPPED_VELOCITY_THRESHOLD = 0.1f;

    [SerializeField] private ShipData myData;

    [Header("Out of Bounds Warning")]
    [SerializeField] private float maxWarningTime = 3f;
    [SerializeField] private TextMeshPro warningText;
    [SerializeField] private PolygonCollider2D polygonCollider;

    private bool isOutOfBounds = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Coroutine warningCoroutine;
    private bool isMerged = false;
    private bool isLaunched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (warningText != null) warningText.gameObject.SetActive(false);

        var mat = new PhysicsMaterial2D { friction = 0f, bounciness = 0.05f };
        foreach (var col in GetComponents<Collider2D>())
            col.sharedMaterial = mat;
    }

    public void Initialize(ShipData data)
    {
        myData = data;
        if (data.shipSprite != null)
        {
            spriteRenderer.sprite = data.shipSprite;
            UpdatePolygonCollider(data.shipSprite);
        }
        transform.localScale = Vector3.one * data.scale;

        if (warningText != null)
        {
            float halfHeight = spriteRenderer.sprite.bounds.extents.y;
            warningText.transform.localPosition = Vector3.up * halfHeight;
        }

        isLaunched = true;

        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(OutBoundsWarningRoutine());
    }

    public void SetReadyState()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        isLaunched = false;
    }

    public void Launch(Vector2 direction, float force, float multiplier)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(direction * force * multiplier, ForceMode2D.Impulse);
        isLaunched = true;

        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(OutBoundsWarningRoutine());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!gameObject.activeInHierarchy || isMerged) return;
        if (other.CompareTag("UniverseBoundary") && rb.bodyType == RigidbodyType2D.Dynamic)
            isOutOfBounds = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("UniverseBoundary"))
        {
            isOutOfBounds = false;
            if (warningText != null) warningText.gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!isLaunched) return;

        if (!other.gameObject.TryGetComponent(out ShipController otherShip)) return;

        if (!otherShip.isLaunched) return;

        // isMerged 플래그로만 판별 — Unity는 싱글스레드이므로 같은 프레임에서
        // 먼저 실행된 쪽이 두 플래그를 모두 true로 세팅하면, 나중에 실행되는
        // 상대방의 OnCollisionEnter2D는 조건을 통과하지 못해 중복 합성이 방지됩니다.
        if (myData.level == otherShip.myData.level && !isMerged && !otherShip.isMerged)
        {
            isMerged = true;
            otherShip.isMerged = true;

            Vector2 spawnPosition = (transform.position + otherShip.transform.position) / 2f;
            ShipManager.Instance.MergeShips(spawnPosition, myData.level);

            Destroy(gameObject);
            Destroy(otherShip.gameObject);
        }
    }

    private void UpdatePolygonCollider(Sprite newSprite)
    {
        if (polygonCollider == null) return;

        int shapeCount = newSprite.GetPhysicsShapeCount();
        polygonCollider.pathCount = shapeCount;

        List<Vector2> path = new();

        for (int i = 0; i < shapeCount; i++)
        {
            newSprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path);
        }
    }

    private void OnDestroy()
    {
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
    }

    private System.Collections.IEnumerator OutBoundsWarningRoutine()
    {
        while (!isMerged)
        {
            // 1. 우주선이 멈출 때까지 대기
            yield return new WaitUntil(() =>
                rb.linearVelocity.sqrMagnitude < STOPPED_VELOCITY_THRESHOLD || isMerged);

            if (isMerged) yield break;

            // 2. 멈췄을 때 경계 밖이면 타이머 시작
            if (isOutOfBounds)
            {
                if (warningText != null) warningText.gameObject.SetActive(true);

                float timer = maxWarningTime;
                while (timer > 0 && isOutOfBounds && !isMerged)
                {
                    timer -= Time.deltaTime;
                    if (warningText != null)
                    {
                        warningText.text = timer.ToString("F1");
                        warningText.color = Color.red;
                    }
                    yield return null;
                }

                if (isMerged) yield break;

                if (isOutOfBounds)
                {
                    GameManager.Instance.ChangeState(GameState.GameOver);
                    Destroy(gameObject);
                    yield break;
                }

                if (warningText != null) warningText.gameObject.SetActive(false);
            }

            // 3. 다시 움직일 때까지 대기 후 반복
            yield return new WaitUntil(() =>
                rb.linearVelocity.sqrMagnitude >= STOPPED_VELOCITY_THRESHOLD || isMerged);
        }
    }
}

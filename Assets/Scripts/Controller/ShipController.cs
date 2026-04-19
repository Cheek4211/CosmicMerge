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
    private float currentWarningTime;
    private bool hasStoppedOnce = false;

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
        hasStoppedOnce = false;
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
        hasStoppedOnce = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"TriggerEnter: {collision.tag}");
        if (!gameObject.activeInHierarchy) return;

        if (isMerged) return;
        
        if (collision.CompareTag("UniverseBoundary") && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            isOutOfBounds = true;
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            warningCoroutine = StartCoroutine(OutBoundsWarningRoutine());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("UniverseBoundary"))
        {
            isOutOfBounds = false;

            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }

            if (warningText != null) warningText.gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched) return;

        ShipController otherShip = collision.gameObject.GetComponent<ShipController>();

        if (otherShip != null)
        {
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
    }
    private void UpdatePolygonCollider(Sprite newSprite)
    {
        if (polygonCollider == null) return;

        int shapeCount = newSprite.GetPhysicsShapeCount();
        polygonCollider.pathCount = shapeCount;

        List<Vector2> path = new List<Vector2>();
        
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
        while (!hasStoppedOnce)
        {
            if (rb.linearVelocity.sqrMagnitude < STOPPED_VELOCITY_THRESHOLD)
            {
                hasStoppedOnce = true;
            }
            yield return null;
        }

        currentWarningTime = maxWarningTime;
        if (warningText != null) warningText.gameObject.SetActive(true);

        while (currentWarningTime > 0)
        {
            currentWarningTime -= Time.deltaTime;

            if (warningText != null)
            {
                warningText.text = currentWarningTime.ToString("F1");
                warningText.color = Color.red;
            }

            yield return null;
        }

        GameManager.Instance.ChangeState(GameState.GameOver);
        Destroy(gameObject);
    }
}

using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ShipController : MonoBehaviour
{
    private const float STOPPED_VELOCITY_THRESHOLD = 0.1f;

    public bool IsMerged      { get; private set; } = false;
    public bool IsLaunched    { get; private set; } = false;
    public bool IsInsideBounds { get; private set; } = true;
    public bool IsStopped     => rb.linearVelocity.sqrMagnitude < STOPPED_VELOCITY_THRESHOLD * STOPPED_VELOCITY_THRESHOLD;
    public bool IsPhysicsActive => rb != null && rb.bodyType == RigidbodyType2D.Dynamic;

    [SerializeField] private ShipData myData;
    [SerializeField] private float softRepelStrength = 10f;
    [SerializeField] private float outOfBoundsLimit = 3f;

    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private TextMeshPro warningText;

    public float ShipRadius { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Coroutine squishCoroutine;
    private Coroutine outOfBoundsCoroutine;
    private Vector3 baseScale;
    private Collider2D boundaryCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var mat = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
        foreach (var col in GetComponents<Collider2D>())
            col.sharedMaterial = mat;

        var boundaryObj = GameObject.FindGameObjectWithTag("UniverseBoundary");
        if (boundaryObj != null)
            boundaryCollider = boundaryObj.GetComponent<Collider2D>();
    }

    public void Initialize(ShipData data)
    {
        myData = data;
        if (data.shipSprite != null)
            spriteRenderer.sprite = data.shipSprite;

        transform.localScale = Vector3.one * data.scale;
        baseScale = transform.localScale;

        ShipRadius = data.shipSprite != null
            ? data.shipSprite.bounds.extents.x * data.scale
            : data.scale * 0.5f;

        if (circleCollider != null)
            circleCollider.radius = ShipRadius / data.scale;

        transform.localScale = Vector3.zero;

        if (warningText != null)
        {
            float halfHeight = data.shipSprite != null ? data.shipSprite.bounds.extents.y * data.scale : data.scale * 0.5f;
            warningText.transform.localPosition = Vector3.up * halfHeight;
            warningText.gameObject.SetActive(false);
        }

        IsLaunched = true;

        if (circleCollider != null)
            ShipManager.Instance.RegisterShipCollider(circleCollider);

        if (squishCoroutine != null) StopCoroutine(squishCoroutine);
        squishCoroutine = StartCoroutine(SpawnPopRoutine());

    }

    public void SetReadyState()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        IsLaunched = false;
    }

    public void Launch(Vector2 direction, float force, float multiplier)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(direction * force * multiplier, ForceMode2D.Impulse);
        IsLaunched = true;

        if (squishCoroutine != null) StopCoroutine(squishCoroutine);
        squishCoroutine = StartCoroutine(LaunchSquishRoutine());
    }

    public void PrepareForMerge()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (squishCoroutine != null)      { StopCoroutine(squishCoroutine);      squishCoroutine = null; }
        if (outOfBoundsCoroutine != null) { StopCoroutine(outOfBoundsCoroutine); outOfBoundsCoroutine = null; }
        if (warningText != null) warningText.gameObject.SetActive(false);
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;
    }

    void Update()
    {
        if (!IsLaunched || IsMerged) return;

        IsInsideBounds = boundaryCollider == null || boundaryCollider.OverlapPoint(transform.position);

        if (outOfBoundsCoroutine != null && IsInsideBounds)
        {
            StopCoroutine(outOfBoundsCoroutine);
            outOfBoundsCoroutine = null;
            if (warningText != null) warningText.gameObject.SetActive(false);
        }

        if (outOfBoundsCoroutine == null && IsStopped && !IsInsideBounds)
            outOfBoundsCoroutine = StartCoroutine(OutOfBoundsTimerRoutine());
    }

    void FixedUpdate()
    {
        if (!IsLaunched || IsMerged || !IsPhysicsActive) return;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, ShipRadius * 3f);
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.TryGetComponent<ShipController>(out var other)) continue;
            if (other.IsMerged || !other.IsLaunched || !other.IsPhysicsActive) continue;

            Vector2 toMe = (Vector2)transform.position - (Vector2)other.transform.position;
            float dist = toMe.magnitude;
            float contactDist = ShipRadius + other.ShipRadius;

            if (dist > contactDist || dist < 0.001f) continue;

            if (myData.level == other.myData.level && !IsMerged && !other.IsMerged)
            {
                IsMerged = true;
                other.IsMerged = true;

                PrepareForMerge();
                other.PrepareForMerge();

                Vector2 spawnPos = ((Vector2)transform.position + (Vector2)other.transform.position) / 2f;
                StartCoroutine(MergeAbsorbRoutine(other, spawnPos));
                return;
            }

            if (myData.level != other.myData.level)
            {
                float overlap = contactDist - dist;
                float forceMag = softRepelStrength * (overlap / contactDist);
                rb.AddForce(toMe.normalized * (forceMag * rb.mass), ForceMode2D.Force);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsLaunched || IsMerged) return;
        CancelWallBounce(other);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (!IsLaunched || IsMerged) return;
        CancelWallBounce(other);
    }

    private void CancelWallBounce(Collision2D other)
    {
        if (other.contactCount == 0) return;
        Vector2 normal = other.contacts[0].normal;
        float normalSpeed = Vector2.Dot(rb.linearVelocity, normal);
        if (normalSpeed < 0f)
            rb.linearVelocity -= normal * normalSpeed;
    }

    private IEnumerator SpawnPopRoutine()
    {
        float duration = 0.42f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = baseScale * SpringScale(Mathf.Clamp01(t / duration));
            yield return null;
        }
        transform.localScale = baseScale;
        squishCoroutine = null;
    }

    private IEnumerator LaunchSquishRoutine()
    {
        float duration = 0.28f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            transform.localScale = baseScale * (1f - Mathf.Sin(p * Mathf.PI) * 0.22f);
            yield return null;
        }
        transform.localScale = baseScale;
        squishCoroutine = null;
    }

    private IEnumerator MergeAbsorbRoutine(ShipController other, Vector2 target)
    {
        float duration = 0.2f;
        float t = 0f;
        Vector3 myStartPos    = transform.position;
        Vector3 otherStartPos = other != null ? other.transform.position : (Vector3)(Vector2)target;
        Vector3 myStartScale    = transform.localScale;
        Vector3 otherStartScale = other != null ? other.transform.localScale : Vector3.zero;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = EaseInCubic(Mathf.Clamp01(t / duration));

            transform.position   = Vector3.Lerp(myStartPos, target, p);
            transform.localScale = Vector3.Lerp(myStartScale, Vector3.zero, p);

            if (other != null && other.gameObject != null)
            {
                other.transform.position   = Vector3.Lerp(otherStartPos, target, p);
                other.transform.localScale = Vector3.Lerp(otherStartScale, Vector3.zero, p);
            }

            yield return null;
        }

        ShipManager.Instance.MergeShips(target, myData.level);

        if (other != null && other.gameObject != null) Destroy(other.gameObject);
        Destroy(gameObject);
    }

    private float SpringScale(float t)
    {
        const float zeta = 0.38f;
        const float omega = 14f;
        float omegaD = omega * Mathf.Sqrt(1f - zeta * zeta);
        return 1f - Mathf.Exp(-zeta * omega * t) * Mathf.Cos(omegaD * t);
    }

    private float EaseInCubic(float t) => t * t * t;

    private IEnumerator OutOfBoundsTimerRoutine()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(true);
            warningText.color = Color.red;
        }

        float timer = outOfBoundsLimit;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (warningText != null)
                warningText.text = Mathf.CeilToInt(timer).ToString();
            yield return null;
        }

        if (warningText != null) warningText.gameObject.SetActive(false);
        GameManager.Instance.ChangeState(GameState.GameOver);
    }

    private void OnDestroy()
    {
        if (squishCoroutine != null)      StopCoroutine(squishCoroutine);
        if (outOfBoundsCoroutine != null) StopCoroutine(outOfBoundsCoroutine);
        if (ShipManager.Instance != null) ShipManager.Instance.RemoveShip(this);
    }
}

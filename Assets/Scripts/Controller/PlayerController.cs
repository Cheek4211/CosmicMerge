using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float MIN_DRAG_THRESHOLD = 0.01f;
    private const float MIN_LAUNCH_FORCE = 0.5f;
    private const float RELOAD_DELAY = 0.5f;

    [Header("Launch Settings")]
    [SerializeField] private float maxPullDistance = 5f;
    [Range(0, 90)]
    [SerializeField] private float maxAngleLimit = 80f;
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private float orbitRadius = 1.5f;

    [Header("Prefabs & Visuals")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LineRenderer dragLine;

    public event Action<Vector2, float> OnDragVectorCalculated;
    public event Action OnDragEnded;

    private Vector2 startPoint;
    private Vector2 currentPoint;
    private bool isDragging = false;

    private Vector2 finalDirection;
    private float finalForce;

    private ShipController currentShip;

    void Start()
    {
        if (dragLine != null) dragLine.enabled = false;
        if (spawnPoint == null) spawnPoint = this.transform;

        GameManager.Instance.OnStateChanged += OnStateChanged;

        if (currentShip == null)
        {
            ReloadShip();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing && currentShip == null)
            ReloadShip();
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (dragLine != null) dragLine.enabled = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateVector();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (dragLine != null) dragLine.enabled = false;

            OnDragEnded?.Invoke();

            if (currentShip != null && finalForce >= MIN_LAUNCH_FORCE)
            {
                currentShip.Launch(finalDirection, finalForce, forceMultiplier);

                currentShip = null;

                StartCoroutine(ReloadShipDelayed());
            }
        }
    }

    private void CalculateVector()
    {
        Vector2 dragVector = startPoint - currentPoint;

        if (dragVector.sqrMagnitude < MIN_DRAG_THRESHOLD)
        {
            finalDirection = Vector2.up;
            finalForce = 0f;
            return;
        }

        float angle = Vector2.SignedAngle(Vector2.up, dragVector);
        float clampedAngle = Mathf.Clamp(angle, -maxAngleLimit, maxAngleLimit);

        finalDirection = Quaternion.Euler(0, 0, clampedAngle) * Vector2.up;
        finalForce = Mathf.Clamp(dragVector.magnitude, 0, maxPullDistance);

        if (currentShip != null)
        {
            currentShip.transform.position = (Vector2)spawnPoint.position + (finalDirection * orbitRadius);

            currentShip.transform.up = finalDirection;
        }

        OnDragVectorCalculated?.Invoke(finalDirection, finalForce);

        if (dragLine != null)
        {
            dragLine.SetPosition(0, spawnPoint.position);
            dragLine.SetPosition(1, (Vector2)spawnPoint.position + (finalDirection * finalForce));
        }
    }

    private System.Collections.IEnumerator ReloadShipDelayed()
    {
        yield return new WaitForSeconds(RELOAD_DELAY);
        ReloadShip();
    }

    private void ReloadShip()
    {
        Vector2 spawnPos = (Vector2)spawnPoint.position + (Vector2.up * orbitRadius);
        int randomDropLevel = ShipManager.Instance.GetRandomSpawnLevelIndex();
        currentShip = ShipManager.Instance.SpawnShip(spawnPos, randomDropLevel);

        currentShip.SetReadyState();
    }
}

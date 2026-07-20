using UnityEngine;

public class EnemyMovementBehavior : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private int damageOnContact = 10;
    [SerializeField] private float contactCooldown = 0.35f;
    [SerializeField] private bool moveLeft = true;
    [SerializeField] private float bobAmplitude = 0.08f;
    [SerializeField] private float bobFrequency = 2.2f;
    [SerializeField] private float heightAdjustSpeedMin = 0.05f;
    [SerializeField] private float heightAdjustSpeedMax = 0.16f;
    private float heightAdjustSpeed;
    [SerializeField] private float knockbackDuration = 0.28f;
    [SerializeField] private float knockbackForceMin = 0.9f;
    [SerializeField] private float knockbackForceMax = 1.5f;
    private float knockbackForce = 1.25f;
    [SerializeField] private float knockbackHeight = 0.02f;
    [SerializeField] private float knockbackAngleVariance = 6f;
    [SerializeField] private float knockbackExtraCooldown = 0.2f;

    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float contactCooldownTimer;
    private float moveDirection = -1f;
    private bool isActive;
    private bool isKnockedBack;
    private float bobTimer;
    private float baseY;
    private float knockbackTimer;
    private Vector2 knockbackStartPosition;
    private Vector2 knockbackTargetPosition;
    private readonly Collider2D[] overlapResults = new Collider2D[4];

    private Vector2 FindSafeKnockbackTarget(Vector2 origin, Vector2 direction, float force)
    {
        float radius = GetKnockbackCollisionRadius();

        for (int step = 10; step >= 1; step--)
        {
            Vector2 candidate = origin + direction * force * (step / 10f);
            if (!IsOverlapEnemy(candidate, radius))
                return candidate;
        }

        for (int angleStep = 1; angleStep <= 3; angleStep++)
        {
            float offset = angleStep * 6f;
            if (TrySafeDirection(origin, direction, force, radius, offset, out Vector2 safeTarget))
                return safeTarget;
            if (TrySafeDirection(origin, direction, force, radius, -offset, out safeTarget))
                return safeTarget;
        }

        return origin + direction * Mathf.Max(radius * 0.5f, force * 0.2f);
    }

    private bool TrySafeDirection(Vector2 origin, Vector2 baseDirection, float force, float radius, float angleOffset, out Vector2 safeTarget)
    {
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float angle = baseAngle + angleOffset;
        Vector2 rotated = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        Vector2 candidate = origin + rotated * force;
        if (!IsOverlapEnemy(candidate, radius))
        {
            safeTarget = candidate;
            return true;
        }

        safeTarget = Vector2.zero;
        return false;
    }

    private bool IsOverlapEnemy(Vector2 position, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit == null || hit == enemyCollider)
                continue;

            var enemy = hit.GetComponentInParent<EnemyGestureCommand>();
            if (enemy != null && enemy.gameObject != gameObject)
                return true;
        }

        return false;
    }

    private float GetKnockbackCollisionRadius()
    {
        if (enemyCollider != null)
            return Mathf.Max(enemyCollider.bounds.extents.x, enemyCollider.bounds.extents.y) * 1.1f;
        return 0.5f;
    }

    public void Initialize(PlayerHealth playerHealthReference, Collider2D collider, SpriteRenderer renderer)
    {
        playerHealth = playerHealthReference;
        enemyCollider = collider;
        spriteRenderer = renderer;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = GetComponentInChildren<Rigidbody2D>(true);

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        playerTransform = playerHealth != null ? playerHealth.transform : null;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        moveDirection = moveLeft ? -1f : 1f;
        baseY = transform.position.y;
        bobTimer = 0f;
        heightAdjustSpeed = Random.Range(heightAdjustSpeedMin, heightAdjustSpeedMax);
        knockbackForce = Random.Range(knockbackForceMin, knockbackForceMax);
    }

    public void SetActive(bool active)
    {
        if (isKnockedBack)
            return;

        isActive = active;
        Debug.Log($"EnemyMovementBehavior.SetActive active={isActive} moveDirection={moveDirection}");
    }

    public void Tick()
    {
        Debug.Log($"EnemyMovementBehavior.Tick isActive={isActive} isKnockedBack={isKnockedBack} moveDirection={moveDirection} position={transform.position} localPos={transform.localPosition}");

        if (contactCooldownTimer > 0f)
            contactCooldownTimer -= Time.deltaTime;

        if (isKnockedBack)
        {
            knockbackTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(knockbackTimer / knockbackDuration);
            float arc = Mathf.Sin(progress * Mathf.PI) * knockbackHeight;
            Vector2 newPosition = Vector2.Lerp(knockbackStartPosition, knockbackTargetPosition, progress);
            newPosition.y += arc;
            MoveToPosition(newPosition);

            if (progress >= 1f)
            {
                isKnockedBack = false;
                isActive = true;
                baseY = newPosition.y;
                bobTimer = 0f;
            }

            return;
        }

        Vector2 currentPosition = rb != null ? rb.position : (Vector2)transform.position;

        Vector2 moveDelta = Vector2.zero;
        if (isActive && playerTransform != null)
        {
            float deltaX = playerTransform.position.x - currentPosition.x;
            float horizontalStep = Mathf.Sign(deltaX) * moveSpeed * Time.deltaTime;
            if (Mathf.Abs(deltaX) < Mathf.Abs(horizontalStep))
                horizontalStep = deltaX;

            moveDelta = new Vector2(horizontalStep, 0f);
            moveDirection = deltaX < 0f ? -1f : 1f;

            baseY = Mathf.MoveTowards(baseY, playerTransform.position.y, heightAdjustSpeed * Time.deltaTime);
        }

        bobTimer += Time.deltaTime * bobFrequency;
        float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;

        Vector2 targetPosition = currentPosition + moveDelta;
        targetPosition.y = baseY + bobOffset;
        MoveToPosition(targetPosition);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection > 0f;
        }

        if (isActive)
            TryDamagePlayerOnContact();
    }

    private void TryDamagePlayerOnContact()
    {
        if (!isActive || playerHealth == null || enemyCollider == null || contactCooldownTimer > 0f)
            return;

        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;
        int hitCount = Physics2D.OverlapCollider(enemyCollider, filter, overlapResults);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = overlapResults[i];
            if (hit == null || hit == enemyCollider)
                continue;

            HandlePlayerContact(hit);
            if (contactCooldownTimer > 0f)
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerContact(other);
    }

    private void HandlePlayerContact(Collider2D hit)
    {
        if (hit == null || playerHealth == null || contactCooldownTimer > 0f)
            return;

        var hitPlayerHealth = hit.GetComponentInParent<PlayerHealth>();
        if (hitPlayerHealth == null && hit.transform.root != null)
            hitPlayerHealth = hit.transform.root.GetComponent<PlayerHealth>();

        if (hitPlayerHealth == null || hitPlayerHealth != playerHealth)
            return;

        playerHealth.TakeDamage(damageOnContact);
        isActive = false;
        float knockbackDuration = PlayKnockback(true);
        contactCooldownTimer = contactCooldown + knockbackDuration + knockbackExtraCooldown;
        Debug.Log($"EnemyMovementBehavior: Player hit, stopping enemy and playing knockback for {knockbackDuration:F2}s.");
    }

    private void MoveToPosition(Vector2 position)
    {
        if (rb != null)
        {
            rb.MovePosition(position);
        }
        else
        {
            transform.position = position;
        }
    }

    public float PlayKnockback(bool strong = false)
    {
        isKnockedBack = true;
        isActive = false;
        knockbackTimer = 0f;
        knockbackDuration = strong ? 0.48f : 0.28f;
        float force = strong ? knockbackForce * 1.4f : knockbackForce;
        Vector2 currentPosition = rb != null ? rb.position : (Vector2)transform.position;

        Vector2 away = Vector2.right * -moveDirection;
        if (playerTransform != null)
        {
            away = ((Vector2)rb.position - (Vector2)playerTransform.position).normalized;
        }

        away.y *= 0.35f;
        away.Normalize();

        float baseAngle = Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg;
        float angleOffset = Random.Range(-knockbackAngleVariance, knockbackAngleVariance);
        float angle = baseAngle + angleOffset;
        float radians = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;

        knockbackStartPosition = currentPosition;
        knockbackTargetPosition = FindSafeKnockbackTarget(currentPosition, direction, force);
        Debug.Log($"EnemyMovementBehavior.PlayKnockback start={knockbackStartPosition} target={knockbackTargetPosition} angle={angle:F1} duration={knockbackDuration}");
        return knockbackDuration;
    }
}

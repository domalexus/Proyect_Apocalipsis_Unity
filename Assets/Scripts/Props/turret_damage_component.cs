using System.Collections;
using UnityEngine;

public class turret_damage_component : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LayerMask attackLayerMask = Physics2D.AllLayers;
    [SerializeField] private float startupDelay = 1.5f; // tiempo antes del primer ataque
    [SerializeField] private bool repeatAttack = false;
    [SerializeField] private float attackInterval = 1f; // intervalo entre ataques si repeatAttack=true

    [Header("Visual")]
    [SerializeField] private bool showAttackRadius = true;
    [SerializeField] private Color attackRadiusColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float attackRadiusLineWidth = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private LineRenderer attackRadiusRenderer;
    private const int AttackRadiusSegments = 64;
    [Header("Animación")]
    [SerializeField] private bool enableAttackAnimation = true;
    [SerializeField] private Color attackAnimationColor = Color.cyan;
    [SerializeField] private float attackAnimationDuration = 0.4f;
    [SerializeField] private float attackScaleMultiplier = 1.15f;

    private Coroutine attackRoutine;

    private void Start()
    {
        CreateAttackRadiusRenderer();
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private void OnDestroy()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);
    }

    private IEnumerator AttackRoutine()
    {
        if (showDebugInfo)
            Debug.Log($"[Turret] Startup delay {startupDelay}s before attacking.");

        yield return new WaitForSeconds(Mathf.Max(0f, startupDelay));

        do
        {
            PerformAttack();
            if (!repeatAttack)
                break;

            yield return new WaitForSeconds(Mathf.Max(0.01f, attackInterval));
        }
        while (true);
    }

    private void CreateAttackRadiusRenderer()
    {
        if (!showAttackRadius)
            return;

        attackRadiusRenderer = gameObject.GetComponent<LineRenderer>();
        if (attackRadiusRenderer == null)
            attackRadiusRenderer = gameObject.AddComponent<LineRenderer>();

        attackRadiusRenderer.loop = true;
        attackRadiusRenderer.positionCount = AttackRadiusSegments;
        attackRadiusRenderer.startWidth = attackRadiusLineWidth;
        attackRadiusRenderer.endWidth = attackRadiusLineWidth;
        attackRadiusRenderer.material = new Material(Shader.Find("Sprites/Default"));
        attackRadiusRenderer.startColor = attackRadiusColor;
        attackRadiusRenderer.endColor = attackRadiusColor;
        attackRadiusRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        UpdateAttackRadiusRenderer();
    }

    private void UpdateAttackRadiusRenderer()
    {
        if (!showAttackRadius || attackRadiusRenderer == null)
            return;

        float angleStep = 360f / AttackRadiusSegments;
        for (int i = 0; i < AttackRadiusSegments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 point = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * attackRadius;
            attackRadiusRenderer.SetPosition(i, point);
        }
    }

    private void PerformAttack()
    {
        if (enableAttackAnimation)
        {
            StartCoroutine(PlayAttackAnimation());
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, attackLayerMask);
        if (showDebugInfo)
            Debug.Log($"[Turret] Attack executed. Range={attackRadius}, hits={hits.Length}");

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
                continue;

            damageable_component damageable = hit.GetComponent<damageable_component>();
            if (damageable != null)
            {
                float applied = damageable.TakeDamage(attackDamage);
                if (showDebugInfo)
                    Debug.Log($"[Turret] Enemy '{hit.name}' hit. Damage applied={applied}");
            }
            else if (showDebugInfo)
            {
                Debug.LogWarning($"[Turret] Enemy '{hit.name}' has tag Enemy but no damageable_component.");
            }
        }
    }

    private IEnumerator PlayAttackAnimation()
    {
        // Cache renderers
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Color srOriginal = sr != null ? sr.color : Color.white;
        Color mrOriginal = mr != null ? mr.material.color : Color.white;
        Color lrStart = attackRadiusRenderer != null ? attackRadiusRenderer.startColor : Color.white;
        Color lrEnd = attackRadiusRenderer != null ? attackRadiusRenderer.endColor : Color.white;

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * attackScaleMultiplier;

        float half = Mathf.Max(0.01f, attackAnimationDuration) * 0.5f;
        float t = 0f;

        // Scale up and change color
        while (t < half)
        {
            float ratio = t / half;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, ratio);
            if (sr != null) sr.color = Color.Lerp(srOriginal, attackAnimationColor, ratio);
            if (mr != null) mr.material.color = Color.Lerp(mrOriginal, attackAnimationColor, ratio);
            if (attackRadiusRenderer != null)
            {
                Color c = Color.Lerp(lrStart, attackAnimationColor, ratio);
                attackRadiusRenderer.startColor = c;
                attackRadiusRenderer.endColor = c;
            }
            t += Time.deltaTime;
            yield return null;
        }

        // Ensure target
        transform.localScale = targetScale;
        if (sr != null) sr.color = attackAnimationColor;
        if (mr != null) mr.material.color = attackAnimationColor;
        if (attackRadiusRenderer != null)
        {
            attackRadiusRenderer.startColor = attackAnimationColor;
            attackRadiusRenderer.endColor = attackAnimationColor;
        }

        // Scale back and revert color
        t = 0f;
        while (t < half)
        {
            float ratio = t / half;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, ratio);
            if (sr != null) sr.color = Color.Lerp(attackAnimationColor, srOriginal, ratio);
            if (mr != null) mr.material.color = Color.Lerp(attackAnimationColor, mrOriginal, ratio);
            if (attackRadiusRenderer != null)
            {
                Color c = Color.Lerp(attackAnimationColor, lrStart, ratio);
                attackRadiusRenderer.startColor = c;
                attackRadiusRenderer.endColor = c;
            }
            t += Time.deltaTime;
            yield return null;
        }

        // Restore originals
        transform.localScale = originalScale;
        if (sr != null) sr.color = srOriginal;
        if (mr != null) mr.material.color = mrOriginal;
        if (attackRadiusRenderer != null)
        {
            attackRadiusRenderer.startColor = lrStart;
            attackRadiusRenderer.endColor = lrEnd;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showAttackRadius)
            return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}

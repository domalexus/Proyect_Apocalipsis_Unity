using UnityEngine;
using UnityEngine.InputSystem;

public class player_attack_component : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LayerMask attackLayerMask = Physics2D.AllLayers;

    [Header("Visual")]
    [SerializeField] private bool showAttackRadius = true;
    [SerializeField] private Color attackRadiusColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float attackRadiusLineWidth = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private InputAction grabAction;
    private LineRenderer attackRadiusRenderer;
    private const int AttackRadiusSegments = 64;

    private void Awake()
    {
        grabAction = InputSystem.actions.FindAction("attack");
        if (grabAction != null)
        {
            grabAction.Enable();
            if (showDebugInfo)
                Debug.Log("[PlayerAttack] Attack action enabled.");
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("[PlayerAttack] No se encontró la acción 'attack' en InputSystem.actions.");
        }

        CreateAttackRadiusRenderer();
    }

    private void OnDestroy()
    {
        if (grabAction != null)
            grabAction.Disable();
    }

    private void CreateAttackRadiusRenderer()
    {
        if (!showAttackRadius)
            return;

        attackRadiusRenderer = gameObject.GetComponent<LineRenderer>();
        if (attackRadiusRenderer == null)
        {
            attackRadiusRenderer = gameObject.AddComponent<LineRenderer>();
        }

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
        if (grabAction != null && grabAction.WasPressedThisFrame())
        {
            PerformAttack();
        }

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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, attackLayerMask);
        if (showDebugInfo)
            Debug.Log($"[PlayerAttack] Attack triggered. Range={attackRadius}, hits={hits.Length}");

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
                continue;

            damageable_component damageable = hit.GetComponent<damageable_component>();
            if (damageable != null)
            {
                float appliedDamage = damageable.TakeDamage(attackDamage);
                if (showDebugInfo)
                    Debug.Log($"[PlayerAttack] Enemy '{hit.name}' hit. Damage applied={appliedDamage}");
            }
            else if (showDebugInfo)
            {
                Debug.LogWarning($"[PlayerAttack] Enemy '{hit.name}' tiene tag Enemy pero no damageable_component.");
            }
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

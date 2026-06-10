using UnityEngine;

/// <summary>
/// Componente de comportamiento del enemigo.
/// Maneja el movimiento hacia el jugador.
/// </summary>
public class enemy_behavior_component : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject player;
    [SerializeField] private Rigidbody2D rb2D;

    [Header("Velocidad de Movimiento")]
    [SerializeField] private float baseSpeed = 3f; // Velocidad normal de persecución
    [SerializeField] private float maxSpeed = 5f; // Velocidad máxima
    [SerializeField] private float acceleration = 0.5f; // Aceleración al iniciar movimiento

    [Header("Sistema de Proximidad")]
    [SerializeField] private float minDistanceToPlayer = 1.5f; // Distancia mínima antes de reducir velocidad
    [SerializeField] private float speedReductionFactor = 0.3f; // Factor de reducción (0.3 = 30% de velocidad)

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    // Variables internas
    private Vector2 moveDirection = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;

    private void Awake()
    {
        // Obtener referencias automáticamente si no están asignadas
        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (player == null)
            return;

        // Calcular dirección hacia el jugador
        Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
        moveDirection = directionToPlayer;
    }

    private void FixedUpdate()
    {
        if (player == null || rb2D == null)
            return;

        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // Limitar la velocidad
        currentVelocity = Vector2.Lerp(currentVelocity, moveDirection * baseSpeed, acceleration * Time.fixedDeltaTime);
        currentVelocity = Vector2.ClampMagnitude(currentVelocity, maxSpeed);

        // Aplicar velocidad al Rigidbody
        rb2D.linearVelocity = currentVelocity;
    }

    /// <summary>
    /// Cambia la velocidad base del enemigo.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        baseSpeed = Mathf.Max(newSpeed, 0.1f);
    }

    /// <summary>
    /// Establece la velocidad máxima.
    /// </summary>
    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxSpeed = Mathf.Max(newMaxSpeed, baseSpeed);
    }

    /// <summary>
    /// Asigna el objetivo (jugador) al enemigo.
    /// </summary>
    public void SetTarget(GameObject target)
    {
        player = target;
    }

    /// <summary>
    /// Detiene el movimiento del enemigo.
    /// </summary>
    public void StopMovement()
    {
        moveDirection = Vector2.zero;
        currentVelocity = Vector2.zero;
        if (rb2D != null)
            rb2D.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo)
            return;

        // Dibujar dirección de movimiento
        if (player != null)
        {
            Gizmos.color = Color.red;
            Vector2 dirToPlayer = (player.transform.position - transform.position).normalized;
            Gizmos.DrawLine(transform.position, (Vector3)transform.position + (Vector3)dirToPlayer * 2f);
        }
    }
}
